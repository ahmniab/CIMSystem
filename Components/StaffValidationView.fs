namespace CEMSystem.Components

open System
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CEMSystem.Models
open CEMSystem.Helpers

module StaffValidationView =
    let view () =
        Component(fun ctx ->
            let ticketId = ctx.useState ""
            let validationResult = ctx.useState ""
            let isValid = ctx.useState false
            let ticketDetails = ctx.useState (None: TicketInfo option)

            // Create validation form state
            let formState =
                { ValidationUIHelpers.TicketId = ticketId
                  ValidationUIHelpers.ValidationResult = validationResult
                  ValidationUIHelpers.IsValid = isValid
                  ValidationUIHelpers.TicketDetails = ticketDetails }

            let validateTicket () =
                let validationState = ValidationHelpers.validateTicketId ticketId.Current
                ValidationUIHelpers.updateValidationState formState validationState

            let redeemTicket () =
                let cleanTicketId = ticketId.Current.Trim()

                match ValidationHelpers.redeemTicketWithSeatClearing cleanTicketId with
                | Result.Ok message ->
                    validationResult.Set message
                    isValid.Set false
                    ticketDetails.Set None
                    ticketId.Set ""
                | Result.Error errorMessage -> validationResult.Set errorMessage

            let renderTicketDetailsSection () =
                match ticketDetails.Current with
                | Some info ->
                    StackPanel.create
                        [ StackPanel.orientation Orientation.Vertical
                          StackPanel.spacing 5.0
                          StackPanel.children
                              [ TextBlock.create
                                    [ TextBlock.text "TICKET DETAILS"
                                      TextBlock.fontSize 16.0
                                      TextBlock.fontWeight FontWeight.Bold
                                      TextBlock.foreground Brushes.White ]
                                TextBlock.create
                                    [ TextBlock.text $"Customer: {info.CustomerName}"
                                      TextBlock.fontSize 14.0
                                      TextBlock.foreground Brushes.White ]
                                TextBlock.create
                                    [ TextBlock.text $"Seat: Row {info.SeatRow}, Column {info.SeatColumn}"
                                      TextBlock.fontSize 14.0
                                      TextBlock.foreground Brushes.White ] ] ]
                | None -> StackPanel.create [ StackPanel.children [] ]

            let renderActionButtons () =
                StackPanel.create
                    [ StackPanel.orientation Orientation.Horizontal
                      StackPanel.spacing 10.0
                      StackPanel.horizontalAlignment HorizontalAlignment.Center
                      StackPanel.children
                          [ Button.create
                                [ Button.content "Validate"
                                  Button.onClick (fun _ -> validateTicket ())
                                  Button.fontSize 14.0
                                  Button.padding (15.0, 8.0)
                                  Button.background Brushes.Blue
                                  Button.foreground Brushes.White ]
                            Button.create
                                [ Button.content "Allow Entry"
                                  Button.onClick (fun _ -> redeemTicket ())
                                  Button.fontSize 14.0
                                  Button.padding (15.0, 8.0)
                                  Button.background Brushes.Green
                                  Button.foreground Brushes.White
                                  Button.isEnabled isValid.Current ]
                            Button.create
                                [ Button.content "Clear"
                                  Button.onClick (fun _ -> ValidationUIHelpers.clearValidationForm formState)
                                  Button.fontSize 14.0
                                  Button.padding (15.0, 8.0)
                                  Button.background Brushes.Gray
                                  Button.foreground Brushes.White ] ] ]

            StackPanel.create
                [ StackPanel.orientation Orientation.Vertical
                  StackPanel.spacing 20.0
                  StackPanel.margin 30.0
                  StackPanel.children
                      [ TextBlock.create
                            [ TextBlock.text "🎫 Staff Validation"
                              TextBlock.fontSize 24.0
                              TextBlock.fontWeight FontWeight.Bold
                              TextBlock.foreground Brushes.White
                              TextBlock.horizontalAlignment HorizontalAlignment.Center ]

                        TextBox.create
                            [ TextBox.text ticketId.Current
                              TextBox.onTextChanged ticketId.Set
                              TextBox.watermark "Enter ticket ID"
                              TextBox.fontSize 16.0
                              TextBox.height 40.0 ]

                        renderActionButtons ()

                        if not (String.IsNullOrWhiteSpace(validationResult.Current)) then
                            TextBlock.create
                                [ TextBlock.text validationResult.Current
                                  TextBlock.fontSize 16.0
                                  TextBlock.foreground Brushes.White
                                  TextBlock.fontWeight FontWeight.Bold
                                  TextBlock.horizontalAlignment HorizontalAlignment.Center ]

                        renderTicketDetailsSection () ] ])
