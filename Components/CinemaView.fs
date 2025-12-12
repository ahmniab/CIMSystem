namespace CEMSystem.Components

open System
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CEMSystem.Models
open CEMSystem.Services
open CEMSystem.Helpers

module CinemaView =

    let view () =
        Component(fun ctx ->
            let cinema =
                ctx.useState (
                    match CinemaService.loadCinemaData () with
                    | Result.Ok c -> c
                    | Result.Error _ -> CinemaService.createCinemaHall 20 11
                )

            // Create UI state helper
            let uiState =
                { UIHelpers.SelectedSeat = ctx.useState (None: (int * int) option)
                  UIHelpers.CustomerName = ctx.useState ""
                  UIHelpers.StatusMessage = ctx.useState "Cinema loaded - Click a seat to select it" }

            let handleSuccessfulBooking (msg: string) (ticketInfo: TicketInfo) =
                let successMessage = TicketHelpers.handleTicketGeneration msg ticketInfo
                UIHelpers.updateStatusMessage uiState successMessage
                UIHelpers.clearBookingForm uiState

                // Reload cinema data
                match BookingHelpers.reloadCinemaData () with
                | Some c -> cinema.Set c
                | None -> ()

            let handleSimpleSuccess (msg: string) =
                UIHelpers.updateStatusMessage uiState msg
                UIHelpers.clearBookingForm uiState

                // Reload cinema data
                match BookingHelpers.reloadCinemaData () with
                | Some c -> cinema.Set c
                | None -> ()

            let onSeatClick row col =
                uiState.SelectedSeat.Set(Some(row, col))
                let seat = cinema.Current.Seats.[row - 1, col - 1]
                let seatMessage = BookingHelpers.getSeatStatusMessage seat row col
                UIHelpers.updateStatusMessage uiState seatMessage

            let onBookSeat () =
                match
                    BookingHelpers.validateBookingInput uiState.SelectedSeat.Current uiState.CustomerName.Current
                with
                | Result.Ok request ->
                    match CinemaService.bookSeat cinema.Current request with
                    | SuccessWithTicket(msg, ticketInfo) -> handleSuccessfulBooking msg ticketInfo
                    | Success msg -> handleSimpleSuccess msg
                    | SeatAlreadyBooked -> UIHelpers.updateStatusMessage uiState "Seat is already booked"
                    | InvalidSeat -> UIHelpers.updateStatusMessage uiState "Invalid seat selection"
                    | Error msg -> UIHelpers.updateStatusMessage uiState $"Error: {msg}"
                | Result.Error errorMsg -> UIHelpers.updateStatusMessage uiState errorMsg

            DockPanel.create
                [ DockPanel.children
                      [
                        // Status bar at bottom
                        Border.create
                            [ Border.dock Dock.Bottom
                              Border.background Brushes.LightGray
                              Border.padding (10.0, 5.0)
                              Border.child (
                                  TextBlock.create
                                      [ TextBlock.text uiState.StatusMessage.Current
                                        TextBlock.fontSize 12.0
                                        TextBlock.foreground Brushes.Black ]
                              ) ]

                        // Control panel on the right
                        StackPanel.create
                            [ StackPanel.dock Dock.Right
                              StackPanel.orientation Orientation.Vertical
                              StackPanel.spacing 10.0
                              StackPanel.margin (20.0, 20.0, 20.0, 0.0)
                              StackPanel.width 200.0
                              StackPanel.children
                                  [ TextBlock.create
                                        [ TextBlock.text "Customer Name:"
                                          TextBlock.fontSize 12.0
                                          TextBlock.fontWeight FontWeight.Bold ]
                                    TextBox.create
                                        [ TextBox.text uiState.CustomerName.Current
                                          TextBox.watermark "Enter customer name"
                                          TextBox.onTextChanged uiState.CustomerName.Set ]

                                    // Selected seat info
                                    (match uiState.SelectedSeat.Current with
                                     | Some(row, col) ->
                                         TextBlock.create
                                             [ TextBlock.text $"Selected: Row {row}, Seat {col}"
                                               TextBlock.fontSize 12.0
                                               TextBlock.foreground Brushes.Blue
                                               TextBlock.fontWeight FontWeight.Bold ]
                                     | None ->
                                         TextBlock.create
                                             [ TextBlock.text "No seat selected"
                                               TextBlock.fontSize 12.0
                                               TextBlock.foreground Brushes.Gray ])

                                    Button.create
                                        [ Button.content "Book Seat"
                                          Button.background Brushes.Green
                                          Button.foreground Brushes.White
                                          Button.isEnabled (
                                              uiState.SelectedSeat.Current.IsSome
                                              && not (String.IsNullOrWhiteSpace(uiState.CustomerName.Current))
                                          )
                                          Button.onClick (fun _ -> onBookSeat ())
                                          Button.margin (0.0, 10.0, 0.0, 0.0) ]

                                    Button.create
                                        [ Button.content "Clear Selection"
                                          Button.background Brushes.Gray
                                          Button.foreground Brushes.White
                                          Button.isEnabled uiState.SelectedSeat.Current.IsSome
                                          Button.onClick (fun _ -> UIHelpers.clearSelectionWithMessage uiState) ]

                                    // Statistics
                                    StackPanel.create
                                        [ StackPanel.orientation Orientation.Vertical
                                          StackPanel.spacing 5.0
                                          StackPanel.margin (0.0, 20.0, 0.0, 0.0)
                                          StackPanel.children
                                              [ TextBlock.create
                                                    [ TextBlock.text "Cinema Statistics:"
                                                      TextBlock.fontSize 12.0
                                                      TextBlock.fontWeight FontWeight.Bold ]
                                                TextBlock.create
                                                    [ TextBlock.text
                                                          $"Available: {CinemaService.getAvailableSeatsCount cinema.Current}"
                                                      TextBlock.fontSize 10.0 ]
                                                TextBlock.create
                                                    [ TextBlock.text
                                                          $"Total: {CinemaService.getTotalSeatsCount cinema.Current}"
                                                      TextBlock.fontSize 10.0 ] ] ]

                                    ] ]

                        // Main cinema seating area
                        ScrollViewer.create
                            [ ScrollViewer.padding 20.0
                              ScrollViewer.content (
                                  SeatGridView.view
                                      { SeatGridView.Cinema = cinema.Current
                                        SeatGridView.SelectedSeat = uiState.SelectedSeat.Current
                                        SeatGridView.OnSeatClick = onSeatClick }
                                  :> Types.IView
                              ) ] ] ])
