namespace CIMSystemGUI.Helpers

open System.IO

module FileStorage =
    // Base directory for all data files
    let private dataDirectory = "Data"
    let private ticketsDirectory = "Data/Tickets"
    
    // File paths
    let private cinemaBookingsFile = "cinema_bookings.json"
    let private ticketsFile = "tickets.json"
    let private moviesFile = "movies.json"
    let private physicalHallsFile = "physical_halls.json"
    
    // Ensure directory exists
    let private ensureDirectoryExists (path: string) =
        if not (Directory.Exists(path)) then
            Directory.CreateDirectory(path) |> ignore
    
    // Initialize all required directories
    let initializeStorage () =
        ensureDirectoryExists dataDirectory
        ensureDirectoryExists ticketsDirectory
    
    // Get full path for a data file
    let getDataFilePath (filename: string) =
        Path.Combine(dataDirectory, filename)
    
    // Get full path for a ticket file
    let getTicketFilePath (filename: string) =
        Path.Combine(ticketsDirectory, filename)
    
    // Specific file path getters
    let getCinemaBookingsPath () = getDataFilePath cinemaBookingsFile
    let getTicketsPath () = getDataFilePath ticketsFile
    let getMoviesPath () = getDataFilePath moviesFile
    let getPhysicalHallsPath () = getDataFilePath physicalHallsFile
    
    // Read file content
    let readFile (filePath: string) =
        try
            if File.Exists(filePath) then
                let content = File.ReadAllText(filePath)
                Result.Ok content
            else
                Result.Error $"File not found: {filePath}"
        with
        | ex -> Result.Error $"Failed to read file {filePath}: {ex.Message}"
    
    // Write file content
    let writeFile (filePath: string) (content: string) =
        try
            let directory = Path.GetDirectoryName(filePath)
            ensureDirectoryExists directory
            File.WriteAllText(filePath, content)
            Result.Ok()
        with
        | ex -> Result.Error $"Failed to write file {filePath}: {ex.Message}"
    
    // Check if file exists
    let fileExists (filePath: string) = File.Exists(filePath)
    
    // Delete file
    let deleteFile (filePath: string) =
        try
            if File.Exists(filePath) then
                File.Delete(filePath)
                Result.Ok()
            else
                Result.Error "File not found"
        with
        | ex -> Result.Error $"Failed to delete file: {ex.Message}"
