namespace CIMSystemGUI.Helpers

open System.Text.Json


module JsonConverter =
    let private defaultOptions =
        let opts = JsonSerializerOptions()
        opts.WriteIndented <- true
        opts

    let ConvertToJson<'T> (obj: 'T) =
        try
            let json = JsonSerializer.Serialize(obj, defaultOptions)
            Ok json
        with
        | ex -> Error $"Failed to serialize object to JSON: {ex.Message}"
    

    let ConvertFromJson<'T> (json: string) =
        try
            let obj = JsonSerializer.Deserialize<'T>(json, defaultOptions)
            Ok obj
        with
        | ex -> Error ex.Message
        
