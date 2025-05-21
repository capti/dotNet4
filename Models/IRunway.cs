using System;

namespace AircraftApp.Models
{
    public interface IRunway
    {
        string Name { get; }
        bool IsAvailable { get; }
        void TakeOff();
        void Land();
        void EmergencyStop();
        string GetStatus();
    }
} 