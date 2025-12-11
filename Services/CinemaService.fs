namespace CEMSystem.Services

open System
open System.IO
open System.Text.Json

open CEMSystem.Models


module CinemaService =
    let private dataFilePath = "cinema_bookings.json"

    // Create initial cinema hall with all seats available
    let createCinemaHall width height =
        let seats =
            Array2D.create
                height
                width
                { Row = 0
                  Column = 0
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        // Initialize each seat with correct coordinates
        for row in 0 .. height - 1 do
            for col in 0 .. width - 1 do
                seats.[row, col] <-
                    { Row = row + 1
                      Column = col + 1
                      Status = SeatStatus.Available
                      BookedBy = None
                      BookingTime = None }

        { Width = width
          Height = height
          Seats = seats }

    // Convert 2D array to list for JSON serialization
    let private seatsToList (seats: Seat[,]) =
        let height = seats.GetLength(0)
        let width = seats.GetLength(1)

        [ for row in 0 .. height - 1 do
              for col in 0 .. width - 1 do
                  yield seats.[row, col] ]

    // Convert list back to 2D array
    let private listToSeats width height (seatList: Seat list) =
        let seats =
            Array2D.create
                height
                width
                { Row = 0
                  Column = 0
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        seatList
        |> List.iteri (fun i seat ->
            let row = i / width
            let col = i % width
            seats.[row, col] <- seat)

        seats

    // Serializable cinema data
    [<CLIMutable>]
    type SerializableCinema =
        { Width: int
          Height: int
          Seats: Seat list }

    // Save cinema data to JSON file
    let saveCinemaData (cinema: CinemaHall) =
        try
            let serializableData =
                { Width = cinema.Width
                  Height = cinema.Height
                  Seats = seatsToList cinema.Seats }

            let options = JsonSerializerOptions()
            options.WriteIndented <- true
            let json = JsonSerializer.Serialize(serializableData, options)
            File.WriteAllText(dataFilePath, json)
            Result.Ok()
        with ex ->
            Result.Error $"Failed to save data: {ex.Message}"

    // Load cinema data from JSON file
    let loadCinemaData () : Result<CinemaHall, string> =
        try
            if File.Exists(dataFilePath) then
                let json = File.ReadAllText(dataFilePath)
                let data = JsonSerializer.Deserialize<SerializableCinema>(json)
                let seats = listToSeats data.Width data.Height data.Seats

                Result.Ok
                    { Width = data.Width
                      Height = data.Height
                      Seats = seats }
            else
                // Create default cinema if file doesn't exist
                let cinema = createCinemaHall 20 11
                saveCinemaData cinema |> ignore
                Result.Ok cinema
        with ex ->
            Result.Error $"Failed to load data: {ex.Message}"

    // Check if seat coordinates are valid
    let private isValidSeat (cinema: CinemaHall) row col =
        row >= 1 && row <= cinema.Height && col >= 1 && col <= cinema.Width

    // Book a seat and create ticket
    let bookSeat (cinema: CinemaHall) (request: BookingRequest) =
        if not (isValidSeat cinema request.Row request.Column) then
            InvalidSeat
        else
            let rowIndex = request.Row - 1
            let colIndex = request.Column - 1
            let currentSeat = cinema.Seats.[rowIndex, colIndex]
            let bookingTime = DateTime.Now

            match currentSeat.Status with
            | SeatStatus.Available ->
                // Book the seat
                let updatedSeat =
                    { currentSeat with
                        Status = SeatStatus.Booked
                        BookedBy = Some request.CustomerName
                        BookingTime = Some bookingTime }

                cinema.Seats.[rowIndex, colIndex] <- updatedSeat

                // Save updated cinema data
                match saveCinemaData cinema with
                | Result.Ok() ->
                    // Create ticket using the Services
                    let ticketResult =
                        CEMSystem.Services.TicketService.createTicket
                            request.CustomerName
                            request.Row
                            request.Column
                            bookingTime

                    match ticketResult with
                    | TicketCreated ticketInfo ->
                        let message =
                            $"Seat {request.Row}-{request.Column} successfully booked for {request.CustomerName}"

                        SuccessWithTicket(message, ticketInfo)
                    | TicketError msg ->
                        // Seat is booked but ticket creation failed
                        Error $"Seat booked but ticket creation failed: {msg}"
                    | _ ->
                        // Handle unexpected ticket operation results
                        Error "Unexpected error during ticket creation"
                | Result.Error msg -> Error msg
            | SeatStatus.Booked -> SeatAlreadyBooked
            | _ -> Error "Invalid seat status"

    // Clear booking when ticket is redeemed
    let clearBooking (cinema: CinemaHall) row col =
        if not (isValidSeat cinema row col) then
            Result.Error "Invalid seat"
        else
            let rowIndex = row - 1
            let colIndex = col - 1
            let currentSeat = cinema.Seats.[rowIndex, colIndex]

            match currentSeat.Status with
            | SeatStatus.Booked ->
                let updatedSeat =
                    { currentSeat with
                        Status = SeatStatus.Available
                        BookedBy = None
                        BookingTime = None }

                cinema.Seats.[rowIndex, colIndex] <- updatedSeat

                match saveCinemaData cinema with
                | Result.Ok() -> Result.Ok $"Seat {row}-{col} has been cleared"
                | Result.Error msg -> Result.Error msg
            | SeatStatus.Available -> Result.Error "Seat is not currently booked"
            | _ -> Result.Error "Invalid seat status"



    // Get seat status
    let getSeatStatus (cinema: CinemaHall) row col =
        if not (isValidSeat cinema row col) then
            None
        else
            Some cinema.Seats.[row - 1, col - 1]

    // Get all booked seats
    let getBookedSeats (cinema: CinemaHall) =
        [ for row in 0 .. cinema.Height - 1 do
              for col in 0 .. cinema.Width - 1 do
                  let seat = cinema.Seats.[row, col]

                  if seat.Status = SeatStatus.Booked then
                      yield seat ]

    // Get available seats count
    let getAvailableSeatsCount (cinema: CinemaHall) =
        let mutable count = 0

        for row in 0 .. cinema.Height - 1 do
            for col in 0 .. cinema.Width - 1 do
                if cinema.Seats.[row, col].Status = SeatStatus.Available then
                    count <- count + 1

        count

    // Get total seats count
    let getTotalSeatsCount (cinema: CinemaHall) = cinema.Width * cinema.Height
