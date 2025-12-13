namespace CIMSystemGUI.Components

open System
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Models
open CIMSystemGUI.Services
open CIMSystemGUI.Helpers

module CinemaView =

    let view (initialHall: CinemaHall) (onBack: unit -> unit) =
        Component(fun ctx ->
            // Initialize state with the passed hall
            let cinema = ctx.useState initialHall

            // Create UI state helper
            let uiState =
                { UIHelpers.SelectedSeat = ctx.useState (None: (int * int) option)
                  UIHelpers.CustomerName = ctx.useState ""
                  UIHelpers.StatusMessage = ctx.useState $"Viewing {initialHall.MovieTitle} - Click a seat" }

            // Helper to reload data for THIS specific hall
            let reloadCurrentHall () =
                match CinemaService.getHallById cinema.Current.Id with
                | Some updatedHall -> cinema.Set updatedHall
                | None -> ()

            let handleSuccessfulBooking (msg: string) (ticketInfo: TicketInfo) =
                let successMessage = TicketHelpers.handleTicketGeneration msg ticketInfo
                UIHelpers.updateStatusMessage uiState successMessage
                UIHelpers.clearBookingForm uiState
                reloadCurrentHall ()

            let handleSimpleSuccess (msg: string) =
                UIHelpers.updateStatusMessage uiState msg
                UIHelpers.clearBookingForm uiState
                reloadCurrentHall ()

            let onSeatClick row col =
                uiState.SelectedSeat.Set(Some(row, col))
                if row <= cinema.Current.Height && col <= cinema.Current.Width then
                    let seat = cinema.Current.Seats.[row - 1, col - 1]
                    let seatMessage = BookingHelpers.getSeatStatusMessage seat row col
                    UIHelpers.updateStatusMessage uiState seatMessage

            // --- 1. NEW FUNCTION: Handle Cancel Booking ---
            let onCancelBooking () =
                match uiState.SelectedSeat.Current with
                | Some(row, col) ->
                    // Call the service to remove the booking
                    match CinemaService.clearBooking cinema.Current row col with
                    | Result.Ok msg -> 
                        UIHelpers.updateStatusMessage uiState $"✅ {msg}"
                        UIHelpers.clearBookingForm uiState // Clear selection
                        reloadCurrentHall() // Refresh the view (turn seat green)
                    | Result.Error err -> 
                        UIHelpers.updateStatusMessage uiState $"❌ Error: {err}"
                | None -> ()

            let onBookSeat () =
                match BookingHelpers.validateBookingInput uiState.SelectedSeat.Current uiState.CustomerName.Current with
                | Result.Ok request ->
                    match CinemaService.bookSeat cinema.Current request with
                    | Success(msg, ticketInfo) -> handleSuccessfulBooking msg ticketInfo
                    | SeatAlreadyBooked -> UIHelpers.updateStatusMessage uiState "Seat is already booked"
                    | InvalidSeat -> UIHelpers.updateStatusMessage uiState "Invalid seat selection"
                    | Error msg -> UIHelpers.updateStatusMessage uiState $"Error: {msg}"
                | Result.Error errorMsg -> UIHelpers.updateStatusMessage uiState errorMsg

            // Helper to get current seat status safely
            let getSelectedSeatStatus () =
                match uiState.SelectedSeat.Current with
                | Some(r, c) -> 
                    if r <= cinema.Current.Height && c <= cinema.Current.Width then
                        Some (cinema.Current.Seats.[r-1, c-1].Status)
                    else None
                | None -> None

            DockPanel.create [
                DockPanel.children [
                    // 1. Navigation Bar (Top)
                    Border.create [
                        Border.dock Dock.Top
                        Border.background (SolidColorBrush(Color.Parse("#2E3440")))
                        Border.padding 10.0
                        Border.child (
                            StackPanel.create [
                                StackPanel.orientation Orientation.Horizontal
                                StackPanel.spacing 15.0
                                StackPanel.children [
                                    Button.create [
                                        Button.content "← Back to Movies"
                                        Button.background Brushes.Transparent
                                        Button.foreground Brushes.White
                                        Button.onClick (fun _ -> onBack())
                                    ]
                                    
                                    TextBlock.create [
                                        TextBlock.text $"Now Playing: {cinema.Current.MovieTitle}"
                                        TextBlock.foreground Brushes.White
                                        TextBlock.fontSize 18.0
                                        TextBlock.fontWeight FontWeight.Bold
                                        TextBlock.verticalAlignment VerticalAlignment.Center
                                        TextBlock.margin (20.0, 0.0, 0.0, 0.0)
                                    ]
                                ]
                            ]
                        )
                    ]

                    // 2. Status Bar (Bottom)
                    Border.create [
                        Border.dock Dock.Bottom
                        Border.background Brushes.LightGray
                        Border.padding (10.0, 5.0)
                        Border.child (
                            TextBlock.create [
                                TextBlock.text uiState.StatusMessage.Current
                                TextBlock.fontSize 12.0
                                TextBlock.foreground Brushes.Black
                            ]
                        )
                    ]

                    // 3. Right Control Panel
                    StackPanel.create [
                        StackPanel.dock Dock.Right
                        StackPanel.orientation Orientation.Vertical
                        StackPanel.spacing 10.0
                        StackPanel.margin (20.0, 20.0, 20.0, 0.0)
                        StackPanel.width 200.0
                        StackPanel.children [
                            
                            // Selected Seat Info
                            (match uiState.SelectedSeat.Current with
                             | Some(row, col) ->
                                 TextBlock.create [
                                     TextBlock.text $"Selected: Row {row}, Seat {col}"
                                     TextBlock.fontSize 14.0
                                     TextBlock.foreground Brushes.Blue
                                     TextBlock.fontWeight FontWeight.Bold
                                     TextBlock.margin (0.0, 0.0, 0.0, 10.0)
                                 ]
                             | None ->
                                 TextBlock.create [
                                     TextBlock.text "No seat selected"
                                     TextBlock.fontSize 12.0
                                     TextBlock.foreground Brushes.Gray
                                 ])

                            // --- 2. DYNAMIC BUTTONS ---
                            // Check status of selected seat to decide which button to show
                            match getSelectedSeatStatus() with
                            | Some SeatStatus.Available ->
                                StackPanel.create [
                                    StackPanel.spacing 10.0
                                    StackPanel.children [
                                        TextBlock.create [
                                            TextBlock.text "Customer Name:"
                                            TextBlock.fontSize 12.0
                                            TextBlock.fontWeight FontWeight.Bold
                                        ]
                                        TextBox.create [
                                            TextBox.text uiState.CustomerName.Current
                                            TextBox.watermark "Enter name"
                                            TextBox.onTextChanged uiState.CustomerName.Set
                                        ]
                                        Button.create [
                                            Button.content "Book Seat"
                                            Button.background Brushes.Green
                                            Button.foreground Brushes.White
                                            Button.horizontalAlignment HorizontalAlignment.Stretch
                                            Button.horizontalContentAlignment HorizontalAlignment.Center
                                            Button.isEnabled (not (String.IsNullOrWhiteSpace(uiState.CustomerName.Current)))
                                            Button.onClick (fun _ -> onBookSeat ())
                                        ]
                                    ]
                                ]
                            
                            | Some SeatStatus.Booked ->
                                // If booked, show Cancel button
                                Button.create [
                                    Button.content "❌ Cancel Booking"
                                    Button.background Brushes.Red
                                    Button.foreground Brushes.White
                                    Button.horizontalAlignment HorizontalAlignment.Stretch
                                    Button.horizontalContentAlignment HorizontalAlignment.Center
                                    Button.onClick (fun _ -> onCancelBooking())
                                ]
                            
                            | _ -> 
                                // No seat selected
                                TextBlock.create [ TextBlock.text "Select a seat to see actions" ]

                            // Clear Selection Button (Always visible if something is selected)
                            Button.create [
                                Button.content "Clear Selection"
                                Button.background Brushes.Gray
                                Button.foreground Brushes.White
                                Button.horizontalAlignment HorizontalAlignment.Stretch
                                Button.isEnabled uiState.SelectedSeat.Current.IsSome
                                Button.onClick (fun _ -> UIHelpers.clearSelectionWithMessage uiState)
                                Button.margin (0.0, 20.0, 0.0, 0.0)
                            ]

                            // Statistics Section
                            StackPanel.create [
                                StackPanel.orientation Orientation.Vertical
                                StackPanel.spacing 5.0
                                StackPanel.margin (0.0, 20.0, 0.0, 0.0)
                                StackPanel.children [
                                    TextBlock.create [
                                        TextBlock.text "Cinema Statistics:"
                                        TextBlock.fontSize 12.0
                                        TextBlock.fontWeight FontWeight.Bold
                                    ]
                                    TextBlock.create [
                                        TextBlock.text $"Available: {CinemaService.getAvailableSeatsCount cinema.Current}"
                                        TextBlock.fontSize 10.0
                                    ]
                                    TextBlock.create [
                                        TextBlock.text $"Total: {CinemaService.getTotalSeatsCount cinema.Current}"
                                        TextBlock.fontSize 10.0
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // 4. Main Seat Grid (Center)
                    ScrollViewer.create [
                        ScrollViewer.padding 20.0
                        ScrollViewer.content (
                            SeatGridView.view
                                { SeatGridView.Cinema = cinema.Current
                                  SeatGridView.SelectedSeat = uiState.SelectedSeat.Current
                                  SeatGridView.OnSeatClick = onSeatClick }
                            :> Types.IView
                        )
                    ]
                ]
            ]
        )