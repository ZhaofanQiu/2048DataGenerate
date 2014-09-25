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
        const double ScoreLostPenalty = 200000.0f;
        const double ScoreMonotonicityPower = 4.0f;
        const double ScoreMonotonicityWeight = 47.0f;
        const double ScoreSumPower = 3.5f;
        const double ScoreSumWeight = 11.0f;
        const double ScoreMergesWeight = 700.0f;
        const double ScoreEmptyWeight = 270.0f;

        static double[] scoreTable = new double[65536];
        public static void Initialize()
        {
            for (int row = 0; row < 65536; ++row)
            {
                int[] line = { ((row >> 0) & 0xf), ((row >> 4) & 0xf), ((row >> 8) & 0xf), ((row >> 12) & 0xf) };
                double sum = 0;
                int empty = 0;
                int merges = 0;
                int prev = 0;
                int counter = 0;
                for (int i = 0; i < 4; ++i)
                {
                    int rank = line[i];
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
                for (int i = 1; i < 4; ++i)
                {
                    if (line[i - 1] > line[i])
                    {
                        monotonicity_left += Math.Pow(line[i - 1], ScoreMonotonicityPower) - Math.Pow(line[i], ScoreMonotonicityPower);
                    }
                    else
                    {
                        monotonicity_right += Math.Pow(line[i], ScoreMonotonicityPower) - Math.Pow(line[i - 1], ScoreMonotonicityPower);
                    }
                }
                scoreTable[row] = ScoreLostPenalty +
                    ScoreEmptyWeight * empty +
                    ScoreMergesWeight * merges -
                    ScoreMonotonicityWeight * Math.Min(monotonicity_left, monotonicity_right) -
                    ScoreSumWeight * sum;
            }
        }
        public static double DirectScore(Board x)
        {
            return BoardControl.ScoreHelper(x, scoreTable) +
               BoardControl.ScoreHelper(BoardControl.Transpose(x), scoreTable);
        }
    }
}
