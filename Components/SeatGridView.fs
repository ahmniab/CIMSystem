namespace CEMSystem.Components

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CEMSystem.Models

module SeatGridView =

    type SeatGridProps =
        { Cinema: CinemaHall
          SelectedSeat: (int * int) option
          OnSeatClick: int -> int -> unit }

    let seatButton row col seat isSelected onSeatClick =
        let (bgColor, fgColor) =
            match seat.Status with
            | SeatStatus.Available ->
                if isSelected then
                    (Brushes.LightBlue, Brushes.Black)
                else
                    (Brushes.LightGreen, Brushes.Black)
            | SeatStatus.Booked -> (Brushes.Red, Brushes.White)
            | _ -> (Brushes.Gray, Brushes.White)

        Button.create
            [ Button.content $"{row}-{col}"
              Button.width 35.0
              Button.height 25.0
              Button.margin (1.0, 1.0)
              Button.background bgColor
              Button.foreground fgColor
              Button.fontSize 8.0
              Button.onClick (fun _ -> onSeatClick row col) ]

    let view (props: SeatGridProps) =
        StackPanel.create
            [ StackPanel.orientation Orientation.Vertical
              StackPanel.spacing 10.0
              StackPanel.horizontalAlignment HorizontalAlignment.Center
              StackPanel.children
                  [
                    // Screen
                    Border.create
                        [ Border.background Brushes.DarkGray
                          Border.height 30.0
                          Border.width 600.0
                          Border.cornerRadius 5.0
                          Border.child (
                              TextBlock.create
                                  [ TextBlock.text "SCREEN"
                                    TextBlock.foreground Brushes.White
                                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                                    TextBlock.verticalAlignment VerticalAlignment.Center
                                    TextBlock.fontSize 14.0
                                    TextBlock.fontWeight FontWeight.Bold ]
                          ) ]

                    // Seats grid
                    StackPanel.create
                        [ StackPanel.orientation Orientation.Vertical
                          StackPanel.spacing 3.0
                          StackPanel.children
                              [ for row in 1 .. props.Cinema.Height do
                                    yield
                                        StackPanel.create
                                            [ StackPanel.orientation Orientation.Horizontal
                                              StackPanel.spacing 3.0
                                              StackPanel.horizontalAlignment HorizontalAlignment.Center
                                              StackPanel.children
                                                  [
                                                    // Row label
                                                    yield
                                                        TextBlock.create
                                                            [ TextBlock.text $"R{row:D2}"
                                                              TextBlock.width 35.0
                                                              TextBlock.verticalAlignment VerticalAlignment.Center
                                                              TextBlock.fontSize 10.0
                                                              TextBlock.textAlignment TextAlignment.Center
                                                              TextBlock.fontWeight FontWeight.Bold ]

                                                    // Seats in this row
                                                    for col in 1 .. props.Cinema.Width do
                                                        let seat = props.Cinema.Seats.[row - 1, col - 1]

                                                        let isSelected =
                                                            match props.SelectedSeat with
                                                            | Some(selRow, selCol) -> selRow = row && selCol = col
                                                            | None -> false

                                                        yield seatButton row col seat isSelected props.OnSeatClick ] ] ] ] ] ]
