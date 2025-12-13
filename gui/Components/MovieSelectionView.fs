namespace CIMSystemGUI.Components

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Models

module MovieSelectionView =

    let view (halls: CinemaHall list) (onSelectHall: CinemaHall -> unit) =
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
                            TextBlock.foreground Brushes.White // لون النص أبيض ليتناسب مع الخلفية الداكنة
                        ]

                        // حاوية الكروت (الأزرار)
                        WrapPanel.create [
                            WrapPanel.horizontalAlignment HorizontalAlignment.Center
                            WrapPanel.children [
                                for hall in halls do
                                    // هنا التغيير: جعلنا الكارت كله عبارة عن Button
                                    yield Button.create [
                                        // تنسيق الزر ليبدو مثل "الكارت"
                                        Button.width 220.0
                                        Button.height 140.0
                                        Button.margin 15.0
                                        Button.cornerRadius 15.0
                                        Button.background Brushes.WhiteSmoke
                                        Button.borderBrush Brushes.Gray
                                        Button.borderThickness 1.0
                                        
                                        // الحدث عند الضغط على المربع بالكامل
                                        Button.onClick (fun _ -> onSelectHall hall)
                                        
                                        // محتوى الزر (النصوص)
                                        Button.content (
                                            StackPanel.create [
                                                StackPanel.verticalAlignment VerticalAlignment.Center
                                                StackPanel.horizontalAlignment HorizontalAlignment.Center
                                                StackPanel.spacing 10.0
                                                StackPanel.children [
                                                    // أيقونة بسيطة أو نص يعبر عن الفيلم
                                                    TextBlock.create [
                                                        TextBlock.text "🎬"
                                                        TextBlock.fontSize 24.0
                                                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                    ]

                                                    // اسم الفيلم
                                                    TextBlock.create [
                                                        TextBlock.text hall.MovieTitle
                                                        TextBlock.fontSize 16.0
                                                        TextBlock.fontWeight FontWeight.Bold
                                                        TextBlock.textWrapping TextWrapping.Wrap
                                                        TextBlock.textAlignment TextAlignment.Center
                                                        TextBlock.foreground Brushes.Black
                                                    ]
                                                    
                                                    // رقم القاعة
                                                    TextBlock.create [
                                                        TextBlock.text $"Hall: {hall.Id}"
                                                        TextBlock.fontSize 12.0
                                                        TextBlock.foreground Brushes.DarkGray
                                                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                    ]

                                                    // نص توضيحي صغير
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