namespace AircraftApp.Models
{
    public class Helicopter : Aircraft
    {
        public override bool TakeOff()
        {
            Altitude = 500;
            OnStatusChanged("Вертолет: Успешно взлетел");
            OnTakeOffStatus(true);
            return true;
        }

        public override void Land()
        {
            Altitude = 0;
            OnStatusChanged("Вертолет: Успешно приземлился");
            OnLandStatus(true);
        }
    }
}