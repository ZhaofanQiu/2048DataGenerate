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
            PlayGame.Play(AI.SearchMove);
        }
    }
}
