using System;
using System.Collections.Generic;
using System.Linq;

namespace MSI
{
    public class GameRunner
    {
        public static void RunGame(int width, int height, int goalWidth, Heuristic<PaperSoccerState> playerA, Heuristic<PaperSoccerState> playerB)
        {
            var state = new PaperSoccerState(width, height, goalWidth);
            var searchA = new AlphaBetaSearch<PaperSoccerState, PaperSoccerAction>(playerA, true);
            var searchB = new AlphaBetaSearch<PaperSoccerState, PaperSoccerAction>(playerB, false);
            bool currentMoveA = true;
            int moveIndex = 0;
            while (!state.IsFinished(out double playerAUtility, true))
            {
                var action = currentMoveA ? searchA.GetMove(state) : searchB.GetMove(state);
                state.ApplyMove(action);
                Console.WriteLine("Move applied." + moveIndex++);
                state.SaveBoardImage($"p/Plansza{moveIndex}.png");

                currentMoveA = !currentMoveA;
            }
            state.SaveBoardImage("Plansza.png");
        }
    }
}
