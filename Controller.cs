using System.Diagnostics;
using System.Threading.Tasks;


namespace CompositeVideoOscilloscope {

    public class Controller {
        private readonly Movements Physics;
        private readonly Stopwatch Stopwatch;
        private readonly Controls Controls = 
            new Controls() {
                 NumberOfDivisions = 10,
                 ScreenPosition =  (0,0,1,1),
                 Units =  (.005, 0.5),
                 VideoStandard = VideoStandard.Pal5MhzInterlaced,
                 TriggerVoltage = 0.6,
                 TriggerEdge = 0
            };

        public Controller() {
            Physics = new Movements();
            Physics.Add(target: 0.2, position: ()=>Controls.ScreenPosition.left, move: d => Controls.ScreenPosition.left += d );
            Physics.Add(target: 0.8, position: ()=>Controls.ScreenPosition.right, move: d => Controls.ScreenPosition.right += d );
            Physics.Add(target: 0.2, position: ()=>Controls.ScreenPosition.top, move: d => Controls.ScreenPosition.top += d );
            Physics.Add(target: 0.8, position: ()=>Controls.ScreenPosition.bottom, move: d => Controls.ScreenPosition.bottom += d );
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public async Task<Controls> Run() {
            await Task.Delay((int)(0.5 * 1000 * Controls.VideoStandard.Timing.FrameTime)).ConfigureAwait(false);
            var currentTime = Stopwatch.Elapsed.TotalSeconds;
            // var elapsedTime = Controls.VideoStandard.Timing.FrameTime; //currentTime - Controls.CurrentTime;
            var elapsedTime = currentTime - Controls.CurrentTime;
            Controls.CurrentTime = currentTime;
            Physics.Run(Controls, elapsedTime);
            return Controls;
        }
    }
}