﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompositeVideoOscilloscope {
    class Program {

        static void Main(string[] args) {
            var timing = new PalTiming();
            //var timing = new PalTiming(dotSize: 1, framesPrSec: 20);

            using (var output = new Output(address: "tcp://*:10001")) {
                var oscilloscope = new Oscilloscope(timing, output);
                var canceller = new CancellationTokenSource();
                Task.Run(() => oscilloscope.Run(canceller.Token));
                Console.WriteLine("Press enter to terminate");
                Console.ReadLine();
                canceller.Cancel();
            }
        }
    }
}