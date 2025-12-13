namespace CIMSystemGUI.Tests.Helpers

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models
open CIMSystemGUI.Helpers.ValidationHelpers

module ValidationHelpersTests =

    [<Fact>]
    let ``createValidationState should create state with correct properties`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-TEST"
              CustomerName = "Test User"
              HallId = "H1"
              HallName = "Hall 1"
              MovieTitle = "Test Movie"
              SeatRow = 1
              SeatColumn = 2
              BookingDate = DateTime.Now }

        // Act
        let state = createValidationState true "Valid ticket" (Some ticketInfo)

        // Assert
        state.IsValid |> should equal true
        state.Message |> should equal "Valid ticket"
        state.TicketInfo |> should equal (Some ticketInfo)

    [<Fact>]
    let ``createValidationState should handle None ticket info`` () =
        // Act
        let state = createValidationState false "Invalid ticket" None

        // Assert
        state.IsValid |> should equal false
        state.Message |> should equal "Invalid ticket"
        state.TicketInfo |> should equal None

    [<Fact>]
    let ``formatTicketDetails should format all ticket information correctly`` () =
        // Arrange
        let bookingDate = DateTime(2025, 12, 13, 14, 30, 0)

        let ticketInfo =
            { TicketId = "TKT-FORMAT"
              CustomerName = "John Doe"
              HallId = "H1"
              HallName = "Hall 1"
              MovieTitle = "Inception"
              SeatRow = 5
              SeatColumn = 10
              BookingDate = bookingDate }

        // Act
        let result = formatTicketDetails ticketInfo

        // Assert
        result.Contains("Movie: Inception") |> should equal true
        result.Contains("Hall: H1") |> should equal true
        result.Contains("Customer: John Doe") |> should equal true
        result.Contains("Seat: Row 5, Column 10") |> should equal true
        result.Contains("Booked: 2025-12-13 14:30") |> should equal true
        result.Contains("Ticket ID: TKT-FORMAT") |> should equal true

    [<Fact>]
    let ``formatValidationResult should include ticket details for valid ticket`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-VALID"
              CustomerName = "Jane Smith"
              HallId = "H2"
              HallName = "Hall 2"
              MovieTitle = "Avatar"
              SeatRow = 3
              SeatColumn = 7
              BookingDate = DateTime(2025, 12, 13, 18, 0, 0) }

        let state = createValidationState true "✅ VALID TICKET" (Some ticketInfo)

        // Act
        let result = formatValidationResult state

        // Assert
        result.Contains("✅ VALID TICKET") |> should equal true
        result.Contains("Movie: Avatar") |> should equal true
        result.Contains("Customer: Jane Smith") |> should equal true

    [<Fact>]
    let ``formatValidationResult should return only message for invalid ticket`` () =
        // Arrange
        let state = createValidationState false "❌ TICKET NOT FOUND" None

        // Act
        let result = formatValidationResult state

        // Assert
        result |> should equal "❌ TICKET NOT FOUND"

    [<Fact>]
    let ``validateTicketId should return error for empty ticket ID`` () =
        // Act
        let result = validateTicketId ""

        // Assert
        result.IsValid |> should equal false
        result.Message |> should equal "Please enter a ticket ID"
        result.TicketInfo |> should equal None

    [<Fact>]
    let ``validateTicketId should return error for whitespace ticket ID`` () =
        // Act
        let result = validateTicketId "   "

        // Assert
        result.IsValid |> should equal false
        result.Message |> should equal "Please enter a ticket ID"

    [<Fact>]
    let ``ValidationState should have correct structure`` () =
        // Arrange & Act
        let state =
            { IsValid = true
              Message = "Test message"
              TicketInfo = None }

        // Assert
        state.IsValid |> should equal true
        state.Message |> should equal "Test message"
        state.TicketInfo.IsNone |> should equal true
