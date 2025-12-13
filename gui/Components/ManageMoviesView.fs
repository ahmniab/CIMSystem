namespace CIMSystemGUI.Components

open Avalonia.FuncUI        // <--- ADD THIS LINE
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Services
open CIMSystemGUI.Models

module ManageMoviesView =

    let view () =
        Component(fun ctx ->
            let movies = ctx.useState (CinemaService.getAllMovies())
            let newMovieTitle = ctx.useState ""

            let refreshMovies () =
                movies.Set (CinemaService.getAllMovies())

            let handleAdd () =
                if not (System.String.IsNullOrWhiteSpace(newMovieTitle.Current)) then
                    CinemaService.addMovie newMovieTitle.Current |> ignore
                    newMovieTitle.Set ""
                    refreshMovies()

            let handleDelete (id: string) =
                CinemaService.deleteMovie id |> ignore
                refreshMovies()

            StackPanel.create [
                StackPanel.spacing 20.0
                StackPanel.margin 30.0
                StackPanel.children [
                    TextBlock.create [
                        TextBlock.text "🎬 Manage Films Database"
                        TextBlock.fontSize 24.0
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]

                    // Add Movie Form
                    StackPanel.create [
                        StackPanel.orientation Orientation.Horizontal
                        StackPanel.spacing 10.0
                        StackPanel.horizontalAlignment HorizontalAlignment.Center
                        StackPanel.children [
                            TextBox.create [
                                TextBox.width 300.0
                                TextBox.watermark "Enter Movie Title..."
                                TextBox.text newMovieTitle.Current
                                TextBox.onTextChanged newMovieTitle.Set
                            ]
                            Button.create [
                                Button.content "Add Movie"
                                Button.background Brushes.Blue
                                Button.foreground Brushes.White
                                Button.onClick (fun _ -> handleAdd())
                            ]
                        ]
                    ]

                    // Movies List
                    ScrollViewer.create [
                        ScrollViewer.height 400.0
                        ScrollViewer.content (
                            StackPanel.create [
                                StackPanel.spacing 5.0
                                StackPanel.children [
                                    for movie in movies.Current do
                                        yield Border.create [
                                            Border.background Brushes.Black
                                            Border.borderBrush Brushes.LightGray
                                            Border.borderThickness 1.0
                                            Border.cornerRadius 5.0
                                            Border.padding 10.0
                                            Border.child (
                                                DockPanel.create [
                                                    DockPanel.children [
                                                        Button.create [
                                                            Button.dock Dock.Right
                                                            Button.content "Delete"
                                                            Button.background Brushes.Red
                                                            Button.foreground Brushes.White
                                                            Button.onClick (fun _ -> handleDelete movie.Id)
                                                        ]
                                                        TextBlock.create [
                                                            TextBlock.text movie.Title
                                                            TextBlock.fontSize 16.0
                                                            TextBlock.verticalAlignment VerticalAlignment.Center
                                                            TextBlock.fontWeight FontWeight.Bold
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