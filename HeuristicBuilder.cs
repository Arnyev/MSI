using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MSI
{
    public class HeuristicBuilder<TState>
    {
        private const int defaultDepth = 2;
        private int _depth;
        private bool _applyOrigianlDepth;
        List<(Heuristic<TState> heuristic, double weight)> parametrizedHeuristics;
        public HeuristicBuilder() : this(defaultDepth)
        {
        }
        public HeuristicBuilder(int depth)
        {
            _applyOrigianlDepth = false;
            _depth = depth;
            parametrizedHeuristics = new List<(Heuristic<TState>, double)>();
        }


        public HeuristicBuilder<TState> Add(Heuristic<TState> heuristic, double weight = 1.0)
        {
            if (weight != 0)
                parametrizedHeuristics.Add((heuristic, weight));
            return this;
        }

        public HeuristicBuilder<TState> ApplyOrigianlDepth(bool value = true)
        {
            _applyOrigianlDepth = value;
            return this;
        }

        public Heuristic<TState> Build()
        {
            if (_applyOrigianlDepth)
                return delegate (TState state, int depth, bool playerA, out double utility)
                {
                    var allFinished = true;

                    double sum = 0;
                    parametrizedHeuristics.ForEach(x =>
                    {
                        allFinished &= x.heuristic(state, depth, playerA, out double tmpUtil);
                        sum += tmpUtil * x.weight;
                    });
                    utility = sum;

                    return allFinished;
                };
            else
                return delegate (TState state, int depth, bool playerA, out double utility)
                {
                    double sum = 0;
                    parametrizedHeuristics.ForEach(x =>
                    {
                        x.heuristic(state, depth, playerA, out double tmpUtil);
                        sum += tmpUtil * x.weight;
                    });
                    utility = sum;

                    return depth == _depth;
                };

        }
    }
}
