using System.Collections.Generic; // for List<T> and Queue<T>

namespace SpeedRush.Models
{

    /// <summary>The three actions a player can take each turn.</summary>
    public enum PlayerAction
    {
        SpeedUp,        // 0  go faster, burn more fuel
        MaintainSpeed,  // 1  normal pace, normal fuel
        PitStop         // 2  stop, refuel, costs extra time
    }

    /// <summary>Everything the UI needs to know after one turn.</summary>

    public struct TurnResult
    {
        public string Message;        // what happened this turn
        public int    DistanceCovered; // 0 for pit stop
        public double FuelRemaining;   // current fuel after this turn
        public bool   RaceOver;        // true = game has ended
        public bool   PlayerWon;       // true = crossed finish line
    }


    // CLASS 

    /// <summary>
    /// Tracks lap number and position within a lap.
    /// Also owns the event log (List) and lap name queue (Queue).
    /// </summary>
    public class Track
    {
        // const = compile-time constant, never changes
        public const int TotalLaps   = 5;
        public const int LapDistance = 100; // units per lap

        /// <summary>Current lap number (1 to 5).</summary>
        public int CurrentLap { get; private set; }

        /// <summary>Distance covered so far in the current lap (0–100).</summary>
        public int LapProgress { get; private set; }

        //  List<string> 
        // A List is a dynamic array — grows as you .Add() to it.
        // We use it as a running log of race events.
        // The UI reads this to show the player what happened.
        public List<string> EventLog { get; private set; }

        //  Queue<string> 
        // A Queue is FIFO (first in, first out) — like a queue at a shop.
        // We Enqueue lap names at startup and Dequeue one per lap completed.
        private Queue<string> _lapNames;

        public Track()
        {
            CurrentLap  = 1;
            LapProgress = 0;
            EventLog    = new List<string>();

            _lapNames = new Queue<string>();
            _lapNames.Enqueue("Lap 1 – The Warm-Up");
            _lapNames.Enqueue("Lap 2 – Finding the Groove");
            _lapNames.Enqueue("Lap 3 – Mid-Race Push");
            _lapNames.Enqueue("Lap 4 – The Pressure Builds");
            _lapNames.Enqueue("Lap 5 – Final Sprint!");
        }

        /// <summary>
        /// Moves the car forward by distance units.
        /// Handles lap rollovers automatically with a while loop.
        /// </summary>
        public void Advance(int distance)
        {
            LapProgress += distance;

            // while: keep rolling over laps as long as we've covered enough distance
            while (LapProgress >= LapDistance && CurrentLap < TotalLaps)
            {
                LapProgress -= LapDistance; // carry over the extra
                CurrentLap++;               // next lap

                // Dequeue removes from the front of the queue and returns it
                if (_lapNames.Count > 0)
                    EventLog.Add($"  --> Starting {_lapNames.Dequeue()}");
            }

            // Cap progress on the final lap so it never exceeds 100
            if (CurrentLap == TotalLaps && LapProgress >= LapDistance)
                LapProgress = LapDistance;
        }

        /// <summary>True when all 5 laps are fully complete.</summary>
        public bool IsFinished =>
            CurrentLap == TotalLaps && LapProgress >= LapDistance;

        /// <summary>
        /// Returns a text progress bar for the current lap.
        /// Example:  [=====>    ] 55%
        /// </summary>
        public string ProgressBar()
        {
            int filled = LapProgress / 10;        // integer division: 55/10 = 5
            int empty  = 10 - filled;
            string bar = new string('=', filled)  // "=====" 
                       + ">"
                       + new string(' ', empty);  // "     "
            return $"[{bar}] {LapProgress}%";
        }
    }
}
