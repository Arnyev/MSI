using System;
using System.Collections.Generic;
using System.Linq;

namespace MSI
{
    class Program
    {
        static void Main(string[] args)
        {
            GameRunner.RunGame(10, 10, 2, PaperSoccerHeuristics.RandomHeuristic, PaperSoccerHeuristics.RandomHeuristic);
        }
    }
}
