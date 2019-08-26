using System;
using System.Collections.Generic;

namespace CompositeVideoOscilloscope {

    public class Movements {
        readonly List<Movement> Moves = new List<Movement>();

        public void Add(double target, double velocity, double acceleration, Func<double> position, Action<double> move) {
            acceleration = position() > target ? -acceleration : acceleration;
            velocity = position() > target ? -velocity : velocity;
            Moves.Add(new Movement() { Target = target, Position=position, Move=move, Accelaration = acceleration, Velocity = velocity});
        }

        public void Run(Controls controls, double elpasedTime) {
            Moves.ForEach(m=>m.Run(elpasedTime));
            Moves.RemoveAll(m=>m.IsFinished);
        }
    } 

    class  Movement {
         public double Accelaration;
         public double  Target;
         public Func<double> Position;
         public Action<double> Move;
         public double Velocity;
         public bool IsFinished;

        public void Run(double elpasedTime) {
            Velocity += Accelaration * elpasedTime;
            var delta = Velocity * elpasedTime;
            var current = Position();

            if (current + delta < Target && Accelaration>0) {
                Move(delta);
            } else if (current + delta > Target && Accelaration<0 ) {
                Move(delta);
            } else {
                Move(Target - current);
                Velocity = -Velocity;
            }
            IsFinished = current == Target && Math.Abs(Velocity)<0.01;
        }
    }
}
