using System;
using System.Collections.Generic;

namespace SpeedRush.Models
{
    /// <summary>
    /// Runs the race: processes turns, enforces rules, decides when it ends.
    /// </summary>
    public class RaceManager
    {
        //  Constants 
        public const int    TotalTime         = 360;  
        public const int    TimePerTurn       = 10;   
        public const double PitStopRefuel     = 40.0; // litres added per pit stop
        public const double SpeedUpMultiplier = 1.5;  // speed & fuel multiplier

        //  Properties 
        public Car   ActiveCar   { get; private set; }
        public Track RaceTrack   { get; private set; }
        public int   ElapsedTime { get; private set; }
        public bool  RaceOver    { get; private set; }
        public bool  PlayerWon   { get; private set; }
        public RaceEndReason EndReason { get; private set; }

        // Computed property, calculates from ElapsedTime automatically
        public int TimeRemaining => TotalTime - ElapsedTime;

        //  Constructor 
        // ?? is the null-coalescing operator:
        //   "use car if not null, otherwise throw ArgumentNullException"
        public RaceManager(Car car)
        {
            ActiveCar   = car ?? throw new ArgumentNullException(nameof(car));
            RaceTrack   = new Track();
            ElapsedTime = 0;
            RaceOver    = false;
            PlayerWon   = false;
            EndReason   = RaceEndReason.None;
        }

        //  Core Method 

        /// <summary>
        /// Processes one player action and returns a TurnResult.
        /// This is the ONLY method the UI calls per turn.
        /// </summary>
        public TurnResult ProcessTurn(PlayerAction action)
        {
            // Guard: refuse to process if already done
            if (RaceOver)
                return new TurnResult
                {
                    Message  = "The race is already over!",
                    RaceOver = true,
                    PlayerWon = PlayerWon,
                    EndReason = EndReason
                };

            TurnResult result = new TurnResult();

            // switch/case: it executes a block of code based on matching value against different cases.
            switch (action)
            {
                case PlayerAction.SpeedUp:
                    try
                    {
                        // BurnFuel may throw - we catch it gracefully
                        ActiveCar.BurnFuel(SpeedUpMultiplier);
                        int fastDist = (int)(ActiveCar.BaseSpeed * SpeedUpMultiplier);
                        RaceTrack.Advance(fastDist);
                        ElapsedTime += TimePerTurn;

                        result.DistanceCovered = fastDist;
                        result.Message = $"SPEED UP!  Covered {fastDist} units. " +
                                         $"Fuel: {ActiveCar.CurrentFuel:F1}L";
                    }
                    catch (InvalidOperationException ex)
                    {
                        result.Message = $"[!] {ex.Message}";
                        EndRace(won: false, RaceEndReason.OutOfFuel);
                    }
                    break;

                case PlayerAction.MaintainSpeed:
                    try
                    {
                        ActiveCar.BurnFuel(1.0);
                        RaceTrack.Advance(ActiveCar.BaseSpeed);
                        ElapsedTime += TimePerTurn;

                        result.DistanceCovered = ActiveCar.BaseSpeed;
                        result.Message = $"Steady pace. Covered {ActiveCar.BaseSpeed} units. " +
                                         $"Fuel: {ActiveCar.CurrentFuel:F1}L";
                    }
                    catch (InvalidOperationException ex)
                    {
                        result.Message = $"[!] {ex.Message}";
                        EndRace(won: false, RaceEndReason.OutOfFuel);
                    }
                    break;

                case PlayerAction.PitStop:
                    try
                    {
                        ActiveCar.Refuel(PitStopRefuel);
                        ElapsedTime += TimePerTurn * 2; // pit stop costs 2 turns

                        result.DistanceCovered = 0;
                        result.Message = $"PIT STOP.  Added {PitStopRefuel}L. " +
                                         $"Tank: {ActiveCar.CurrentFuel:F1}L  (costs 2 turns)";
                    }
                    catch (ArgumentException ex)
                    {
                        // Tank too full - don't crash, just warn
                        result.Message = $"[!] Pit stop cancelled: {ex.Message}";
                        ElapsedTime += TimePerTurn; // still wastes one turn
                    }
                    break;

                default:
                    result.Message = "Unknown action.";
                    break;
            }

            //  End-condition checks (order matters) 

            if (RaceTrack.IsFinished && !RaceOver)
            {
                EndRace(won: true, RaceEndReason.Finished);
                result.Message += "\n*** YOU WIN! Race complete! ***";
            }
            else if (TimeRemaining <= 0 && !RaceOver)
            {
                EndRace(won: false, RaceEndReason.OutOfTime);
                result.Message += "\n*** TIME'S UP! Race over. ***";
            }
            else if (ActiveCar.CurrentFuel <= 0 && !RaceOver)
            {
                EndRace(won: false, RaceEndReason.OutOfFuel);
                result.Message += "\n*** OUT OF FUEL! Race over. ***";
            }

            result.FuelRemaining = ActiveCar.CurrentFuel;
            result.RaceOver      = RaceOver;
            result.PlayerWon     = PlayerWon;
            result.EndReason     = EndReason;

            RaceTrack.EventLog.Add(result.Message);
            return result;
        }

        //  Private helper 
        private void EndRace(bool won, RaceEndReason reason)
        {
            RaceOver  = true;
            PlayerWon = won;
            EndReason = reason;
        }

        //  Static factory 
        // static = belongs to the class, not an instance.
        // Called as RaceManager.GetAvailableCars() before any race exists.
        public static List<Car> GetAvailableCars() =>
            new List<Car>
            {
                new Car("BMW e30",      baseSpeed: 18, fuelConsumption: 6.8, fuelCapacity: 95.0),
                new Car("Benz 140E",    baseSpeed: 16, fuelConsumption: 10, fuelCapacity: 90.0),
                new Car("Toyota Amarok",baseSpeed: 17, fuelConsumption: 8.5, fuelCapacity: 100.0),
            };
    }
}
