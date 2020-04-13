using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CompositeVideoOscilloscope {

    public class Movements {
        readonly Controls Controls;
        public Movements(Controls controls) {
            Controls = controls;
        }

        readonly List<Movement> Moves = new List<Movement>();

        public void Add(double target, double velocity, double acceleration, Expression<Func<Controls, double>> member) {

            BinaryExpression assignExp = Expression.AddAssign(member.Body as MemberExpression, Expression.Parameter(typeof(double)));

            Func<double> position = () => member.Compile()(Controls);

            Action<double> move = delta => Expression.Lambda<Action<Controls, double>>
                (assignExp, member.Parameters.First(), assignExp.Right as ParameterExpression).Compile()(Controls,delta);

            Add(target, velocity, acceleration, position, move);
        }

        private void Add(double target, double velocity, double acceleration, Func<double> position, Action<double> move) =>
            Moves.Add(new Movement() { Target = target, Position=position, Move=move, Accelaration = acceleration, Velocity = velocity});
        
        public void Finish() =>
            Run(elapsedTime: -1);

        public void Run(double elapsedTime) {
            Moves.ForEach(m=>m.Run(elapsedTime));
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
            if (elpasedTime == -1) {
                Move(Target);
                IsFinished = true;
            } else {
                Velocity += Accelaration * elpasedTime;
                var delta = Velocity * elpasedTime;
                var current = Position();

                if (current + delta != Target) {
                    Move(delta);
                } else {
                    Move(Target - current);
                    Velocity = -Velocity;
                }
                IsFinished = current == Target && Math.Abs(Velocity) < 0.01;
            }
        }
    }
}
