using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Board = System.UInt64;
using Row = System.UInt16;

namespace _2048AI
{
    class Search
    {
        public delegate double Score(Board x);
        static Score HeurScore = PreScore.DirectScore;
        class Param
        {
            public int depth = 0;
            public int depthLimit = 0;
            public Dictionary<Board, double> scoreTable = null;
        }
        const double ProbTresh = 0.0001f;
        const int DictionaryLimit = 9;

        public static int SearchMove(Board x)
        {
            int d;
            double best = 0;
            int bestd = -1;
            double[] res = new double[4];
            Parallel.For(0, 4, (i, loopState) =>
            {
                res[i] = ScoreFirstMove(x, i);
            });

            for (d = 0; d < 4; d++)
            {
                if (res[d] > best)
                {
                    best = res[d];
                    bestd = d;
                }
            }
            return bestd;
        }
        public static double ScoreFirstMove(Board x, int d)
        {
            double res;

            Param para = new Param();
            para.scoreTable = new Dictionary<ulong, double>();
            para.depthLimit = Math.Max(3, BoardControl.CountDistinctTiles(x) - 2);
            UInt64 newboard = BoardControl.ExecuteMove(x, d);
            if (x == newboard)
                res = 0;
            else
                res = ScoreInsert(newboard, 1.0f, para) + 1e-6;
            return res;
        }
        static double ScoreInsert(UInt64 x, float prob, Param para)
        {
            if (prob < ProbTresh || para.depth >= para.depthLimit)
            {
                return HeurScore(x);
            }

            if (para.depth < DictionaryLimit)
            {
                if (para.scoreTable.ContainsKey(x))
                {
                    return para.scoreTable[x];
                }
            }

            int num = BoardControl.CountEmpty(x);
            prob /= num;

            double res = 0.0f;
            UInt64 tmp = x;
            UInt64 tile = 1;
            while (tile != 0)
            {
                if ((tmp & 0xf) == 0)
                {
                    res += ScoreMove(x | tile, prob * 0.9f, para) * 0.9f;
                    res += ScoreMove(x | (tile << 1), prob * 0.1f, para) * 0.1f;
                }
                tmp >>= 4;
                tile <<= 4;
            }
            res = res / num;

            if (para.depth < DictionaryLimit)
            {
                para.scoreTable[x] = res;
            }

            return res;
        }
        static double ScoreMove(UInt64 x, float prob, Param para)
        {
            double best = 0.0f;
            UInt64 newboard;
            para.depth++;
            for (int d = 0; d < 4; ++d)
            {
                newboard = BoardControl.ExecuteMove(x, d);
                if (x != newboard)
                {
                    best = Math.Max(best, ScoreInsert(newboard, prob, para));
                }
            }
            para.depth--;
            return best;
        }
    }
}
