namespace CIMSystemGUI.Services

open System
open System.IO
open System.Text.Json
open CIMSystemGUI.Models

module TicketService =

    let private ticketsFilePath = "tickets.json"

    // Serializable ticket for storage
    [<CLIMutable>]
    type SerializableTicket =
        { TicketId: string
          CustomerName: string
          SeatRow: int
          SeatColumn: int
          BookingDate: DateTime
          IsRedeemed: bool }

    // Generate simple ticket ID based on ticket data
    let private generateTicketId (customerName: string) (seatInfo: string) (bookingTime: DateTime) =
        let formattedTime = bookingTime.ToString("yyyy-MM-dd-HH-mm-ss")
        let data = $"{customerName}:{seatInfo}:{formattedTime}"
        let hash = data.GetHashCode().ToString("X")
        $"TKT-{hash}"

    // Load tickets from file
    let loadTickets () =
        try
            if File.Exists(ticketsFilePath) then
                let json = File.ReadAllText(ticketsFilePath)
                let tickets = JsonSerializer.Deserialize<SerializableTicket list>(json)
                Result.Ok tickets
            else
                Result.Ok []
        with ex ->
            Result.Error $"Failed to load tickets: {ex.Message}"

    // Save tickets to file
    let private saveTickets (tickets: SerializableTicket list) =
        try
            let options = JsonSerializerOptions()
            options.WriteIndented <- true
            let json = JsonSerializer.Serialize(tickets, options)
            File.WriteAllText(ticketsFilePath, json)
            Result.Ok()
        with ex ->
            Result.Error $"Failed to save tickets: {ex.Message}"

    // Create a new ticket
    let createTicket (customerName: string) (seatRow: int) (seatColumn: int) (bookingDate: DateTime) =
        try
            let seatInfo = $"Row {seatRow}, Seat {seatColumn}"
            let ticketId = generateTicketId customerName seatInfo bookingDate

            let ticket =
                { TicketId = ticketId
                  CustomerName = customerName
                  SeatRow = seatRow
                  SeatColumn = seatColumn
                  BookingDate = bookingDate
                  IsRedeemed = false }

            match loadTickets () with
            | Result.Ok existingTickets ->
                let updatedTickets = ticket :: existingTickets

                match saveTickets updatedTickets with
                | Result.Ok() ->
                    let ticketInfo =
                        { CustomerName = customerName
                          SeatRow = seatRow
                          SeatColumn = seatColumn
                          BookingDate = bookingDate
                          TicketId = ticketId }

                    TicketCreated ticketInfo
                | Result.Error msg -> TicketError msg
            | Result.Error msg -> TicketError msg
        with ex ->
            TicketError $"Failed to create ticket: {ex.Message}"

    // Validate ticket by ID
    let validateTicket (ticketId: string) =
        try
            match loadTickets () with
            | Result.Ok tickets ->
                match tickets |> List.tryFind (fun t -> t.TicketId = ticketId) with
                | Some ticket ->
                    if ticket.IsRedeemed then
                        InvalidTicket "Ticket has already been redeemed"
                    else
                        let ticketInfo =
                            { CustomerName = ticket.CustomerName
                              SeatRow = ticket.SeatRow
                              SeatColumn = ticket.SeatColumn
                              BookingDate = ticket.BookingDate
                              TicketId = ticket.TicketId }

                        ValidTicket ticketInfo
                | None -> TicketNotFound
            | Result.Error msg -> ValidationError msg
        with ex ->
            ValidationError $"Failed to validate ticket: {ex.Message}"

    // Redeem ticket and clear booking
    let redeemTicket (ticketId: string) =
        try
            match loadTickets () with
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

                        match saveTickets updatedTickets with
                        | Result.Ok() ->
                            let ticketInfo =
                                { CustomerName = ticket.CustomerName
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

    // Get ticket by ID without redeeming
    let getTicketInfo (ticketId: string) =
        try
            match loadTickets () with
            | Result.Ok tickets ->
                match tickets |> List.tryFind (fun t -> t.TicketId = ticketId) with
                | Some ticket ->
                    let ticketInfo =
                        { CustomerName = ticket.CustomerName
                          SeatRow = ticket.SeatRow
                          SeatColumn = ticket.SeatColumn
                          BookingDate = ticket.BookingDate
                          TicketId = ticket.TicketId }

                    Some(ticketInfo, ticket.IsRedeemed)
                | None -> None
            | Result.Error _ -> None
        with _ ->
            None