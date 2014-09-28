using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Board = System.UInt64;
using Row = System.UInt16;

namespace _2048AI
{
    class AI
    {
        static public int InputMove(Board x)
        {
            var d = System.Console.ReadKey().KeyChar;
            System.Console.WriteLine();
            System.Console.WriteLine();
            switch (d)
            {
                case 'w':
                    return 0;
                case 'd':
                    return 1;
                case 's':
                    return 2;
                case 'a':
                    return 3;
                default:
                    return -1;
            }
        }
        static public int RandomMove(Board x)
        {
            return new Random(DateTime.Now.Millisecond).Next() % 4;
        }
        static public int DirectScoreMove(Board x)
        {
            double maxScore = 0;
            int maxi = -1;
            for (int k = 0; k < 4; k++)
            {
                if (BoardControl.ExecuteMove(x, k) == x)
                    continue;
                if (PreScore.DirectScore(BoardControl.ExecuteMove(x, k)) > maxScore)
                {
                    maxScore = PreScore.DirectScore(BoardControl.ExecuteMove(x, k));
                    maxi = k;
                }
            }
            if (maxi == -1)
                return new Random(DateTime.Now.Millisecond).Next() % 4;
            else
                return maxi;
        }
        static public int MonteCarloMove(Board x)
        {
            int iterN = 100;
            double[] score = new double[4] { 0, 0, 0, 0 };
            double maxScore = 0;
            int maxi = 0;
            Parallel.For(0, 4, (i, loopState) =>
            {
                for (int j = 0; j < iterN; j++)
                {
                    score[i] = score[i] + PlayGame.MontePlay(DirectScoreMove, x, i);
                }
            });
            for (int k = 0; k < 4; k++)
            {
                if (score[k] > maxScore)
                {
                    maxScore = score[k];
                    maxi = k;
                }
            }
            Console.WriteLine("Predict average score:" + (maxScore / iterN).ToString());
            Console.WriteLine();
            return maxi;
        }
        static public int SearchMove(Board x)
        {
            return Search.SearchMove(x);
        }
    }
}
