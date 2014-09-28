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
                    sum += Math.Pow(rank, ScoreSumPower);
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
                double monotonicity_left = 0;
                double monotonicity_right = 0;
                for (int j = 1; j < 4; ++j)
                {
                    if (num[j - 1] > num[j])
                    {
                        monotonicity_left += Math.Pow(num[j - 1], ScoreMonotonicityPower) - Math.Pow(num[j], ScoreMonotonicityPower);
                    }
                    else
                    {
                        monotonicity_right += Math.Pow(num[j], ScoreMonotonicityPower) - Math.Pow(num[j - 1], ScoreMonotonicityPower);
                    }
                }
                scoreTable[row] = ScoreLostPenalty + ScoreEmptyWeight * empty + ScoreMergesWeight * merges - ScoreMonotonicityWeight * Math.Min(monotonicity_left, monotonicity_right) -
                    ScoreSumWeight * sum;
            }
        }
        public static double DirectScore(Board x)
        {
            return BoardControl.ScoreHelper(x, scoreTable) +
               BoardControl.ScoreHelper(BoardControl.Transpose(x), scoreTable);
        }
        const double ScoreLostPenalty = 200000.0f;
        const double ScoreMonotonicityPower = 4.0f;
        const double ScoreMonotonicityWeight = 47.0f;
        const double ScoreSumPower = 3.5f;
        const double ScoreSumWeight = 11.0f;
        const double ScoreMergesWeight = 700.0f;
        const double ScoreEmptyWeight = 270.0f;
    }
}
