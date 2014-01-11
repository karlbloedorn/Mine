#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Mine
{
  public static class Program
  {
        [STAThread]
        static void Main()
        {
            using (var game = new MineGame())
                game.Run();
        }
  }
}
