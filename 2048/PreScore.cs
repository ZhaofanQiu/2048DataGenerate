/*************************************************************************
  > File Name: PreScore.cs
  > Copyright (C) 2013 Zhaofan Qiu<zhaofanqiu@gmail.com>
  > Created Time: 2014/9/19 16:01:19
  > Functions: Score 2048 Table by Rules
  > Reference: https://github.com/nneonneo/2048-ai
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Board = System.UInt64;
using Row = System.UInt16;

namespace _2048AI
{
    class PreScore
    {
        static double[] scoreTable = new double[65536];
        /// <summary>
        /// initialize score table
        /// </summary>
        public static void Initialize()
        {
            for (int i = 0; i < 65536; ++i)
            {
                Row row = (Row)i;

                int[] num = { ((row >> 0) & 0xf), ((row >> 4) & 0xf), ((row >> 8) & 0xf), ((row >> 12) & 0xf) };
                double sum = 0;
                int empty = 0, merges = 0, prev = 0, counter = 0;
                for (int j = 0; j < 4; ++j)
                {
                    int rank = num[j];
                    sum += Math.Pow(rank, SumPower);
                    if (rank == 0)
                    {
                        empty++;
                    }
                    else
                    {
                        if (prev == rank)
                        {
                            counter++;
                        }
                        else if (counter > 0)
                        {
                            merges += 1 + counter;
                            counter = 0;
                        }
                        prev = rank;
                    }
                }
                if (counter > 0)
                {
                    merges += 1 + counter;
                }
                double left = 0;
                double right = 0;
                for (int j = 1; j < 4; ++j)
                {
                    if (num[j - 1] > num[j])
                    {
                        left += Math.Pow(num[j - 1], MonotonicityPower) - Math.Pow(num[j], MonotonicityPower);
                    }
                    else
                    {
                        right += Math.Pow(num[j], MonotonicityPower) - Math.Pow(num[j - 1], MonotonicityPower);
                    }
                }
                scoreTable[row] = LostPenalty + EmptyWeight * empty + MergesWeight * merges - MonotonicityWeight * Math.Min(left,right) - SumWeight * sum;
            }
        }
        /// <summary>
        /// calculate direct score by rules
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>direct score</returns>
        public static double DirectScore(Board x)
        {
            return BoardControl.ScoreHelper(x, scoreTable) +
               BoardControl.ScoreHelper(BoardControl.Transpose(x), scoreTable);
        }
        const double LostPenalty = 200000.0f;
        const double MonotonicityPower = 4.0f;
        const double MonotonicityWeight = 47.0f;
        const double SumPower = 3.5f;
        const double SumWeight = 11.0f;
        const double MergesWeight = 700.0f;
        const double EmptyWeight = 270.0f;
    }
}
