namespace SpeedRush.Models
{
    /// <summary>
    /// Represents a racing car with its stats and current fuel level.
    /// </summary>
    public class Car
    {
       
        /// <summary>Display name of the car.</summary>
    
        // Properties
       
        public string Name { get; set; }


        /// <summary>Distance units covered per normal turn.</summary>
        
        public int BaseSpeed { get; set; }


        /// <summary>Fuel burned per normal turn.</summary>
        
        public double FuelConsumption { get; set; }

        /// <summary>Maximum fuel the tank can hold.</summary>
        
        public double FuelCapacity { get; set; }

        /// <summary>Current fuel in the tank. Starts full.</summary>
        
        public double CurrentFuel { get; set; }

        // Constructor

        public Car(string name, int baseSpeed, double fuelConsumption, double fuelCapacity)
        {
            Name            = name;
            BaseSpeed       = baseSpeed;
            FuelConsumption = fuelConsumption;
            FuelCapacity    = fuelCapacity;
            CurrentFuel     = fuelCapacity;   // start with a full tank
        }

        //  Methods 

        /// <summary>
        /// Burns fuel for one turn.
        /// multiplier 1.0 = normal, 1.5 = speed up (costs more fuel).
        /// Throws InvalidOperationException if the tank is already empty.
        /// </summary>
        public void BurnFuel(double multiplier = 1.0)
        {
            // Exception handling: refuse to burn from an empty tank
            if (CurrentFuel <= 0)
                throw new InvalidOperationException("Out of fuel! The race has ended.");

            CurrentFuel -= FuelConsumption * multiplier;

           
            if (CurrentFuel < 0)
                CurrentFuel = 0;
        }

        /// <summary>
        /// Adds fuel during a pit stop.
        /// Throws ArgumentException if the amount would overflow the tank.
        /// </summary>
        public void Refuel(double amount)
        {
            if (CurrentFuel + amount > FuelCapacity)
                throw new ArgumentException(
                    $"Cannot overfill! Tank holds {FuelCapacity}L, " +
                    $"currently {CurrentFuel:F1}L.");

            CurrentFuel += amount;
        }

        /// <summary>
        /// Short summary used when displaying car info in the UI.
        /// WPF ComboBox and our console menu both call this automatically.
        /// </summary>
        public override string ToString() =>
            $"{Name} | Speed: {BaseSpeed}/turn | Fuel use: {FuelConsumption}L/turn | Tank: {FuelCapacity}L";
    }
}
