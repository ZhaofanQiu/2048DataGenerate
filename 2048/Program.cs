using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Board = System.UInt64;
using Row = System.UInt16;

namespace _2048AI
{
    class Program
    {
        static void Main(string[] args)
        {
            BoardControl.Initialize();
            PreScore.Initialize();
            //byte[] bb = new byte[9];
            //Board tile;
            //for (int i = 4; i < 1000; i++ )
            //{
            //    Search.fs = new System.IO.FileStream("2048game" + i.ToString() + ".dat", System.IO.FileMode.Create);
            //    Search.bw = new System.IO.BinaryWriter(Search.fs);
            //    //PlayGame.Play(AI.SearchMove);
            //    for (int j = 0; j < 10000; j++ )
            //    {
            //        Board x;
            //        (new Random(DateTime.Now.Millisecond)).NextBytes(bb);
            //        x = (Board)(bb[1] * (bb[0] & 1)) + ((Board)(bb[2] * (bb[0] & 2)) << 8) +  
            //            ((Board)(bb[3] * (bb[0] & 4))<< 16) + 
            //            ((Board)(bb[4] * (bb[0] & 8))<< 24) + 
            //            ((Board)(bb[5] * (bb[0] & 16))<< 32) + 
            //            ((Board)(bb[6] * (bb[0] & 32))<< 40) + 
            //            ((Board)(bb[7] * (bb[0] & 64))<< 48) +
            //            ((Board)(bb[8] * (bb[0] & 128)) << 56);
            //        x = BoardControl.ExecuteMove(x, (new Random(DateTime.Now.Millisecond)).Next() % 4);
            //        tile = BoardControl.DrawTile();
            //        x = BoardControl.InsertTileRand(x, tile);
            //        x = BoardControl.ExecuteMove(x, (new Random(DateTime.Now.Millisecond)).Next() % 4);
            //        tile = BoardControl.DrawTile();
            //        x = BoardControl.InsertTileRand(x, tile);
            //        x = BoardControl.ExecuteMove(x, (new Random(DateTime.Now.Millisecond)).Next() % 4);
            //        tile = BoardControl.DrawTile();
            //        x = BoardControl.InsertTileRand(x, tile);
            //        x = BoardControl.ExecuteMove(x, (new Random(DateTime.Now.Millisecond)).Next() % 4);
            //        tile = BoardControl.DrawTile();
            //        x = BoardControl.InsertTileRand(x, tile);
                    
            //        Search.FindBestMove(x);
            //        Console.WriteLine(i.ToString() + '\t' + j.ToString());
            //    }
            //    Search.bw.Close();
            //    Search.fs.Close();
            //}
            PlayGame.Play(AI.SearchMove);
        }
    }
}
