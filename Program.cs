using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSI
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var builder = new HeuristicBuilder<PaperSoccerState>(4);
            builder
            .Add(PaperSoccerHeuristics.ClosestYDistanceD2, 0.3)
            .Add(PaperSoccerHeuristics.MaxPathsToEnd, 0.4);
            // GameRunner.RunGame(6, 8, 2, PaperSoccerHeuristics.ParametrizedHeuristics, PaperSoccerHeuristics.ClosestYDistance);
            GameRunner.RunGame(6, 6, 2, builder.Build(), PaperSoccerHeuristics.ClosestYDistanceD3);
            Console.WriteLine("Finish");
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
