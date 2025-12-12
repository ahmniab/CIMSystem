namespace CEMSystem.Helpers

open Avalonia.FuncUI

module UIHelpers =

    type BookingFormState =
        { SelectedSeat: IWritable<(int * int) option>
          CustomerName: IWritable<string>
          StatusMessage: IWritable<string> }

    let clearBookingForm (state: BookingFormState) =
        state.SelectedSeat.Set None
        state.CustomerName.Set ""

    let updateStatusMessage (state: BookingFormState) (message: string) = state.StatusMessage.Set message

    let clearSelectionWithMessage (state: BookingFormState) =
        clearBookingForm state
        updateStatusMessage state "Selection cleared"
