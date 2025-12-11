namespace CEMSystem.Models

open System

// Seat status enum
type SeatStatus =
    | Available = 0
    | Booked = 1

// Seat data structure
[<CLIMutable>]
type Seat =
    { Row: int
      Column: int
      Status: SeatStatus
      BookedBy: string option
      BookingTime: DateTime option }

// Cinema hall configuration
type CinemaHall =
    { Width: int // 20 seats
      Height: int // 11 rows
      Seats: Seat[,] }

// Booking request
type BookingRequest =
    { Row: int
      Column: int
      CustomerName: string }

// Booking result
type BookingResult =
    | Success of string
    | SuccessWithTicket of string * TicketInfo // Message and ticket info
    | SeatAlreadyBooked
    | InvalidSeat
    | Error of string