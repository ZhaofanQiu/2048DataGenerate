/*************************************************************************
  > File Name: Search.cs
  > Copyright (C) 2013 Zhaofan Qiu<zhaofanqiu@gmail.com>
  > Created Time: 2014/9/19 16:28:15
  > Functions: Search best move by maxmize mean score
  > Reference: https://github.com/nneonneo/2048-ai
 ************************************************************************/

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
        class Param
        {
            public int depth = 0;
            public int depthLimit = 0;
            public Dictionary<Board, double> scoreTable = null;
        }     
        public delegate double Score(Board x);
        static Score HeurScore = PreScore.DirectScore;
        const double ProbTresh = 0.0001f;
        const int DictionaryLimit = 20;
        /// <summary>
        /// search best move
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>next move</returns>
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
        /// <summary>
        /// score board while pointed the next move
        /// </summary>
        /// <param name="x">current board</param>
        /// <param name="d">pre-pointed next move</param>
        /// <returns>score</returns>
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
                res = ScoreNextInsert(newboard, 1.0f, para) + 1e-6;
            return res;
        }
        /// <summary>
        /// score while next process is insert
        /// </summary>
        /// <param name="x">current board</param>
        /// <param name="prob">current board probability</param>
        /// <param name="para">parameters of search</param>
        /// <returns>score</returns>
        static double ScoreNextInsert(UInt64 x, float prob, Param para)
        {
            if (para.depth < DictionaryLimit)
            {
                if (para.scoreTable.ContainsKey(x))
                {
                    return para.scoreTable[x];
                }
            }

            if (prob < ProbTresh || para.depth >= para.depthLimit)
            {
                return HeurScore(x);
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
                    res += ScoreNextMove(x | tile, prob * 0.9f, para) * 0.9f;
                    res += ScoreNextMove(x | (tile << 1), prob * 0.1f, para) * 0.1f;
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
        /// <summary>
        /// score while next process is move
        /// </summary>
        /// <param name="x">current board</param>
        /// <param name="prob">current board probability</param>
        /// <param name="para">parameters of search</param>
        /// <returns>score</returns>
        static double ScoreNextMove(UInt64 x, float prob, Param para)
        {
            double best = 0.0f;
            UInt64 newboard;
            para.depth++;
            for (int d = 0; d < 4; ++d)
            {
                newboard = BoardControl.ExecuteMove(x, d);
                if (x != newboard)
                {
                    best = Math.Max(best, ScoreNextInsert(newboard, prob, para));
                }
            }
            para.depth--;
            return best;
        }
    }
}
