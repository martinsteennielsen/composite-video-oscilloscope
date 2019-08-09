using System;


public struct Controls {
    public readonly double NumberOfDivisions;
    public (double Time, double Voltage) Units;

    public Controls WithUnits(double timePrDivision, double voltagePrDivision) =>
         new Controls(this) { Units = (timePrDivision, voltagePrDivision) };

    private Controls(Controls source) { this = source; NumberOfDivisions = 8; }

}