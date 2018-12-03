﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;

namespace MSI
{
    public class PaperSoccerState : IState<PaperSoccerAction>
    {
        public readonly bool[,] Horizontals;
        public readonly bool[,] Verticals;
        public readonly bool[,] DiagonalsNE;
        public readonly bool[,] DiagonalsNW;
        public Point Position { get; private set; }
        private readonly List<PaperSoccerAction> takenActions;
        private bool IsInGoal => IsEndPosition(Position);

        public int Width { get; }
        public int Height { get; }
        public int GoalWidth { get; }

        public PaperSoccerState(int width, int height, int goalWidth)
        {
            Width = width;
            Height = height;
            GoalWidth = goalWidth;

            if (goalWidth % 2 != width % 2)
                throw new ArgumentException("Niesymetryczna plansza!");
            if (goalWidth > width)
                throw new ArgumentException("Za duży gol!");
            if (goalWidth < 1 || height < 1)
                throw new ArgumentException("Za mała plansza!");

            takenActions = new List<PaperSoccerAction>();
            int arrayWidth = width + 3;
            int arrayHeight = height + 6;// Jest konieczny padding żeby sprawdzanie warunków nie wychodziło poza tablicę.

            Horizontals = new bool[arrayWidth, arrayHeight];
            Verticals = new bool[arrayWidth, arrayHeight];
            DiagonalsNE = new bool[arrayWidth, arrayHeight];
            DiagonalsNW = new bool[arrayWidth, arrayHeight];

            InitializeBounds(width, height, goalWidth, true);
            Position = new Point(width / 2 + 1, height / 2 + 3);
            SaveBoardImage("p.png");
        }

        private void InitializeBounds(int width, int height, int goalWidth, bool value)
        {
            //Obramowanie golu
            var goalStart = (width - goalWidth) / 2 + 1;
            for (int i = goalStart - 1; i <= goalStart + goalWidth; i++)
            {
                Horizontals[i, 1] = value;
                Verticals[i, 0] = value;
                DiagonalsNE[i, 0] = value;
                DiagonalsNW[i + 1, 0] = value;
                Horizontals[i, height + 5] = value;
                Verticals[i, height + 5] = value;
                DiagonalsNE[i, height + 5] = value;
                DiagonalsNW[i + 1, height + 5] = value;
            }

            Verticals[goalStart, 1] = value;
            DiagonalsNW[goalStart, 1] = value;
            Verticals[goalStart, 2] = value;
            Horizontals[goalStart - 1, 2] = value;
            DiagonalsNE[goalStart - 1, 1] = value;

            Verticals[goalStart + goalWidth, 1] = value;
            DiagonalsNE[goalStart + goalWidth, 1] = value;
            Verticals[goalStart + goalWidth, 2] = value;
            Horizontals[goalStart + goalWidth, 2] = value;
            DiagonalsNW[goalStart + goalWidth + 1, 1] = value;

            Verticals[goalStart, height + 3] = value;
            Verticals[goalStart, height + 4] = value;
            DiagonalsNW[goalStart, height + 4] = value;
            Horizontals[goalStart - 1, height + 4] = value;
            DiagonalsNE[goalStart - 1, height + 4] = value;

            Verticals[goalStart + goalWidth, height + 3] = value;
            Verticals[goalStart + goalWidth, height + 4] = value;
            Horizontals[goalStart + goalWidth, height + 4] = value;
            DiagonalsNE[goalStart + goalWidth, height + 4] = value;
            DiagonalsNW[goalStart + goalWidth + 1, height + 4] = value;

            //Reszta ścianek
            for (int i = 2; i < height + 5; i++)
            {
                DiagonalsNW[1, i] = value;
                Verticals[1, i] = value;
                DiagonalsNE[0, i - 1] = value;
                Horizontals[0, i] = value;
                DiagonalsNW[width + 2, i - 1] = value;
                Verticals[width + 1, i] = value;
                DiagonalsNE[width + 1, i] = value;
                Horizontals[width + 1, i] = value;
            }

            for (int i = 1; i < width + 1; i++)
            {
                if (i >= goalStart && i < goalStart + goalWidth)
                    continue;
                Horizontals[i, 3] = value;
                DiagonalsNE[i, 2] = value;
                Verticals[i, 2] = value;
                DiagonalsNW[i + 1, 2] = value;
                Horizontals[i, height + 3] = value;
                DiagonalsNE[i, height + 3] = value;
                Verticals[i, height + 3] = value;
                DiagonalsNW[i + 1, height + 3] = value;
            }
        }

        public bool IsFinished(out double playerUtility, bool northPlayer)
        {
            playerUtility = 0;

            if (IsInGoal)
            {
                playerUtility = northPlayer && Position.Y > 2 || (!northPlayer && Position.Y < 2) ?
                    double.PositiveInfinity : double.NegativeInfinity;
                return true;
            }

            if (AvailableActions.Count == 0)
            {
                playerUtility = northPlayer ? double.NegativeInfinity : double.PositiveInfinity;
                return true;
            }

            return false;
        }

