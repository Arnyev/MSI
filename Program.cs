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
            var builder = new HeuristicBuilder<PaperSoccerState>(3);
            builder
            .Add(PaperSoccerHeuristics.ClosestYDistance, 0.8)
            .Add(PaperSoccerHeuristics.MinOpponentMovesCountD4);
            GameRunner.RunGame(4, 6, 2, PaperSoccerHeuristics.ClosestYDistance, PaperSoccerHeuristics.ClosestYDistance);
            // GameRunner.RunGame(4, 6, 2, PaperSoccerHeuristics.MaxMovesCount, PaperSoccerHeuristics.EmptyBoard);

            // GameRunner.RunGame(4, 6, 3, PaperSoccerHeuristics.NoHeuristic, PaperSoccerHeuristics.NoHeuristic);


            Console.WriteLine("Finish");
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
