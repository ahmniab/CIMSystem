namespace CIMSystemGUI.Components

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Services
open CIMSystemGUI.Models


module ManageHallsView =

    let view () =
        Component(fun ctx ->
            let halls = ctx.useState (CinemaService.getAllPhysicalHalls())
            let name = ctx.useState ""
            let width = ctx.useState 10
            let height = ctx.useState 10
            let errorMessage = ctx.useState ""

            let refresh () = 
                halls.Set (CinemaService.getAllPhysicalHalls())
                errorMessage.Set "" 

            let handleAdd () =
                if not (System.String.IsNullOrWhiteSpace(name.Current)) then
                    try
                        CinemaService.addPhysicalHall name.Current width.Current height.Current |> ignore
                        name.Set ""
                        refresh()
                    with ex ->
                        errorMessage.Set $"Error adding: {ex.Message}"

            let handleDelete id =
                try
                    CinemaService.deletePhysicalHall id |> ignore
                    refresh()
                with ex ->
                    errorMessage.Set $"Cannot delete: Hall involves active Showtimes or Reservations."

            StackPanel.create [
                StackPanel.spacing 20.0; StackPanel.margin 30.0
                StackPanel.children [
                    TextBlock.create [ TextBlock.text "🏗️ Manage Physical Halls"; TextBlock.fontSize 24.0; TextBlock.fontWeight FontWeight.Bold; TextBlock.horizontalAlignment HorizontalAlignment.Center ]

                    if not (System.String.IsNullOrEmpty(errorMessage.Current)) then
                        TextBlock.create [ 
                            TextBlock.text errorMessage.Current
                            TextBlock.foreground Brushes.Red
                            TextBlock.fontWeight FontWeight.Bold
                            TextBlock.horizontalAlignment HorizontalAlignment.Center
                        ]

                    Border.create [
                        Border.background Brushes.Black; Border.padding 15.0; Border.cornerRadius 10.0
                        Border.child (
                            StackPanel.create [
                                StackPanel.spacing 10.0
                                StackPanel.children [
                                    TextBlock.create [ TextBlock.text "Define New Hall Structure" ]
                                    TextBox.create [ TextBox.watermark "Hall Name (e.g., Hall 1)"; TextBox.text name.Current; TextBox.onTextChanged name.Set ]
                                    StackPanel.create [
                                        StackPanel.orientation Orientation.Horizontal; StackPanel.spacing 10.0
                                        StackPanel.children [
                                            TextBlock.create [ TextBlock.text "Width:"; TextBlock.verticalAlignment VerticalAlignment.Center ]
                                            NumericUpDown.create [ NumericUpDown.minimum 5.0m; NumericUpDown.value (decimal width.Current); NumericUpDown.onValueChanged (fun v -> if v.HasValue then width.Set(int v.Value)) ]
                                            TextBlock.create [ TextBlock.text "Height:"; TextBlock.verticalAlignment VerticalAlignment.Center ]
                                            NumericUpDown.create [ NumericUpDown.minimum 5.0m; NumericUpDown.value (decimal height.Current); NumericUpDown.onValueChanged (fun v -> if v.HasValue then height.Set(int v.Value)) ]
                                        ]
                                    ]
                                    Button.create [ Button.content "➕ Add Structure"; Button.background Brushes.Orange; Button.foreground Brushes.White; Button.onClick (fun _ -> handleAdd()) ]
                                ]
                            ]
                        )
                    ]

                    ScrollViewer.create [
                        ScrollViewer.height 400.0
                        ScrollViewer.content (
                            StackPanel.create [
                                StackPanel.spacing 5.0
                                StackPanel.children [
                                    for hall in halls.Current do
                                        yield Border.create [
                                            Border.background Brushes.Black; Border.padding 10.0; Border.cornerRadius 5.0; Border.borderBrush Brushes.LightGray; Border.borderThickness 1.0
                                            Border.child (
                                                DockPanel.create [
                                                    DockPanel.children [
                                                        Button.create [ 
                                                            Button.dock Dock.Right
                                                            Button.content "Delete"
                                                            Button.background Brushes.Red
                                                            Button.foreground Brushes.White
                                                            Button.onClick (fun _ -> handleDelete hall.Id) 
                                                        ]
                                                        StackPanel.create [
                                                            StackPanel.children [
                                                                TextBlock.create [ TextBlock.text hall.Name; TextBlock.fontWeight FontWeight.Bold ]
                                                                TextBlock.create [ TextBlock.text $"Dimensions: {hall.Width} x {hall.Height} seats"; TextBlock.fontSize 12.0; TextBlock.foreground Brushes.Gray ]
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