        public List<PaperSoccerAction> AvailableActions
        {
            get
            {
                var actions = new List<PaperSoccerAction>();
                var analysisStack = new Stack<AnalysedPointDirection>();
                var analyzedDirections = new List<Direction>();

                FirstStepFindingActions(Position, actions, analysisStack);

                while (analysisStack.Count != 0)
                {
                    var point = analysisStack.Peek();
                    var lastPosition = LastPosition(point.Point, point.Direction);

                    if (point.Analysed) // odpowiada wyjściu z rekurencji
                    {
                        UpdateBlockedMoves(lastPosition, point.Direction, false); // Cofnięcie ruchu skoro cofamy się z rekurencji
                        analysisStack.Pop();
                        analyzedDirections.RemoveAt(analyzedDirections.Count - 1);
                    }
                    else
                    {
                        UpdateBlockedMoves(lastPosition, point.Direction, true); // Dopiero w tym momencie uznajemy że ruch został wykonany
                        var directions = AvailableDirections(point.Point);

                        foreach (var direction in directions)
                        {
                            var newPosition = NewPosition(point.Point, direction);
                            var isBounce = IsBounce(newPosition);
                            var endPosition = IsEndPosition(newPosition);
                            if (!isBounce || endPosition)
                                AddFinishedAction(analyzedDirections, point, direction, actions); // Doszliśmy do końca odbić
                            else
                                analysisStack.Push(new AnalysedPointDirection(newPosition, direction)); // Przygotowuje nowe zejście rekurencyjne
                        }

                        point.Analysed = true;
                        analyzedDirections.Add(point.Direction); // Skoro zeszliśmy dalej trzeba zapamiętać dotychczasową ścieżkę
                    }
                }

                return actions;
            }
        }

        private void FirstStepFindingActions(Point startingPosition, List<PaperSoccerAction> actions, Stack<AnalysedPointDirection> stack)
        {
            var startingDirections = AvailableDirections(startingPosition);

            foreach (var direction in startingDirections)
            {
                var newP = NewPosition(startingPosition, direction);
                var isBounce = IsBounce(newP);
                var endPosition = IsEndPosition(newP);
                if (!isBounce || endPosition)
                    actions.Add(new PaperSoccerAction(new[] {direction}));
                else
                    stack.Push(new AnalysedPointDirection(newP, direction));
            }
        }

        private static void AddFinishedAction(List<Direction> stackAnalyzedDirections, AnalysedPointDirection point, Direction direction,
            List<PaperSoccerAction> actions)
        {
            var action = new Direction[stackAnalyzedDirections.Count + 2];
            stackAnalyzedDirections.CopyTo(action, 0);
            action[action.Length - 2] = point.Direction;
            action[action.Length - 1] = direction;
            actions.Add(new PaperSoccerAction(action));
        }

        private bool IsEndPosition(Point p) => p.Y == 2 || p.Y == Horizontals.GetLength(1) - 2;

        private bool IsBounce(Point p)
        {
            return Verticals[p.X, p.Y] || DiagonalsNE[p.X, p.Y] || Horizontals[p.X, p.Y] || DiagonalsNW[p.X + 1, p.Y - 1] ||
                Verticals[p.X, p.Y - 1] || DiagonalsNE[p.X - 1, p.Y - 1] || Horizontals[p.X - 1, p.Y] || DiagonalsNW[p.X, p.Y];
        }

        private static Point NewPosition(Point position, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Point(position.X, position.Y + 1);
                case Direction.NorthEast:
                    return new Point(position.X + 1, position.Y + 1);
                case Direction.East:
                    return new Point(position.X + 1, position.Y);
                case Direction.SouthEast:
                    return new Point(position.X + 1, position.Y - 1);
                case Direction.South:
                    return new Point(position.X, position.Y - 1);
                case Direction.SouthWest:
                    return new Point(position.X - 1, position.Y - 1);
                case Direction.West:
                    return new Point(position.X - 1, position.Y);
                case Direction.NorthWest:
                    return new Point(position.X - 1, position.Y + 1);
                default:
                    throw new ArgumentException("Brak wybranego kierunku");
            }
        }

