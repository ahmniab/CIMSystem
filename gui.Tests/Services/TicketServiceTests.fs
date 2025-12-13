namespace CIMSystemGUI.Tests.Services

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models

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
