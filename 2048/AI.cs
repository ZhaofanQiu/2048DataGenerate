/*************************************************************************
  > File Name: Search.cs
  > Copyright (C) 2013 Zhaofan Qiu<zhaofanqiu@gmail.com>
  > Created Time: 2014/9/19 16:18:47
  > Functions: Define defferent strategy to play 2048
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
    class AI
    {
        /// <summary>
        /// input next move
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>next move</returns>
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
        /// <summary>
        /// random next move
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>next move</returns>
        static public int RandomMove(Board x)
        {
            return new Random(DateTime.Now.Millisecond).Next() % 4;
        }
        /// <summary>
        /// find next move by direct score
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>next move</returns>
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
        /// <summary>
        /// find next move by monte carlo method
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>next move</returns>
        static public int MonteCarloMove(Board x)
        {
            int iterN = 2000;
            double[] score = new double[4] { 0, 0, 0, 0 };
            double maxScore = 0;
            int maxi = 0;
            Parallel.For(0, 4, (i, loopState) =>
            {
                if (BoardControl.ExecuteMove(x, i) != x)
                {
                    for (int j = 0; j < iterN; j++)
                    {
                        score[i] = score[i] + PlayGame.MontePlay(DirectScoreMove, x, 10, i);
                    }
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
            if (BoardControl.ExecuteMove(x, maxi) == x)
                return new Random(DateTime.Now.Millisecond).Next() % 4;

            Console.WriteLine("Predict average score:" + (maxScore / iterN).ToString());
            Console.WriteLine();
            return maxi;
        }
        /// <summary>
        /// heuristic search for next move
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>next move</returns>
        static public int SearchMove(Board x)
        {
            return Search.SearchMove(x);
        }
    }
}