        private static Point LastPosition(Point position, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Point(position.X, position.Y - 1);
                case Direction.NorthEast:
                    return new Point(position.X - 1, position.Y - 1);
                case Direction.East:
                    return new Point(position.X - 1, position.Y);
                case Direction.SouthEast:
                    return new Point(position.X - 1, position.Y + 1);
                case Direction.South:
                    return new Point(position.X, position.Y + 1);
                case Direction.SouthWest:
                    return new Point(position.X + 1, position.Y + 1);
                case Direction.West:
                    return new Point(position.X + 1, position.Y);
                case Direction.NorthWest:
                    return new Point(position.X + 1, position.Y - 1);
                default:
                    throw new ArgumentException("Brak wybranego kierunku");
            }
        }

        private List<Direction> AvailableDirections(Point p)
        {
            var result = new List<Direction>(8);

            if (!Verticals[p.X, p.Y])
                result.Add(Direction.North);
            if (!DiagonalsNE[p.X, p.Y])
                result.Add(Direction.NorthEast);
            if (!Horizontals[p.X, p.Y])
                result.Add(Direction.East);
            if (!DiagonalsNW[p.X + 1, p.Y - 1])
                result.Add(Direction.SouthEast);
            if (!Verticals[p.X, p.Y - 1])
                result.Add(Direction.South);
            if (!DiagonalsNE[p.X - 1, p.Y - 1])
                result.Add(Direction.SouthWest);
            if (!Horizontals[p.X - 1, p.Y])
                result.Add(Direction.West);
            if (!DiagonalsNW[p.X, p.Y])
                result.Add(Direction.NorthWest);

            return result;
        }

        private void UpdateBlockedMoves(Point p, Direction direction, bool value)
        {
            switch (direction)
            {
                case Direction.North:
                    Verticals[p.X, p.Y] = value;
                    return;
                case Direction.NorthEast:
                    DiagonalsNE[p.X, p.Y] = value;
                    return;
                case Direction.East:
                    Horizontals[p.X, p.Y] = value;
                    return;
                case Direction.SouthEast:
                    DiagonalsNW[p.X + 1, p.Y - 1] = value;
                    return;
                case Direction.South:
                    Verticals[p.X, p.Y - 1] = value;
                    return;
                case Direction.SouthWest:
                    DiagonalsNE[p.X - 1, p.Y - 1] = value;
                    return;
                case Direction.West:
                    Horizontals[p.X - 1, p.Y] = value;
                    return;
                case Direction.NorthWest:
                    DiagonalsNW[p.X, p.Y] = value;
                    return;
            }
        }

        public void ReverseLastMove()
        {
            var action = takenActions[takenActions.Count - 1];
            var directions = action.Directions.ToArray();
            Array.Reverse(directions);
            foreach (var direction in directions)
            {
                Position = LastPosition(Position, direction);
                UpdateBlockedMoves(Position, direction, false);
            }
            takenActions.RemoveAt(takenActions.Count - 1);
        }

        public void ApplyMove(PaperSoccerAction action)
        {
            foreach (var direction in action.Directions)
            {
                UpdateBlockedMoves(Position, direction, true);
                Position = NewPosition(Position, direction);
            }
            takenActions.Add(action);
        }

        public void SaveBoardImage(string filename)
        {
            var width = Verticals.GetLength(0);
            var height = Verticals.GetLength(1);

            var image = new Bitmap(width * 50, height * 50);
            var actions = takenActions.ToList();
            var pen = Pens.Blue;
            using (var g = Graphics.FromImage(image))
            {
                g.DrawEllipse(new Pen(Color.Green, 10), Position.X * 50 - 10, Position.Y * 50 - 10, 20, 20);

                while (takenActions.Count > 0)
                {
                    DrawLines(width, height, pen, g);
                    ReverseLastMove();
                    pen = pen == Pens.Blue ? Pens.Red : Pens.Blue;
                }
                DrawLines(width, height, Pens.Black, g);
            }
            image.RotateFlip(RotateFlipType.Rotate180FlipX);
            image.Save(filename, ImageFormat.Png);
            actions.ForEach(ApplyMove);
        }

        private void DrawLines(int width, int height, Pen pen, Graphics g)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (Verticals[i, j])
                        g.DrawLine(pen, new Point(i * 50, j * 50), new Point(i * 50, j * 50 + 50));
                    if (Horizontals[i, j])
                        g.DrawLine(pen, new Point(i * 50, j * 50), new Point(i * 50 + 50, j * 50));
                    if (DiagonalsNE[i, j])
                        g.DrawLine(pen, new Point(i * 50, j * 50), new Point(i * 50 + 50, j * 50 + 50));
                    if (DiagonalsNW[i, j])
                        g.DrawLine(pen, new Point(i * 50, j * 50), new Point(i * 50 - 50, j * 50 + 50));
                }
        }
    }

    /// <summary>
    /// Klasa mająca za zadanie zasymulować jeden węzeł przetwarzanie rekurencji.
    /// </summary>
    public class AnalysedPointDirection
    {
        public readonly Point Point;
        public bool Analysed;
        public readonly Direction Direction;

        public AnalysedPointDirection(Point point, Direction direction)
        {
            Point = point;
            Direction = direction;
        }
    }

    public class PaperSoccerAction
    {
        public readonly Direction[] Directions;
        public PaperSoccerAction(Direction[] directions)
        {
            Directions = directions;
        }
        public PaperSoccerAction()
        {
            Directions = new Direction[0];
        }
    }

    public enum Direction : byte
    {
        None = 0,
        North = 1,
        NorthEast = 2,
        East = 3,
        SouthEast = 4,
        South = 5,
        SouthWest = 6,
        West = 7,
        NorthWest = 8
    }
}
