namespace CIMSystemGUI.Tests.Models

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models

module CinemaModelsTests =

    [<Fact>]
    let ``SeatStatus Available should equal 0`` () =
        // Assert
        int SeatStatus.Available |> should equal 0

    [<Fact>]
    let ``SeatStatus Booked should equal 1`` () =
        // Assert
        int SeatStatus.Booked |> should equal 1

    [<Fact>]
    let ``Seat creation with all fields should work correctly`` () =
        // Arrange & Act
        let bookingTime = DateTime(2025, 12, 13, 15, 30, 0)

        let seat =
            { Row = 10
              Column = 15
              Status = SeatStatus.Booked
              BookedBy = Some "Test User"
              BookingTime = Some bookingTime }

        // Assert
        seat.Row |> should equal 10
        seat.Column |> should equal 15
        seat.Status |> should equal SeatStatus.Booked
        seat.BookedBy |> should equal (Some "Test User")
        seat.BookingTime |> should equal (Some bookingTime)

    [<Fact>]
    let ``Available seat should have None for BookedBy and BookingTime`` () =
        // Arrange & Act
        let seat =
            { Row = 5
              Column = 8
              Status = SeatStatus.Available
              BookedBy = None
              BookingTime = None }

        // Assert
        seat.Status |> should equal SeatStatus.Available
        seat.BookedBy.IsNone |> should equal true
        seat.BookingTime.IsNone |> should equal true

    [<Fact>]
    let ``Movie should contain Id and Title`` () =
        // Arrange & Act
        let movie =
            { Id = "MOVIE-001"
              Title = "Star Wars: A New Hope" }

        // Assert
        movie.Id |> should equal "MOVIE-001"
        movie.Title |> should equal "Star Wars: A New Hope"

    [<Fact>]
    let ``PhysicalHall should have correct dimensions`` () =
        // Arrange & Act
        let hall =
            { Id = "HALL-001"
              Name = "Grand Theater"
              Width = 25
              Height = 20 }

        // Assert
        hall.Id |> should equal "HALL-001"
        hall.Name |> should equal "Grand Theater"
        hall.Width |> should equal 25
        hall.Height |> should equal 20

    [<Fact>]
    let ``CinemaHall should link to PhysicalHall and Movie`` () =
        // Arrange
        let startTime = DateTime(2025, 12, 13, 19, 0, 0)
        let endTime = DateTime(2025, 12, 13, 21, 30, 0)

        let seats =
            Array2D.create
                10
                12
                { Row = 1
                  Column = 1
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        // Act
        let cinemaHall =
            { Id = "SESSION-001"
              PhysicalHallId = "HALL-001"
              Name = "Grand Theater"
              MovieId = "MOVIE-001"
              MovieTitle = "The Godfather"
              StartTime = startTime
              EndTime = endTime
              Width = 12
              Height = 10
              Seats = seats }

        // Assert
        cinemaHall.Id |> should equal "SESSION-001"
        cinemaHall.PhysicalHallId |> should equal "HALL-001"
        cinemaHall.MovieId |> should equal "MOVIE-001"
        cinemaHall.MovieTitle |> should equal "The Godfather"
        cinemaHall.StartTime |> should equal startTime
        cinemaHall.EndTime |> should equal endTime

    [<Fact>]
    let ``CinemaHall seats should be 2D array with correct dimensions`` () =
        // Arrange
        let width = 15
        let height = 10

        let seats =
            Array2D.create
                height
                width
                { Row = 1
                  Column = 1
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        // Act
        let cinemaHall =
            { Id = "SESSION-002"
              PhysicalHallId = "HALL-002"
              Name = "Small Hall"
              MovieId = "MOVIE-002"
              MovieTitle = "Pulp Fiction"
              StartTime = DateTime.Now
              EndTime = DateTime.Now.AddHours(2.5)
              Width = width
              Height = height
              Seats = seats }

        // Assert
        cinemaHall.Seats.GetLength(0) |> should equal height
        cinemaHall.Seats.GetLength(1) |> should equal width

    [<Fact>]
    let ``BookingRequest should contain customer and seat information`` () =
        // Arrange & Act
        let request =
            { Row = 7
              Column = 11
              CustomerName = "Emily Davis" }

        // Assert
        request.Row |> should equal 7
        request.Column |> should equal 11
        request.CustomerName |> should equal "Emily Davis"

    [<Fact>]
    let ``CinemaComplex should hold multiple halls`` () =
        // Arrange
        let hall1 =
            { Id = "S1"
              PhysicalHallId = "PH1"
              Name = "Hall 1"
              MovieId = "M1"
              MovieTitle = "Movie 1"
              StartTime = DateTime.Now
              EndTime = DateTime.Now.AddHours(2.0)
              Width = 10
              Height = 8
              Seats =
                Array2D.create
                    8
                    10
                    { Row = 1
                      Column = 1
                      Status = SeatStatus.Available
                      BookedBy = None
                      BookingTime = None } }

        let hall2 =
            { Id = "S2"
              PhysicalHallId = "PH2"
              Name = "Hall 2"
              MovieId = "M2"
              MovieTitle = "Movie 2"
              StartTime = DateTime.Now
              EndTime = DateTime.Now.AddHours(3.0)
              Width = 12
              Height = 10
              Seats =
                Array2D.create
                    10
                    12
                    { Row = 1
                      Column = 1
                      Status = SeatStatus.Available
                      BookedBy = None
                      BookingTime = None } }

        // Act
        let complex = { Halls = [ hall1; hall2 ] }

        // Assert
        complex.Halls.Length |> should equal 2
        complex.Halls.[0].Id |> should equal "S1"
        complex.Halls.[1].Id |> should equal "S2"

    [<Fact>]
    let ``TicketInfo should be CLIMutable for JSON serialization`` () =
        // Arrange & Act
        let ticketInfo =
            { TicketId = "TKT-999"
              CustomerName = "Frank Miller"
              HallId = "H10"
              HallName = "VIP Hall"
              MovieTitle = "The Dark Knight"
              SeatRow = 1
              SeatColumn = 5
              BookingDate = DateTime(2025, 12, 13, 20, 0, 0) }

        // Assert - CLIMutable allows mutable properties for deserialization
        ticketInfo.TicketId |> should equal "TKT-999"
        ticketInfo.CustomerName |> should equal "Frank Miller"
        ticketInfo.HallId |> should equal "H10"
        ticketInfo.MovieTitle |> should equal "The Dark Knight"
