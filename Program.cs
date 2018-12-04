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
            var builder = new HeuristicBuilder<PaperSoccerState>(2);
            builder.Add(PaperSoccerHeuristics.ClosestDistance, 0.9)
            .Add(PaperSoccerHeuristics.ClosestYDistance, 0.8)
            .Add(PaperSoccerHeuristics.EmptyBoard, 0.3)
            .Add(PaperSoccerHeuristics.MaxMovesCount, 0.4)
            .Add(PaperSoccerHeuristics.MinOpponentMovesCount, 0.2);
            // GameRunner.RunGame(6, 8, 2, PaperSoccerHeuristics.ParametrizedHeuristics, PaperSoccerHeuristics.ClosestYDistance);
            GameRunner.RunGame(6, 8, 2, builder.Build(), PaperSoccerHeuristics.ClosestYDistance);
            Console.WriteLine("Finish");
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
