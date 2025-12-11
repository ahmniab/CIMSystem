namespace CEMSystem.Helpers

open System
open CEMSystem.Models
open CEMSystem.Services

module BookingHelpers =

    let validateBookingInput (selectedSeat: (int * int) option) (customerName: string) =
        match selectedSeat with
        | Some(row, col) when not (String.IsNullOrWhiteSpace(customerName)) ->
            Result.Ok
                { Row = row
                  Column = col
                  CustomerName = customerName.Trim() }
        | None -> Result.Error "Please select a seat first"
        | Some _ -> Result.Error "Please enter customer name"

    let reloadCinemaData () =
        match CinemaService.loadCinemaData () with
        | Result.Ok c -> Some c
        | Result.Error _ -> None

    let getSeatStatusMessage (seat: Seat) (row: int) (col: int) =
        match seat.Status with
        | SeatStatus.Available -> $"Selected seat {row}-{col} (Available)"
        | SeatStatus.Booked ->
            let bookedBy = seat.BookedBy |> Option.defaultValue "Unknown"
            $"Selected seat {row}-{col} (Booked by {bookedBy})"
        | _ -> "Invalid seat"
