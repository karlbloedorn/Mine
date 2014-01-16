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

      public Dictionary<Point3, Chunk> requested_chunks;
      public Dictionary<Point3, Chunk> loaded_chunks;
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
        int x = (int) position.X / MineGame.chunk_size;
        int z = (int)position.Z / MineGame.chunk_size;

        for (int y = 0; y < 1; y++)
        {
          for (int offset_x = -distance; offset_x < distance; offset_x++)
          {
            for (int offset_z = -distance; offset_z < distance; offset_z++)
            {
              double dist = Math.Sqrt(Math.Pow(offset_x , 2)+ Math.Pow(offset_z, 2));
              if (dist < distance)
              {
                nearest.Add(new Point3(offset_x + x, y, z + offset_z));
              }
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
      public Block RetrieveBlock(Vector3 v)
      {
        return RetrieveBlock((int)v.X, (int)v.Y, (int)v.Z);
      }
      public Block RetrieveBlock(float x, float y, float z)
      {
        return RetrieveBlock((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
      }
      public Block RetrieveBlock(int x, int y, int z)
      {
        int chunk_x = x / MineGame.chunk_size;
        int chunk_y = y / MineGame.chunk_size;
        int chunk_z = z / MineGame.chunk_size;

        int offset_x = x % MineGame.chunk_size;
        int offset_y = y % MineGame.chunk_size;
        int offset_z = z % MineGame.chunk_size;

        if (offset_x < 0)
        {
          chunk_x--;
          offset_x += 16;
        }
        if (offset_y < 0)
        {
          chunk_y--;
          offset_y += 16;

        }
        if (offset_z < 0)
        {
          chunk_z--;
          offset_z += 16;
        }
        var chunk_key = new Point3(chunk_x, chunk_y,chunk_z);
        if (loaded_chunks.ContainsKey(chunk_key))
        {
          return loaded_chunks[chunk_key].blocks[offset_x, offset_y, offset_z];
        }
        return null;
      }
  }
}
