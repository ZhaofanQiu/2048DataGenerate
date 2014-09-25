using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Board = System.UInt64;
using Row = System.UInt16;

namespace _2048AI
{
    class Search
    {
        public delegate double Score(Board x);
        static Score HeurScore = PreScore.DirectScore;

        class EvalState
        {
            public Dictionary<Board, double> transTable = null;
            public int maxdepth = 0;
            public int curdepth = 0;
            public int cachehits = 0;
            public Board moves_evaled = 0;
            public int depthLimit = 0;
        }
        const double CprobTreshBase = 0.0001f;
        const int CacheDepthLimit = 6;

        public static int FindBestMove(Board x)
        {
            int move;
            double best = 0;
            int bestmove = -1;

            for (move = 0; move < 4; move++)
            {
                double res = ScoreToplevelMove(x, move);

                if (res > best)
                {
                    best = res;
                    bestmove = move;
                }
            }

            return bestmove;
        }
        public static double ScoreToplevelMove(Board x, int move)
        {
            double res;

            EvalState state = new EvalState();
            state.transTable = new Dictionary<ulong, double>();
            state.depthLimit = Math.Max(3, BoardControl.CountDistinctTiles(x) - 2);

            res = _ScoreToplevelMove(state, x, move);

            Console.WriteLine("Move " + move.ToString() + ": result " + res.ToString() + ": eval'd " + state.moves_evaled.ToString() + " moves ( " + state.cachehits.ToString() + "cache hits, " + state.transTable.Count.ToString() + " cache size)" + "(maxdepth=" + state.maxdepth.ToString() + ")");

            return res;
        }
        static double _ScoreToplevelMove(EvalState state, UInt64 x, int move)
        {
            UInt64 newboard = BoardControl.ExecuteMove(x, move);

            if (x == newboard)
                return 0;

            return ScoreTilechooseNode(state, newboard, 1.0f) + 1e-6;
        }
        static double ScoreTilechooseNode(EvalState state, UInt64 x, float cprob)
        {
            if (cprob < CprobTreshBase || state.curdepth >= state.depthLimit)
            {
                state.maxdepth = Math.Max(state.curdepth, state.maxdepth);
                return HeurScore(x);
            }

            if (state.curdepth < CacheDepthLimit)
            {
                if (state.transTable.ContainsKey(x))
                {
                    state.cachehits++;
                    return state.transTable[x];
                }
            }

            int num_open = BoardControl.CountEmpty(x);
            cprob /= num_open;

            double res = 0.0f;
            UInt64 tmp = x;
            UInt64 tile_2 = 1;
            while (tile_2 != 0)
            {
                if ((tmp & 0xf) == 0)
                {
                    res += ScoreMoveNode(state, x | tile_2, cprob * 0.9f) * 0.9f;
                    res += ScoreMoveNode(state, x | (tile_2 << 1), cprob * 0.1f) * 0.1f;
                }
                tmp >>= 4;
                tile_2 <<= 4;
            }
            res = res / num_open;

            if (state.curdepth < CacheDepthLimit)
            {
                state.transTable[x] = res;
            }

            return res;
        }
        static double ScoreMoveNode(EvalState state, UInt64 x, float cprob)
        {
            double best = 0.0f;
            state.curdepth++;
            for (int move = 0; move < 4; ++move)
            {
                UInt64 newboard = BoardControl.ExecuteMove(x, move);
                state.moves_evaled++;

                if (x != newboard)
                {
                    best = Math.Max(best, ScoreTilechooseNode(state, newboard, cprob));
                }
            }
            state.curdepth--;

            return best;
        }
    }
}
