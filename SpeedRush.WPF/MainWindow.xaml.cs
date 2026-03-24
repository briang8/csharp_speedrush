using System;
using System.Windows;
using System.Windows.Media;      // for Brushes (label colours)
using SpeedRush.Models;          // Car, Track, RaceManager, PlayerAction, TurnResult

namespace SpeedRush.WPF
{
    // partial = this class is split across MainWindow.xaml and this file
    // : Window = inherits all WPF window behaviour (title bar, close button, etc.)
    public partial class MainWindow : Window
    {
        //  Field 
        // Stores the current race. Null until Start is clicked.
        // Every method in this class can access it.
        private RaceManager? _raceManager;

        //  Constructor  
        public MainWindow()
        {
            // Reads MainWindow.xaml and creates all the controls.
            // Must be first - without it every x:Name variable is null.
            InitializeComponent();

            // Fill the car dropdown as soon as the window opens
            LoadCars();
        }

        //  Setup 

        private void LoadCars()
        {
            // Static method — called without a RaceManager object
            var cars = RaceManager.GetAvailableCars();

            // ItemsSource = the data behind the dropdown.
            // WPF calls .ToString() on each Car to get the display text.
            CarComboBox.ItemsSource   = cars;
            CarComboBox.SelectedIndex = 0; // pre-select the first car
        }

        //  Event Handlers 
        // These methods run when the user interacts with a control.
        // Signature is always: (object sender, SomeEventArgs e)

        // Fires when the user picks a different car in the dropdown
        private void CarComboBox_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // SelectedItem comes back as "object" so we cast it to Car.
            // "as Car" = safe cast, returns null instead of crashing if it fails.
            var car = CarComboBox.SelectedItem as Car;
            if (car != null)
                CarDescText.Text = car.ToString();
        }

        // Fires when Start Race (or Restart Race) is clicked
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = CarComboBox.SelectedItem as Car;
            if (selected == null)
            {
                MessageBox.Show("Please select a car first.",
                                "No Car Selected",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Create a fresh copy of the car so every restart has a full tank.
            // We don't reuse the selected car object because it may already
            // have spent fuel from a previous race.
            var freshCar = new Car(
                selected.Name,
                selected.BaseSpeed,
                selected.FuelConsumption,
                selected.FuelCapacity
            );

            // RaceManager takes the car and creates a fresh Track internally
            _raceManager = new RaceManager(freshCar);

            // Enable the three action buttons now a race is running
            SetButtons(enabled: true);

            // Lock the dropdown so the car can't be changed mid-race
            CarComboBox.IsEnabled = false;

            StartBtn.Content = "Restart Race";

            // Clear the log and write the opening line
            LogBox.Text =
                $"Race started with {freshCar.Name}!\n" +
                $"Tank: {freshCar.CurrentFuel}L  |  Speed: {freshCar.BaseSpeed} units/turn\n" +
                "─────────────────────────────\n";

            // Push starting values to all the stat controls
            RefreshUI();
        }

        // Each button calls ProcessAction with its matching enum value.
        // The => shorthand means: "this method does exactly this one thing"
        private void SpeedUpBtn_Click(object sender, RoutedEventArgs e)
            => ProcessAction(PlayerAction.SpeedUp);

        private void MaintainBtn_Click(object sender, RoutedEventArgs e)
            => ProcessAction(PlayerAction.MaintainSpeed);

        private void PitStopBtn_Click(object sender, RoutedEventArgs e)
            => ProcessAction(PlayerAction.PitStop);

        //  Core processor  

        // Called by all three action buttons.
        // Sends the action to the game engine and updates the UI.
        private void ProcessAction(PlayerAction action)
        {
            if (_raceManager == null) return; // safety check

            // Hand the action to the game engine.
            // Get back a TurnResult struct with the outcome.
            TurnResult result = _raceManager.ProcessTurn(action);

            // Add the result message to the log
            LogBox.Text += result.Message + "\n";

            // Scroll to the bottom so the newest message is always visible
            LogBox.ScrollToEnd();

            // Update all the labels and progress bars
            RefreshUI();

            // If the race just ended, lock the buttons and show a popup
            if (result.RaceOver)
            {
                SetButtons(enabled: false);
                CarComboBox.IsEnabled = true;

                string outcome = result.PlayerWon ? "YOU WON" : "YOU LOST";

                string message = result.EndReason switch
                {
                    RaceEndReason.Finished => "Congratulations! You finished the race!",
                    RaceEndReason.OutOfTime => "Race over. You ran out of time.",
                    RaceEndReason.OutOfFuel => "Race over. You ran out of fuel.",
                    _ => result.PlayerWon
                        ? "Congratulations! You finished the race!"
                        : "Race over."
                };

                LogBox.Text += $"\n===== {outcome} =====\n{message}\n";
                LogBox.ScrollToEnd();

                MessageBox.Show(message, outcome,
                    MessageBoxButton.OK,
                    result.PlayerWon
                        ? MessageBoxImage.Information
                        : MessageBoxImage.Warning);
            }
        }

        //  UI refresh 

        // Reads the current state from _raceManager and
        // pushes every value into the matching control.
        // Called after every action and on race start.
        private void RefreshUI()
        {
            if (_raceManager == null) return;

            var car   = _raceManager.ActiveCar;
            var track = _raceManager.RaceTrack;

            // Lap label
            LapLabel.Text = $"{track.CurrentLap} / {Track.TotalLaps}";

            // Time label — turns red when under 60 seconds
            int timeLeft = _raceManager.TimeRemaining;
            TimeLabel.Text       = $"{timeLeft}s";
            TimeLabel.Foreground = timeLeft < 60
                ? Brushes.Red
                : Brushes.Black;

            // Fuel progress bar
            // ProgressBar expects 0-100 so we convert fuel to a percentage
            double fuelPct = (car.CurrentFuel / car.FuelCapacity) * 100.0;
            FuelBar.Value  = Math.Clamp(fuelPct, 0, 100);
            FuelReadout.Text = $"{car.CurrentFuel:F1} / {car.FuelCapacity}L";

            // Time progress bar — shows how much time is LEFT (counts down)
            double timePct = ((double)_raceManager.TimeRemaining / RaceManager.TotalTime) * 100.0;
            TimeBar.Value  = Math.Clamp(timePct, 0, 100);
            TimeReadout.Text = $"{_raceManager.ElapsedTime}s used";

            // ASCII lap progress bar from Track.ProgressBar()
            // e.g.  Lap 2  [=====>    ] 55%
            ProgressText.Text = $"Lap {track.CurrentLap}  {track.ProgressBar()}";
        }

        //  Helper 

        // Enables or disables all three action buttons in one call.
        private void SetButtons(bool enabled)
        {
            SpeedUpBtn.IsEnabled = enabled;
            MaintainBtn.IsEnabled = enabled;
            PitStopBtn.IsEnabled  = enabled;
        }
    }
}
