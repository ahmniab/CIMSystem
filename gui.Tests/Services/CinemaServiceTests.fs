namespace CIMSystemGUI.Tests.Services

open System
open Xunit
open FsUnit.Xunit
open CIMSystemGUI.Models

module CinemaServiceTests =

    [<Fact>]
    let ``Seat record should have all required fields`` () =
        // Arrange & Act
        let seat =
            { Row = 5
              Column = 10
              Status = SeatStatus.Available
              BookedBy = None
              BookingTime = None }

        // Assert
        seat.Row |> should equal 5
        seat.Column |> should equal 10
        seat.Status |> should equal SeatStatus.Available
        seat.BookedBy |> should equal None
        seat.BookingTime |> should equal None

    [<Fact>]
    let ``Seat with booking should contain customer name and time`` () =
        // Arrange
        let bookingTime = DateTime(2025, 12, 13, 20, 30, 0)

        // Act
        let seat =
            { Row = 3
              Column = 7
              Status = SeatStatus.Booked
              BookedBy = Some "John Doe"
              BookingTime = Some bookingTime }

        // Assert
        seat.Status |> should equal SeatStatus.Booked
        seat.BookedBy |> should equal (Some "John Doe")
        seat.BookingTime |> should equal (Some bookingTime)

    [<Fact>]
    let ``Movie record should have Id and Title`` () =
        // Arrange & Act
        let movie = { Id = "M1"; Title = "The Dark Knight" }

        // Assert
        movie.Id |> should equal "M1"
        movie.Title |> should equal "The Dark Knight"

    [<Fact>]
    let ``PhysicalHall record should have dimensions`` () =
        // Arrange & Act
        let hall =
            { Id = "PH1"
              Name = "Main Hall"
              Width = 20
              Height = 15 }

        // Assert
        hall.Id |> should equal "PH1"
        hall.Name |> should equal "Main Hall"
        hall.Width |> should equal 20
        hall.Height |> should equal 15

    [<Fact>]
    let ``CinemaHall should contain scheduling information`` () =
        // Arrange
        let startTime = DateTime(2025, 12, 13, 18, 0, 0)
        let endTime = DateTime(2025, 12, 13, 21, 0, 0)

        let seats =
            Array2D.create
                10
                15
                { Row = 1
                  Column = 1
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        // Act
        let hall =
            { Id = "CH1"
              PhysicalHallId = "PH1"
              Name = "Hall 1"
              MovieId = "M1"
              MovieTitle = "Interstellar"
              StartTime = startTime
              EndTime = endTime
              Width = 15
              Height = 10
              Seats = seats }

        // Assert
        hall.Id |> should equal "CH1"
        hall.PhysicalHallId |> should equal "PH1"
        hall.MovieTitle |> should equal "Interstellar"
        hall.StartTime |> should equal startTime
        hall.EndTime |> should equal endTime
        hall.Width |> should equal 15
        hall.Height |> should equal 10

    [<Fact>]
    let ``BookingRequest should have customer and seat info`` () =
        // Arrange & Act
        let request =
            { Row = 8
              Column = 12
              CustomerName = "Jane Smith" }

        // Assert
        request.Row |> should equal 8
        request.Column |> should equal 12
        request.CustomerName |> should equal "Jane Smith"

    [<Fact>]
    let ``BookingResult SuccessWithTicket should contain message and ticket`` () =
        // Arrange
        let ticketInfo =
            { TicketId = "TKT-123"
              CustomerName = "Alice"
              HallId = "H1"
              HallName = "Hall 1"
              MovieTitle = "Dune"
              SeatRow = 5
              SeatColumn = 8
              BookingDate = DateTime.Now }

        // Act
        let result = Success("Booked!", ticketInfo)

        // Assert
        match result with
        | Success(msg, info) ->
            msg |> should equal "Booked!"
            info.TicketId |> should equal "TKT-123"
        | _ -> failwith "Expected Success"

    [<Fact>]
    let ``BookingResult SeatAlreadyBooked should be distinct case`` () =
        // Act
        let result = SeatAlreadyBooked

        // Assert
        match result with
        | SeatAlreadyBooked -> true |> should equal true
        | _ -> failwith "Expected SeatAlreadyBooked"

    [<Fact>]
    let ``BookingResult InvalidSeat should be distinct case`` () =
        // Act
        let result = InvalidSeat

        // Assert
        match result with
        | InvalidSeat -> true |> should equal true
        | _ -> failwith "Expected InvalidSeat"

    [<Fact>]
    let ``BookingResult Error should contain error message`` () =
        // Act
        let result = Error "Database connection failed"

        // Assert
        match result with
        | Error msg -> msg |> should equal "Database connection failed"
        | _ -> failwith "Expected Error"

    [<Fact>]
    let ``SeatStatus enum should have Available and Booked values`` () =
        // Assert
        SeatStatus.Available |> should equal (enum<SeatStatus> 0)
        SeatStatus.Booked |> should equal (enum<SeatStatus> 1)

    [<Fact>]
    let ``CinemaComplex should contain list of halls`` () =
        // Arrange
        let hall1 =
            { Id = "CH1"
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

        // Act
        let complex = { Halls = [ hall1 ] }

        // Assert
        complex.Halls.Length |> should equal 1
        complex.Halls.Head.Id |> should equal "CH1"

    [<Fact>]
    let ``getTotalSeatsCount should calculate correct total`` () =
        // Arrange
        let hall =
            { Id = "CH1"
              PhysicalHallId = "PH1"
              Name = "Hall 1"
              MovieId = "M1"
              MovieTitle = "Test Movie"
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

        // Act
        let total = CIMSystemGUI.Services.CinemaService.getTotalSeatsCount hall

        // Assert
        total |> should equal 80

    [<Fact>]
    let ``getAvailableSeatsCount should return correct count when all seats available`` () =
        // Arrange
        let seats =
            Array2D.create
                5
                6
                { Row = 1
                  Column = 1
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        let hall =
            { Id = "CH1"
              PhysicalHallId = "PH1"
              Name = "Hall 1"
              MovieId = "M1"
              MovieTitle = "Test Movie"
              StartTime = DateTime.Now
              EndTime = DateTime.Now.AddHours(2.0)
              Width = 6
              Height = 5
              Seats = seats }

        // Act
        let available = CIMSystemGUI.Services.CinemaService.getAvailableSeatsCount hall

        // Assert
        available |> should equal 30

    [<Fact>]
    let ``getAvailableSeatsCount should return correct count with some booked seats`` () =
        // Arrange
        let seats =
            Array2D.create
                5
                6
                { Row = 1
                  Column = 1
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        // Book 3 seats
        seats.[0, 0] <-
            { seats.[0, 0] with
                Status = SeatStatus.Booked
                BookedBy = Some "Alice" }

        seats.[1, 2] <-
            { seats.[1, 2] with
                Status = SeatStatus.Booked
                BookedBy = Some "Bob" }

        seats.[3, 4] <-
            { seats.[3, 4] with
                Status = SeatStatus.Booked
                BookedBy = Some "Charlie" }

        let hall =
            { Id = "CH1"
              PhysicalHallId = "PH1"
              Name = "Hall 1"
              MovieId = "M1"
              MovieTitle = "Test Movie"
              StartTime = DateTime.Now
              EndTime = DateTime.Now.AddHours(2.0)
              Width = 6
              Height = 5
              Seats = seats }

        // Act
        let available = CIMSystemGUI.Services.CinemaService.getAvailableSeatsCount hall

        // Assert
        available |> should equal 27

    [<Fact>]
    let ``isHallAvailable should return true when no overlap`` () =
        // Arrange
        let physicalHallId = "PH1"
        let newStart = DateTime(2025, 12, 14, 18, 0, 0)
        let newEnd = DateTime(2025, 12, 14, 20, 0, 0)

        // Act
        let isAvailable =
            CIMSystemGUI.Services.CinemaService.isHallAvailable physicalHallId newStart newEnd

        // Assert - should be true since there are no existing sessions in test environment
        isAvailable |> should equal true

    [<Fact>]
    let ``getHallById should return None when hall does not exist`` () =
        // Arrange
        let nonExistentId = "NON_EXISTENT_ID_12345"

        // Act
        let result = CIMSystemGUI.Services.CinemaService.getHallById nonExistentId

        // Assert
        result |> should equal None

    [<Fact>]
    let ``getAllPhysicalHalls should return a list`` () =
        // Act
        let halls = CIMSystemGUI.Services.CinemaService.getAllPhysicalHalls ()

        // Assert
        halls |> should be instanceOfType<PhysicalHall list>

    [<Fact>]
    let ``getAllMovies should return a list`` () =
        // Act
        let movies = CIMSystemGUI.Services.CinemaService.getAllMovies ()

        // Assert
        movies |> should be instanceOfType<Movie list>

    [<Fact>]
    let ``getAllSessions should return a list`` () =
        // Act
        let sessions = CIMSystemGUI.Services.CinemaService.getAllSessions ()

        // Assert
        sessions |> should be instanceOfType<CinemaHall list>
