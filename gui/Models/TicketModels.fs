namespace CIMSystemGUI.Models

open System

type TicketInfo =
    { TicketId: string
      CustomerName: string
      HallId: string      
      HallName: string    
      MovieTitle: string  
      SeatRow: int
      SeatColumn: int
      BookingDate: DateTime }

type TicketValidationResult =
    | ValidTicket of TicketInfo
    | InvalidTicket of string
    | TicketNotFound
    | ValidationError of string

type TicketOperationResult =
    | TicketCreated of TicketInfo
    | TicketRedeemed of TicketInfo
    | TicketError of string