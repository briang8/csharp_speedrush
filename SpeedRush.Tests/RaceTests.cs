using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpeedRush.Models;

namespace SpeedRush.Tests
{
    [TestClass] // tells the test runner: look inside here for tests
    public class RaceTests
    {

        // CAR FUEL TESTS
   

        [TestMethod]
        public void BurnFuel_Normal_ReducesFuelByConsumptionAmount()
        {
            // Arrange
            var car = new Car("Test", baseSpeed: 20, fuelConsumption: 10.0, fuelCapacity: 80.0);
            // CurrentFuel starts at 80

            // Act
            car.BurnFuel(1.0); // normal multiplier

            // Assert — 80 - (10 × 1.0) = 70
            Assert.AreEqual(70.0, car.CurrentFuel,
                "Normal burn should reduce fuel by FuelConsumption (10).");
        }

        [TestMethod]
        public void BurnFuel_SpeedUp_ReducesFuelByOnePointFiveTimes()
        {
            // Arrange
            var car = new Car("Test", 20, 10.0, 80.0);

            // Act
            car.BurnFuel(1.5); // speed-up multiplier

            // Assert — 80 - (10 × 1.5) = 80 - 15 = 65
            Assert.AreEqual(65.0, car.CurrentFuel,
                "Speed-up burn should cost 1.5x fuel (15 total).");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        // ExpectedException: test PASSES if this exception is thrown,
        // FAILS if no exception is thrown.
        public void BurnFuel_EmptyTank_ThrowsInvalidOperationException()
        {
            var car = new Car("Test", 20, 10.0, 80.0);
            car.CurrentFuel = 0; // force empty

            car.BurnFuel(1.0); // should throw here
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Refuel_OverfullTank_ThrowsArgumentException()
        {
            var car = new Car("Test", 20, 10.0, 80.0);
            // CurrentFuel = 80 (full)

            car.Refuel(1.0); // any amount on a full tank should throw
        }


        // TRACK TESTS


        [TestMethod]
        public void Track_Advance_StaysOnSameLapUnder100Units()
        {
            var track = new Track();

            track.Advance(50);

            Assert.AreEqual(1, track.CurrentLap,   "Should still be lap 1.");
            Assert.AreEqual(50, track.LapProgress,  "Progress should be 50.");
        }

        [TestMethod]
        public void Track_Advance_AdvancesLapAfter100Units()
        {
            var track = new Track();

            track.Advance(100); // complete exactly one lap

            Assert.AreEqual(2, track.CurrentLap,   "Should be on lap 2.");
            Assert.AreEqual(0, track.LapProgress,   "Progress resets to 0.");
        }

        [TestMethod]
        public void Track_IsFinished_TrueAfterAllLaps()
        {
            var track = new Track();

            track.Advance(500); // 5 laps × 100 units

            Assert.IsTrue(track.IsFinished, "Track should be finished after 500 units.");
        }


        // RACEMANAGER INTEGRATION TESTS


        [TestMethod]
        public void RaceManager_MaintainSpeed_AdvancesAndBurnsFuel()
        {
            var car     = new Car("Test", baseSpeed: 20, fuelConsumption: 10.0, fuelCapacity: 80.0);
            var manager = new RaceManager(car);

            TurnResult result = manager.ProcessTurn(PlayerAction.MaintainSpeed);

            Assert.AreEqual(20,   result.DistanceCovered, "Should cover BaseSpeed (20).");
            Assert.AreEqual(70.0, car.CurrentFuel,         "Fuel should drop by 10.");
        }

        [TestMethod]
        public void RaceManager_PitStop_RefuelsCar()
        {
            var car = new Car("Test", 20, 10.0, 80.0);
            car.CurrentFuel = 30.0; // partially empty
            var manager = new RaceManager(car);

            manager.ProcessTurn(PlayerAction.PitStop);

            // 30 + 40 (PitStopRefuel) = 70
            Assert.AreEqual(70.0, car.CurrentFuel, "Pit stop should add 40L.");
        }

        [TestMethod]
        public void RaceManager_OutOfFuel_EndsRace()
        {
            var car = new Car("Test", 20, 10.0, 80.0);
            car.CurrentFuel = 5.0; // not enough for one turn (needs 10)
            var manager = new RaceManager(car);

            TurnResult result = manager.ProcessTurn(PlayerAction.MaintainSpeed);

            Assert.IsTrue(result.RaceOver,   "Race should be over when fuel runs out.");
            Assert.IsFalse(result.PlayerWon, "Player should not have won.");
        }

        [TestMethod]
        public void RaceManager_CompletingAllLaps_WinsRace()
        {
            // Use Tortoise V stats: speed 15, so needs 34 turns to cover 500 units
            // We'll use a faster car just to make the test quick
            var car     = new Car("SpeedTest", baseSpeed: 100, fuelConsumption: 1.0, fuelCapacity: 1000.0);
            var manager = new RaceManager(car);

            // 5 laps × 100 units = 500 units; BaseSpeed=100 so 5 turns wins
            TurnResult result = default;
            for (int i = 0; i < 5; i++)
                result = manager.ProcessTurn(PlayerAction.MaintainSpeed);

            Assert.IsTrue(result.RaceOver,  "Race should be over after all laps.");
            Assert.IsTrue(result.PlayerWon, "Player should have won.");
        }
    }
}
