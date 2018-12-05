using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MSI
{
    public static class PaperSoccerHeuristics
    {
        // H_1
        public static bool ClosestDistance(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var distVector = GetVectorToClosestGoal(state, playerNorth);
            var maxDist = Math.Sqrt(state.Width * state.Width + state.Height * state.Height);

            util = 1 + -Math.Sqrt(distVector.X * distVector.X + distVector.Y * distVector.Y) / maxDist;
            return depth == 2;
        }

        // H_2

        public static bool ClosestYDistanceD3(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var distVector = GetVectorToClosestGoal(state, playerNorth);
            var maxDist = state.Width * (state.Height + 1.0);
            util = 1 + -(state.Width * distVector.Y + distVector.X) / maxDist;

            return depth == 3;
        }

        public static bool ClosestYDistanceD4(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var distVector = GetVectorToClosestGoal(state, playerNorth);
            var maxDist = state.Width * (state.Height + 1.0);
            util = 1 + -(state.Width * distVector.Y + distVector.X) / maxDist;

            return depth == 4;
        }

        public static bool ClosestYDistanceD5(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var distVector = GetVectorToClosestGoal(state, playerNorth);
            var maxDist = state.Width * (state.Height + 1.0);
            util = 1 + -(state.Width * distVector.Y + distVector.X) / maxDist;

            return depth == 4;
        }

        public static bool ClosestYDistanceD2(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var distVector = GetVectorToClosestGoal(state, playerNorth);
            var maxDist = state.Width * (state.Height + 1.0);
            util = 1 + -(state.Width * distVector.Y + distVector.X) / maxDist;

            return depth == 2;
        }

        // H_3
        public static bool EmptyBoard(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            var distVector = GetVectorToClosestGoal(state, playerNorth);
            util = 1 + (-distVector.X - distVector.Y) / (state.Width + (double)state.Height);
            return depth == 3;
        }


        // H_4
        public static bool MaxMovesCount(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = 1 - Math.Exp(-Math.Sqrt(state.AvailableActions.Count / (state.takenActions.Count * state.takenActions.Count + 1.0)));
            if (util < 0)
                util = 0;
            if (util > 1)
                util = 1;
            return depth == 3;
        }

        // H_5
        public static bool MinOpponentMovesCount(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = Math.Exp(-Math.Sqrt(state.AvailableActions.Count / (state.takenActions.Count * state.takenActions.Count + 1.0)));
            if (util < 0)
                util = 0;
            if (util > 1)
                util = 1;
            return depth == 2;
        }

        public static bool MinOpponentMovesCountD4(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = Math.Exp(-Math.Sqrt(state.AvailableActions.Count / (state.takenActions.Count * state.takenActions.Count + 1.0)));
            if (util < 0)
                util = 0;
            if (util > 1)
                util = 1;
            return depth == 4;
        }

        public static bool MaxPathsToEnd(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            if (depth != 2)
            {
                util = 0;
                return false;
            }

            var achievableVertices = AchievableVertices(state);
            var adjMatrixArray = CreateAdjacencyMatrix(state, achievableVertices);

            var distVector = GetVectorToClosestGoal(state, playerNorth);
            int dist = (int)(3 * (distVector.X + distVector.Y));

            Matrix<double> A = DenseMatrix.OfArray(adjMatrixArray);
            Matrix<double> expo = DenseMatrix.OfArray(adjMatrixArray);
            for (int i = 0; i < dist; i++)
                expo *= A;

            var endPositionY = playerNorth ? (state.Horizontals.GetLength(1) - 2) : 2;
            var endPositionX = state.Horizontals.GetLength(0) / 2;
            var pointIndex = achievableVertices.IndexOf(new Point(endPositionX, endPositionY));
            if (pointIndex == -1)
                util = 0;

            var paths = expo[endPositionX, endPositionY];
            var root = Math.Pow(paths, 1.0 / dist);
            // util = root / (8 * Math.Sqrt(dist));
            util = root / 8;

            return true;
        }

        public static List<Point> AchievableVertices(PaperSoccerState state)
        {
            var vertices = new Queue<Point>();
            var alreadyAnalysed = new bool[state.Horizontals.GetLength(0), state.Horizontals.GetLength(1)];
            vertices.Enqueue(state.Position);
            while (vertices.Count != 0)
            {
                var p = vertices.Dequeue();
                var nexts = state.AvailableDirections(p);

                foreach (var dir in nexts)
                {
                    var nextP = PaperSoccerState.NewPosition(p, dir);
                    if (!alreadyAnalysed[nextP.X, nextP.Y])
                        vertices.Enqueue(nextP);
                }

                alreadyAnalysed[p.X, p.Y] = true;
            }

            var result = new List<Point>();

            for (int i = 0; i < alreadyAnalysed.GetLength(0); i++)
                for (int j = 0; j < alreadyAnalysed.GetLength(1); j++)
                    if (alreadyAnalysed[i, j])
                        result.Add(new Point(i, j));

            return result;
        }

        public static double[,] CreateAdjacencyMatrix(PaperSoccerState state, List<Point> achievableVertices)
        {
            var dictPointToIndex = achievableVertices.Select((x, i) => (Point: x, Index: i)).ToDictionary(t => t.Point, t => t.Index);
            var adjacency = new double[achievableVertices.Count, achievableVertices.Count];
            foreach (var p in achievableVertices)
            {
                var vertexIndex = dictPointToIndex[p];
                var dirs = state.AvailableDirections(p);
                foreach (var dir in dirs)
                {
                    var nextP = PaperSoccerState.NewPosition(p, dir);
                    adjacency[vertexIndex, dictPointToIndex[nextP]] = 1;
                }
            }

            return adjacency;
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
            List<(Heuristic<PaperSoccerState> heuristic, float weight)> parametrizedHeuristics
                = new List<(Heuristic<PaperSoccerState>, float)>()
            {
                (ClosestDistance, 0.9f),
                (ClosestYDistanceD2, 0.8f),
                (EmptyBoard, 0.3f),
                (MaxMovesCount, 0.4f),
                (MinOpponentMovesCount, 0.2f)
            };

            double sum = 0;
            parametrizedHeuristics.ForEach(x =>
            {
                x.heuristic(state, depth, playerNorth, out double tmpUtil);
                sum += tmpUtil * x.weight;
            });
            util = sum;

            return depth == 2;
        }

        public static bool NoHeuristic(PaperSoccerState state, int depth, bool playerNorth, out double util)
        {
            util = 0;
            return depth == 4;
        }


        private static Point GetVectorToClosestGoal(PaperSoccerState state, bool playerNorth)
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

            return new Point(xDist, yDist);
        }
    }
}
