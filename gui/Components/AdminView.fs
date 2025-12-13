namespace CIMSystemGUI.Components

open System
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Services
open CIMSystemGUI.Models

module AdminView =

    let view (onGoToMovies: unit -> unit) (onGoToHalls: unit -> unit) =
        Component(fun ctx ->
            // --- State ---
            let sessions = ctx.useState (CinemaService.getAllHalls())
            let movies = ctx.useState (CinemaService.getAllMovies())
            let physicalHalls = ctx.useState (CinemaService.getAllPhysicalHalls())
            
            // Form Selection
            let selectedMovieIndex = ctx.useState 0
            let selectedHallIndex = ctx.useState 0
            
            let startDateStr = ctx.useState (DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
            let endDateStr = ctx.useState (DateTime.Now.AddHours(2.0).ToString("yyyy-MM-dd HH:mm"))
            let statusMsg = ctx.useState ""

            let refreshData () =
                sessions.Set (CinemaService.getAllHalls())
                movies.Set (CinemaService.getAllMovies())
                physicalHalls.Set (CinemaService.getAllPhysicalHalls())

            let handleSchedule () =
                if movies.Current.Length = 0 then statusMsg.Set "Error: No movies available."
                else if physicalHalls.Current.Length = 0 then statusMsg.Set "Error: No halls defined. Go to Manage Halls."
                else
                    let isValidStart, startT = DateTime.TryParse(startDateStr.Current)
                    let isValidEnd, endT = DateTime.TryParse(endDateStr.Current)

                    if isValidStart && isValidEnd && startT < endT then
                        let movie = movies.Current.[selectedMovieIndex.Current]
                        let hall = physicalHalls.Current.[selectedHallIndex.Current]

                        match CinemaService.scheduleMovie hall movie startT endT with
                        | Result.Ok _ -> 
                            statusMsg.Set "Success: Movie Scheduled!"
                            refreshData()
                        | Result.Error msg -> 
                            statusMsg.Set $"Error: {msg}"
                    else
                        statusMsg.Set "Error: Invalid Dates"

            // --- UI ---
            StackPanel.create [
                StackPanel.spacing 20.0; StackPanel.margin 20.0
                StackPanel.children [
                    
                    // Header
                    DockPanel.create [
                        DockPanel.children [
                            StackPanel.create [
                                StackPanel.dock Dock.Right; StackPanel.orientation Orientation.Horizontal; StackPanel.spacing 10.0
                                StackPanel.children [
                                    Button.create [ Button.content "🏗️ Manage Halls"; Button.background Brushes.Orange; Button.foreground Brushes.White; Button.onClick (fun _ -> onGoToHalls()) ]
                                    Button.create [ Button.content "🎬 Manage Films"; Button.background Brushes.DarkBlue; Button.foreground Brushes.White; Button.onClick (fun _ -> onGoToMovies()) ]
                                ]
                            ]
                            TextBlock.create [ TextBlock.text "📅 Schedule Management"; TextBlock.fontSize 24.0; TextBlock.fontWeight FontWeight.Bold ]
                        ]
                    ]

                    // Schedule Form
                    Border.create [
                        Border.background Brushes.Black; Border.padding 20.0; Border.cornerRadius 10.0
                        Border.child (
                            StackPanel.create [
                                StackPanel.spacing 15.0
                                StackPanel.children [
                                    TextBlock.create [ TextBlock.text "Schedule a Movie" ; TextBlock.fontWeight FontWeight.Bold ]
                                    
                                    // 1. Select Physical Hall
                                    TextBlock.create [ TextBlock.text "Select Hall:" ]
                                    if physicalHalls.Current.Length > 0 then
                                        ComboBox.create [
                                            ComboBox.width 300.0
                                            ComboBox.dataItems physicalHalls.Current
                                            ComboBox.selectedIndex selectedHallIndex.Current
                                            ComboBox.itemTemplate (DataTemplateView<PhysicalHall>.create (fun (h: PhysicalHall) -> 
                                                TextBlock.create [ TextBlock.text (sprintf "%s (%dx%d)" h.Name h.Width h.Height) ]
                                            ))
                                            ComboBox.onSelectedIndexChanged selectedHallIndex.Set
                                        ]
                                    else TextBlock.create [ TextBlock.text "⚠️ No Halls Defined"; TextBlock.foreground Brushes.Red ]

                                    // 2. Select Movie
                                    TextBlock.create [ TextBlock.text "Select Movie:" ]
                                    if movies.Current.Length > 0 then
                                        ComboBox.create [
                                            ComboBox.width 300.0
                                            ComboBox.dataItems movies.Current
                                            ComboBox.selectedIndex selectedMovieIndex.Current
                                            ComboBox.itemTemplate (DataTemplateView<Movie>.create (fun (m: Movie) -> 
                                                TextBlock.create [ TextBlock.text m.Title ]
                                            ))
                                            ComboBox.onSelectedIndexChanged selectedMovieIndex.Set
                                        ]
                                    else TextBlock.create [ TextBlock.text "⚠️ No Movies Defined"; TextBlock.foreground Brushes.Red ]

                                    // 3. Time 
                                    StackPanel.create [
                                        StackPanel.orientation Orientation.Horizontal; StackPanel.spacing 10.0
                                        StackPanel.children [
                                            // Start Time Input
                                            StackPanel.create [ 
                                                StackPanel.children [ 
                                                    TextBlock.create [ TextBlock.text "Start (yyyy-MM-dd HH:mm)" ]
                                                    TextBox.create [ TextBox.text startDateStr.Current; TextBox.onTextChanged startDateStr.Set ] 
                                                ] 
                                            ]
                                            // End Time Input
                                            StackPanel.create [ 
                                                StackPanel.children [ 
                                                    TextBlock.create [ TextBlock.text "End (yyyy-MM-dd HH:mm)" ]
                                                    TextBox.create [ TextBox.text endDateStr.Current; TextBox.onTextChanged endDateStr.Set ] 
                                                ] 
                                            ]
                                        ]
                                    ]

                                    // Status
                                    if not (String.IsNullOrWhiteSpace(statusMsg.Current)) then
                                        TextBlock.create [ TextBlock.text statusMsg.Current; TextBlock.foreground (if statusMsg.Current.StartsWith("Success") then Brushes.Green else Brushes.Red); TextBlock.fontWeight FontWeight.Bold ]

                                    Button.create [ Button.content "📅 Schedule Session"; Button.background Brushes.Green; Button.foreground Brushes.White; Button.onClick (fun _ -> handleSchedule()) ]
                                ]
                            ]
                        )
                    ]

                    // List Active Sessions
                    TextBlock.create [ TextBlock.text "Active Schedules"; TextBlock.fontSize 18.0; TextBlock.fontWeight FontWeight.Bold ]
                    ScrollViewer.create [
                        ScrollViewer.height 300.0
                        ScrollViewer.content (
                            StackPanel.create [
                                StackPanel.spacing 10.0
                                StackPanel.children [
                                    for s in sessions.Current do
                                        yield Border.create [
                                            Border.background Brushes.White; Border.padding 10.0; Border.cornerRadius 5.0; Border.borderBrush Brushes.Gray; Border.borderThickness 1.0
                                            Border.child (
                                                DockPanel.create [
                                                    DockPanel.children [
                                                        Button.create [ Button.dock Dock.Right; Button.content "Cancel"; Button.background Brushes.Red; Button.foreground Brushes.White; Button.onClick (fun _ -> CinemaService.deleteSession s.Id |> ignore; refreshData()) ]
                                                        StackPanel.create [
                                                            StackPanel.children [
                                                                TextBlock.create [ TextBlock.text (sprintf "%s: %s" s.Name s.MovieTitle); TextBlock.fontWeight FontWeight.Bold ]
                                                                let timeText = sprintf "%s -> %s" (s.StartTime.ToString("MMM dd HH:mm")) (s.EndTime.ToString("HH:mm"))
                                                                TextBlock.create [ TextBlock.text timeText; TextBlock.foreground Brushes.Blue ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            )
                                        ]
                                ]
                            ]
                        )
                    ]
                ]
            ]
        )