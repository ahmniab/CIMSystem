namespace CIMSystemGUI.Services

open System
open CIMSystemGUI.Models
open CIMSystemGUI.Data

module TicketService =

    // Generate simple ticket ID based on ticket data
    let private generateTicketId (customerName: string) (seatInfo: string) (bookingTime: DateTime) =
        let formattedTime = bookingTime.ToString("yyyy-MM-dd-HH-mm-ss")
        let data = $"{customerName}:{seatInfo}:{formattedTime}"
        let hash = data.GetHashCode().ToString("X")
        $"TKT-{hash}"
    
    // Create a new ticket - UPDATED signature
    let createTicket (customerName: string) (hallId: string) (hallName: string) (movieTitle: string) (seatRow: int) (seatColumn: int) (bookingDate: DateTime) =
        try
            let seatInfo = $"Hall {hallName} R{seatRow} S{seatColumn}"
            let ticketId = generateTicketId customerName seatInfo bookingDate

            let ticket : DB.SerializableTicket =
                { TicketId = ticketId
                  CustomerName = customerName
                  HallId = hallId
                  HallName = hallName
                  MovieTitle = movieTitle
                  SeatRow = seatRow
                  SeatColumn = seatColumn
                  BookingDate = bookingDate
                  IsRedeemed = false }

            match DB.loadTickets() with
            | Result.Ok existingTickets ->
                let updatedTickets = ticket :: existingTickets
                match DB.saveTickets updatedTickets with
                | Result.Ok() ->
                    let ticketInfo =
                        { CustomerName = customerName
                          HallId = hallId
                          HallName = hallName
                          MovieTitle = movieTitle
                          SeatRow = seatRow
                          SeatColumn = seatColumn
                          BookingDate = bookingDate
                          TicketId = ticketId }
                    TicketCreated ticketInfo
                | Result.Error msg -> TicketError msg
            | Result.Error msg -> TicketError msg
        with ex -> TicketError ex.Message
    // Validate ticket by ID - UPDATED to return full info
    let validateTicket (ticketId: string) =
        try
            match DB.loadTickets() with
            | Result.Ok tickets ->
                match tickets |> List.tryFind (fun t -> t.TicketId = ticketId) with
                | Some ticket ->
                    if ticket.IsRedeemed then
                        InvalidTicket "Ticket has already been used"
                    else
                        let ticketInfo =
                            { CustomerName = ticket.CustomerName
                              HallId = ticket.HallId
                              HallName = ticket.HallName
                              MovieTitle = ticket.MovieTitle
                              SeatRow = ticket.SeatRow
                              SeatColumn = ticket.SeatColumn
                              BookingDate = ticket.BookingDate
                              TicketId = ticket.TicketId }

                        ValidTicket ticketInfo
                | None -> TicketNotFound
            | Result.Error msg -> ValidationError msg
        with ex ->
            ValidationError $"Failed to validate ticket: {ex.Message}"

    // Redeem ticket - UPDATED
    let redeemTicket (ticketId: string) =
        try
            match DB.loadTickets() with
            | Result.Ok tickets ->
                match tickets |> List.tryFind (fun t -> t.TicketId = ticketId) with
                | Some ticket ->
                    if ticket.IsRedeemed then
                        TicketError "Ticket has already been redeemed"
                    else
                        // Mark ticket as redeemed
                        let updatedTickets =
                            tickets
                            |> List.map (fun t ->
                                if t.TicketId = ticketId then
                                    { t with IsRedeemed = true }
                                else
                                    t)

                        match DB.saveTickets updatedTickets with
                        | Result.Ok() ->
                            let ticketInfo =
                                { CustomerName = ticket.CustomerName
                                  HallId = ticket.HallId
                                  HallName = ticket.HallName
                                  MovieTitle = ticket.MovieTitle
                                  SeatRow = ticket.SeatRow
                                  SeatColumn = ticket.SeatColumn
                                  BookingDate = ticket.BookingDate
                                  TicketId = ticket.TicketId }

                            TicketRedeemed ticketInfo
                        | Result.Error msg -> TicketError msg
                | None -> TicketError "Ticket not found"
            | Result.Error msg -> TicketError msg
        with ex ->
            TicketError $"Failed to redeem ticket: {ex.Message}"

    // Get ticket info - UPDATED
    let getTicketInfo (ticketId: string) =
        try
            match DB.loadTickets() with
            | Result.Ok tickets ->
                match tickets |> List.tryFind (fun t -> t.TicketId = ticketId) with
                | Some ticket ->
                    let ticketInfo =
                        { CustomerName = ticket.CustomerName
                          HallId = ticket.HallId
                          HallName = ticket.HallName
                          MovieTitle = ticket.MovieTitle
                          SeatRow = ticket.SeatRow
                          SeatColumn = ticket.SeatColumn
                          BookingDate = ticket.BookingDate
                          TicketId = ticket.TicketId }

                    Some(ticketInfo, ticket.IsRedeemed)
                | None -> None
            | Result.Error _ -> None
        with _ ->
            None