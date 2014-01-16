using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
    public class Chunk
    {
        private int chunk_x;
        private int chunk_y;
        private int chunk_z;
        public Block[, ,] blocks;
        private MineGame game;
        public bool active;
        public bool buffer_ready;
        public int vertex_count = 0;
        public VertexBuffer vertex_buffer;
        public VertexPositionColorTexture[] block_vertices;
        public int block_vertex_index = 0;
        public Chunk(MineGame g, int x, int y, int z)
        {
            this.chunk_x = x;
            this.chunk_y = y;
            this.chunk_z = z;
            this.game = g;
            this.blocks = new Block[MineGame.chunk_size, MineGame.chunk_size, MineGame.chunk_size];
        }
        public void Generate(SharpNoise.Models.Plane noise_plane)
        {
            for (int x = 0; x < MineGame.chunk_size; x++)
            {
                for (int z = 0; z < MineGame.chunk_size; z++)
                {
                    for (int y = 0; y < MineGame.chunk_size; y++)
                    {
                        var pvalue = noise_plane.GetValue((chunk_x + x) / 125.5, (chunk_z + z) / 125.5);
                        var height = 5  + pvalue * 4;
                     
                        BlockType t;
                        if ((int) height == (chunk_y + y))
                        {
                          t = BlockType.Grass;
                        }
                        else if ((int)height > (chunk_y + y))
                        {
                          t = BlockType.Dirt;
                        }
                        else
                        {
                          t = BlockType.Air;
                        }
                        var b = new Block(chunk_x + x, chunk_y + y, chunk_z + z, t);
                        b.active = (b.type != BlockType.Air);
                        blocks[x, y, z] = b;
                    }
                }
          
            }

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
                if (tree_height > chunk_y && (tree_height + 7) < chunk_y + MineGame.chunk_size && tree_x > 4 && tree_x < (MineGame.chunk_size - 4) && tree_z > 4 && tree_z < (MineGame.chunk_size - 4))
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
        public void Cull(){
            vertex_count = 0;
            for (int x = 0; x < MineGame.chunk_size; x++)
            {
                for (int y = 0; y < MineGame.chunk_size; y++)
                {
                    for (int z = 0; z < MineGame.chunk_size; z++)
                    {
                        Block cur = blocks[x, y, z];

                        if (!cur.active)
                        {
                            continue;
                        }
                        cur.render_faces[Block.YPositive] = Convert.ToInt16(y == MineGame.chunk_size - 1 || !blocks[x, y + 1, z].active);
                        cur.render_faces[Block.XNegative] = Convert.ToInt16(x == 0 || !blocks[x - 1, y, z].active);
                        cur.render_faces[Block.XPositive] = Convert.ToInt16(x == MineGame.chunk_size - 1 || !blocks[x + 1, y, z].active);
                        cur.render_faces[Block.YNegative] = Convert.ToInt16( (y == 0 || !blocks[x, y - 1, z].active) && chunk_y != 0);
                        cur.render_faces[Block.ZNegative] = Convert.ToInt16(z == 0 || !blocks[x, y, z - 1].active);
                        cur.render_faces[Block.ZPositive] = Convert.ToInt16(z == MineGame.chunk_size - 1 || !blocks[x, y, z + 1].active);
                        vertex_count += 6 * cur.renderedVerticeCount();
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
                    for (int z = 0; z < MineGame.chunk_size; z++)
                    {
                        Block cur = blocks[x, y, z];
                        if (cur.renderedVerticeCount() == 0)
                        {
                            continue;
                        }
                        Vector3 topLeftFront = cur.shapePosition + new Vector3(-0.5f, 0.5f, -0.5f);
                        Vector3 bottomLeftFront = cur.shapePosition + new Vector3(-0.5f, -0.5f, -0.5f);
                        Vector3 topRightFront = cur.shapePosition + new Vector3(0.5f, 0.5f, -0.5f);
                        Vector3 bottomRightFront = cur.shapePosition + new Vector3(0.5f, -0.5f, -0.5f);
                        Vector3 topLeftBack = cur.shapePosition + new Vector3(-0.5f, 0.5f, 0.5f);
                        Vector3 topRightBack = cur.shapePosition + new Vector3(0.5f, 0.5f, 0.5f);
                        Vector3 bottomLeftBack = cur.shapePosition + new Vector3(-0.5f, -0.5f, 0.5f);
                        Vector3 bottomRightBack = cur.shapePosition + new Vector3(0.5f, -0.5f, 0.5f);
 
                        for (int i = 0; i < 6; i++)
                        {
                          if (cur.render_faces[i] == 1) //render it
                          {
                            var color = Color.White;
                            if (cur.type == BlockType.Grass && i == Block.YPositive)
                            {
                              color = Color.FromNonPremultiplied(107, 168, 64, 255);
                            } 

                            Vector2 textureTopLeft = game.texture_coordinates[cur.type][i, Block.textureTopLeft];
                            Vector2 textureBottomLeft = game.texture_coordinates[cur.type][i, Block.textureBottomLeft];
                            Vector2 textureTopRight = game.texture_coordinates[cur.type][i, Block.textureTopRight];
                            Vector2 textureBottomRight = game.texture_coordinates[cur.type][i, Block.textureBottomRight];

                            int offset = block_vertex_index;

                            switch (i)
                            {
                              case Block.ZNegative: //front
                                block_vertices[offset + 0] = new VertexPositionColorTexture(topLeftFront, color, textureTopLeft);
                                block_vertices[offset + 1] = new VertexPositionColorTexture(bottomLeftFront, color, textureBottomLeft);
                                block_vertices[offset + 2] = new VertexPositionColorTexture(topRightFront, color, textureTopRight);
                                block_vertices[offset + 3] = new VertexPositionColorTexture(bottomLeftFront, color, textureBottomLeft);
                                block_vertices[offset + 4] = new VertexPositionColorTexture(bottomRightFront, color, textureBottomRight);
                                block_vertices[offset + 5] = new VertexPositionColorTexture(topRightFront, color, textureTopRight);
                                break;
                              case Block.ZPositive: //back
                                block_vertices[offset + 0] = new VertexPositionColorTexture(topLeftBack, color, textureTopRight);
                                block_vertices[offset + 1] = new VertexPositionColorTexture(topRightBack, color, textureTopLeft);
                                block_vertices[offset + 2] = new VertexPositionColorTexture(bottomLeftBack, color, textureBottomRight);
                                block_vertices[offset + 3] = new VertexPositionColorTexture(bottomLeftBack, color, textureBottomRight);
                                block_vertices[offset + 4] = new VertexPositionColorTexture(topRightBack, color, textureTopLeft);
                                block_vertices[offset + 5] = new VertexPositionColorTexture(bottomRightBack, color, textureBottomLeft);
                                break;
                              case Block.YPositive: //top
                                block_vertices[offset + 0] = new VertexPositionColorTexture(topLeftFront, color, textureBottomLeft);
                                block_vertices[offset + 1] = new VertexPositionColorTexture(topRightBack, color, textureTopRight);
                                block_vertices[offset + 2] = new VertexPositionColorTexture(topLeftBack, color, textureTopLeft);
                                block_vertices[offset + 3] = new VertexPositionColorTexture(topLeftFront, color, textureBottomLeft);
                                block_vertices[offset + 4] = new VertexPositionColorTexture(topRightFront, color, textureBottomRight);
                                block_vertices[offset + 5] = new VertexPositionColorTexture(topRightBack, color, textureTopRight);
                                break;
                              case Block.YNegative: //Bottom
                                block_vertices[offset + 0] = new VertexPositionColorTexture(bottomLeftFront, color, textureTopLeft);
                                block_vertices[offset + 1] = new VertexPositionColorTexture(bottomLeftBack, color, textureBottomLeft);
                                block_vertices[offset + 2] = new VertexPositionColorTexture(bottomRightBack, color, textureBottomRight);
                                block_vertices[offset + 3] = new VertexPositionColorTexture(bottomLeftFront, color, textureTopLeft);
                                block_vertices[offset + 4] = new VertexPositionColorTexture(bottomRightBack, color, textureBottomRight);
                                block_vertices[offset + 5] = new VertexPositionColorTexture(bottomRightFront, color, textureTopRight);
                                break;
                              case Block.XPositive:  //Right
                                block_vertices[offset + 0] = new VertexPositionColorTexture(topRightFront, color, textureTopLeft);
                                block_vertices[offset + 1] = new VertexPositionColorTexture(bottomRightFront, color, textureBottomLeft);
                                block_vertices[offset + 2] = new VertexPositionColorTexture(bottomRightBack, color, textureBottomRight);
                                block_vertices[offset + 3] = new VertexPositionColorTexture(topRightBack, color, textureTopRight);
                                block_vertices[offset + 4] = new VertexPositionColorTexture(topRightFront, color, textureTopLeft);
                                block_vertices[offset + 5] = new VertexPositionColorTexture(bottomRightBack, color, textureBottomRight);
                                break;
                              case Block.XNegative: //Left
                                block_vertices[offset + 0] = new VertexPositionColorTexture(topLeftFront, color, textureTopRight);
                                block_vertices[offset + 1] = new VertexPositionColorTexture(bottomLeftBack, color, textureBottomLeft);
                                block_vertices[offset + 2] = new VertexPositionColorTexture(bottomLeftFront, color, textureBottomRight);
                                block_vertices[offset + 3] = new VertexPositionColorTexture(topLeftBack, color, textureTopLeft);
                                block_vertices[offset + 4] = new VertexPositionColorTexture(bottomLeftBack, color, textureBottomLeft);
                                block_vertices[offset + 5] = new VertexPositionColorTexture(topLeftFront, color, textureTopRight);
                                break;
                            }
                            block_vertex_index += 6;
                          }
                        }
                    }
                }
            }
            if (vertex_buffer != null)
            {
                block_vertices = null;
            }
        }
    }
}
