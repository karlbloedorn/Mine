using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
  class Tree
  {

    /*
           if (chunk_x > 3 || chunk_z > 3) { return; }

          
           Random random = new Random( (int)( 10000* noise_plane.GetValue((chunk_x) / 205.0, (chunk_z) / 205.0)));

           for (int i = 0; i <2; i++)
           {
               int tree_x = random.Next(4, 12);
               int tree_z = random.Next(4, 12);
               var tree_pvalue = noise_plane.GetValue((chunk_x + tree_x) / 200.0, (chunk_z + tree_z) / 200.0);
               var tree_height = (int)(Math.Floor(15 + tree_pvalue * 10));

               int base_tree_level = tree_height - chunk_y-1;
               if (tree_height > chunk_y && (tree_height + 7) < chunk_y + MineGame.chunk_height && tree_x > 4 && tree_x < (MineGame.chunk_size - 4) && tree_z > 4 && tree_z < (MineGame.chunk_size - 4))
               {
                   for (int m = 0; m < 8; m++)
                   {
                     int level = base_tree_level + m;
                     if (m < 7)
                     {
                       blocks[tree_x, level, tree_z].type = BlockType.Oak_Wood;
                       blocks[tree_x, level, tree_z].active = true;
                     }
                     if (m == 4 || m == 5 )
                     {
                       for (int a = -2; a < 3;a++)
                       {
                         for (int b = -2; b < 3; b++)
                         {
                            if( (a != 0 || b != 0) &&  (Math.Abs(a) + Math.Abs( b ) != 4) ){
                              blocks[tree_x + a, level, tree_z+b].type = BlockType.Oak_Leaves;
                              blocks[tree_x + a, level, tree_z+b].active = true;
                            }
                         }
                       }
                     }
                     if (m == 6 || m == 7)
                     {
                       blocks[tree_x + 1, level, tree_z].type = BlockType.Oak_Leaves;
                       blocks[tree_x + 1, level, tree_z].active = true;

                       blocks[tree_x - 1, level, tree_z].type = BlockType.Oak_Leaves;
                       blocks[tree_x - 1, level, tree_z].active = true;

                       blocks[tree_x, level, tree_z - 1].type = BlockType.Oak_Leaves;
                       blocks[tree_x, level, tree_z - 1].active = true;

                       blocks[tree_x, level, tree_z + 1].type = BlockType.Oak_Leaves;
                       blocks[tree_x, level, tree_z + 1].active = true;
                     }
                     if (m == 7)
                     {
                       blocks[tree_x, level, tree_z].type = BlockType.Oak_Leaves;
                       blocks[tree_x, level, tree_z].active = true;
                     }
                   }
               }
           
         }*/
  }
}
