using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mine;
using SharpNoise.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mine
{
  public class World
  {
      public MineGame game;
      public GraphicsDevice GraphicsDevice;

      public ConcurrentDictionary<Point3, Chunk> requested_chunks;
      public ConcurrentDictionary<Point3, Chunk> loaded_chunks;
      private Simplex noisemodule;
      private SharpNoise.Models.Plane p;

      public World()
      {
        this.noisemodule = new Simplex();
        this.p =  new SharpNoise.Models.Plane(noisemodule);
      }

      public List<Point3> Near(Vector3 position, int distance)
      {
        var nearest = new List<Point3>();
        int x = (int) position.X / 16;
        int z = (int) position.Z / 16;

        for (int y = 0; y < 2; y++)
        {
          for (int offset_x = -distance; offset_x < distance; offset_x++)
          {
            for (int offset_z = -distance; offset_z < distance; offset_z++)
            {
              nearest.Add(new Point3(offset_x+x, y, z + offset_z));
            }
          }
        }
        return nearest;
      }
      public Chunk Generate(int x,int y, int z)
      {
        var chunk = new Chunk(game, x * MineGame.chunk_size, y * MineGame.chunk_size, z * MineGame.chunk_size);
        chunk.active = false;
        chunk.Generate(p);
        return chunk;
      }
  }
}
