using System;
using System.Collections.Generic;
using System.Linq;

namespace MSI
{
    public static class PaperSoccerHeuristics
    {
        // H_1
        public static bool ClosestDistance(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            int goalPositionX = state.Width / 2 + 1;
            int goalMargin = state.GoalWidth / 2 - 1;
            int closestGoalPositionX =
                Math.Clamp(state.Position.X, goalPositionX - goalMargin, goalPositionX + goalMargin);

            int xDist = state.Position.X - closestGoalPositionX;
            int yDist = 0;

            if (playerNorth)
                yDist = state.Position.Y - 2;
            else
                yDist = state.Position.Y - (state.Height + 4);

            util = -Math.Sqrt(xDist * xDist + yDist * yDist);

            return depth == 2;
        }

        // H_2
        public static bool ClosestYDistance(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            int goalPositionX = state.Width / 2 + 1;
            int goalMargin = state.GoalWidth / 2 - 1;
            int closestGoalPositionX =
                Math.Clamp(state.Position.X, goalPositionX - goalMargin, goalPositionX + goalMargin);

            int xDist = Math.Abs(state.Position.X - closestGoalPositionX);
            int yDist = 0;

            if (playerNorth)
                yDist = state.Position.Y - 2;
            else
                yDist = (state.Height + 4) - state.Position.Y;

            util = -(state.Width * yDist + xDist);

            return depth == 2;
        }

        // H_3
        public static bool EmptyBoard(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = 0;
            return depth == 2;
        }

        // H_4
        public static bool MaxMovesCount(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var availableActions = state.AvailableActions;
            util = availableActions.Count;

            return depth == 2;
        }

        // H_5
        public static bool MinOpponentMovesCount(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var availableActions = state.AvailableActions;
            int opponentMovesCount = int.MaxValue;

            foreach (var action in availableActions)
            {
                state.ApplyMove(action);
                opponentMovesCount = Math.Min(opponentMovesCount, state.AvailableActions.Count);
                state.ReverseLastMove();
            }
            util = -opponentMovesCount;

            return depth == 2;
        }

        // H_6
        public static bool RandomHeuristic(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = new Random().NextDouble();
            return depth == 2;
        }

        // H_7
        public static bool ParametrizedHeuristics(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            List<(Heuristic<PaperSoccerState> h, float p)> parametrizedHeuristics
                = new List<(Heuristic<PaperSoccerState>, float)>()
            {
                (ClosestDistance, 0.9f),
                (ClosestYDistance, 0.8f),
                (EmptyBoard, 0),
                (MaxMovesCount, 0.4f),
                (MinOpponentMovesCount, 0.2f),
                (RandomHeuristic, 0.2f)
            };

            double sum = 0;
            parametrizedHeuristics.ForEach(x =>
            {
                x.h(state, depth, playerNorth, out double tmpUtil);
                sum += tmpUtil * x.p;
            });
            util = sum;

            return depth == 2;
        }

        public static bool NoHeuristic(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = 0;
            return depth == 2;
        }
    }
}
