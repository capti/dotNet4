namespace AircraftApp.Models
{
    public class Airplane : Aircraft
    {
        public double RunwayLength { get; }

        public Airplane(double runwayLength)
        {
            RunwayLength = runwayLength;
        }

        public override bool TakeOff()
        {
            if (RunwayLength >= 1500)
            {
                Altitude = 10000;
                OnStatusChanged("Самолет: Успешно взлетел");
                OnTakeOffStatus(true);
                return true;
            }
            OnStatusChanged("Самолет: Не смог взлететь (малая длина ВПП)");
            OnTakeOffStatus(false);
            return false;
        }

        public override void Land()
        {
            Altitude = 0;
            OnStatusChanged("Самолет: Успешно приземлился");
            OnLandStatus(true);
        }
    }
}