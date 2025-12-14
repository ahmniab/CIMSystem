namespace CIMSystemGUI.Tests.Services

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models
open CIMSystemGUI.Services

module TicketServiceTests =

    [<Fact>]
    let ``TicketInfo should contain all required fields`` () =
        // Arrange & Act
        let ticketInfo =
            { TicketId = "TKT-ABC123"
              CustomerName = "Alice Johnson"
              HallId = "H1"
              HallName = "Cinema Hall 1"
              MovieTitle = "The Matrix"
              SeatRow = 8
              SeatColumn = 12
              BookingDate = DateTime(2025, 12, 13, 19, 0, 0) }

        // Assert
        ticketInfo.TicketId |> should equal "TKT-ABC123"
        ticketInfo.CustomerName |> should equal "Alice Johnson"
        ticketInfo.HallId |> should equal "H1"
        ticketInfo.HallName |> should equal "Cinema Hall 1"
        ticketInfo.MovieTitle |> should equal "The Matrix"
        ticketInfo.SeatRow |> should equal 8
        ticketInfo.SeatColumn |> should equal 12

    [<Fact>]
    let ``TicketValidationResult ValidTicket should contain ticket info`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-TEST"
              CustomerName = "Bob Smith"
              HallId = "H2"
              HallName = "Hall 2"
              MovieTitle = "Inception"
              SeatRow = 5
              SeatColumn = 7
              BookingDate = DateTime.Now }

        // Act
        let result = ValidTicket ticketInfo

        // Assert
        match result with
        | ValidTicket info ->
            info.TicketId |> should equal "TKT-TEST"
            info.CustomerName |> should equal "Bob Smith"
        | _ -> failwith "Expected ValidTicket"

    [<Fact>]
    let ``TicketValidationResult InvalidTicket should contain error message`` () =
        // Arrange
        let errorMsg = "Ticket has already been redeemed"

        // Act
        let result = InvalidTicket errorMsg

        // Assert
        match result with
        | InvalidTicket msg -> msg |> should equal "Ticket has already been redeemed"
        | _ -> failwith "Expected InvalidTicket"

    [<Fact>]
    let ``TicketValidationResult TicketNotFound should be distinct case`` () =
        // Act
        let result = TicketNotFound

        // Assert
        match result with
        | TicketNotFound -> true |> should equal true
        | _ -> failwith "Expected TicketNotFound"

    [<Fact>]
    let ``TicketValidationResult ValidationError should contain error message`` () =
        // Arrange
        let errorMsg = "Database connection failed"

        // Act
        let result = ValidationError errorMsg

        // Assert
        match result with
        | ValidationError msg -> msg |> should equal "Database connection failed"
        | _ -> failwith "Expected ValidationError"

    [<Fact>]
    let ``TicketOperationResult TicketCreated should contain ticket info`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-NEW"
              CustomerName = "Charlie Brown"
              HallId = "H3"
              HallName = "Hall 3"
              MovieTitle = "Avatar"
              SeatRow = 3
              SeatColumn = 4
              BookingDate = DateTime.Now }

        // Act
        let result = TicketCreated ticketInfo

        // Assert
        match result with
        | TicketCreated info ->
            info.TicketId |> should equal "TKT-NEW"
            info.CustomerName |> should equal "Charlie Brown"
        | _ -> failwith "Expected TicketCreated"

    [<Fact>]
    let ``TicketOperationResult TicketRedeemed should contain ticket info`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-REDEEM"
              CustomerName = "Diana Prince"
              HallId = "H4"
              HallName = "Hall 4"
              MovieTitle = "Wonder Woman"
              SeatRow = 10
              SeatColumn = 15
              BookingDate = DateTime.Now }

        // Act
        let result = TicketRedeemed ticketInfo

        // Assert
        match result with
        | TicketRedeemed info ->
            info.TicketId |> should equal "TKT-REDEEM"
            info.CustomerName |> should equal "Diana Prince"
        | _ -> failwith "Expected TicketRedeemed"

    [<Fact>]
    let ``TicketOperationResult TicketError should contain error message`` () =
        // Arrange
        let errorMsg = "Failed to save ticket"

        // Act
        let result = TicketError errorMsg

        // Assert
        match result with
        | TicketError msg -> msg |> should equal "Failed to save ticket"
        | _ -> failwith "Expected TicketError"

    [<Fact>]
    let ``createTicket should return TicketCreated with valid data`` () =
        // Arrange
        let customerName = "Test Customer"
        let hallId = "H-TEST"
        let hallName = "Test Hall"
        let movieTitle = "Test Movie"
        let seatRow = 5
        let seatColumn = 10
        let bookingDate = DateTime(2025, 12, 14, 20, 0, 0)

        // Act
        let result =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        // Assert
        match result with
        | TicketCreated info ->
            info.CustomerName |> should equal customerName
            info.HallId |> should equal hallId
            info.HallName |> should equal hallName
            info.MovieTitle |> should equal movieTitle
            info.SeatRow |> should equal seatRow
            info.SeatColumn |> should equal seatColumn
            info.TicketId |> should not' (equal "")
        | _ -> failwith "Expected TicketCreated"

    [<Fact>]
    let ``validateTicket should return TicketNotFound for non-existent ticket`` () =
        // Arrange
        let nonExistentTicketId = "TKT-NONEXISTENT-12345"

        // Act
        let result = TicketService.validateTicket nonExistentTicketId

        // Assert
        match result with
        | TicketNotFound -> true |> should equal true
        | _ -> failwith "Expected TicketNotFound"

    [<Fact>]
    let ``validateTicket should return ValidTicket for existing unredeemed ticket`` () =
        // Arrange - First create a ticket
        let customerName = "Validation Test"
        let hallId = "H-VAL"
        let hallName = "Validation Hall"
        let movieTitle = "Validation Movie"
        let seatRow = 3
        let seatColumn = 7
        let bookingDate = DateTime.Now

        let createResult =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        let ticketId =
            match createResult with
            | TicketCreated info -> info.TicketId
            | _ -> failwith "Failed to create ticket for test"

        // Act
        let result = TicketService.validateTicket ticketId

        // Assert
        match result with
        | ValidTicket info ->
            info.TicketId |> should equal ticketId
            info.CustomerName |> should equal customerName
        | _ -> failwith "Expected ValidTicket"

    [<Fact>]
    let ``validateTicket should return InvalidTicket for redeemed ticket`` () =
        // Arrange - Create and redeem a ticket
        let customerName = "Redeem Test"
        let hallId = "H-REDEEM"
        let hallName = "Redeem Hall"
        let movieTitle = "Redeem Movie"
        let seatRow = 4
        let seatColumn = 8
        let bookingDate = DateTime.Now

        let createResult =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        let ticketId =
            match createResult with
            | TicketCreated info -> info.TicketId
            | _ -> failwith "Failed to create ticket for test"

        // Redeem the ticket
        let _ = TicketService.redeemTicket ticketId

        // Act
        let result = TicketService.validateTicket ticketId

        // Assert
        match result with
        | InvalidTicket msg -> msg |> should equal "Ticket has already been used"
        | _ -> failwith "Expected InvalidTicket"

    [<Fact>]
    let ``redeemTicket should return TicketRedeemed for valid unredeemed ticket`` () =
        // Arrange - Create a ticket
        let customerName = "Redeem Success"
        let hallId = "H-REDEEM-OK"
        let hallName = "Redeem Success Hall"
        let movieTitle = "Redeem Success Movie"
        let seatRow = 6
        let seatColumn = 9
        let bookingDate = DateTime.Now

        let createResult =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        let ticketId =
            match createResult with
            | TicketCreated info -> info.TicketId
            | _ -> failwith "Failed to create ticket for test"

        // Act
        let result = TicketService.redeemTicket ticketId

        // Assert
        match result with
        | TicketRedeemed info ->
            info.TicketId |> should equal ticketId
            info.CustomerName |> should equal customerName
        | _ -> failwith "Expected TicketRedeemed"

    [<Fact>]
    let ``redeemTicket should return TicketError for already redeemed ticket`` () =
        // Arrange - Create and redeem a ticket
        let customerName = "Double Redeem"
        let hallId = "H-DOUBLE"
        let hallName = "Double Hall"
        let movieTitle = "Double Movie"
        let seatRow = 7
        let seatColumn = 11
        let bookingDate = DateTime.Now

        let createResult =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        let ticketId =
            match createResult with
            | TicketCreated info -> info.TicketId
            | _ -> failwith "Failed to create ticket for test"

        // Redeem once
        let _ = TicketService.redeemTicket ticketId

        // Act - Try to redeem again
        let result = TicketService.redeemTicket ticketId

        // Assert
        match result with
        | TicketError msg -> msg |> should equal "Ticket has already been redeemed"
        | _ -> failwith "Expected TicketError"

    [<Fact>]
    let ``redeemTicket should return TicketError for non-existent ticket`` () =
        // Arrange
        let nonExistentTicketId = "TKT-REDEEM-NOTFOUND"

        // Act
        let result = TicketService.redeemTicket nonExistentTicketId

        // Assert
        match result with
        | TicketError msg -> msg |> should equal "Ticket not found"
        | _ -> failwith "Expected TicketError"

    [<Fact>]
    let ``getTicketInfo should return Some with ticket info for existing ticket`` () =
        // Arrange - Create a ticket
        let customerName = "Info Test"
        let hallId = "H-INFO"
        let hallName = "Info Hall"
        let movieTitle = "Info Movie"
        let seatRow = 8
        let seatColumn = 12
        let bookingDate = DateTime.Now

        let createResult =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        let ticketId =
            match createResult with
            | TicketCreated info -> info.TicketId
            | _ -> failwith "Failed to create ticket for test"

        // Act
        let result = TicketService.getTicketInfo ticketId

        // Assert
        result |> should not' (equal None)

        match result with
        | Some(info, isRedeemed) ->
            info.TicketId |> should equal ticketId
            info.CustomerName |> should equal customerName
            isRedeemed |> should equal false
        | None -> failwith "Expected Some"

    [<Fact>]
    let ``getTicketInfo should return None for non-existent ticket`` () =
        // Arrange
        let nonExistentTicketId = "TKT-INFO-NOTFOUND"

        // Act
        let result = TicketService.getTicketInfo nonExistentTicketId

        // Assert
        result |> should equal None

    [<Fact>]
    let ``getTicketInfo should show isRedeemed as true for redeemed ticket`` () =
        // Arrange - Create and redeem a ticket
        let customerName = "Info Redeemed Test"
        let hallId = "H-INFO-REDEEMED"
        let hallName = "Info Redeemed Hall"
        let movieTitle = "Info Redeemed Movie"
        let seatRow = 9
        let seatColumn = 13
        let bookingDate = DateTime.Now

        let createResult =
            TicketService.createTicket customerName hallId hallName movieTitle seatRow seatColumn bookingDate

        let ticketId =
            match createResult with
            | TicketCreated info -> info.TicketId
            | _ -> failwith "Failed to create ticket for test"

        // Redeem the ticket
        let _ = TicketService.redeemTicket ticketId

        // Act
        let result = TicketService.getTicketInfo ticketId

        // Assert
        match result with
        | Some(info, isRedeemed) ->
            info.TicketId |> should equal ticketId
            isRedeemed |> should equal true
        | None -> failwith "Expected Some"
