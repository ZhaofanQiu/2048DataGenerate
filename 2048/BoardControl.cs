/*************************************************************************
  > File Name: BoardControl.cs
  > Copyright (C) 2013 Zhaofan Qiu<zhaofanqiu@gmail.com>
  > Created Time: 2014/9/19 15:28:35
  > Functions: Control 2048 Table by bitwise operation
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
    class BoardControl
    {
        #region StaticMethod
        const Board RowMask = 0xFFFFUL;
        const Board ColMask = 0x000F000F000F000FUL;

        static int[] ShowNum = new int[16] { 0, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
        static Row[] RowLeft = new Row[65536];
        static Row[] RowRight = new Row[65536];
        static Board[] colUp = new Board[65536];
        static Board[] colDown = new Board[65536];
        static double[] scoreTable = new double[65536];
        #endregion StaticMethod
        /// <summary>
        /// initialize board control
        /// </summary>
        public static void Initialize()
        {
            for (int i = 0; i < 65536; ++i)
            {
                Row row = (Row)i;
                int[] num = { ((row >> 0) & 0xf), ((row >> 4) & 0xf), ((row >> 8) & 0xf), ((row >> 12) & 0xf) };
                double score = 0.0f;
                for (int j = 0; j < 4; ++j)
                {
                    int rank = num[j];
                    if (rank >= 2)
                    {
                        score += (rank - 1) * (1 << rank);
                    }
                }
                scoreTable[row] = score;
                for (int j = 0; j < 3; ++j)
                {
                    int k;
                    for (k = j + 1; k < 4; ++k)
                    {
                        if (num[k] != 0) break;
                    }
                    if (k == 4) break;

                    if (num[j] == 0)
                    {
                        num[j] = num[k];
                        num[k] = 0;
                        j--;
                    }
                    else if (num[j] == num[k] && num[j] != 0xf)
                    {
                        num[j]++;
                        num[k] = 0;
                    }
                }
                Row result = (Row)((num[0] << 0) | (num[1] << 4) | (num[2] << 8) | (num[3] << 12));
                Row revResult = ReverseRow(result);
                Row revRow = ReverseRow((Row)row);
                RowLeft[row] = (Row)(row ^ result);
                RowRight[revRow] = (Row)(revRow ^ revResult);
                colUp[row] = UnpackCol((Row)row) ^ UnpackCol(result);
                colDown[revRow] = UnpackCol(revRow) ^ UnpackCol(revResult);
            }
        }
        
        #region Play
        /// <summary>
        /// initialize a game
        /// </summary>
        /// <returns>original board</returns>
        public static Board InitBoard()
        {
            Board board = RandomTile() << (4 * new Random(DateTime.Now.Millisecond).Next(16));
            return InsertTileRand(board, RandomTile());
        }
        /// <summary>
        /// print board
        /// </summary>
        /// <param name="x">board for print</param>
        public static void Print(Board x)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    Console.Write(string.Format("{0:D}\t", ShowNum[(int)((x) & 0xf)]));
                    x >>= 4;
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        /// <summary>
        /// execute move
        /// </summary>
        /// <param name="x">current board</param>
        /// <param name="d">move direction</param>
        /// <returns></returns>
        public static Board ExecuteMove(Board x, int d)
        {
            switch (d)
            {
                case 0: // up 
                    return MoveUp(x);
                case 1: // right
                    return MoveRight(x);
                case 2: // down
                    return MoveDown(x);
                case 3: // left 
                    return MoveLeft(x);
                default:
                    return ~0UL;
            }
        }
        /// <summary>
        /// return a random tile
        /// </summary>
        /// <returns></returns>
        public static Board RandomTile()
        {
            return (Board)((new Random(DateTime.Now.Millisecond).Next(10) < 9) ? 1 : 2);
        }
        /// <summary>
        /// insert a tile randomly to board
        /// </summary>
        /// <param name="x">current board</param>
        /// <param name="tile">tile to insert</param>
        /// <returns>new board</returns>
        public static Board InsertTileRand(Board x, Board tile)
        {
            int index = new Random(DateTime.Now.Millisecond).Next(CountEmpty(x));
            Board tmp = x;
            while (true)
            {
                while ((tmp & 0xf) != 0)
                {
                    tmp >>= 4;
                    tile <<= 4;
                }
                if (index == 0) break;
                --index;
                tmp >>= 4;
                tile <<= 4;
            }
            return x | tile;
        }
        #endregion Play
        /// <summary>
        /// help to calculate score of all rows
        /// </summary>
        /// <param name="x">current board</param>
        /// <param name="table">score of row</param>
        /// <returns>summation score of all rows</returns>
        public static double ScoreHelper(Board x, double[] table)
        {
            return table[(x >> 0) & RowMask] +
                   table[(x >> 16) & RowMask] +
                   table[(x >> 32) & RowMask] +
                   table[(x >> 48) & RowMask];
        }
        /// <summary>
        /// get system score of board
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>summation score</returns>
        public static double Score(Board x)
        {
            return ScoreHelper(x, scoreTable); 
        }
        /// <summary>
        /// convert uint64 board to int[,]
        /// </summary>
        /// <param name="x">uint64 board</param>
        /// <returns>board defined by int[,]</returns>
        static public int[,] BoardToInt(Board x)
        {
            int i, j;
            int[,] num = new int[4, 4];

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    num[i, j] = ShowNum[(int)((x) & 0xf)];
                    x >>= 4;
                }
            }
            return num;
        }
        /// <summary>
        /// convert int[,] to uint64 board
        /// </summary>
        /// <param name="num">board defined by int[,]</param>
        /// <returns>uint64 board</returns>
        static public Board IntToBoard(int[,] num)
        {
            Board res = 0;
            int inp = 0;
            int cf = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    inp = num[i, j];
                    if (inp == 0)
                    {
                        inp = 1;
                    }
                    res += ((Board)Math.Round(Math.Log(inp, 2))) << cf;
                    cf += 4;
                }
            }
            return res;
        }
        /// <summary>
        /// transpose board
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>transposed board</returns>
        public static Board Transpose(Board x)
        {
            Board a1 = x & 0xF0F00F0FF0F00F0FUL;
            Board a2 = x & 0x0000F0F00000F0F0UL;
            Board a3 = x & 0x0F0F00000F0F0000UL;
            Board a = a1 | (a2 << 12) | (a3 >> 12);
            Board b1 = a & 0xFF00FF0000FF00FFUL;
            Board b2 = a & 0x00FF00FF00000000UL;
            Board b3 = a & 0x00000000FF00FF00UL;
            return b1 | (b2 >> 24) | (b3 << 24);
        }
        /// <summary>
        /// count empty tile of board 
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>number of empty tile</returns>
        public static int CountEmpty(Board x)
        {
            x |= (x >> 2) & 0x3333333333333333UL;
            x |= (x >> 1);
            x = ~x & 0x1111111111111111UL;
            x += x >> 32;
            x += x >> 16;
            x += x >> 8;
            x += x >> 4;
            return (int)(x & 0xf);
        }
        /// <summary>
        /// get max rank of tile
        /// </summary>
        /// <param name="board">current board</param>
        /// <returns>max rank</returns>
        public static int GetMaxRank(Board board)
        {
            int maxrank = 0;
            while (board != 0)
            {
                maxrank = Math.Max(maxrank, (int)(board & 0xf));
                board >>= 4;
            }
            return maxrank;
        }
        /// <summary>
        /// count distinct tiles on board
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>number of distinct tiles</returns>
        public static int CountDistinctTiles(Board x)
        {
            Row bitset = 0;
            while (x != 0)
            {
                bitset |= (Row)(1 << ((ushort)(x & 0xf)));
                x >>= 4;
            }
            bitset >>= 1;
            int count = 0;
            while (bitset != 0)
            {
                bitset &= (Row)(bitset - 1);
                count++;
            }
            return count;
        }
        /// <summary>
        /// move up
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>new board</returns>
        static Board MoveUp(Board x)
        {
            Board ret = x;
            Board t = Transpose(x);
            ret ^= colUp[(t >> 0) & RowMask] << 0;
            ret ^= colUp[(t >> 16) & RowMask] << 4;
            ret ^= colUp[(t >> 32) & RowMask] << 8;
            ret ^= colUp[(t >> 48) & RowMask] << 12;
            return ret;
        }
        /// <summary>
        /// move down
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>new board</returns>
        static Board MoveDown(Board x)
        {
            Board ret = x;
            Board t = Transpose(x);
            ret ^= colDown[(t >> 0) & RowMask] << 0;
            ret ^= colDown[(t >> 16) & RowMask] << 4;
            ret ^= colDown[(t >> 32) & RowMask] << 8;
            ret ^= colDown[(t >> 48) & RowMask] << 12;
            return ret;
        }
        /// <summary>
        /// move left
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>new board</returns>
        static Board MoveLeft(Board x)
        {
            Board ret = x;
            ret ^= (Board)(RowLeft[(x >> 0) & RowMask]) << 0;
            ret ^= (Board)(RowLeft[(x >> 16) & RowMask]) << 16;
            ret ^= (Board)(RowLeft[(x >> 32) & RowMask]) << 32;
            ret ^= (Board)(RowLeft[(x >> 48) & RowMask]) << 48;
            return ret;
        }
        /// <summary>
        /// move right
        /// </summary>
        /// <param name="x">current board</param>
        /// <returns>new board</returns>
        static Board MoveRight(Board x)
        {
            Board ret = x;
            ret ^= (Board)(RowRight[(x >> 0) & RowMask]) << 0;
            ret ^= (Board)(RowRight[(x >> 16) & RowMask]) << 16;
            ret ^= (Board)(RowRight[(x >> 32) & RowMask]) << 32;
            ret ^= (Board)(RowRight[(x >> 48) & RowMask]) << 48;
            return ret;
        }
        /// <summary>
        /// unpack col from Row to Board
        /// </summary>
        /// <param name="row">col defined by Row</param>
        /// <returns>unpacked col</returns>
        static Board UnpackCol(Row row)
        {
            Board tmp = row;
            return (tmp | (tmp << 12) | (tmp << 24) | (tmp << 36)) & ColMask;
        }
        /// <summary>
        /// reverse row
        /// </summary>
        /// <param name="row">input row</param>
        /// <returns>reversed row</returns>
        static Row ReverseRow(Row row)
        {
            return (Row)((row >> 12) | ((row >> 4) & 0x00F0) | ((row << 4) & 0x0F00) | (row << 12));
        }
    }
}
