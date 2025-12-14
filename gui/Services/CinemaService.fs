namespace CIMSystemGUI.Services

open System
open CIMSystemGUI.Models
open CIMSystemGUI.Data

module CinemaService =

    let getAllPhysicalHalls () : PhysicalHall list =
        match DB.loadPhysicalHalls() with
        | Result.Ok halls -> halls
        | Result.Error _ -> []

    let addPhysicalHall (name: string) (width: int) (height: int) =
        let halls = getAllPhysicalHalls()
        let newHall = { Id = Guid.NewGuid().ToString(); Name = name; Width = width; Height = height }
        let updated = halls @ [newHall]
        DB.savePhysicalHalls updated

    let deletePhysicalHall (id: string) =
        let halls = getAllPhysicalHalls() |> List.filter (fun h -> h.Id <> id)
        DB.savePhysicalHalls halls

    let getAllMovies () : Movie list =
        match DB.loadMovies() with
        | Result.Ok movies -> movies
        | Result.Error _ -> []

    let addMovie (title: string) =
        let movies = getAllMovies()
        let newMovie = { Id = Guid.NewGuid().ToString(); Title = title }
        let updated = movies @ [newMovie]
        DB.saveMovies updated

    let deleteMovie (id: string) =
        let movies = getAllMovies() |> List.filter (fun m -> m.Id <> id)
        DB.saveMovies movies

    
    let getAllSessions () =
        match DB.loadAllSessions() with
        | Result.Ok halls -> halls
        | Result.Error _ -> []
    

    let getHallById (hallId: string) =
        getAllSessions() |> List.tryFind (fun h -> h.Id = hallId)

    let isHallAvailable (physicalHallId: string) (newStart: DateTime) (newEnd: DateTime) =
        let allSessions = getAllSessions()
        let hallSessions = allSessions |> List.filter (fun s -> s.PhysicalHallId = physicalHallId)
        
        let hasOverlap = 
            hallSessions 
            |> List.exists (fun s -> 
                newStart < s.EndTime && newEnd > s.StartTime
            )
        
        not hasOverlap 

    let scheduleMovie (physicalHall: PhysicalHall) (movie: Movie) (startT: DateTime) (endT: DateTime) =
        if isHallAvailable physicalHall.Id startT endT then
            let allSessions = getAllSessions()
            let newId = $"session-{allSessions.Length + 1}-{DateTime.Now.Ticks}"
            
            let newSession : CinemaHall = 
                { Id = newId
                  PhysicalHallId = physicalHall.Id
                  Name = physicalHall.Name
                  MovieId = movie.Id
                  MovieTitle = movie.Title
                  StartTime = startT
                  EndTime = endT
                  Width = physicalHall.Width
                  Height = physicalHall.Height
                  Seats = Array2D.create physicalHall.Height physicalHall.Width
                             { Row = 0; Column = 0; Status = SeatStatus.Available
                               BookedBy = None; BookingTime = None } }
            
            for row in 0 .. physicalHall.Height - 1 do
                for col in 0 .. physicalHall.Width - 1 do
                    newSession.Seats.[row, col] <- 
                        { Row = row + 1; Column = col + 1
                          Status = SeatStatus.Available
                          BookedBy = None; BookingTime = None }

            DB.saveAllSessions (allSessions @ [newSession])
        else
            Result.Error "Conflict: The hall is already booked for another movie during this time!"

    let deleteSession (sessionId: string) =
        let all = getAllSessions() |> List.filter (fun h -> h.Id <> sessionId)
        DB.saveAllSessions all


    let private updateHallInDb (updatedHall: CinemaHall) =
        DB.updateSession updatedHall

    let private isValidSeat (hall: CinemaHall) row col =
        row >= 1 && row <= hall.Height && col >= 1 && col <= hall.Width

    let bookSeat (hall: CinemaHall) (request: BookingRequest) =
        if not (isValidSeat hall request.Row request.Column) then
            InvalidSeat
        else
            let rowIndex = request.Row - 1
            let colIndex = request.Column - 1
            let currentSeat = hall.Seats.[rowIndex, colIndex]
            let bookingTime = DateTime.Now

            match currentSeat.Status with
            | SeatStatus.Available ->
                let updatedSeat =
                    { currentSeat with
                        Status = SeatStatus.Booked
                        BookedBy = Some request.CustomerName
                        BookingTime = Some bookingTime }

                hall.Seats.[rowIndex, colIndex] <- updatedSeat

                match DB.updateSession hall with
                | Result.Ok() ->
                    let ticketResult =
                        TicketService.createTicket
                            request.CustomerName
                            hall.Id
                            hall.Name        
                            hall.MovieTitle
                            request.Row
                            request.Column
                            bookingTime

                    match ticketResult with
                    | TicketCreated ticketInfo ->
                        Success($"Success! Booked {hall.MovieTitle}", ticketInfo)
                    | TicketError msg -> Error $"Ticket Error: {msg}"
                    | _ -> Error "Unknown ticket error"

                | Result.Error msg -> Error msg
            
            | SeatStatus.Booked -> SeatAlreadyBooked
            | _ -> Error "Invalid seat status"

    let clearBooking (hall: CinemaHall) row col =
        if not (isValidSeat hall row col) then Result.Error "Invalid seat"
        else
            let r = row - 1
            let c = col - 1
            let currentSeat = hall.Seats.[r, c]

            match currentSeat.Status with
            | SeatStatus.Booked ->
                let updatedSeat =
                    { currentSeat with
                        Status = SeatStatus.Available
                        BookedBy = None
                        BookingTime = None }

                hall.Seats.[r, c] <- updatedSeat

                match DB.updateSession hall with
                | Result.Ok() -> Result.Ok "Seat cleared successfully"
                | Result.Error msg -> Result.Error msg
            | _ -> Result.Error "Sat is not currently booked"


    let getAvailableSeatsCount (hall: CinemaHall) =
        let mutable count = 0
        for row in 0 .. hall.Height - 1 do
            for col in 0 .. hall.Width - 1 do
                if hall.Seats.[row, col].Status = SeatStatus.Available then
                    count <- count + 1
        count

    let getTotalSeatsCount (hall: CinemaHall) = hall.Width * hall.Height