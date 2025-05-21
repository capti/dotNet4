using System;

namespace AircraftApp.Models
{
    public abstract class Aircraft
    {
        public event EventHandler<string>? StatusChanged;
        public string Type => GetType().Name;
        private double _altitude;
        public double Altitude
        {
            get => _altitude;
            protected set
            {
                _altitude = value;
                OnStatusChanged($"Altitude: {_altitude}m");
            }
        }

        public abstract bool TakeOff();
        public abstract void Land();

        protected virtual void OnTakeOffStatus(bool success) {}
        protected virtual void OnLandStatus(bool success) {}

        protected void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }
}