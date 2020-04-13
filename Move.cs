using System;
using System.Linq.Expressions;

namespace CompositeVideoOscilloscope {

    public class Move {
        public double Target;
        public double Velocity;
        public double Accelaration;
        public Func<double> Get;
        public Action<double> Update;

        public static Move Create(double target, double velocity, double acceleration, Func<double> get, Action<double> set) =>
            new Move() {
                Target = target, Velocity = velocity, Accelaration = acceleration, Get = get, Update = set
            };

        public static Move Create(Controls controls, Expression<Func<Controls, double>> member, double target, double velocity, double acceleration = 0) {
            BinaryExpression assignExp = Expression.AddAssign(member.Body as MemberExpression, Expression.Parameter(typeof(double)));
            Func<double> getPos = () => member.Compile()(controls);
            Action<double> update = delta => Expression.Lambda<Action<Controls, double>>
                (assignExp, member.Parameters[0], assignExp.Right as ParameterExpression).Compile()(controls, delta);

            return Create(target, velocity, acceleration, getPos, update);
        }

        public bool Run(double elpasedTime) {
            Velocity += Accelaration * elpasedTime;
            var delta = Velocity * elpasedTime;
            var current = Get();

            if (current + delta != Target) {
                Update(delta);
            } else {
                Update(Target - current);
                Velocity = 0;
            }
            return current == Target && Math.Abs(Velocity) < 0.01;
        }
    }
}

