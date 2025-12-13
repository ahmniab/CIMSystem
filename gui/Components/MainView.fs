namespace CIMSystemGUI.Components

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CIMSystemGUI.Models
open CIMSystemGUI.Services

module MainView =

    type ViewState =
        | Dashboard
        | MovieSelection
        | BookingHall of CinemaHall
        | StaffValidation
        | AdminDashboard
        | ManageMovies
        | ManageHalls

    let view () =
        Component(fun ctx ->
            let currentView = ctx.useState Dashboard
            
            // Load sessions (schedules) on startup
            let availableSessions = ctx.useState (CinemaService.getAllSessions()) 

            let renderContent () =
                match currentView.Current with
                | Dashboard -> 
                    StackPanel.create [
                        StackPanel.verticalAlignment VerticalAlignment.Center
                        StackPanel.horizontalAlignment HorizontalAlignment.Center
                        StackPanel.spacing 20.0
                        StackPanel.children [
                            TextBlock.create [
                                TextBlock.text "🎬 CIM Cinema System"
                                TextBlock.fontSize 32.0
                                TextBlock.fontWeight FontWeight.Bold
                                TextBlock.margin (0.0, 0.0, 0.0, 30.0)
                            ]
                            
                            Button.create [ 
                                Button.content "🎫 Customer Booking" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalContentAlignment HorizontalAlignment.Center
                                Button.padding (10.0, 15.0)
                                Button.background Brushes.Blue
                                Button.foreground Brushes.White
                                Button.onClick (fun _ -> 
                                    // Refresh data before showing movies
                                    availableSessions.Set (CinemaService.getAllSessions())
                                    currentView.Set MovieSelection
                                ) 
                            ]

                            Button.create [ 
                                Button.content "👮 Staff Ticket Check" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalContentAlignment HorizontalAlignment.Center
                                Button.padding (10.0, 15.0)
                                Button.onClick (fun _ -> currentView.Set StaffValidation) 
                            ]

                            Button.create [ 
                                Button.content "⚙️ Admin Dashboard" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalContentAlignment HorizontalAlignment.Center
                                Button.padding (10.0, 15.0)
                                Button.background Brushes.DarkSlateGray
                                Button.foreground Brushes.White
                                Button.onClick (fun _ -> currentView.Set AdminDashboard) 
                            ]
                        ]
                    ] :> Types.IView

                | MovieSelection ->
                    // FIX: Direct call, removed Component.create wrapper
                    MovieSelectionView.view availableSessions.Current (fun session ->
                        currentView.Set (BookingHall session)
                    ) :> Types.IView

                | BookingHall hall ->
                    // FIX: Wrapped in ContentControl for consistent IView return type
                    ContentControl.create [
                        ContentControl.content (
                            CinemaView.view hall (fun _ -> 
                                availableSessions.Set (CinemaService.getAllSessions())
                                currentView.Set MovieSelection
                            )
                        )
                    ] :> Types.IView

                | StaffValidation ->
                    DockPanel.create [
                        DockPanel.children [
                            Button.create [
                                Button.dock Dock.Top
                                Button.content "← Back to Main Menu"
                                Button.margin 10.0
                                Button.onClick (fun _ -> currentView.Set Dashboard)
                            ]
                            ContentControl.create [
                                ContentControl.content (StaffValidationView.view())
                            ]
                        ]
                    ] :> Types.IView

                | AdminDashboard ->
                    DockPanel.create [
                        DockPanel.children [
                            Button.create [
                                Button.dock Dock.Top
                                Button.content "← Back to Main Menu"
                                Button.margin 10.0
                                Button.onClick (fun _ -> currentView.Set Dashboard)
                            ]
                            ContentControl.create [
                                ContentControl.content (
                                    AdminView.view 
                                        (fun _ -> currentView.Set ManageMovies)
                                        (fun _ -> currentView.Set ManageHalls)
                                )
                            ]
                        ]
                    ] :> Types.IView

                | ManageMovies ->
                    DockPanel.create [
                        DockPanel.children [
                            Button.create [
                                Button.dock Dock.Top
                                Button.content "← Back to Scheduling"
                                Button.margin 10.0
                                Button.onClick (fun _ -> currentView.Set AdminDashboard)
                            ]
                            ContentControl.create [
                                ContentControl.content (ManageMoviesView.view())
                            ]
                        ]
                    ] :> Types.IView

                | ManageHalls ->
                    DockPanel.create [
                        DockPanel.children [
                            Button.create [
                                Button.dock Dock.Top
                                Button.content "← Back to Scheduling"
                                Button.margin 10.0
                                Button.onClick (fun _ -> currentView.Set AdminDashboard)
                            ]
                            ContentControl.create [
                                ContentControl.content (ManageHallsView.view())
                            ]
                        ]
                    ] :> Types.IView

            Border.create [
                Border.background (SolidColorBrush(Color.Parse("#000000ff")))
                Border.child (renderContent())
            ]
        )
