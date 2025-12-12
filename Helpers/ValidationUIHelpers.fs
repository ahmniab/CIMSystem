namespace CEMSystem.Helpers

open Avalonia.FuncUI
open CEMSystem.Models
open CEMSystem.Services

module ValidationUIHelpers =

    type ValidationFormState =
        { TicketId: IWritable<string>
          ValidationResult: IWritable<string>
          IsValid: IWritable<bool>
          TicketDetails: IWritable<TicketInfo option> }

    type ExtendedValidationFormState =
        { TicketId: IWritable<string>
          ValidationResult: IWritable<string>
          IsValidating: IWritable<bool>
          ValidatedTicket: IWritable<TicketInfo option>
          CanRedeem: IWritable<bool> }

    let clearValidationForm (state: ValidationFormState) =
        state.TicketId.Set ""
        state.ValidationResult.Set ""
        state.IsValid.Set false
        state.TicketDetails.Set None

    let clearExtendedValidationForm (state: ExtendedValidationFormState) =
        state.TicketId.Set ""
        state.ValidationResult.Set ""
        state.IsValidating.Set false
        state.ValidatedTicket.Set None
        state.CanRedeem.Set false

    let updateValidationState (state: ValidationFormState) (validationState: ValidationHelpers.ValidationState) =
        state.ValidationResult.Set validationState.Message
        state.IsValid.Set validationState.IsValid
        state.TicketDetails.Set validationState.TicketInfo

    let updateExtendedValidationState
        (state: ExtendedValidationFormState)
        (validationState: ValidationHelpers.ValidationState)
        =
        let formattedResult = ValidationHelpers.formatValidationResult validationState
        state.ValidationResult.Set formattedResult
        state.ValidatedTicket.Set validationState.TicketInfo
        state.CanRedeem.Set validationState.IsValid
        state.IsValidating.Set false
