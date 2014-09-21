using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048
{
    class EvalState {
        public Dictionary<UInt64, double> transTable = null;
        public int maxdepth = 0;
        public int curdepth = 0;
        public int cachehits = 0;
        public UInt64 moves_evaled = 0;
        public int depthLimit = 0;
    };
    class Table
    {
        const UInt64 ROW_MASK = 0xFFFFUL;
        const UInt64 COL_MASK = 0x000F000F000F000FUL;
        const double SCORE_LOST_PENALTY = 200000.0f;
        const double SCORE_MONOTONICITY_POWER = 4.0f;
        const double SCORE_MONOTONICITY_WEIGHT = 47.0f;
        const double SCORE_SUM_POWER = 3.5f;
        const double SCORE_SUM_WEIGHT = 11.0f;
        const double SCORE_MERGES_WEIGHT = 700.0f;
        const double SCORE_EMPTY_WEIGHT = 270.0f;
        const double CPROB_THRESH_BASE = 0.0001f;
        const int CACHE_DEPTH_LIMIT  = 6;
        static int[] tableNum = new int[16] { 0, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
        static UInt16[] rowLeftTable = new UInt16[65536];
        static UInt16[] rowRightTable = new UInt16[65536];
        static UInt64[] colUpTable = new UInt64[65536];
        static UInt64[] colDownTable = new UInt64[65536];
        static double[] heurScoreTable = new double[65536];
        static double[] scoreTable = new double[65536];
        static void PrintBoard(UInt64 board)
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
        static UInt64 UnpackCol(UInt16 row)
        {
            UInt64 tmp = row;
            return (tmp | (tmp << 12) | (tmp << 24) | (tmp << 36)) & COL_MASK;
        }
        static UInt16 ReverseRow(UInt16 row)
        {
            return (UInt16)((row >> 12) | ((row >> 4) & 0x00F0) | ((row << 4) & 0x0F00) | (row << 12));
        }
        public static void InitTables()
        {
            for (int row = 0; row < 65536; ++row)
            {
                int[] line = { ((row >> 0) & 0xf), ((row >> 4) & 0xf), ((row >> 8) & 0xf), ((row >> 12) & 0xf) };
                double score = 0.0f;
                for (int i = 0; i < 4; ++i)
                {
                    int rank = line[i];
                    if (rank >= 2)
                    {
                        score += (rank - 1) * (1 << rank);
                    }
                }
                scoreTable[row] = score;
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
                heurScoreTable[row] = SCORE_LOST_PENALTY +
                    SCORE_EMPTY_WEIGHT * empty +
                    SCORE_MERGES_WEIGHT * merges -
                    SCORE_MONOTONICITY_WEIGHT * Math.Min(monotonicity_left, monotonicity_right) -
                    SCORE_SUM_WEIGHT * sum;
                for (int i = 0; i < 3; ++i)
                {
                    int j;
                    for (j = i + 1; j < 4; ++j)
                    {
                        if (line[j] != 0) break;
                    }
                    if (j == 4) break;

                    if (line[i] == 0)
                    {
                        line[i] = line[j];
                        line[j] = 0;
                        i--;
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
                UInt16 rev_result = ReverseRow(result);
                UInt16 rev_row = ReverseRow((UInt16)row);
                rowLeftTable[row] = (UInt16)(row ^ result);
                rowRightTable[rev_row] = (UInt16)(rev_row ^ rev_result);
                colUpTable[row] = UnpackCol((UInt16)row) ^ UnpackCol(result);
                colDownTable[rev_row] = UnpackCol(rev_row) ^ UnpackCol(rev_result);
            }
        }
        public static UInt64 Transpose(UInt64 originTable)
        {
            UInt64 a1 = originTable & 0xF0F00F0FF0F00F0FUL;
            UInt64 a2 = originTable & 0x0000F0F00000F0F0UL;
            UInt64 a3 = originTable & 0x0F0F00000F0F0000UL;
            UInt64 a = a1 | (a2 << 12) | (a3 >> 12);
            UInt64 b1 = a & 0xFF00FF0000FF00FFUL;
            UInt64 b2 = a & 0x00FF00FF00000000UL;
            UInt64 b3 = a & 0x00000000FF00FF00UL;
            return b1 | (b2 >> 24) | (b3 << 24);
        }
        public static int CountEmpty(ulong originTable)
        {
            ulong x = originTable;
            x |= (x >> 2) & 0x3333333333333333UL;
            x |= (x >> 1);
            x = ~x & 0x1111111111111111UL;
            x += x >> 32;
            x += x >> 16;
            x += x >> 8;
            x += x >> 4;
            return (int)(x & 0xf);
        }
        static UInt64 ExecuteMove0(UInt64 board)
        {
            UInt64 ret = board;
            UInt64 t = Transpose(board);
            ret ^= colUpTable[(t >> 0) & ROW_MASK] << 0;
            ret ^= colUpTable[(t >> 16) & ROW_MASK] << 4;
            ret ^= colUpTable[(t >> 32) & ROW_MASK] << 8;
            ret ^= colUpTable[(t >> 48) & ROW_MASK] << 12;
            return ret;
        }
        static UInt64 ExecuteMove1(UInt64 board)
        {
            UInt64 ret = board;
            UInt64 t = Transpose(board);
            ret ^= colDownTable[(t >> 0) & ROW_MASK] << 0;
            ret ^= colDownTable[(t >> 16) & ROW_MASK] << 4;
            ret ^= colDownTable[(t >> 32) & ROW_MASK] << 8;
            ret ^= colDownTable[(t >> 48) & ROW_MASK] << 12;
            return ret;
        }
        static UInt64 ExecuteMove2(UInt64 board)
        {
            UInt64 ret = board;
            ret ^= (UInt64)(rowLeftTable[(board >> 0) & ROW_MASK]) << 0;
            ret ^= (UInt64)(rowLeftTable[(board >> 16) & ROW_MASK]) << 16;
            ret ^= (UInt64)(rowLeftTable[(board >> 32) & ROW_MASK]) << 32;
            ret ^= (UInt64)(rowLeftTable[(board >> 48) & ROW_MASK]) << 48;
            return ret;
        }
        static UInt64 ExecuteMove3(UInt64 board)
        {
            UInt64 ret = board;
            ret ^= (UInt64)(rowRightTable[(board >> 0) & ROW_MASK]) << 0;
            ret ^= (UInt64)(rowRightTable[(board >> 16) & ROW_MASK]) << 16;
            ret ^= (UInt64)(rowRightTable[(board >> 32) & ROW_MASK]) << 32;
            ret ^= (UInt64)(rowRightTable[(board >> 48) & ROW_MASK]) << 48;
            return ret;
        }
        static public UInt64 ExecuteMove(int move, UInt64 board)
        {
            switch (move)
            {
                case 0: // up 
                    return ExecuteMove0(board);
                case 1: // down 
                    return ExecuteMove3(board);
                case 2: // left
                    return ExecuteMove1(board);
                case 3: // right 
                    return ExecuteMove2(board);
                default:
                    return ~0UL;
            }
        }
        static int GetMaxRank(UInt64 board)
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
            bitset >>= 1;
            int count = 0;
            while (bitset != 0)
            {
                bitset &= (UInt16)(bitset - 1);
                count++;
            }
            return count;
        }
        static UInt64 DrawTile()
        {
            return (UInt64)(((new Random(DateTime.Now.Millisecond)).Next(10) < 9) ? 1 : 2);
        }
        static UInt64 InsertTileRand(UInt64 board, UInt64 tile)
        {
            int index = (new Random(DateTime.Now.Millisecond)).Next(CountEmpty(board));
            UInt64 tmp = board;
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
            return board | tile;
        }
        static UInt64 InitialBoard()
        {
            UInt64 board = DrawTile() << (4 * (new Random()).Next(16));
            return InsertTileRand(board, DrawTile());
        }
        static double ScoreHelper(UInt64 board, double[] table)
        {
            return table[(board >> 0) & ROW_MASK] +
                   table[(board >> 16) & ROW_MASK] +
                   table[(board >> 32) & ROW_MASK] +
                   table[(board >> 48) & ROW_MASK];
        }
        static public double ScoreHeurBoard(UInt64 board)
        {
            return ScoreHelper(board, heurScoreTable) +
                   ScoreHelper(Transpose(board), heurScoreTable);
        }
        static double ScoreBoard(UInt64 board)
        {
            return ScoreHelper(board, scoreTable);
        }
        static public int[,] TableToInt(UInt64 board)
        {
            int i, j;
            int[,] num = new int[4, 4];

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    num[i, j] = tableNum[(int)((board) & 0xf)];
                    board >>= 4;
                }
            }
            return num;
        }
        static public UInt64 IntToTable(int[,] num)
        {
            UInt64 res = 0;
            int inp = 0;
            int cf = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0 ; j < 4; j++)
                {
                    inp = num[i, j];
                    if (inp == 0)
                    {
                        inp = 1;
                    }
                    res += ((UInt64)Math.Round(Math.Log(inp, 2))) << cf;
                    cf += 4;
                }
            }
            return res;
        }
        static double ScoreMoveNode(EvalState state, UInt64 board, float cprob) {
            double best = 0.0f;
            state.curdepth++;
            for (int move = 0; move < 4; ++move) {
                UInt64 newboard = ExecuteMove(move, board);
                state.moves_evaled++;

                if (board != newboard) {
                    best = Math.Max(best, ScoreTilechooseNode(state, newboard, cprob));
                }
            }
            state.curdepth--;

            return best;
        }
        static double ScoreTilechooseNode(EvalState state, UInt64 board, float cprob) {
            if (cprob < CPROB_THRESH_BASE || state.curdepth >= state.depthLimit) {
                state.maxdepth = Math.Max(state.curdepth, state.maxdepth);
                return ScoreHeurBoard(board);
            }

            if (state.curdepth < CACHE_DEPTH_LIMIT) {
                if (state.transTable.ContainsKey(board)) {
                    state.cachehits++;
                    return state.transTable[board];
                }
            }

            int num_open = CountEmpty(board);
            cprob /= num_open;

            double res = 0.0f;
            UInt64 tmp = board;
            UInt64 tile_2 = 1;
            while (tile_2 != 0) {
                if ((tmp & 0xf) == 0) {
                    res += ScoreMoveNode(state, board |  tile_2      , cprob * 0.9f) * 0.9f;
                    res += ScoreMoveNode(state, board | (tile_2 << 1), cprob * 0.1f) * 0.1f;
                }
                tmp >>= 4;
                tile_2 <<= 4;
            }
            res = res / num_open;

            if (state.curdepth < CACHE_DEPTH_LIMIT) {
                state.transTable[board] = res;
            }

            return res;
        }
        static double _ScoreToplevelMove(EvalState state, UInt64 board, int move) {
            UInt64 newboard = ExecuteMove(move, board);

            if(board == newboard)
                return 0;

            return ScoreTilechooseNode(state, newboard, 1.0f) + 1e-6;
        }
        static double ScoreToplevelMove(UInt64 board, int move) {
            double res;
            DateTime start, finish;
            
            double elapsed;
            EvalState state = new EvalState();
            state.transTable = new Dictionary<ulong, double>();
            state.depthLimit = Math.Max(3, count_distinct_tiles(board) - 2);

            start = DateTime.Now;
            res = _ScoreToplevelMove(state, board, move);
            finish = DateTime.Now;

            elapsed = (finish.Second - start.Second);
            elapsed += (finish.Millisecond - start.Millisecond) / 1000000.0;

            Console.WriteLine("Move " + move.ToString() + ": result " + res.ToString() + ": eval'd " + state.moves_evaled.ToString() + " moves ( " + state.cachehits.ToString() + "cache hits, " + state.transTable.Count.ToString() + " cache size) in " + elapsed.ToString() + " seconds (maxdepth=" + state.maxdepth.ToString() + ")");

            return res;
        }
        static public int FindBestMove(UInt64 board)
        {
            int move;
            double best = 0;
            int bestmove = -1;

            PrintBoard(board);
            Console.WriteLine("Current scores: heur "+ (ScoreHeurBoard(board)).ToString() +
                ", actual" + (ScoreBoard(board)).ToString());

            for (move = 0; move < 4; move++)
            {
                double res = ScoreToplevelMove(board, move);

                if (res > best)
                {
                    best = res;
                    bestmove = move;
                }
            }

            return bestmove;
        }
        static public void PlayGame(int opt = 0)
        {
            UInt64 board = InitialBoard();
            int moveno = 0;
            int scorepenalty = 0;
            while (true)
            {
                int move;
                UInt64 newboard;

                for (move = 0; move < 4; move++)
                {
                    if (ExecuteMove(move, board) != board)
                        break;
                }
                if (move == 4)
                    break;
                PrintBoard(board);
                System.Console.WriteLine("\nMove #" + (++moveno).ToString() + ", current score=" + (ScoreBoard(board) - scorepenalty).ToString() + "\n");
                move = AI.AINextMove(TableToInt(board), opt);
                if (move < 0)
                    break;
                newboard = ExecuteMove(move, board);
                if (newboard == board)
                {
                    System.Console.WriteLine("Illegal move!\n");
                    moveno--;
                    continue;
                }
                UInt64 tile = DrawTile();
                if (tile == 2) scorepenalty += 4;
                board = InsertTileRand(newboard, tile);
            }

            PrintBoard(board);
            System.Console.WriteLine("\nGame over. Your score is " + (ScoreBoard(board) - scorepenalty).ToString() +
            ". The highest rank you achieved was " + GetMaxRank(board).ToString() + ".\n");
        }
        static public double PredictScore(UInt64 board, int move)
        {
            bool first = true;
            int scorepenalty = 0;
            while (true)
            {
                UInt64 newboard;
                if (first)
                {
                    if (ExecuteMove(move, board) == board)
                        return -1;
                    first = false;
                }
                else
                {
                    for (move = 0; move < 4; move++)
                    {
                        if (ExecuteMove(move, board) != board)
                            break;
                    }
                    if (move == 4)
                        break;
                    move = AI.AINextMove(TableToInt(board), 3);
                    if (move < 0)
                        break;
                }

                newboard = ExecuteMove(move, board);
                if (newboard == board)
                {
                    //System.Console.WriteLine("Illegal move!\n");
                    continue;
                }
                UInt64 tile = DrawTile();
                if (tile == 2) scorepenalty += 4;
                board = InsertTileRand(newboard, tile);
            }
            return ScoreBoard(board) - scorepenalty;
        }
    }
}
