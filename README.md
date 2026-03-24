# C# Speed Rush

C# Speed Rush is a turn-based racing simulation game built with .NET 8. The player selects a car and tries to complete five laps before running out of fuel or time. The project is structured across three .NET projects with clear responsibilities: game logic, WPF desktop interface, and automated tests.

## How to Play

Start by selecting one of three available cars. Each car has different speed, fuel consumption, and tank capacity values, so your selection affects race strategy.

Once the race starts, every turn you pick one of three actions:

- Speed Up: moves the car 1.5 times base speed and burns 1.5 times normal fuel.
- Maintain Speed: moves at base speed and burns normal fuel.
- Pit Stop: adds 40 litres of fuel, covers no distance, and costs 2 turns worth of time.

The race ends when one of these happens:

- You complete all five laps (win).
- You run out of time (loss).
- You run out of fuel (loss).

Each lap is 100 distance units, so total race distance is 500 units. Total available race time is 360 seconds.

The available cars are:

- BMW e30: base speed 18, fuel consumption 6.8 litres per turn, fuel capacity 95 litres.
- Benz 140E: base speed 16, fuel consumption 10 litres per turn, fuel capacity 90 litres.
- Toyota Amarok: base speed 17, fuel consumption 8.5 litres per turn, fuel capacity 100 litres.

## How to Run the Project

You need .NET 8 SDK installed:
https://dotnet.microsoft.com/download

From the repository root:

Run the WPF desktop app (Windows only):
dotnet run --project SpeedRush.WPF

Run automated tests:
dotnet test
or
dotnet test SpeedRush.Tests.csproj

## Project Structure

- SpeedRush.Models: core game logic (cars, track progression, race processing, turn results).
- SpeedRush.WPF: desktop UI built with WPF. Handles user interaction and display.
- SpeedRush.Tests: MSTest project validating game logic behavior.

## Requirements

- .NET 8 SDK for build and test.
- Windows for running the WPF UI.
