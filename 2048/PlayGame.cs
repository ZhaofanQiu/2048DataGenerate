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
                Board tile = BoardControl.DrawTile();
                if (tile == 2) scorepenalty += 4;
                board = BoardControl.InsertTileRand(newboard, tile);
            }

            BoardControl.Print(board);
            System.Console.WriteLine("\nGame over. Your score is " + (BoardControl.Score(board) - scorepenalty).ToString() + ".\n");
        }
        public static double MontePlay(Strategy Str, Board board, int move = -1)
        {
            bool first = true;
            int scorepenalty = 0;
            while (true)
            {
                UInt64 newboard;
                if (first && (move != -1))
                {
                    if (BoardControl.ExecuteMove(board, move) == board)
                        return -1;
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
                UInt64 tile = BoardControl.DrawTile();
                if (tile == 2) scorepenalty += 4;
                board = BoardControl.InsertTileRand(newboard, tile);
            }
            return BoardControl.Score(board) - scorepenalty;
        }
    }
}
