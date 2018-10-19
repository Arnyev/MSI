using System;
using System.Collections.Generic;
using System.Linq;

namespace MSI
{
    public delegate bool Heuristic<TState>(TState state, int depth, out double utility);
    public interface IGame<TState, TAction> where TState : IState<TAction>
    {
        TState Start { get; }
    }

    public interface IState<TAction>
    {
        bool IsFinished(out double playerAUtility);
        void ApplyMove(TAction action);
        void ReverseLastMove();
        List<TAction> AvailableActions { get; }
    }

    public class AlphaBetaSearch<TGame, TState, TAction>
     where TState : IState<TAction> where TGame : IGame<TState, TAction> where TAction : new()
    {
        private readonly TGame game;
        private readonly Heuristic<TState> heuristic;

        public AlphaBetaSearch(TGame game, Heuristic<TState> heuristic)
        {
            this.game = game;
            this.heuristic = heuristic;
        }

        public TAction GetMove(TState state)
        {
            return MaxValue(state, double.NegativeInfinity, double.PositiveInfinity, 0).Action;
        }

        (double Utility, TAction Action) MaxValue(TState state, double alpha, double beta, int depth)
        {
            if (state.IsFinished(out double endUtility))
                return (endUtility, new TAction());

            if (heuristic(state, depth + 1, out double utility))
                return (utility, new TAction());

            foreach (var action in state.AvailableActions)
            {
                state.ApplyMove(action);
                var moveValue = MinValue(state, alpha, beta, depth + 1).Utility;
                state.ReverseLastMove();
                if (moveValue >= beta)
                    return (moveValue, action);
                alpha = new[] { alpha, moveValue }.Max();
            }
            throw new Exception("Błędna konfiguracja gry, gra się nie zakończyła a nie ma dostępnych ruchów.");
        }

        (double Utility, TAction Action) MinValue(TState state, double alpha, double beta, int depth)
        {
            if (state.IsFinished(out double endUtility))
                return (endUtility, new TAction());

            if (heuristic(state, depth + 1, out double utility))
                return (utility, new TAction());

            foreach (var action in state.AvailableActions)
            {
                state.ApplyMove(action);
                var moveValue = MaxValue(state, alpha, beta, depth + 1).Utility;
                state.ReverseLastMove();
                if (moveValue <= alpha)
                    return (moveValue, action);
                beta = new[] { beta, moveValue }.Min();
            }
            throw new Exception("Błędna konfiguracja gry, gra się nie zakończyła a nie ma dostępnych ruchów.");
        }
    }
}
