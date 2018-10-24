using System;
using System.Collections.Generic;
using System.Linq;

namespace MSI
{
    /// <param name="state"> Stan gry do oceny </param>
    /// <param name="depth"> Głebokość stanu w obecnym drzewie przeszukiwania. </param>
    /// <param name="playerA"> Czy gracz jest graczem A czy B. </param>
    /// <param name="utility"> Wartość obecnej pozycji. </param>
    /// <returns> Czy zakończyć dalsze przeszukiwanie poddrzewa w tym punkcie. </returns>
    public delegate bool Heuristic<TState>(TState state, int depth, bool playerA, out double utility);

    public interface IState<TAction>
    {
        bool IsFinished(out double playerUtility, bool playerA);
        void ApplyMove(TAction action);
        void ReverseLastMove();
        List<TAction> AvailableActions { get; }
    }

    public class AlphaBetaSearch<TState, TAction>
     where TState : IState<TAction> where TAction : new()
    {
        private readonly Heuristic<TState> heuristic;
        private readonly bool playerA;

        public AlphaBetaSearch(Heuristic<TState> heuristic, bool playerA)
        {
            this.heuristic = heuristic;
            this.playerA = playerA;
        }

        public TAction GetMove(TState state)
        {
            return MaxValue(state, double.NegativeInfinity, double.PositiveInfinity, 0).Action;
        }

        (double Utility, TAction Action) MaxValue(TState state, double alpha, double beta, int depth)
        {
            if (state.IsFinished(out double endUtility, playerA))
                return (endUtility, new TAction());

            if (heuristic(state, depth + 1, playerA, out double utility))
                return (utility, new TAction());

            (double Utility, TAction Action) ret = (double.NaN, new TAction());
            foreach (var action in state.AvailableActions)
            {
                state.ApplyMove(action);
                var moveValue = MinValue(state, alpha, beta, depth + 1).Utility;

                if (double.IsNaN(ret.Utility) || moveValue > ret.Utility)
                    ret = (moveValue, action);

                state.ReverseLastMove();
                if (moveValue >= beta)
                    return (moveValue, action);
                alpha = new[] { alpha, moveValue }.Max();
            }

            return ret;
        }

        (double Utility, TAction Action) MinValue(TState state, double alpha, double beta, int depth)
        {
            if (state.IsFinished(out double endUtility, playerA))
                return (endUtility, new TAction());

            if (heuristic(state, depth + 1, playerA, out double utility))
                return (utility, new TAction());

            (double Utility, TAction Action) ret = (double.NaN, new TAction());
            foreach (var action in state.AvailableActions)
            {
                state.ApplyMove(action);
                var moveValue = MaxValue(state, alpha, beta, depth + 1).Utility;

                if (double.IsNaN(ret.Utility) || moveValue < ret.Utility)
                    ret = (moveValue, action);

                state.ReverseLastMove();
                if (moveValue <= alpha)
                    return (moveValue, action);
                beta = new[] { beta, moveValue }.Min();
            }

            return ret;
        }
    }
}
