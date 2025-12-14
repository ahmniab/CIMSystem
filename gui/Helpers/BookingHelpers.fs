namespace CIMSystemGUI.Helpers

open CIMSystemGUI.Models
open CIMSystemGUI.Services

module BookingHelpers =

    let getSeatStatusMessage (seat: Seat) row col =
        match seat.Status with
        | SeatStatus.Available -> $"Seat {row}-{col} is available"
        | SeatStatus.Booked -> 
            match seat.BookedBy with
            | Some name -> $"Seat {row}-{col} booked by {name}"
            | None -> $"Seat {row}-{col} is booked"
        | _ -> $"Seat {row}-{col} status unknown"

    let validateBookingInput (selectedSeat: (int * int) option) (customerName: string) =
        match selectedSeat with
        | None -> Result.Error "Please select a seat first"
        | Some(row, col) ->
            if System.String.IsNullOrWhiteSpace(customerName) then
                Result.Error "Please enter customer name"
            else
                Result.Ok { Row = row; Column = col; CustomerName = customerName }
