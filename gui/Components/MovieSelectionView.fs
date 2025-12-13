namespace CIMSystemGUI.Components

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Models

module MovieSelectionView =

    // 1. أضفنا (onBack: unit -> unit) في الباراميترز
    let view (halls: CinemaHall list) (onSelectHall: CinemaHall -> unit) (onBack: unit -> unit) =
        DockPanel.create [
            DockPanel.children [
                
                // 2. زر الرجوع في الأعلى
                Button.create [
                    Button.dock Dock.Top
                    Button.horizontalAlignment HorizontalAlignment.Left
                    Button.content "← Back to Dashboard"
                    Button.fontSize 16.0
                    Button.margin (20.0, 20.0, 0.0, 0.0)
                    Button.background Brushes.Gray // خلفية شفافة لشكله أجمل
                    Button.foreground Brushes.Black // أو Brushes.White حسب خلفية البرنامج لديك
                    Button.borderThickness 0.0
                    Button.onClick (fun _ -> onBack())
                ]

                // 3. المحتوى الأصلي (Scroll Viewer)
                ScrollViewer.create [
                    ScrollViewer.content (
                        StackPanel.create [
                            StackPanel.spacing 20.0
                            StackPanel.horizontalAlignment HorizontalAlignment.Center
                            StackPanel.verticalAlignment VerticalAlignment.Center
                            StackPanel.children [
                                
                                // العنوان
                                TextBlock.create [
                                    TextBlock.text "🎬 Now Showing"
                                    TextBlock.fontSize 28.0
                                    TextBlock.fontWeight FontWeight.Bold
                                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                                    TextBlock.margin (0.0, 20.0)
                                    // تأكد أن هذا اللون مناسب لخلفية تطبيقك
                                    TextBlock.foreground Brushes.Black 
                                ]

                                // حاوية الكروت
                                WrapPanel.create [
                                    WrapPanel.horizontalAlignment HorizontalAlignment.Center
                                    WrapPanel.children [
                                        for hall in halls do
                                            yield Button.create [
                                                Button.width 220.0
                                                Button.height 140.0
                                                Button.margin 15.0
                                                Button.cornerRadius 15.0
                                                Button.background Brushes.WhiteSmoke
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
                                                                TextBlock.foreground Brushes.Black
                                                            ]
                                                            
                                                            TextBlock.create [
                                                                TextBlock.text $"Hall: {hall.Id}"
                                                                TextBlock.fontSize 12.0
                                                                TextBlock.foreground Brushes.DarkGray
                                                                TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                            ]

                                                            TextBlock.create [
                                                                TextBlock.text "Click to Book"
                                                                TextBlock.fontSize 10.0
                                                                TextBlock.foreground Brushes.Blue
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