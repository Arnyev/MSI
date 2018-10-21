using System;

namespace MSI
{
    public static class PaperSoccerHeuristics
    {
        public static bool ClosestDistance(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            throw new System.NotImplementedException();
        }

        public static bool NoHeuristic(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = 0;
            return false;
        }

        public static bool RandomHeuristic(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = new Random().NextDouble();
            return depth == 2;
        }
    }
}
