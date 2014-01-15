using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using SharpNoise.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
  public class Planet
  {
      public Dictionary<Coordinate, Quadrangle> requested_quadrangles;
      public Dictionary<Coordinate, Quadrangle> loaded_quadrangles;
      //private Simplex noisemodule;
      //public SharpNoise.Models.Sphere noise;
      //public SharpNoise.Models.Line equator_noise;
      public MineGame game;
      public int vertex_count;
      public float height_step = 0;
      public float step = 0;
      public float radial_distance = 0;
      public int chunks_latitude = 0;
      public int chunks_longitude = 0;
      public Planet(float radial_distance)
      {
        this.radial_distance = radial_distance;
        //noisemodule = new Simplex();
        //noise = new SharpNoise.Models.Sphere(noisemodule);
        //equator_noise = new SharpNoise.Models.Line(noisemodule);
        //equator_noise.Attenuate = true;
        //equator_noise.SetStartPoint(0, 0, 0);
        //equator_noise.SetEndPoint(360, 0, 0);
        
        height_step = radial_distance / 70;
        float circumference = radial_distance * MathHelper.Pi;
        float blocks_around_circumference = circumference / 70;
        int chunk_count = 2 * (int)Math.Round(blocks_around_circumference / (MineGame.chunk_size * 2));
        int block_count = chunk_count * MineGame.chunk_size;
        step = 360.0f / block_count;
        int total = block_count * (block_count / 2);
        chunks_latitude = chunk_count;
        chunks_longitude = chunk_count / 2;
      }

      public List<Coordinate> Near(float latitude, float longitude, int distance)
      {
        var nearest = new List<Coordinate>();

        for (int j = -chunks_latitude / 4 + 1; j < chunks_latitude / 4 - 1; j++)
        {
          for (int i = -chunks_longitude; i < chunks_longitude; i++)
          {
            float cur_lat = step * 16 * j;
            float cur_long = step * 16 * i;
            if (Math.Pow(latitude - cur_lat, 2) + Math.Pow(longitude - cur_long, 2) < distance)
            {
              nearest.Add(new Coordinate(cur_lat, cur_long, this.radial_distance));
            }
          }
        }
        return nearest;
      }
    
      public Vector3 ToCartesian(float r, float latitude, float longitude)
      {
        return new Vector3(
          (float)(r * Math.Cos(MathHelper.ToRadians(latitude)) * Math.Cos(MathHelper.ToRadians(longitude))),

            (float)(r * Math.Sin(MathHelper.ToRadians(latitude))),
           (float)(r * Math.Cos(MathHelper.ToRadians(latitude)) * Math.Sin(MathHelper.ToRadians(longitude))));
      }

    /*
      public Chunk Generate(int x, int y, int z)
      {
        var chunk = new Chunk(game, x * MineGame.chunk_size, y * MineGame.chunk_height, z * MineGame.chunk_size);
        chunk.active = false;
        chunk.Generate(p);
        return chunk;
      }
    */

  }
}
