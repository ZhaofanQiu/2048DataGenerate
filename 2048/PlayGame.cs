/*************************************************************************
  > File Name: BoardControl.cs
  > Copyright (C) 2013 Zhaofan Qiu<zhaofanqiu@gmail.com>
  > Created Time: 2014/9/19 15:35:30
  > Functions: Play 2048 game
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
    class PlayGame
    {
        public delegate int Strategy(Board x);
        /// <summary>
        /// play game by pointed strategy
        /// </summary>
        /// <param name="Str">play strategy</param>
        public static void Play(Strategy Str)
        {
            Board board = BoardControl.InitBoard();
            int moveno = 0;
            int scorepenalty = 0;
            while (true)
            {
                int move;
                Board newboard;

                for (move = 0; move < 4; move++)
                {
                    if (BoardControl.ExecuteMove(board, move) != board)
                        break;
                }
                if (move == 4)
                    break;
                BoardControl.Print(board);
                System.Console.WriteLine("Move:" + (++moveno).ToString() + ", score:" + (BoardControl.Score(board) - scorepenalty).ToString() + '\n');
                move = Str(board);
                if (move < 0)
                    break;
                newboard = BoardControl.ExecuteMove(board, move);
                if (newboard == board)
                {
                    System.Console.WriteLine("Illegal move!\n");
                    moveno--;
                    continue;
                }
                Board tile = BoardControl.RandomTile();
                if (tile == 2) scorepenalty += 4;
                board = BoardControl.InsertTileRand(newboard, tile);
            }

            BoardControl.Print(board);
            System.Console.WriteLine("\nGame over.\nScore:" + (BoardControl.Score(board) - scorepenalty).ToString() + ".\n");
        }
        /// <summary>
        /// play game for monte carlo method
        /// </summary>
        /// <param name="Str">play strategy</param>
        /// <param name="board">current board</param>
        /// <param name="nmove">monte carlo steps</param>
        /// <param name="move">pre-pointed next step</param>
        /// <returns>score by monte carlo</returns>
        public static double MontePlay(Strategy Str, Board board, int nmove, int move = -1)
        {
            bool first = true;
            while (true)
            {
                UInt64 newboard;
                if (first && (move != -1))
                {
                    if (BoardControl.ExecuteMove(board, move) == board)
                        return 0;
                    first = false;
                }
                else
                {
                    for (move = 0; move < 4; move++)
                    {
                        if (BoardControl.ExecuteMove(board, move) != board)
                            break;
                    }
                    if (move == 4)
                        break;
                    move = Str(board);
                    if (move < 0)
                        break;
                }

                newboard = BoardControl.ExecuteMove(board, move);
                if (newboard == board)
                {
                    continue;
                }
                UInt64 tile = BoardControl.RandomTile();
                board = BoardControl.InsertTileRand(newboard, tile);
                nmove--;
                if (nmove <= 0)
                    return PreScore.DirectScore(board);
            }
            return 0;
        }
    }
}
