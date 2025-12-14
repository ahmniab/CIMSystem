namespace CIMSystemGUI.Components

open System
open System.IO
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open CIMSystemGUI.Models
open CIMSystemGUI.Services
open Avalonia.Styling
open Avalonia.Media

module MainView =

    type ViewState =
        | Dashboard
        | MovieSelection
        | BookingHall of CinemaHall
        | StaffValidation
        | AdminDashboard
        | ManageMovies
        | ManageHalls
        | AutomationTesting // Added new state here

    // دالة مساعدة لتحميل الصورة بأمان
    let private loadImage (path: string) =
        try
            if File.Exists(path) then
                // تحميل الصورة من الملف
                new Bitmap(path)
            else
                null
        with _ -> null

    let view () =
        Component(fun ctx ->
            let currentView = ctx.useState Dashboard
            
            // Load sessions (schedules) on startup
            let availableSessions = ctx.useState (CinemaService.getAllSessions()) 

            // تحميل صورة الخلفية
            let bgImage = loadImage "Backgrounds/mainView.jpg"

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
                                TextBlock.foreground Brushes.White
                                TextBlock.horizontalAlignment HorizontalAlignment.Center // تأكيد توسيط العنوان
                                TextBlock.margin (0.0, 0.0, 0.0, 30.0)
                            ]
                            
                            Button.create [ 
                                Button.content "🎫 Customer Booking" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalAlignment HorizontalAlignment.Center
                                Button.padding (10.0, 15.0)
                                Button.background Brushes.DarkBlue
                                Button.foreground Brushes.White
                                Button.onClick (fun _ -> 
                                    availableSessions.Set (CinemaService.getAllSessions())
                                    currentView.Set MovieSelection
                                )
                            ]

                            Button.create [ 
                                Button.content "👮 Staff Ticket Check" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalAlignment HorizontalAlignment.Center // <--- تضاف هنا أيضاً
                                Button.horizontalContentAlignment HorizontalAlignment.Center
                                Button.background Brushes.DarkBlue
                                Button.padding (10.0, 15.0)
                                Button.onClick (fun _ -> currentView.Set StaffValidation) 
                            ]

                            Button.create [ 
                                Button.content "⚙️ Admin Dashboard" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalAlignment HorizontalAlignment.Center
                                Button.horizontalContentAlignment HorizontalAlignment.Center
                                Button.padding (10.0, 15.0)
                                Button.background Brushes.DarkBlue
                                Button.foreground Brushes.White
                                Button.onClick (fun _ -> currentView.Set AdminDashboard) 
                            ]
                            
                            Button.create [ 
                                Button.content "☑ Automation Testing" 
                                Button.fontSize 18.0
                                Button.width 250.0
                                Button.horizontalAlignment HorizontalAlignment.Center
                                Button.horizontalContentAlignment HorizontalAlignment.Center
                                Button.padding (10.0, 15.0)
                                Button.background Brushes.DarkBlue
                                Button.foreground Brushes.White
                                Button.onClick (fun _ -> currentView.Set AutomationTesting) // Corrected to use the Union Case
                            ]
                        ]
                    ] :> Types.IView

                | MovieSelection ->
                    MovieSelectionView.view 
                        availableSessions.Current
                        (fun session -> currentView.Set (BookingHall session))
                        (fun _ -> currentView.Set Dashboard) :> Types.IView

                | BookingHall hall ->
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
                                Button.background Brushes.DarkBlue
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
                                Button.background Brushes.DarkBlue
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
                                Button.background Brushes.DarkBlue
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
                                Button.background Brushes.DarkBlue
                                Button.onClick (fun _ -> currentView.Set AdminDashboard)
                            ]
                            ContentControl.create [
                                ContentControl.content (ManageHallsView.view())
                            ]
                        ]
                    ] :> Types.IView

                | AutomationTesting ->
                    DockPanel.create [
                        DockPanel.children [
                            Button.create [
                                Button.dock Dock.Top
                                Button.content "← Back to Main Menu"
                                Button.margin 10.0
                                Button.background Brushes.DarkBlue
                                Button.onClick (fun _ -> currentView.Set Dashboard)
                            ]
                            ContentControl.create [
                                ContentControl.content (AutomationTestingView.view())
                            ]
                        ]
                    ] :> Types.IView

            let backgroundBrush =
                if bgImage <> null then
                    // الطريقة الصحيحة لتعريف ImageBrush في F#
                    let brush = ImageBrush()
                    brush.Source <- bgImage
                    brush.Stretch <- Stretch.UniformToFill
                    brush :> IBrush
                else
                    // لون احتياطي في حالة عدم وجود الصورة
                    SolidColorBrush(Color.Parse("#2d2d2d")) :> IBrush

            Border.create [
                Border.background backgroundBrush
                Border.child (renderContent())
            ]
        )