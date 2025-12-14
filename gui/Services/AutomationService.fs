namespace CIMSystemGUI.Services

open System
open CIMSystemGUI.Models
open CIMSystemGUI.Data

module AutomationService =

    type StepStatus = 
        | Idle
        | Running
        | Passed
        | Failed of string

    type StepDetail = {
        Inputs: Map<string, string>
        Outputs: Map<string, string>
        Logs: string list
    }

    type ProcessResult = {
        StepName: string
        Status: StepStatus
        Details: StepDetail
        Timestamp: DateTime
    }

    type AutomationContext = {
        mutable CreatedHallId: string option
        mutable CreatedMovieId: string option
        mutable CreatedSessionId: string option
        mutable CreatedTicketId: string option
    }

    let sharedContext = {
        CreatedHallId = None
        CreatedMovieId = None
        CreatedSessionId = None
        CreatedTicketId = None
    }

    let emptyResult name = {
        StepName = name
        Status = Idle
        Details = { Inputs = Map.empty; Outputs = Map.empty; Logs = [] }
        Timestamp = DateTime.Now
    }

    let testDatabase () =
        async {
            let inputs = Map [ "Action", "Check DB Connection" ]
            try
                match DB.loadPhysicalHalls() with
                | Ok count -> 
                    let outputs = Map [ "Status", "Connected"; "HallsCount", string (List.length count) ]
                    return { StepName = "Database Check"; Status = Passed; Timestamp = DateTime.Now; 
                             Details = { Inputs = inputs; Outputs = outputs; Logs = ["Connection successful"] } }
                | Result.Error e -> 
                    return { StepName = "Database Check"; Status = Failed e; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = Map.empty; Logs = ["Connection failed"] } }
            with ex ->
                return { StepName = "Database Check"; Status = Failed ex.Message; Timestamp = DateTime.Now;
                         Details = { Inputs = inputs; Outputs = Map.empty; Logs = [ex.Message] } }
        }

    let createHall () =
        async {
            let name = $"AUTO_HALL_{Guid.NewGuid().ToString().Substring(0,4)}"
            let w, h = 10, 8
            let inputs = Map [ "HallName", name; "Width", string w; "Height", string h ]
            
            try
                CinemaService.addPhysicalHall name w h |> ignore
                
                let halls = CinemaService.getAllPhysicalHalls()
                match halls |> List.tryFind (fun x -> x.Name = name) with
                | Some createdHall ->
                    sharedContext.CreatedHallId <- Some createdHall.Id 
                    let outputs = Map [ "HallId", createdHall.Id; "Result", "Created Successfully" ]
                    return { StepName = "Create Hall"; Status = Passed; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = outputs; Logs = ["Hall saved to DB"] } }
                | None ->
                    return { StepName = "Create Hall"; Status = Failed "Saved but not found"; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = Map.empty; Logs = ["Verification failed"] } }
            with ex ->
                return { StepName = "Create Hall"; Status = Failed ex.Message; Timestamp = DateTime.Now;
                         Details = { Inputs = inputs; Outputs = Map.empty; Logs = [ex.Message] } }
        }

    let createMovie () =
        async {
            let title = $"AUTO_MOVIE_{Guid.NewGuid().ToString().Substring(0,4)}"
            let inputs = Map [ "MovieTitle", title ]
            
            try
                CinemaService.addMovie title |> ignore
                
                let movies = CinemaService.getAllMovies()
                match movies |> List.tryFind (fun m -> m.Title = title) with
                | Some m ->
                    sharedContext.CreatedMovieId <- Some m.Id 
                    let outputs = Map [ "MovieId", m.Id; "Result", "Created Successfully" ]
                    return { StepName = "Create Movie"; Status = Passed; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = outputs; Logs = ["Movie saved to DB"] } }
                | None ->
                    return { StepName = "Create Movie"; Status = Failed "Not found"; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }
            with ex ->
                return { StepName = "Create Movie"; Status = Failed ex.Message; Timestamp = DateTime.Now;
                         Details = { Inputs = inputs; Outputs = Map.empty; Logs = [ex.Message] } }
        }

    let scheduleSession () =
        async {
            match sharedContext.CreatedHallId, sharedContext.CreatedMovieId with
            | Some hId, Some mId ->
                let hall = CinemaService.getAllPhysicalHalls() |> List.find (fun h -> h.Id = hId)
                let movie = CinemaService.getAllMovies() |> List.find (fun m -> m.Id = mId)
                let startT = DateTime.Now.AddDays(1.0)
                let endT = startT.AddHours(2.0)

                let inputs = Map [ 
                    "HallName", hall.Name; 
                    "MovieTitle", movie.Title;
                    "Time", startT.ToString("yyyy-MM-dd HH:mm") 
                ]

                match CinemaService.scheduleMovie hall movie startT endT with
                | Result.Ok () ->
                    let sessions = CinemaService.getAllSessions()
                    match sessions |> List.tryFind (fun s -> s.PhysicalHallId = hId && s.MovieId = mId) with
                    | Some session ->
                        sharedContext.CreatedSessionId <- Some session.Id
                        let outputs = Map [ "SessionId", session.Id; "Status", "Scheduled" ]
                        return { StepName = "Schedule Session"; Status = Passed; Timestamp = DateTime.Now;
                                 Details = { Inputs = inputs; Outputs = outputs; Logs = ["Session added to DB"] } }
                    | None ->
                        return { StepName = "Schedule Session"; Status = Failed "ID not found"; Timestamp = DateTime.Now;
                                 Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }
                | Result.Error e ->
                     return { StepName = "Schedule Session"; Status = Failed e; Timestamp = DateTime.Now;
                              Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }

            | _ -> 
                return { StepName = "Schedule Session"; Status = Failed "Missing Inputs"; Timestamp = DateTime.Now;
                         Details = { Inputs = Map.empty; Outputs = Map.empty; Logs = ["Please run Create Hall & Movie first"] } }
        }

    let bookSeat () =
        async {
            match sharedContext.CreatedSessionId with
            | Some sId ->
                match CinemaService.getHallById sId with
                | Some session ->
                    let row, col = 1, 1
                    let customer = "AUTO_USER"
                    let inputs = Map [ "SessionId", sId; "Customer", customer; "Seat", $"{row}-{col}" ]

                    let request = { Row = row; Column = col; CustomerName = customer }
                    match CinemaService.bookSeat session request with
                    | BookingResult.SuccessWithTicket (_, ticket) ->
                        sharedContext.CreatedTicketId <- Some ticket.TicketId
                        let outputs = Map [ "TicketId", ticket.TicketId; "SeatStatus", "Booked" ]
                        return { StepName = "Book Seat"; Status = Passed; Timestamp = DateTime.Now;
                                 Details = { Inputs = inputs; Outputs = outputs; Logs = ["Ticket Generated"] } }
                    | BookingResult.Error e ->
                         return { StepName = "Book Seat"; Status = Failed e; Timestamp = DateTime.Now;
                                  Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }
                    | _ -> 
                         return { StepName = "Book Seat"; Status = Failed "Other Error"; Timestamp = DateTime.Now;
                                  Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }
                | None ->
                    return { StepName = "Book Seat"; Status = Failed "Session Invalid"; Timestamp = DateTime.Now;
                             Details = { Inputs = Map ["SessionId", sId]; Outputs = Map.empty; Logs = ["Session not found in memory"] } }
            | None ->
                return { StepName = "Book Seat"; Status = Failed "Missing Input"; Timestamp = DateTime.Now;
                         Details = { Inputs = Map.empty; Outputs = Map.empty; Logs = ["Please run Schedule Session first"] } }
        }

    let validateTicket () =
        async {
            match sharedContext.CreatedTicketId with
            | Some tId ->
                let inputs = Map [ "TicketIdToValidate", tId ]
                match TicketService.validateTicket tId with
                | ValidTicket info ->
                    let outputs = Map [ "IsValid", "True"; "Customer", info.CustomerName ]
                    return { StepName = "Validate Ticket"; Status = Passed; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = outputs; Logs = ["Ticket exists and is valid"] } }
                | InvalidTicket e ->
                    return { StepName = "Validate Ticket"; Status = Failed e; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }
                | _ ->
                    return { StepName = "Validate Ticket"; Status = Failed "Unknown"; Timestamp = DateTime.Now;
                             Details = { Inputs = inputs; Outputs = Map.empty; Logs = [] } }
            | None ->
                return { StepName = "Validate Ticket"; Status = Failed "Missing Input"; Timestamp = DateTime.Now;
                         Details = { Inputs = Map.empty; Outputs = Map.empty; Logs = ["Please run Booking first"] } }
        }