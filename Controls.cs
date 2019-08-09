using System;


public struct Controls {
    public readonly double NoOfDivisions;
    public double Elapsed;
    public double Time;
    public (double Time, double Voltage) Units;

    public Controls ElapseTime(double elapsedTime) => 
        new Controls(this) { Time = Time+elapsedTime,  Elapsed = elapsedTime};

    public Controls WithUnits(double timePrDivision, double voltagePrDivision) =>
         new Controls(this) { Units = (timePrDivision, voltagePrDivision) };

    private Controls(Controls source ) { this = source; NoOfDivisions = 8; }

}