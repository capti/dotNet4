using System;

namespace AircraftApp.Models
{
    public class Runway : IRunway
    {
        public string Name { get; private set; }
        public bool IsAvailable { get; private set; }

        public Runway(string name)
        {
            Name = name;
            IsAvailable = true;
        }

        public void TakeOff()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Runway is not available for takeoff");
            
            IsAvailable = false;
            // Simulate takeoff process
            System.Threading.Thread.Sleep(2000);
            IsAvailable = true;
        }

        public void Land()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Runway is not available for landing");
            
            IsAvailable = false;
            // Simulate landing process
            System.Threading.Thread.Sleep(2000);
            IsAvailable = true;
        }

        public void EmergencyStop()
        {
            IsAvailable = false;
            // Simulate emergency stop
            System.Threading.Thread.Sleep(1000);
            IsAvailable = true;
        }

        public string GetStatus()
        {
            return $"Runway {Name} is {(IsAvailable ? "available" : "in use")}";
        }
    }
} 