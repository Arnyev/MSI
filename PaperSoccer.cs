using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MSI
{
    public class PaperSoccer : IGame<PaperSoccerState, PaperSoccerAction>
    {
        public PaperSoccerState Start => throw new System.NotImplementedException();
    }

    public class PaperSoccerState : IState<PaperSoccerAction>
    {
        public int Width => Horizontals.GetLength(0);
        public int Height => Horizontals.GetLength(1);
        public readonly int GoalWidth;
        public readonly bool[,] Horizontals;
        public readonly bool[,] Verticals;
        public readonly bool[,] DiagonalsNE;
        public readonly bool[,] DiagonalsNW;
        public readonly Point Position;
        private readonly List<PaperSoccerAction> takenActions;
        public bool IsInGoal => Position.Y == 0 || Position.Y == Height - 1;
        private int GoalLeftBound => (Width - GoalWidth) / 2;
        private int GoalRightBound => (Width + GoalWidth) / 2;

        public PaperSoccerState(int width, int height, int goalWidth)
        {
            takenActions = new List<PaperSoccerAction>();
            GoalWidth = goalWidth;
            //TODO dodać inicjalizację
            //Moim zdaniem najwygodniej będzie obramowanie planszy i zasady w stylu nie wolno poruszać się przy ścianie zrobić przez inicjalizację tablic z wartościami true.
            //Tym sposobem nie trzeba robić tyle ifów i nie trzeba się bać wyjścia poza planszę
        }

        public bool IsFinished(out double playerAUtility)
        {
            playerAUtility = 0;//TODO
            return IsInGoal || AvailableActions.Count == 0;
        }

        public List<PaperSoccerAction> AvailableActions =>
            ActionsFromPoint(Position).Select(x => new PaperSoccerAction(x.ToArray())).ToList();

        private List<List<Direction>> ActionsFromPoint(Point position)
        {
            var actions = new List<List<Direction>>();
            var directions = AvailableDirections(position);
            foreach (var direction in directions)
            {
                var newP = NewPosition(Position, direction);
                var isBounce = Horizontals[newP.X, newP.Y] || Verticals[newP.X, newP.Y] || DiagonalsNE[newP.X, newP.Y] || DiagonalsNW[newP.X, newP.Y];
                if (!isBounce)
                    actions.Add(new List<Direction> { direction });
                else
                {
                    var actionsFromNext = ActionsFromPoint(newP);
                    actionsFromNext.ForEach(x => x.Insert(0, direction));
                    actions.AddRange(actionsFromNext);
                }
            }
            return actions;
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
                    return new Point(position.X - 1);
                case Direction.NorthWest:
                    return new Point(position.X - 1, position.Y + 1);
                default:
                    throw new ArgumentException("Brak wybranego kierunku");
            }
        }

        private List<Direction> AvailableDirections(Point p)
        {
            var result = new List<Direction>();

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
        public void ReverseLastMove()
        {
            throw new System.NotImplementedException();
        }

        public void ApplyMove(PaperSoccerAction action)
        {
            throw new System.NotImplementedException();
        }

        public void Draw()
        {
            throw new System.NotImplementedException();
            //Trzeba dodać jakoś pokazywanie planszy, wydaje mi się że wygodne może być używanie klas Image, Graphics itp. a potem zapisać do png. 
        }
    }

    public static class PaperSoccerHeuristics
    {
        public static bool ClosestDistance(PaperSoccerState state, int depth, out double util)
        {
            throw new System.NotImplementedException();
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
