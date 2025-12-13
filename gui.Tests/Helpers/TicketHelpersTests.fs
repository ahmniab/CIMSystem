namespace CIMSystemGUI.Tests.Helpers

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models

module TicketHelpersTests =

    [<Fact>]
    let ``createSuccessMessage should format message with HTML success`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-12345"
              CustomerName = "John Doe"
              HallId = "H1"
              HallName = "Hall 1"
              MovieTitle = "Test Movie"
              SeatRow = 5
              SeatColumn = 10
              BookingDate = DateTime.Now }

        let msg = "Booking successful!"
        let htmlResult = Result.Ok "ticket_TKT-12345.html"

        // Act
        let result =
            CIMSystemGUI.Components.TicketHelpers.createSuccessMessage msg ticketInfo htmlResult

        // Assert
        result.Contains("Booking successful!") |> should equal true
        result.Contains("Ticket created: ticket_TKT-12345.html") |> should equal true
        result.Contains("Ticket ID: TKT-12345") |> should equal true

    [<Fact>]
    let ``createSuccessMessage should handle HTML generation failure`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-67890"
              CustomerName = "Jane Smith"
              HallId = "H2"
              HallName = "Hall 2"
              MovieTitle = "Another Movie"
              SeatRow = 3
              SeatColumn = 7
              BookingDate = DateTime.Now }

        let msg = "Seat booked"
        let htmlResult = Result.Error "Failed to write file"

        // Act
        let result =
            CIMSystemGUI.Components.TicketHelpers.createSuccessMessage msg ticketInfo htmlResult

        // Assert
        result.Contains("Seat booked") |> should equal true

        result.Contains("HTML generation failed: Failed to write file")
        |> should equal true

        result.Contains("Ticket ID: TKT-67890") |> should equal true

    [<Fact>]
    let ``TicketInfo record should be created with all required fields`` () =
        // Arrange & Act
        let ticketInfo =
            { TicketId = "TKT-TEST"
              CustomerName = "Test Customer"
              HallId = "H3"
              HallName = "Hall 3"
              MovieTitle = "Test Movie"
              SeatRow = 1
              SeatColumn = 1
              BookingDate = DateTime(2025, 12, 13, 14, 30, 0) }

        // Assert
        ticketInfo.TicketId |> should equal "TKT-TEST"
        ticketInfo.CustomerName |> should equal "Test Customer"
        ticketInfo.HallId |> should equal "H3"
        ticketInfo.HallName |> should equal "Hall 3"
        ticketInfo.MovieTitle |> should equal "Test Movie"
        ticketInfo.SeatRow |> should equal 1
        ticketInfo.SeatColumn |> should equal 1
