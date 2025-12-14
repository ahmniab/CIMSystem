namespace CIMSystemGUI.Components

open System
open System.Threading
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open Avalonia.Media.Imaging
open CIMSystemGUI.Models
open CIMSystemGUI.Helpers
open OpenCvSharp

module StaffValidationView =
    
    let mutable private capture: VideoCapture option = None
    let mutable private isScanning = false

    let view () =
        Component(fun ctx ->
            let ticketId = ctx.useState ""
            let validationResult = ctx.useState ""
            let isValid = ctx.useState false
            let ticketDetails = ctx.useState (None: TicketInfo option)
            
            let cameraImage = ctx.useState (null: Bitmap)
            let isCameraRunning = ctx.useState false

            let formState =
                { ValidationUIHelpers.TicketId = ticketId
                  ValidationUIHelpers.ValidationResult = validationResult
                  ValidationUIHelpers.IsValid = isValid
                  ValidationUIHelpers.TicketDetails = ticketDetails }

            let validateTicket () =
                let validationState = ValidationHelpers.validateTicketId ticketId.Current
                ValidationUIHelpers.updateValidationState formState validationState

            let redeemTicket () =
                let cleanTicketId = ticketId.Current.Trim()
                match ValidationHelpers.redeemTicketWithSeatClearing cleanTicketId with
                | Result.Ok message ->
                    validationResult.Set message
                    isValid.Set false
                    ticketDetails.Set None
                    ticketId.Set ""
                | Result.Error errorMessage -> validationResult.Set errorMessage

            let stopCamera () =
                isScanning <- false
                isCameraRunning.Set false
                match capture with
                | Some cap -> 
                    cap.Release()
                    cap.Dispose()
                | None -> ()
                capture <- None
                cameraImage.Set null

            let startCamera () =
                if isCameraRunning.Current then stopCamera()
                else
                    try
                        let cap = new VideoCapture(0)
                        if cap.IsOpened() then
                            capture <- Some cap
                            isScanning <- true
                            isCameraRunning.Set true
                            
                            let thread = new Thread(fun () ->
                                use detector = new QRCodeDetector()
                                
                                while isScanning do
                                    try
                                        use mat = new Mat()
                                        if cap.Read(mat) && not (mat.Empty()) then
                                            let bitmap = CameraHelper.matToBitmap mat
                                            Dispatcher.UIThread.Post(fun () -> cameraImage.Set bitmap)

                                            let qrCodeText, _ = detector.DetectAndDecode(mat)
                                            
                                            if not (String.IsNullOrWhiteSpace(qrCodeText)) then
                                                Dispatcher.UIThread.Post(fun () ->
                                                    stopCamera()
                                                    ticketId.Set qrCodeText
                                                    validateTicket()
                                                )
                                                isScanning <- false
                                    with _ -> ()
                                    Thread.Sleep(50)
                            )
                            thread.IsBackground <- true
                            thread.Start()
                        else
                            validationResult.Set "Error: Could not open camera."
                    with ex ->
                        validationResult.Set $"Camera Error: {ex.Message}"

            ctx.useEffect (
                (fun () -> 
                    { new IDisposable with member _.Dispose() = stopCamera() }
                ), 
                [] 
            )

            let onKeyDown (args: Avalonia.Input.KeyEventArgs) =
                if args.Key = Avalonia.Input.Key.Enter then validateTicket()

            let renderTicketDetailsSection () =
                match ticketDetails.Current with
                | Some info ->
                    StackPanel.create [
                        StackPanel.orientation Orientation.Vertical
                        StackPanel.spacing 5.0
                        StackPanel.children [
                            TextBlock.create [ TextBlock.text "✅ TICKET FOUND"; TextBlock.foreground Brushes.Green; TextBlock.fontWeight FontWeight.Bold; TextBlock.fontSize 18.0 ]
                            TextBlock.create [ TextBlock.text $"Movie: {info.MovieTitle}"; TextBlock.fontSize 16.0; TextBlock.fontWeight FontWeight.Bold ]
                            TextBlock.create [ TextBlock.text $"Hall: {info.HallName}"; TextBlock.fontSize 14.0 ]
                            TextBlock.create [ TextBlock.text $"Customer: {info.CustomerName}"; TextBlock.fontSize 14.0 ]
                            TextBlock.create [ TextBlock.text $"Seat: R{info.SeatRow}-S{info.SeatColumn}"; TextBlock.fontSize 14.0; TextBlock.foreground Brushes.Blue ]
                        ]
                    ]
                | None -> StackPanel.create []

            StackPanel.create [
                StackPanel.orientation Orientation.Vertical
                StackPanel.spacing 20.0
                StackPanel.margin 30.0
                StackPanel.children [
                    
                    TextBlock.create [
                        TextBlock.text "🎫 Staff Ticket Validator"
                        TextBlock.fontSize 24.0
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]

                    StackPanel.create [
                        StackPanel.horizontalAlignment HorizontalAlignment.Center
                        StackPanel.spacing 10.0
                        StackPanel.children [
                            Button.create [
                                Button.content (if isCameraRunning.Current then "⏹ Stop Camera" else "📷 Scan QR Code")
                                Button.background (if isCameraRunning.Current then Brushes.Red else Brushes.DarkBlue)
                                Button.foreground Brushes.White
                                Button.horizontalAlignment HorizontalAlignment.Center
                                Button.onClick (fun _ -> startCamera())
                            ]

                            if cameraImage.Current <> null then
                                Image.create [
                                    Image.source cameraImage.Current
                                    Image.width 320.0
                                    Image.height 240.0
                                    Image.stretch Stretch.Uniform
                                ]
                        ]
                    ]

                    TextBlock.create [
                        TextBlock.text "Or Enter Ticket ID manually:"
                        TextBlock.foreground Brushes.Gray
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]

                    TextBox.create [
                        TextBox.text ticketId.Current
                        TextBox.onTextChanged ticketId.Set
                        TextBox.watermark "Waiting for Scan..."
                        TextBox.fontSize 20.0
                        TextBox.height 50.0
                        TextBox.textAlignment TextAlignment.Center
                        TextBox.onKeyDown onKeyDown
                    ]

                    StackPanel.create [
                        StackPanel.orientation Orientation.Horizontal
                        StackPanel.horizontalAlignment HorizontalAlignment.Center
                        StackPanel.spacing 10.0
                        StackPanel.children [
                            Button.create [
                                Button.content "🔍 Check Ticket"
                                Button.onClick (fun _ -> validateTicket())
                                Button.width 120.0
                            ]
                            Button.create [
                                Button.content "✅ Allow Entry"
                                Button.onClick (fun _ -> redeemTicket())
                                Button.background Brushes.Green
                                Button.foreground Brushes.White
                                Button.isEnabled isValid.Current
                                Button.width 120.0
                            ]
                            Button.create [
                                Button.content "Clear"
                                Button.onClick (fun _ -> 
                                    ValidationUIHelpers.clearValidationForm formState
                                    stopCamera()
                                )
                            ]
                        ]
                    ]

                    if not (String.IsNullOrWhiteSpace(validationResult.Current)) then
                        Border.create [
                            Border.padding 15.0
                            Border.cornerRadius 5.0
                            Border.background (if validationResult.Current.Contains("REDEMMED") || validationResult.Current.Contains("VALID") then Brushes.LightGreen else Brushes.LightSalmon)
                            Border.child (
                                TextBlock.create [
                                    TextBlock.text validationResult.Current
                                    TextBlock.fontSize 16.0
                                    TextBlock.textWrapping TextWrapping.Wrap
                                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                                ]
                            )
                        ]

                    renderTicketDetailsSection ()
                ]
            ]
        )