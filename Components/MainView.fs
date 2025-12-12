namespace CEMSystem.Components

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media

module MainView =

    type NavigationState = { CurrentView: IWritable<string> }

    let createNavigationButton (text: string) (viewKey: string) (navState: NavigationState) =
        let isActive = navState.CurrentView.Current = viewKey

        Button.create
            [ Button.content text
              Button.onClick (fun _ -> navState.CurrentView.Set(viewKey))
              Button.fontSize 16.0
              Button.fontWeight FontWeight.SemiBold
              Button.padding (20.0, 10.0)
              Button.cornerRadius 8.0
              Button.background (
                  if isActive then
                      SolidColorBrush(Color.Parse("#5E81AC"))
                  else
                      SolidColorBrush(Color.Parse("#4C566A"))
              )
              Button.foreground Brushes.White ]

    let renderNavigationHeader (navState: NavigationState) =
        Border.create
            [ Border.dock Dock.Top
              Border.background (SolidColorBrush(Color.Parse("#2E3440")))
              Border.padding (15.0, 10.0)
              Border.child (
                  StackPanel.create
                      [ StackPanel.orientation Orientation.Vertical
                        StackPanel.spacing 15.0
                        StackPanel.children
                            [
                              // Title
                              TextBlock.create
                                  [ TextBlock.text "🎬 CEM Cinema Management System"
                                    TextBlock.fontSize 24.0
                                    TextBlock.fontWeight FontWeight.Bold
                                    TextBlock.foreground Brushes.White
                                    TextBlock.horizontalAlignment HorizontalAlignment.Center ]

                              // Navigation buttons
                              StackPanel.create
                                  [ StackPanel.orientation Orientation.Horizontal
                                    StackPanel.horizontalAlignment HorizontalAlignment.Center
                                    StackPanel.spacing 20.0
                                    StackPanel.children
                                        [ createNavigationButton "🎬 Cinema Booking" "booking" navState
                                          createNavigationButton "🎫 Staff Validation" "validation" navState ] ] ] ]
              ) ]

    let renderContentArea (currentView: string) =
        ContentControl.create
            [ ContentControl.content (
                  match currentView with
                  | "validation" -> StaffValidationView.view ()
                  | _ -> CinemaView.view ()
              ) ]

    let view () =
        Component(fun ctx ->
            let currentView = ctx.useState "booking" // "booking", "validation", or "ticketValidation"

            let navState = { CurrentView = currentView }

            DockPanel.create
                [ DockPanel.children [ renderNavigationHeader navState; renderContentArea currentView.Current ] ])
