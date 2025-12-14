namespace CIMSystemGUI.Components

open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Services
open CIMSystemGUI.Services.AutomationService
open Avalonia.FuncUI
open Avalonia.FuncUI.Types

module AutomationTestingView =

    type ViewState = {
        IsSequenceRunning: bool
        DbCheck: ProcessResult
        CreateHall: ProcessResult
        CreateMovie: ProcessResult
        Schedule: ProcessResult
        Booking: ProcessResult
        Validation: ProcessResult
        ExpandedSteps: Set<string> 
    }

    let initialResult name = emptyResult name

    let view () =
        Component.create("AutomationTestingView", fun ctx ->
            let state = ctx.useState {
                IsSequenceRunning = false
                DbCheck = initialResult "1. Files Exists Check"
                CreateHall = initialResult "2. Create Hall"
                CreateMovie = initialResult "3. Create Movie"
                Schedule = initialResult "4. Schedule Session"
                Booking = initialResult "5. Book Seat"
                Validation = initialResult "6. Validate Ticket"
                ExpandedSteps = Set.empty
            }

            let updateStepResult (updater: ProcessResult -> ViewState -> ViewState) result =
                let s = state.Current
                let s1 = updater result s
                let finalState = 
                    if result.Status = Passed || (match result.Status with Failed _ -> true | _ -> false) then
                        { s1 with ExpandedSteps = s1.ExpandedSteps.Add(result.StepName) }
                    else s1
                state.Set finalState

            let runFullSequence () =
                state.Set { state.Current with 
                                IsSequenceRunning = true
                                ExpandedSteps = Set.empty
                                DbCheck = initialResult "1. Files Exists Check"
                                CreateHall = initialResult "2. Create Hall"
                                CreateMovie = initialResult "3. Create Movie"
                                Schedule = initialResult "4. Schedule Session"
                                Booking = initialResult "5. Book Seat"
                                Validation = initialResult "6. Validate Ticket" }

                async {
                   let runStepTask (task: unit -> Async<ProcessResult>) (updater: ProcessResult -> ViewState -> ViewState) previousSuccess =
                       async {
                           if not previousSuccess then return false
                           else
                               let currentRes = emptyResult "" 
                               let runningRes = { currentRes with Status = Running }
                               
                               let runningState = updater runningRes state.Current
                               state.Set runningState
                               
                               do! Async.Sleep 300

                               let! result = task()
                               
                               updateStepResult updater result
                               do! Async.Sleep 500
                               return result.Status = Passed
                       }

                   let! s1 = runStepTask AutomationService.testDatabase (fun r s -> { s with DbCheck = if r.Status = Running then { s.DbCheck with Status = Running } else r }) true
                   let! s2 = runStepTask AutomationService.createHall (fun r s -> { s with CreateHall = if r.Status = Running then { s.CreateHall with Status = Running } else r }) s1
                   let! s3 = runStepTask AutomationService.createMovie (fun r s -> { s with CreateMovie = if r.Status = Running then { s.CreateMovie with Status = Running } else r }) s2
                   let! s4 = runStepTask AutomationService.scheduleSession (fun r s -> { s with Schedule = if r.Status = Running then { s.Schedule with Status = Running } else r }) s3
                   let! s5 = runStepTask AutomationService.bookSeat (fun r s -> { s with Booking = if r.Status = Running then { s.Booking with Status = Running } else r }) s4
                   let! _  = runStepTask AutomationService.validateTicket (fun r s -> { s with Validation = if r.Status = Running then { s.Validation with Status = Running } else r }) s5

                   state.Set { state.Current with IsSequenceRunning = false }

                } |> Async.StartImmediate

            
            let renderDataTable title (data: Map<string, string>) (headerColor: string) (bgColor: string) =
                Border.create [
                    Border.background (SolidColorBrush(Color.Parse bgColor))
                    Border.cornerRadius 4.0
                    Border.padding 8.0
                    Border.child (
                        StackPanel.create [
                            StackPanel.children [
                                TextBlock.create [
                                    TextBlock.text title
                                    TextBlock.fontWeight FontWeight.Bold
                                    TextBlock.foreground (SolidColorBrush(Color.Parse headerColor))
                                    TextBlock.fontSize 10.0
                                    TextBlock.margin (0.0, 0.0, 0.0, 5.0)
                                ]
                                if data.IsEmpty then
                                    TextBlock.create [ TextBlock.text "No Data"; TextBlock.fontSize 10.0; TextBlock.foreground Brushes.Gray ]
                                else
                                    StackPanel.create [
                                        StackPanel.spacing 3.0
                                        StackPanel.children [
                                            for kvp in data do
                                                DockPanel.create [
                                                    DockPanel.children [
                                                        TextBlock.create [ DockPanel.dock Dock.Left; TextBlock.text $"{kvp.Key}:"; TextBlock.fontWeight FontWeight.SemiBold; TextBlock.fontSize 10.0; TextBlock.width 70.0 ]
                                                        TextBlock.create [ TextBlock.text kvp.Value; TextBlock.fontSize 10.0; TextBlock.textWrapping TextWrapping.Wrap ]
                                                    ]
                                                ]
                                        ]
                                    ]
                            ]
                        ]
                    )
                ]

            let renderConnector (previousResult: ProcessResult) =
                let color = if previousResult.Status = Passed then Brushes.Green else Brushes.Red
                StackPanel.create [
                    StackPanel.horizontalAlignment HorizontalAlignment.Center
                    StackPanel.children [
                        Border.create [ 
                            Border.width 2.0; Border.height 15.0; Border.background color; Border.horizontalAlignment HorizontalAlignment.Center 
                        ]
                        TextBlock.create [ 
                            TextBlock.text "▼"; TextBlock.fontSize 10.0; TextBlock.foreground color; TextBlock.horizontalAlignment HorizontalAlignment.Center; TextBlock.margin(0.0, -3.0, 0.0, 0.0)
                        ]
                    ]
                ]

            let renderProcessNode (result: ProcessResult) =
                let borderColor, iconText, bgColor = 
                    match result.Status with
                    | Idle -> Brushes.LightGray, "⏳", "#FFFFFF"
                    | Running -> Brushes.Orange, "⚙️", "#FFF8E1"
                    | Passed -> Brushes.Green, "✅", "#F0FFF4"
                    | Failed _ -> Brushes.Red, "❌", "#FFF5F5"

                StackPanel.create [
                   StackPanel.margin (20.0, 0.0)
                   StackPanel.children [
                       Border.create [
                           Border.cornerRadius 8.0
                           Border.borderThickness 2.0
                           Border.borderBrush borderColor
                           Border.background Brushes.Black
                           Border.padding 10.0
                           Border.child (
                               DockPanel.create [
                                   DockPanel.children [
                                       TextBlock.create [ 
                                           DockPanel.dock Dock.Left; TextBlock.text iconText; TextBlock.fontSize 18.0; TextBlock.margin(0.0,0.0,10.0,0.0); TextBlock.verticalAlignment VerticalAlignment.Center
                                       ]
                                       TextBlock.create [ 
                                           DockPanel.dock Dock.Left; TextBlock.text result.StepName; TextBlock.fontWeight FontWeight.Bold; TextBlock.fontSize 14.0; TextBlock.verticalAlignment VerticalAlignment.Center
                                       ]
                                       TextBlock.create [ 
                                           DockPanel.dock Dock.Right; TextBlock.verticalAlignment VerticalAlignment.Center; TextBlock.fontSize 11.0; TextBlock.fontWeight FontWeight.SemiBold
                                           TextBlock.foreground borderColor
                                           TextBlock.text (match result.Status with Idle->"Waiting" | Running->"Processing..." | Passed->"COMPLETED" | Failed _->"FAILED")
                                       ]
                                   ]
                               ]
                           )
                       ]

                       Expander.create [
                           Expander.horizontalAlignment HorizontalAlignment.Stretch
                           Expander.isExpanded (state.Current.ExpandedSteps.Contains(result.StepName))
                           Expander.isEnabled false 
                           Expander.header (
                               TextBlock.create [ TextBlock.text "Process Details (Inputs → Outputs)"; TextBlock.fontSize 10.0; TextBlock.foreground Brushes.White; ]
                           )
                           Expander.content (
                               StackPanel.create [
                                   StackPanel.spacing 10.0
                                   StackPanel.children [
                                        match result.Status with
                                        | Failed msg -> TextBlock.create [ TextBlock.text $"Error: {msg}"; TextBlock.foreground Brushes.Red; TextBlock.fontWeight FontWeight.Bold; TextBlock.padding 5.0 ]
                                        | _ -> ()

                                        Grid.create [
                                            Grid.columnDefinitions "4*, 1*, 4*" 
                                            Grid.children [
                                                Border.create [ Grid.column 0; Border.child (renderDataTable "⬇️ INPUTS" result.Details.Inputs "#000000ff" "#000000ff") ]
                                                
                                                TextBlock.create [ Grid.column 1; TextBlock.text "→"; TextBlock.fontSize 24.0; TextBlock.foreground Brushes.Gray; TextBlock.horizontalAlignment HorizontalAlignment.Center; TextBlock.verticalAlignment VerticalAlignment.Center ]

                                                Border.create [ Grid.column 2; Border.child (renderDataTable "⬆️ OUTPUTS" result.Details.Outputs "#276749" "#000000ff") ]
                                            ]
                                        ]
                                        
                                        if not result.Details.Logs.IsEmpty then
                                             Border.create [
                                                 Border.background (SolidColorBrush(Color.Parse "#000000ff")); Border.padding 5.0; Border.cornerRadius 4.0
                                                 Border.child (TextBlock.create [ TextBlock.text (String.concat " | " result.Details.Logs); TextBlock.fontSize 9.0; TextBlock.fontStyle FontStyle.Italic; TextBlock.foreground Brushes.Gray ])
                                             ]
                                   ]
                               ]
                           )
                       ]
                   ]
                ]

            DockPanel.create [
                DockPanel.children [
                    StackPanel.create [
                        DockPanel.dock Dock.Top
                        StackPanel.margin (20.0, 20.0, 20.0, 10.0)
                        StackPanel.children [
                            DockPanel.create [
                                DockPanel.children [
                                    StackPanel.create [
                                        DockPanel.dock Dock.Left
                                        StackPanel.children [
                                            TextBlock.create [ TextBlock.text "🔄 System Automation Pipeline"; TextBlock.fontSize 22.0; TextBlock.fontWeight FontWeight.Bold; TextBlock.foreground Brushes.White ]
                                            TextBlock.create [ TextBlock.text "Visualizes data flow between system components sequentially."; TextBlock.fontSize 11.0; TextBlock.foreground Brushes.White ]
                                        ]
                                    ]
                                    Button.create [
                                        DockPanel.dock Dock.Right
                                        Button.content (if state.Current.IsSequenceRunning then "Executing Pipeline..." else "▶ Run Full Sequence")
                                        Button.isEnabled (not state.Current.IsSequenceRunning)
                                        Button.padding (20.0, 10.0)
                                        Button.fontSize 14.0
                                        Button.fontWeight FontWeight.Bold
                                        Button.background (if state.Current.IsSequenceRunning then Brushes.Black else Brushes.DarkBlue)
                                        Button.foreground Brushes.White
                                        Button.cornerRadius 25.0
                                        Button.onClick (fun _ -> runFullSequence())
                                    ]
                                ]
                            ]
                            Border.create [ Border.height 2.0; Border.background Brushes.Black; Border.margin(0.0, 15.0, 0.0, 0.0); Border.cornerRadius 1.0 ]
                        ]
                    ]

                    ScrollViewer.create [
                        DockPanel.dock Dock.Bottom
                        ScrollViewer.content (
                            StackPanel.create [
                                StackPanel.margin (0.0, 10.0, 0.0, 30.0)
                                StackPanel.maxWidth 800.0
                                StackPanel.horizontalAlignment HorizontalAlignment.Center
                                StackPanel.children [
                                    renderProcessNode state.Current.DbCheck
                                    renderConnector state.Current.DbCheck
                                    
                                    renderProcessNode state.Current.CreateHall
                                    renderConnector state.Current.CreateHall

                                    renderProcessNode state.Current.CreateMovie
                                    renderConnector state.Current.CreateMovie

                                    renderProcessNode state.Current.Schedule
                                    renderConnector state.Current.Schedule

                                    renderProcessNode state.Current.Booking
                                    renderConnector state.Current.Booking

                                    renderProcessNode state.Current.Validation
                                    
                                    TextBlock.create [
                                        TextBlock.text "--- End of Pipeline ---"
                                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                                        TextBlock.fontSize 10.0
                                        TextBlock.foreground Brushes.Gray
                                        TextBlock.margin (0.0, 20.0)
                                    ]
                                ]
                            ]
                        )
                    ]
                ]
            ]
        ) :> IView