public class Controller {
    public readonly static Controls StartupControls = new Controls().WithUnits(timePrDivision: 5, voltagePrDivision: 0.5);

    public Controls Run(Controls controls, double elapsed) => controls;
}