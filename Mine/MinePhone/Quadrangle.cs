using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
  public class Quadrangle
  {
    public Planet planet;
    public float latitude;
    public float longitude;
    public float radial_distance;
    private Block[, ,] blocks;
    public bool active;
    public bool buffer_ready;
    public int vertex_count = 0;
    public int block_vertex_index = 0;
    public VertexBuffer vertex_buffer;
    public VertexPositionColorTexture[] block_vertices;

    public Quadrangle(float latitude, float longitude, float radial_distance)
    {
      this.radial_distance = radial_distance;
      this.latitude = latitude;
      this.longitude = longitude;
      this.blocks = new Block[MineGame.chunk_size, MineGame.chunk_size, MineGame.chunk_height];
    }
    public void Generate(){
      for (int latitude_offset = 0; latitude_offset < MineGame.chunk_size; latitude_offset++)
      {
        for (int longitude_offset = 0; longitude_offset < MineGame.chunk_size; longitude_offset++)
        {
          for (int height_offset = 0; height_offset < MineGame.chunk_height; height_offset++)
          {
            var block_radial_distance = (height_offset*planet.height_step + radial_distance);
            var block_latitude = latitude + latitude_offset * planet.step;
            var block_longitude = longitude + longitude_offset * planet.step;
            var pvalue = 5; // planet.noise.GetValue(block_latitude, block_longitude * 2);
            var terrain_height = planet.radial_distance + 10*planet.height_step +  (int)(  Math.Abs(pvalue * 3000));

            var equator = 25.0; // *planet.equator_noise.GetValue(block_longitude / 3000.0);

            BlockType t;
            if ( (block_radial_distance - terrain_height ) < 1)
            {
              if (height_offset > 18)
              {
                t = BlockType.Snow;
              }
              else if(height_offset > 17)
              {
                t = BlockType.Stone;
              }
              else if (Math.Abs(block_latitude) > 50)
              {
                t = BlockType.Snow;
              }
              else if (Math.Abs(block_latitude - equator) < 7)
              {
                t = BlockType.Sand;
              }
              else
              {
                t = BlockType.Grass;
              }
            }
            else if ((int)terrain_height > block_radial_distance)
            {
              t = BlockType.Dirt;
            }
            else
            {
              t = BlockType.Air;
            }
            var b = new Block(block_latitude, block_longitude, block_radial_distance, t);
            b.active = (b.type != BlockType.Air);
            blocks[latitude_offset, longitude_offset, height_offset] = b;
          }
        }
      }
    }
    public void Cull(bool combine)
    {

      for (int x = 0; x < MineGame.chunk_size; x++)
      {
        for (int y = 0; y < MineGame.chunk_size; y++)
        {
            for (int z = 0; z < MineGame.chunk_height; z++)
            {
               Block cur = blocks[x, y, z];

              if (!cur.active)
              {
                continue;
              }

              cur.render_faces[Block.top_face] =  Convert.ToInt16(z == MineGame.chunk_height - 1 || !blocks[x, y, z + 1].active);
              cur.render_faces[Block.bottom_face] = Convert.ToInt16((z == 0 || !blocks[x, y, z - 1].active) && this.radial_distance != planet.radial_distance);
              cur.render_faces[Block.north_face] = Convert.ToInt16(x == MineGame.chunk_size - 1 || !blocks[x + 1, y, z].active);
              cur.render_faces[Block.south_face] = Convert.ToInt16(x == 0 || !blocks[x - 1, y, z].active);
              cur.render_faces[Block.west_face] = Convert.ToInt16(y == 0 || !blocks[x, y-1, z].active);
              cur.render_faces[Block.east_face] =  Convert.ToInt16(y == MineGame.chunk_size - 1 || !blocks[x, y+1, z].active);
            }
         }
      }
      vertex_count = 0;

      for (int x = 0; x < MineGame.chunk_size; x++)
      {
        for (int y = 0; y < MineGame.chunk_size; y++)
        {
          int starting_index = 0;
          for (int z = 0; z < MineGame.chunk_height; z++)
          {
            Block cur = blocks[x, y, z];
            cur.combine_primary = false;

            bool isLastBlockInColumn = true;// !combine || (y == MineGame.chunk_height - 1) || !cur.are_combinable(blocks[x, y + 1, z]);
            var bottom_height_offset = starting_index - y;

            if (!isLastBlockInColumn)
            {
              continue;
            }
            else
            {
              starting_index = y + 1;
            }
            if (cur.rendered_vertice_count() == 0)
            {
              continue;
            }
            cur.combine_height = bottom_height_offset;
            cur.combine_primary = true;

            vertex_count += 6 * cur.rendered_vertice_count();
          }
        }
      }
    }
    public void UpdateBuffer()
    {
      if (vertex_count != 0)
      {
        block_vertices = new VertexPositionColorTexture[vertex_count];
        block_vertex_index = 0;
      }
      for (int x = 0; x < MineGame.chunk_size; x++)
      {
        for (int y = 0; y < MineGame.chunk_size; y++)
        {
          for (int z= 0; z < MineGame.chunk_height; z++)
          {
            Block cur = blocks[x, y, z];
            if (cur.rendered_vertice_count() == 0)
            {
                continue;
            }
            float bottom = cur.radial_distance;
            float top = cur.radial_distance + planet.height_step;

            Vector3 north_east_top = ToCartesian(top, cur.latitude + planet.step, cur.longitude + planet.step);
            Vector3 north_west_top = ToCartesian(top, cur.latitude + planet.step, cur.longitude);
            Vector3 south_east_top = ToCartesian(top, cur.latitude, cur.longitude + planet.step);
            Vector3 south_west_top = ToCartesian(top, cur.latitude, cur.longitude);
            Vector3 north_east_bottom = ToCartesian(bottom, cur.latitude + planet.step, cur.longitude + planet.step);
            Vector3 north_west_bottom = ToCartesian(bottom, cur.latitude + planet.step, cur.longitude);
            Vector3 south_east_bottom = ToCartesian(bottom, cur.latitude, cur.longitude + planet.step);
            Vector3 south_west_bottom = ToCartesian(bottom, cur.latitude, cur.longitude);


            for (int i = 0; i < 6; i++)
            {
              if (cur.render_faces[i] != 1) // dont render it
              {
                continue;
              }
              var a = Color.White;
              if (cur.type == BlockType.Grass && i == Block.top_face)
              {
                a = Color.FromNonPremultiplied(107, 168,64,255);
              }


              int offset = block_vertex_index;

              Vector2 textureTopLeft = planet.game.texture_coordinates[cur.type][i, Block.textureTopLeft];
              Vector2 textureBottomLeft = planet.game.texture_coordinates[cur.type][i, Block.textureBottomLeft];
              Vector2 textureTopRight = planet.game.texture_coordinates[cur.type][i, Block.textureTopRight];
              Vector2 textureBottomRight = planet.game.texture_coordinates[cur.type][i, Block.textureBottomRight];

              switch (i + 1)
              {
                case 1: //front   //north
                  block_vertices[offset + 0] = new VertexPositionColorTexture(north_west_bottom, a, textureBottomRight);
                  block_vertices[offset + 1] = new VertexPositionColorTexture(north_west_top, a, textureTopRight);
                  block_vertices[offset + 2] = new VertexPositionColorTexture(north_east_top, a, textureTopLeft);
                  block_vertices[offset + 3] = new VertexPositionColorTexture(north_east_top, a, textureTopLeft);
                  block_vertices[offset + 4] = new VertexPositionColorTexture(north_east_bottom, a, textureBottomLeft);
                  block_vertices[offset + 5] = new VertexPositionColorTexture(north_west_bottom, a, textureBottomRight);
                  break;
                case 2: //back  //south
                  block_vertices[offset + 0] = new VertexPositionColorTexture(south_west_bottom, a, textureBottomLeft);
                  block_vertices[offset + 1] = new VertexPositionColorTexture(south_east_bottom, a, textureBottomRight);
                  block_vertices[offset + 2] = new VertexPositionColorTexture(south_east_top, a, textureTopRight);
                  block_vertices[offset + 3] = new VertexPositionColorTexture(south_east_top, a, textureTopRight);
                  block_vertices[offset + 4] = new VertexPositionColorTexture(south_west_top, a, textureTopLeft);
                  block_vertices[offset + 5] = new VertexPositionColorTexture(south_west_bottom, a, textureBottomLeft);
                  break;
                case 3: //top
                  block_vertices[offset + 0] = new VertexPositionColorTexture(north_west_bottom, a, textureTopLeft);
                  block_vertices[offset + 1] = new VertexPositionColorTexture(south_east_bottom, a, textureBottomRight);
                  block_vertices[offset + 2] = new VertexPositionColorTexture(south_west_bottom, a, textureBottomLeft);
                  block_vertices[offset + 5] = new VertexPositionColorTexture(south_east_bottom, a, textureBottomRight);
                  block_vertices[offset + 4] = new VertexPositionColorTexture(north_east_bottom, a, textureTopRight);
                  block_vertices[offset + 3] = new VertexPositionColorTexture(north_west_bottom, a, textureTopLeft);
                  break;
                case 4: //Bottom
                  block_vertices[offset + 0] = new VertexPositionColorTexture(south_west_top, a, textureBottomLeft);
                  block_vertices[offset + 1] = new VertexPositionColorTexture(south_east_top, a, textureBottomRight);
                  block_vertices[offset + 2] = new VertexPositionColorTexture(north_west_top, a, textureTopLeft);
                  block_vertices[offset + 3] = new VertexPositionColorTexture(south_east_top, a, textureBottomRight);
                  block_vertices[offset + 4] = new VertexPositionColorTexture(north_east_top, a, textureTopRight);
                  block_vertices[offset + 5] = new VertexPositionColorTexture(north_west_top, a, textureTopLeft);
                  break;
                case 5:  //east
                  block_vertices[offset + 0] = new VertexPositionColorTexture(north_east_top, a, textureTopRight);
                  block_vertices[offset + 1] = new VertexPositionColorTexture(south_east_top, a, textureTopLeft);
                  block_vertices[offset + 2] = new VertexPositionColorTexture(south_east_bottom, a, textureBottomLeft);
                  block_vertices[offset + 3] = new VertexPositionColorTexture(south_east_bottom, a, textureBottomLeft);
                  block_vertices[offset + 4] = new VertexPositionColorTexture(north_east_bottom, a, textureBottomRight);
                  block_vertices[offset + 5] = new VertexPositionColorTexture(north_east_top, a, textureTopRight);
                  break;
                case 6: //west
                  block_vertices[offset + 0] = new VertexPositionColorTexture(south_west_bottom, a, textureBottomRight);
                  block_vertices[offset + 1] = new VertexPositionColorTexture(south_west_top, a, textureTopRight);
                  block_vertices[offset + 2] = new VertexPositionColorTexture(north_west_top, a, textureTopLeft);
                  block_vertices[offset + 5] = new VertexPositionColorTexture(south_west_bottom, a, textureBottomRight);
                  block_vertices[offset + 4] = new VertexPositionColorTexture(north_west_bottom, a, textureBottomLeft);
                  block_vertices[offset + 3] = new VertexPositionColorTexture(north_west_top, a, textureTopLeft);
                  break;
              }
              block_vertex_index += 6;
            }
          }
        }
      }
    }

    public Vector3 ToCartesian(float r, float latitude, float longitude)
    {
      return new Vector3(
        (float)(r * Math.Cos(MathHelper.ToRadians(latitude)) * Math.Cos(MathHelper.ToRadians(longitude))),

          (float)(r * Math.Sin(MathHelper.ToRadians(latitude))),
         (float)(r * Math.Cos(MathHelper.ToRadians(latitude)) * Math.Sin(MathHelper.ToRadians(longitude))));
    }
  }
}
