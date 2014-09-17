using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048
{
    class Table
    {
        static const UInt64 ROW_MASK = 0xFFFFUL;
        static const UInt64 COL_MASK = 0x000F000F000F000FUL;
        static const double SCORE_LOST_PENALTY = 200000.0f;
        static const double SCORE_MONOTONICITY_POWER = 4.0f;
        static const double SCORE_MONOTONICITY_WEIGHT = 47.0f;
        static const double SCORE_SUM_POWER = 3.5f;
        static const double SCORE_SUM_WEIGHT = 11.0f;
        static const double SCORE_MERGES_WEIGHT = 700.0f;
        static const double SCORE_EMPTY_WEIGHT = 270.0f;
        static UInt16[] row_left_table = new UInt16[65536];
        static UInt16[] row_right_table = new UInt16[65536];
        static UInt64[] col_up_table = new UInt64[65536];
        static UInt64[] col_down_table = new UInt64[65536];
        static double[] heur_score_table = new double[65536];
        static double[] score_table = new double[65536];
        static void print_board(UInt64 board)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    Console.Write("0123456789abcdef"[(int)((board) & 0xf)]);
                    board >>= 4;
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        static UInt64 unpack_col(UInt16 row)
        {
            UInt64 tmp = row;
            return (tmp | (tmp << 12) | (tmp << 24) | (tmp << 36)) & COL_MASK;
        }
        static UInt16 reverse_row(UInt16 row)
        {
            return (UInt16)((row >> 12) | ((row >> 4) & 0x00F0) | ((row << 4) & 0x0F00) | (row << 12));
        }



        public static void InitTables()
        {
            for (int row = 0; row < 65536; ++row)
            {
                int[] line = {
                           (row >>  0) & 0xf, 
                 (row >>  4) & 0xf, 
                 (row >>  8) & 0xf, 
                 (row >> 12) & 0xf 
         };


                // Score 
                double score = 0.0f;
                for (int i = 0; i < 4; ++i)
                {
                    int rank = line[i];
                    if (rank >= 2)
                    {
                        // the score is the total sum of the tile and all intermediate merged tiles 
                        score += (rank - 1) * (1 << rank);
                    }
                }
                score_table[row] = score;




                // Heuristic score 
                double sum = 0;
                int empty = 0;
                int merges = 0;


                int prev = 0;
                int counter = 0;
                for (int i = 0; i < 4; ++i)
                {
                    int rank = line[i];
                    sum += Math.Pow(rank, SCORE_SUM_POWER);
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
                        monotonicity_left += Math.Pow(line[i - 1], SCORE_MONOTONICITY_POWER) - Math.Pow(line[i], SCORE_MONOTONICITY_POWER);
                    }
                    else
                    {
                        monotonicity_right += Math.Pow(line[i], SCORE_MONOTONICITY_POWER) - Math.Pow(line[i - 1], SCORE_MONOTONICITY_POWER);
                    }
                }


                heur_score_table[row] = SCORE_LOST_PENALTY +
                    SCORE_EMPTY_WEIGHT * empty +
                    SCORE_MERGES_WEIGHT * merges -
                    SCORE_MONOTONICITY_WEIGHT * Math.Min(monotonicity_left, monotonicity_right) -
                    SCORE_SUM_WEIGHT * sum;


                // execute a move to the left 
                for (int i = 0; i < 3; ++i)
                {
                    int j;
                    for (j = i + 1; j < 4; ++j)
                    {
                        if (line[j] != 0) break;
                    }
                    if (j == 4) break; // no more tiles to the right 


                    if (line[i] == 0)
                    {
                        line[i] = line[j];
                        line[j] = 0;
                        i--; // retry this entry 
                    }
                    else if (line[i] == line[j] && line[i] != 0xf)
                    {
                        line[i]++;
                        line[j] = 0;
                    }
                }


                UInt16 result = (UInt16)((line[0] << 0) |
                               (line[1] << 4) |
                               (line[2] << 8) |
                               (line[3] << 12));
                UInt16 rev_result = reverse_row(result);
                UInt16 rev_row = reverse_row((UInt16)row);


                row_left_table[row] = (UInt16)(row ^ result);
                row_right_table[rev_row] = (UInt16)(rev_row ^ rev_result);
                col_up_table[row] = (UInt16)(unpack_col((UInt16)row) ^ unpack_col(result));
                col_down_table[rev_row] = (UInt16)(unpack_col(rev_row) ^ unpack_col(rev_result));
            }
        }

        public static ulong Transpose(ulong originTable)
        {
            ulong a1 = originTable & 0xF0F00F0FF0F00F0FUL;
            ulong a2 = originTable & 0x0000F0F00000F0F0UL;
            ulong a3 = originTable & 0x0F0F00000F0F0000UL;
            ulong a = a1 | (a2 << 12) | (a3 >> 12);
            ulong b1 = a & 0xFF00FF0000FF00FFUL;
            ulong b2 = a & 0x00FF00FF00000000UL;
            ulong b3 = a & 0x00000000FF00FF00UL;
            return b1 | (b2 >> 24) | (b3 << 24);
        }
        public static ulong CountEmpty(ulong originTable)
        {
            ulong x = originTable;
            x |= (x >> 2) & 0x3333333333333333UL;
            x |= (x >> 1);
            x = ~x & 0x1111111111111111UL;
            x += x >> 32;
            x += x >> 16;
            x += x >> 8;
            x += x >> 4;
            return x & 0xf;
        }
        static UInt64 execute_move_0(UInt64 board)
        {
            UInt64 ret = board;
            UInt64 t = Transpose(board);
            ret ^= col_up_table[(t >> 0) & ROW_MASK] << 0;
            ret ^= col_up_table[(t >> 16) & ROW_MASK] << 4;
            ret ^= col_up_table[(t >> 32) & ROW_MASK] << 8;
            ret ^= col_up_table[(t >> 48) & ROW_MASK] << 12;
            return ret;
        }


        static UInt64 execute_move_1(UInt64 board)
        {
            UInt64 ret = board;
            UInt64 t = Transpose(board);
            ret ^= col_down_table[(t >> 0) & ROW_MASK] << 0;
            ret ^= col_down_table[(t >> 16) & ROW_MASK] << 4;
            ret ^= col_down_table[(t >> 32) & ROW_MASK] << 8;
            ret ^= col_down_table[(t >> 48) & ROW_MASK] << 12;
            return ret;
        }


        static UInt64 execute_move_2(UInt64 board)
        {
            UInt64 ret = board;
            ret ^= (UInt64)(row_left_table[(board >> 0) & ROW_MASK]) << 0;
            ret ^= (UInt64)(row_left_table[(board >> 16) & ROW_MASK]) << 16;
            ret ^= (UInt64)(row_left_table[(board >> 32) & ROW_MASK]) << 32;
            ret ^= (UInt64)(row_left_table[(board >> 48) & ROW_MASK]) << 48;
            return ret;
        }


        static UInt64 execute_move_3(UInt64 board)
        {
            UInt64 ret = board;
            ret ^= (UInt64)(row_right_table[(board >> 0) & ROW_MASK]) << 0;
            ret ^= (UInt64)(row_right_table[(board >> 16) & ROW_MASK]) << 16;
            ret ^= (UInt64)(row_right_table[(board >> 32) & ROW_MASK]) << 32;
            ret ^= (UInt64)(row_right_table[(board >> 48) & ROW_MASK]) << 48;
            return ret;
        }


        /* Execute a move. */
        static UInt64 execute_move(int move, UInt64 board)
        {
            switch (move)
            {
                case 0: // up 
                    return execute_move_0(board);
                case 1: // down 
                    return execute_move_1(board);
                case 2: // left
                    return execute_move_2(board);
                case 3: // right 
                    return execute_move_3(board);
                default:
                    return ~0UL;
            }
        }
        static int get_max_rank(UInt64 board)
        {
            int maxrank = 0;
            while (board != 0)
            {
                maxrank = Math.Max(maxrank, (int)(board & 0xf));
                board >>= 4;
            }
            return maxrank;
        }


        static int count_distinct_tiles(UInt64 board)
        {
            UInt16 bitset = 0;
            while (board != 0)
            {
                bitset |= (UInt16)(1 << ((ushort)(board & 0xf)));
                board >>= 4;
            }


            // Don't count empty tiles.
            bitset >>= 1;

            int count = 0;
            while (bitset != 0)
            {
                bitset &= (UInt16)(bitset - 1);
                count++;
            }
            return count;
        }


    }
}
