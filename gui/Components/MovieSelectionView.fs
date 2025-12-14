namespace CIMSystemGUI.Components

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Models

module MovieSelectionView =

    let view (halls: CinemaHall list) (onSelectHall: CinemaHall -> unit) (onBack: unit -> unit) =
        DockPanel.create [
            DockPanel.children [
                
                Button.create [
                    Button.dock Dock.Top
                    Button.horizontalAlignment HorizontalAlignment.Left
                    Button.content "← Back to Main Menu"
                    Button.fontSize 16.0
                    Button.margin (20.0, 20.0, 0.0, 0.0)
                    Button.background Brushes.DarkBlue 
                    Button.foreground Brushes.White
                    Button.borderThickness 0.0
                    Button.onClick (fun _ -> onBack())
                ]

                ScrollViewer.create [
                    ScrollViewer.content (
                        StackPanel.create [
                            StackPanel.spacing 20.0
                            StackPanel.horizontalAlignment HorizontalAlignment.Center
                            StackPanel.verticalAlignment VerticalAlignment.Center
                            StackPanel.children [
                                
                                TextBlock.create [
                                    TextBlock.text "🎬 Now Showing"
                                    TextBlock.fontSize 28.0
                                    TextBlock.fontWeight FontWeight.Bold
                                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                                    TextBlock.margin (0.0, 20.0)
                                    TextBlock.foreground Brushes.White 
                                ]

                                WrapPanel.create [
                                    WrapPanel.horizontalAlignment HorizontalAlignment.Center
                                    WrapPanel.children [
                                        for hall in halls do
                                            yield Button.create [
                                                Button.width 220.0
                                                Button.height 140.0
                                                Button.margin 15.0
                                                Button.foreground Brushes.White
                                                Button.cornerRadius 15.0
                                                Button.background Brushes.DarkBlue
                                                Button.borderBrush Brushes.Gray
                                                Button.borderThickness 1.0
                                                
                                                Button.onClick (fun _ -> onSelectHall hall)
                                                
                                                Button.content (
                                                    StackPanel.create [
                                                        StackPanel.verticalAlignment VerticalAlignment.Center
                                                        StackPanel.horizontalAlignment HorizontalAlignment.Center
                                                        StackPanel.spacing 10.0
                                                        StackPanel.children [
                                                            TextBlock.create [
                                                                TextBlock.text "🎬"
                                                                TextBlock.fontSize 24.0
                                                                TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                            ]

                                                            TextBlock.create [
                                                                TextBlock.text hall.MovieTitle
                                                                TextBlock.fontSize 16.0
                                                                TextBlock.fontWeight FontWeight.Bold
                                                                TextBlock.textWrapping TextWrapping.Wrap
                                                                TextBlock.textAlignment TextAlignment.Center
                                                                TextBlock.foreground Brushes.White
                                                            ]
                                                            
                                                            TextBlock.create [
                                                                TextBlock.text $"Hall: {hall.Name}"
                                                                TextBlock.fontSize 12.0
                                                                TextBlock.foreground Brushes.DarkGray
                                                                TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                            ]

                                                            TextBlock.create [
                                                                TextBlock.text "Click to Book"
                                                                TextBlock.fontSize 10.0
                                                                TextBlock.foreground Brushes.White
                                                                TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                            ]
                                                        ]
                                                    ]
                                                )
                                            ]
                                    ]
                                ]
                            ]
                        ]
                    )
                ]
            ]
        ]