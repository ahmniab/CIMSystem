namespace CIMSystemGUI.Models

open System

type SeatStatus =
    | Available = 0
    | Booked = 1

type Seat =
    { Row: int
      Column: int
      Status: SeatStatus
      BookedBy: string option
      BookingTime: DateTime option }

type Movie =
    { Id: string
      Title: string }

type PhysicalHall =
    { Id: string
      Name: string
      Width: int
      Height: int }

type CinemaHall =
    { Id: string
      PhysicalHallId: string 
      Name: string
      MovieId: string
      MovieTitle: string
      StartTime: DateTime
      EndTime: DateTime
      Width: int
      Height: int
      Seats: Seat[,] }

type CinemaComplex = 
    { Halls: CinemaHall list }

type BookingRequest =
    { Row: int
      Column: int
      CustomerName: string }

type BookingResult =
    | Success of string
    | SuccessWithTicket of string * TicketInfo
    | SeatAlreadyBooked
    | InvalidSeat
    | Error of string