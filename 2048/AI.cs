using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048
{
    class AI
    {
        static public int AINextMove(int[,] grids, int opt = 0)
        {
            switch(opt)
            {
                case 1:
                    var move = System.Console.ReadKey().KeyChar;
                    System.Console.WriteLine();
                    switch (move)
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
                case 2:
                    int iterN = 100;
                    double[] score = new double[4]{0, 0, 0, 0};
                    UInt64 board = Table.IntToTable(grids);
                    double maxScore = 0;
                    int maxi = 0;
                    Parallel.For(0, 4, (i, loopState) =>
                    {
                        for (int j = 0; j < iterN; j++)
                        {
                            score[i] = score[i] + Table.PredictScore(board, i);
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
                    return maxi;
                case 3:
                    UInt64 boardt = Table.IntToTable(grids);
                    double maxScoret = 0;
                    int maxit = -1;
                    for (int k = 0; k < 4; k++)
                    {
                        if (Table.ExecuteMove(k, boardt) == boardt)
                            continue;
                        if (Table.ScoreHeurBoard(Table.ExecuteMove(k, boardt)) > maxScoret)
                        {
                            maxScoret = Table.ScoreHeurBoard(Table.ExecuteMove(k, boardt));
                            maxit = k;
                        }
                    }
                    if (maxit == -1)
                        return new Random(DateTime.Now.Millisecond).Next() % 4; 
                    else 
                        return maxit;
                case 4:
                    UInt64 board4 = Table.IntToTable(grids);
                    return Table.FindBestMove(board4);
                default:
                    return new Random(DateTime.Now.Millisecond).Next() % 4;
            }
        }
    }
}
