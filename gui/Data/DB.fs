namespace CIMSystemGUI.Data

open System
open CIMSystemGUI.Models
open CIMSystemGUI.Helpers

module DB =
    
    [<CLIMutable>]
    type SerializableHall =
        { Id: string
          PhysicalHallId: string
          Name: string
          MovieId: string
          MovieTitle: string
          StartTime: DateTime
          EndTime: DateTime
          Width: int
          Height: int
          Seats: Seat list }

    [<CLIMutable>]
    type SerializableComplex =
        { Halls: SerializableHall list }

    [<CLIMutable>]
    type SerializableTicket =
        { TicketId: string
          CustomerName: string
          HallId: string
          HallName: string
          MovieTitle: string
          SeatRow: int
          SeatColumn: int
          BookingDate: DateTime
          IsRedeemed: bool }

    
    let private seatsToList (seats: Seat[,]) =
        let height = seats.GetLength(0)
        let width = seats.GetLength(1)
        [ for row in 0 .. height - 1 do
              for col in 0 .. width - 1 do
                  yield seats.[row, col] ]

    let private listToSeats width height (seatList: Seat list) =
        let seats =
            Array2D.create height width
                { Row = 0
                  Column = 0
                  Status = SeatStatus.Available
                  BookedBy = None
                  BookingTime = None }

        seatList |> List.iter (fun seat ->
            if seat.Row > 0 && seat.Column > 0 then
                let r = seat.Row - 1
                let c = seat.Column - 1
                if r < height && c < width then
                    seats.[r, c] <- seat
        )

        for row in 0 .. height - 1 do
            for col in 0 .. width - 1 do
                if seats.[row, col].Row = 0 then
                    seats.[row, col] <- { 
                        Row = row + 1
                        Column = col + 1
                        Status = SeatStatus.Available
                        BookedBy = None
                        BookingTime = None 
                    }
        seats

    
    let saveAllSessions (halls: CinemaHall list) =
        try
            let serializableHalls = 
                halls |> List.map (fun h -> 
                    { Id = h.Id
                      PhysicalHallId = h.PhysicalHallId
                      Name = h.Name
                      MovieId = h.MovieId
                      MovieTitle = h.MovieTitle
                      StartTime = h.StartTime
                      EndTime = h.EndTime
                      Width = h.Width
                      Height = h.Height
                      Seats = seatsToList h.Seats })
            
            let complex = { Halls = serializableHalls }
            
            match JsonConverter.ConvertToJson complex with
            | Ok json ->
                let filePath = FileStorage.getCinemaBookingsPath()
                FileStorage.writeFile filePath json
            | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to save sessions: {ex.Message}"
    
    let loadAllSessions () =
        try
            let filePath = FileStorage.getCinemaBookingsPath()
            
            if not (FileStorage.fileExists filePath) then
                Result.Ok []
            else
                match FileStorage.readFile filePath with
                | Result.Ok json ->
                    match JsonConverter.ConvertFromJson<SerializableComplex> json with
                    | Result.Ok data ->
                        let halls = 
                            data.Halls |> List.map (fun h -> 
                                { Id = h.Id
                                  PhysicalHallId = h.PhysicalHallId
                                  Name = h.Name
                                  MovieId = h.MovieId
                                  MovieTitle = h.MovieTitle
                                  StartTime = h.StartTime
                                  EndTime = h.EndTime
                                  Width = h.Width
                                  Height = h.Height
                                  Seats = listToSeats h.Width h.Height h.Seats } : CinemaHall)
                        Result.Ok halls
                    | Result.Error msg -> Result.Error msg
                | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to load sessions: {ex.Message}"
    
    let updateSession (updatedHall: CinemaHall) =
        match loadAllSessions() with
        | Result.Ok halls ->
            let updated = halls |> List.map (fun h -> 
                if h.Id = updatedHall.Id then updatedHall else h)
            saveAllSessions updated
        | Result.Error msg -> Result.Error msg

    
    let savePhysicalHalls (halls: PhysicalHall list) =
        try
            match JsonConverter.ConvertToJson halls with
            | Result.Ok json ->
                let filePath = FileStorage.getPhysicalHallsPath()
                FileStorage.writeFile filePath json
            | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to save physical halls: {ex.Message}"
    
    let loadPhysicalHalls () =
        try
            let filePath = FileStorage.getPhysicalHallsPath()
            
            if not (FileStorage.fileExists filePath) then
                Result.Ok []
            else
                match FileStorage.readFile filePath with
                | Result.Ok json ->
                    JsonConverter.ConvertFromJson<PhysicalHall list> json
                | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to load physical halls: {ex.Message}"

    
    let saveMovies (movies: Movie list) =
        try
            match JsonConverter.ConvertToJson movies with
            | Result.Ok json ->
                let filePath = FileStorage.getMoviesPath()
                FileStorage.writeFile filePath json
            | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to save movies: {ex.Message}"
    
    let loadMovies () =
        try
            let filePath = FileStorage.getMoviesPath()
            
            if not (FileStorage.fileExists filePath) then
                Result.Ok []
            else
                match FileStorage.readFile filePath with
                | Result.Ok json ->
                    JsonConverter.ConvertFromJson<Movie list> json
                | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to load movies: {ex.Message}"

    
    let saveTickets (tickets: SerializableTicket list) =
        try
            match JsonConverter.ConvertToJson tickets with
            | Result.Ok json ->
                let filePath = FileStorage.getTicketsPath()
                FileStorage.writeFile filePath json
            | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to save tickets: {ex.Message}"
    
    let loadTickets () =
        try
            let filePath = FileStorage.getTicketsPath()
            
            if not (FileStorage.fileExists filePath) then
                Result.Ok []
            else
                match FileStorage.readFile filePath with
                | Result.Ok json ->
                    JsonConverter.ConvertFromJson<SerializableTicket list> json
                | Result.Error msg -> Result.Error msg
        with
        | ex -> Result.Error $"Failed to load tickets: {ex.Message}"

        