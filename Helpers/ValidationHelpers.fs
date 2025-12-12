namespace CEMSystem.Helpers

open System
open CEMSystem.Models
open CEMSystem.Services

module ValidationHelpers =

    type ValidationState =
        { IsValid: bool
          Message: string
          TicketInfo: TicketInfo option }

    let createValidationState isValid message ticketInfo =
        { IsValid = isValid
          Message = message
          TicketInfo = ticketInfo }

    let validateTicketId (ticketId: string) =
        if String.IsNullOrWhiteSpace(ticketId) then
            createValidationState false "Please enter a ticket ID" None
        else
            let cleanTicketId = ticketId.Trim()

            match TicketService.validateTicket cleanTicketId with
            | ValidTicket info -> createValidationState true "✅ VALID TICKET" (Some info)
            | InvalidTicket msg -> createValidationState false $"❌ INVALID: {msg}" None
            | TicketNotFound -> createValidationState false "❌ TICKET NOT FOUND" None
            | ValidationError msg -> createValidationState false $"❌ ERROR: {msg}" None

    let formatTicketDetails (ticketInfo: TicketInfo) =
        sprintf
            "Customer: %s\nSeat: Row %d, Column %d\nBooked: %s\nTicket ID: %s"
            ticketInfo.CustomerName
            ticketInfo.SeatRow
            ticketInfo.SeatColumn
            (ticketInfo.BookingDate.ToString("yyyy-MM-dd HH:mm"))
            ticketInfo.TicketId

    let formatValidationResult (state: ValidationState) =
        match state.TicketInfo with
        | Some info when state.IsValid -> $"{state.Message}\n{formatTicketDetails info}"
        | _ -> state.Message

    let redeemTicketWithSeatClearing (ticketId: string) =
        match TicketService.redeemTicket ticketId with
        | TicketRedeemed ticketInfo ->
            match CinemaService.loadCinemaData () with
            | Result.Ok cinema ->
                match CinemaService.clearBooking cinema ticketInfo.SeatRow ticketInfo.SeatColumn with
                | Result.Ok msg -> Result.Ok $"🎬 TICKET REDEEMED\n{msg}\nCustomer can enter the cinema!"
                | Result.Error msg -> Result.Ok $"⚠️ Ticket redeemed but seat clearing failed: {msg}"
            | Result.Error msg -> Result.Ok $"⚠️ Ticket redeemed but cinema data error: {msg}"
        | TicketError msg -> Result.Error $"❌ ERROR: {msg}"
        | _ -> Result.Error "❌ FAILED"
