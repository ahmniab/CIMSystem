# GUI Tests

This is the unit test project for the CIMSystem GUI application, written in F# using xUnit and FsUnit.

## Test Coverage

The test suite covers the following components:

### Models Tests (`Models/CinemaModelsTests.fs`)

- **SeatStatus enum**: Validates Available and Booked status values
- **Seat record**: Tests seat creation with booking information
- **Movie and PhysicalHall**: Validates basic movie and hall data structures
- **CinemaHall**: Tests scheduling information and seat arrays
- **BookingRequest**: Validates booking request structure
- **BookingResult**: Tests all booking result variants (Success, SuccessWithTicket, SeatAlreadyBooked, InvalidSeat, Error)
- **CinemaComplex**: Tests hall collections

### Helpers Tests

#### BookingHelpers (`Helpers/BookingHelpersTests.fs`)

- **getSeatStatusMessage**: Tests seat status message generation for available, booked, and unknown seats
- **validateBookingInput**: Tests input validation for seat selection and customer name

#### ValidationHelpers (`Helpers/ValidationHelpersTests.fs`)

- **createValidationState**: Tests validation state creation
- **formatTicketDetails**: Tests ticket information formatting
- **formatValidationResult**: Tests validation result formatting for valid and invalid tickets
- **validateTicketId**: Tests ticket ID validation for empty and whitespace inputs

#### TicketHelpers (`Helpers/TicketHelpersTests.fs`)

- **createSuccessMessage**: Tests success message formatting with HTML ticket generation results
- **TicketInfo record**: Validates ticket information structure

### Services Tests

#### TicketService (`Services/TicketServiceTests.fs`)

- **TicketInfo**: Tests all required ticket fields
- **TicketValidationResult**: Tests ValidTicket, InvalidTicket, TicketNotFound, and ValidationError cases
- **TicketOperationResult**: Tests TicketCreated, TicketRedeemed, and TicketError cases

#### CinemaService (`Services/CinemaServiceTests.fs`)

- **Seat operations**: Tests seat status and booking information
- **Movie and PhysicalHall**: Tests basic cinema entities
- **CinemaHall**: Tests complete hall configuration with scheduling
- **BookingRequest and BookingResult**: Tests booking workflow types
- **SeatStatus enum**: Validates enum values
- **CinemaComplex**: Tests hall collections

## Running the Tests

### Run all tests

```bash
dotnet test gui.Tests/gui.Tests.fsproj
```

### Run tests with detailed output

```bash
dotnet test gui.Tests/gui.Tests.fsproj --verbosity normal
```

### Run tests with coverage (if configured)

```bash
dotnet test gui.Tests/gui.Tests.fsproj --collect:"XPlat Code Coverage"
```

## Test Results

✅ **Total Tests**: 50  
✅ **Passed**: 50  
❌ **Failed**: 0  
⏭️ **Skipped**: 0

## Dependencies

- **Microsoft.NET.Test.Sdk** (17.8.0): Core testing framework
- **xUnit** (2.6.2): Test framework
- **xunit.runner.visualstudio** (2.5.4): Visual Studio test adapter
- **FsUnit.xUnit** (6.0.0): F# friendly assertions for xUnit

## Project Structure

```
gui.Tests/
├── gui.Tests.fsproj          # Project file
├── README.md                 # This file
├── Helpers/
│   ├── BookingHelpersTests.fs
│   ├── ValidationHelpersTests.fs
│   └── TicketHelpersTests.fs
├── Services/
│   ├── TicketServiceTests.fs
│   └── CinemaServiceTests.fs
└── Models/
    └── CinemaModelsTests.fs
```

## Adding New Tests

To add new tests:

1. Create a new test file in the appropriate directory
2. Add the file to `gui.Tests.fsproj` in the correct order (F# requires files to be ordered by dependency)
3. Use the xUnit `[<Fact>]` attribute for individual tests
4. Use FsUnit's `should` syntax for readable assertions

Example:

```fsharp
[<Fact>]
let ``your test description`` () =
    // Arrange
    let value = someFunction()

    // Act
    let result = transformValue value

    // Assert
    result |> should equal expectedValue
```

## Notes

- The test project references the main `gui` project
- Tests are isolated and do not require external dependencies or database connections
- All tests focus on pure functions and data structures
- The warning about empty main module is expected for test projects
