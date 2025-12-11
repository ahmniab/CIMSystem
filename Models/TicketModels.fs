namespace CEMSystem.Models

open System

// Ticket data structures
[<CLIMutable>]
type TicketInfo =
    { CustomerName: string
      SeatRow: int
      SeatColumn: int
      BookingDate: DateTime
      TicketId: string }

// Ticket validation result
type TicketValidationResult =
    | ValidTicket of TicketInfo
    | InvalidTicket of string
    | TicketNotFound
    | ValidationError of string

// Ticket operation result
type TicketOperationResult =
    | TicketCreated of TicketInfo
    | TicketRedeemed of TicketInfo
    | TicketError of string