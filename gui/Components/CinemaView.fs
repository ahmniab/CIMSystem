namespace CIMSystemGUI.Components

open System
open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Models
open CIMSystemGUI.Services
open CIMSystemGUI.Helpers

module CinemaView =

    let view (initialHall: CinemaHall) (onBack: unit -> unit) =
        Component.create (
            "CinemaView",
            fun ctx ->
                let cinema = ctx.useState initialHall
                let ticketHtml = ctx.useState (None: string option)
                let currentTicket = ctx.useState (None: CIMSystemGUI.Models.TicketInfo option)

                let uiState =
                    { UIHelpers.SelectedSeat = ctx.useState (None: (int * int) option)
                      UIHelpers.CustomerName = ctx.useState ""
                      UIHelpers.StatusMessage = ctx.useState $"Viewing {initialHall.MovieTitle} - Click a seat" }

                let reloadCurrentHall () =
                    match CinemaService.getHallById cinema.Current.Id with
                    | Some updatedHall -> cinema.Set updatedHall
                    | None -> ()

                let handleSuccessfulBooking (msg: string) (ticketInfo: CIMSystemGUI.Models.TicketInfo) =
                    let successMessage = TicketHelpers.handleTicketGeneration msg ticketInfo
                    UIHelpers.updateStatusMessage uiState successMessage

                    currentTicket.Set(Some ticketInfo)

                    ticketHtml.Set(Some "ShowTicketOverlay")

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
                        UIHelpers.updateStatusMessage uiState (BookingHelpers.getSeatStatusMessage seat row col)

                let onCancelBooking () =
                    match uiState.SelectedSeat.Current with
                    | Some(row, col) ->
                        match CinemaService.clearBooking cinema.Current row col with
                        | Result.Ok msg ->
                            UIHelpers.updateStatusMessage uiState $"✅ {msg}"
                            UIHelpers.clearBookingForm uiState
                            reloadCurrentHall ()
                        | Result.Error err -> UIHelpers.updateStatusMessage uiState $"❌ Error: {err}"
                    | None -> ()

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

                let getSelectedSeatStatus () =
                    match uiState.SelectedSeat.Current with
                    | Some(r, c) ->
                        if r <= cinema.Current.Height && c <= cinema.Current.Width then
                            Some(cinema.Current.Seats.[r - 1, c - 1].Status)
                        else
                            None
                    | None -> None

                DockPanel.create
                    [ DockPanel.children
                          [

                            Border.create
                                [ Border.dock Dock.Top
                                  Border.background (SolidColorBrush(Color.Parse("#2E3440")))
                                  Border.padding 10.0
                                  Border.child (
                                      StackPanel.create
                                          [ StackPanel.orientation Orientation.Horizontal
                                            StackPanel.spacing 15.0
                                            StackPanel.children
                                                [ Button.create
                                                      [ Button.content "← Back"
                                                        Button.onClick (fun _ -> onBack ())
                                                        Button.background Brushes.Transparent
                                                        Button.foreground Brushes.White ]
                                                  TextBlock.create
                                                      [ TextBlock.text $"Now Playing: {cinema.Current.MovieTitle}"
                                                        TextBlock.foreground Brushes.White
                                                        TextBlock.fontWeight FontWeight.Bold
                                                        TextBlock.verticalAlignment VerticalAlignment.Center ] ] ]
                                  ) ]

                            match ticketHtml.Current with
                            | Some _ ->
                                Border.create
                                    [
                                      Border.background (
                                          try
                                              let bitmap = new Avalonia.Media.Imaging.Bitmap("Backgrounds/mainView.jpg")
                                              let brush = ImageBrush(bitmap)
                                              brush.Stretch <- Stretch.UniformToFill
                                              brush :> IBrush
                                          with _ ->
                                              Brushes.DarkBlue
                                      )
                                      Border.margin 20.0
                                      Border.cornerRadius 10.0
                                      Border.borderBrush Brushes.Green
                                      Border.borderThickness 2.0

                                      Border.child (
                                          Border.create
                                              [ Border.background (SolidColorBrush(Color.Parse("#CCFFFFFF")))
                                                Border.cornerRadius 8.0
                                                Border.padding 20.0
                                                Border.horizontalAlignment HorizontalAlignment.Center
                                                Border.verticalAlignment VerticalAlignment.Center

                                                Border.child (
                                                    StackPanel.create
                                                        [ StackPanel.spacing 20.0
                                                          StackPanel.children
                                                              [ TextBlock.create
                                                                    [ TextBlock.text "✅ Booking Successful!"
                                                                      TextBlock.fontSize 24.0
                                                                      TextBlock.foreground Brushes.Green
                                                                      TextBlock.fontWeight FontWeight.Bold
                                                                      TextBlock.textAlignment TextAlignment.Center ]

                                                                TextBlock.create
                                                                    [ TextBlock.text "Your ticket has been generated."
                                                                      TextBlock.textAlignment TextAlignment.Center
                                                                      TextBlock.foreground Brushes.Black ]

                                                                Button.create
                                                                    [ Button.content "🖨 Print Ticket (Open in Browser)"
                                                                      Button.padding (20.0, 10.0)
                                                                      Button.background Brushes.Blue
                                                                      Button.foreground Brushes.White
                                                                      Button.horizontalAlignment
                                                                          HorizontalAlignment.Center
                                                                      Button.onClick (fun _ ->
                                                                          match currentTicket.Current with
                                                                          | Some info ->
                                                                              match
                                                                                  HtmlTicketGenerator.saveAndPrintTicket
                                                                                      info
                                                                              with
                                                                              | Result.Ok msg ->
                                                                                  printfn "Print Success: %s" msg
                                                                              | Result.Error err ->
                                                                                  printfn "Print Error: %s" err
                                                                          | None -> ()) ]

                                                                Button.create
                                                                    [ Button.content "Close & Continue"
                                                                      Button.padding (20.0, 10.0)
                                                                      Button.background Brushes.Gray
                                                                      Button.foreground Brushes.White
                                                                      Button.horizontalAlignment
                                                                          HorizontalAlignment.Center
                                                                      Button.onClick (fun _ ->
                                                                          ticketHtml.Set None
                                                                          currentTicket.Set None) ] ] ]
                                                ) ]
                                      ) ]

                            | None ->
                                DockPanel.create
                                    [ DockPanel.children
                                          [
                                            Border.create
                                                [ Border.dock Dock.Bottom
                                                  Border.background Brushes.LightGray
                                                  Border.padding (10.0, 5.0)
                                                  Border.child (
                                                      TextBlock.create
                                                          [ TextBlock.text uiState.StatusMessage.Current
                                                            TextBlock.fontSize 12.0 ]
                                                  ) ]

                                            StackPanel.create
                                                [ StackPanel.dock Dock.Right
                                                  StackPanel.width 200.0
                                                  StackPanel.margin (20.0, 20.0, 20.0, 0.0)
                                                  StackPanel.spacing 10.0
                                                  StackPanel.children
                                                      [ (match uiState.SelectedSeat.Current with
                                                         | Some(row, col) ->
                                                             TextBlock.create
                                                                 [ TextBlock.text $"Selected: R{row} S{col}"
                                                                   TextBlock.foreground Brushes.Blue ]
                                                         | None ->
                                                             TextBlock.create
                                                                 [ TextBlock.text "No seat selected"
                                                                   TextBlock.foreground Brushes.Gray ])

                                                        (match getSelectedSeatStatus () with
                                                         | Some SeatStatus.Available ->
                                                             StackPanel.create
                                                                 [ StackPanel.spacing 10.0
                                                                   StackPanel.children
                                                                       [ TextBlock.create [ TextBlock.text "Name:" ]
                                                                         TextBox.create
                                                                             [ TextBox.text uiState.CustomerName.Current
                                                                               TextBox.onTextChanged
                                                                                   uiState.CustomerName.Set
                                                                               TextBox.watermark "Enter name" ]
                                                                         Button.create
                                                                             [ Button.content "Book Seat"
                                                                               Button.background Brushes.Green
                                                                               Button.foreground Brushes.White
                                                                               Button.isEnabled (
                                                                                   not (
                                                                                       String.IsNullOrWhiteSpace(
                                                                                           uiState.CustomerName.Current
                                                                                       )
                                                                                   )
                                                                               )
                                                                               Button.onClick (fun _ -> onBookSeat ()) ] ] ]
                                                             :> Types.IView

                                                         | Some SeatStatus.Booked ->
                                                             Button.create
                                                                 [ Button.content "❌ Cancel Booking"
                                                                   Button.background Brushes.Red
                                                                   Button.foreground Brushes.White
                                                                   Button.onClick (fun _ -> onCancelBooking ()) ]
                                                             :> Types.IView

                                                         | _ ->
                                                             TextBlock.create [ TextBlock.text "Select a seat" ]
                                                             :> Types.IView)


                                                        TextBlock.create
                                                            [ TextBlock.text "Statistics:"
                                                              TextBlock.fontWeight FontWeight.Bold
                                                              TextBlock.margin (0.0, 10.0, 0.0, 0.0) ]
                                                        TextBlock.create
                                                            [ TextBlock.text
                                                                  $"Available: {CinemaService.getAvailableSeatsCount cinema.Current}" ] ] ]

                                            ScrollViewer.create
                                                [ ScrollViewer.padding 20.0
                                                  ScrollViewer.content (
                                                      SeatGridView.view
                                                          { SeatGridView.Cinema = cinema.Current
                                                            SeatGridView.SelectedSeat = uiState.SelectedSeat.Current
                                                            SeatGridView.OnSeatClick = onSeatClick }
                                                      :> Types.IView
                                                  ) ] ] ] ] ]
        )
