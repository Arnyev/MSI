using System;
using System.Collections.Generic;
using System.Linq;

namespace MSI
{
    class Program
    {
        static void Main(string[] args)
        {
            GameRunner.RunGame(8, 10, 2, PaperSoccerHeuristics.ClosestDistance, PaperSoccerHeuristics.ClosestYDistance);
            Console.WriteLine("Finish");
        }
    }
}
