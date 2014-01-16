using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpNoise.Modules;
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
      private Simplex noisemodule;
      public SharpNoise.Models.Sphere noise;
      public SharpNoise.Models.Line equator_noise;
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
        noisemodule = new Simplex();
        noise = new SharpNoise.Models.Sphere(noisemodule);
        equator_noise = new SharpNoise.Models.Line(noisemodule);
        equator_noise.Attenuate = true;
        equator_noise.SetStartPoint(0, 0, 0);
        equator_noise.SetEndPoint(360, 0, 0);
        
        float circumference = radial_distance * MathHelper.Pi;
        float blocks_around_circumference = circumference / 70;

        height_step = (float)(circumference / blocks_around_circumference * 2);


        int chunk_count = 2 * (int)Math.Round(blocks_around_circumference / (MineGame.chunk_size * 2));
        int block_count = chunk_count * MineGame.chunk_size;
        step = 360.0f / block_count;
        int total = block_count * (block_count / 2);
        chunks_latitude = chunk_count / 2;
        chunks_longitude = chunk_count ;
      }

      public List<Coordinate> Near(float latitude, float longitude, int distance)
      {
        var nearest = new List<Coordinate>();

        float each_chunk_latitude =  180.0f / chunks_latitude;
        float each_chunk_longitude =  360.0f / chunks_longitude;

        int over_chunk_latitude = (int) ( latitude / each_chunk_latitude);
        int over_chunk_longitude = (int)(longitude / each_chunk_longitude);

        for (int a = -distance; a < distance; a++)
        {
          for (int b = -distance; b < distance; b++)
          {
            float cur_latitude = (over_chunk_latitude * each_chunk_latitude) + (a * MineGame.chunk_size * step);
            float cur_longitude = (over_chunk_longitude * each_chunk_longitude) + (b * MineGame.chunk_size * step);

            nearest.Add(new Coordinate(cur_latitude, cur_longitude, this.radial_distance));
          }
        }
        return nearest;
      }
    
      public Vector3 ToCartesian(float r, float latitude, float longitude)
      {


        /*
         * return new Vector3(
          (float)(r * Math.Cos(MathHelper.ToRadians(latitude)) * Math.Cos(MathHelper.ToRadians(longitude))),

            (float)(r * Math.Sin(MathHelper.ToRadians(latitude))),
           (float)(r * Math.Cos(MathHelper.ToRadians(latitude)) * Math.Sin(MathHelper.ToRadians(longitude))));
         */
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
