namespace CIMSystemGUI.Tests.Helpers

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models
open CIMSystemGUI.Helpers.BookingHelpers

module BookingHelpersTests =

    [<Fact>]
    let ``getSeatStatusMessage should return available message for available seat`` () =
        // Arrange
        let seat =
            { Row = 1
              Column = 2
              Status = SeatStatus.Available
              BookedBy = None
              BookingTime = None }

        // Act
        let result = getSeatStatusMessage seat 1 2

        // Assert
        result |> should equal "Seat 1-2 is available"

    [<Fact>]
    let ``getSeatStatusMessage should return booked message with customer name`` () =
        // Arrange
        let seat =
            { Row = 3
              Column = 4
              Status = SeatStatus.Booked
              BookedBy = Some "John Doe"
              BookingTime = Some DateTime.Now }

        // Act
        let result = getSeatStatusMessage seat 3 4

        // Assert
        result |> should equal "Seat 3-4 booked by John Doe"

    [<Fact>]
    let ``getSeatStatusMessage should return generic booked message when name is missing`` () =
        // Arrange
        let seat =
            { Row = 5
              Column = 6
              Status = SeatStatus.Booked
              BookedBy = None
              BookingTime = Some DateTime.Now }

        // Act
        let result = getSeatStatusMessage seat 5 6

        // Assert
        result |> should equal "Seat 5-6 is booked"

    [<Fact>]
    let ``validateBookingInput should return error when no seat is selected`` () =
        // Arrange
        let selectedSeat = None
        let customerName = "John Doe"

        // Act
        let result = validateBookingInput selectedSeat customerName

        // Assert
        match result with
        | Result.Error msg -> msg |> should equal "Please select a seat first"
        | _ -> failwith "Expected Error result"

    [<Fact>]
    let ``validateBookingInput should return error when customer name is empty`` () =
        // Arrange
        let selectedSeat = Some(1, 2)
        let customerName = ""

        // Act
        let result = validateBookingInput selectedSeat customerName

        // Assert
        match result with
        | Result.Error msg -> msg |> should equal "Please enter customer name"
        | _ -> failwith "Expected Error result"

    [<Fact>]
    let ``validateBookingInput should return error when customer name is whitespace`` () =
        // Arrange
        let selectedSeat = Some(1, 2)
        let customerName = "   "

        // Act
        let result = validateBookingInput selectedSeat customerName

        // Assert
        match result with
        | Result.Error msg -> msg |> should equal "Please enter customer name"
        | _ -> failwith "Expected Error result"

    [<Fact>]
    let ``validateBookingInput should return success with valid input`` () =
        // Arrange
        let selectedSeat = Some(3, 5)
        let customerName = "Jane Smith"

        // Act
        let result = validateBookingInput selectedSeat customerName

        // Assert
        match result with
        | Result.Ok bookingRequest ->
            bookingRequest.Row |> should equal 3
            bookingRequest.Column |> should equal 5
            bookingRequest.CustomerName |> should equal "Jane Smith"
        | Result.Error _ -> failwith "Expected Ok result"
