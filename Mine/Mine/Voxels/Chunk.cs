using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine.Voxels
{
    public class Chunk
    {
        private int chunk_x;
        private int chunk_y;
        private int chunk_z;
        private Block[, ,] blocks;
        private MineGame game;
        public bool active;
        public Dictionary<BlockType, BlockSet> block_sets;

        public Chunk(MineGame g, int x, int y, int z)
        {
            this.chunk_x = x;
            this.chunk_y = y;
            this.chunk_z = z;
            this.game = g;
            this.blocks = new Block[MineGame.chunk_size, MineGame.chunk_size, MineGame.chunk_size];

            block_sets = new Dictionary<BlockType, BlockSet>();

            var oak = new BlockSet(BlockType.Oak,BlockType.Oak_Top);
            block_sets.Add(BlockType.Oak, oak);
            var dirt = new BlockSet(BlockType.Dirt, null);
            block_sets.Add(BlockType.Dirt, dirt);
            var snow = new BlockSet(BlockType.Snow, BlockType.Snow_Top);
            block_sets.Add(BlockType.Snow, snow);
        }
        public void Generate(SharpNoise.Models.Plane noise_plane)
        {
            for (int x = 0; x < MineGame.chunk_size; x++)
            {
                for (int z = 0; z < MineGame.chunk_size; z++)
                {
                    for (int y = 0; y < MineGame.chunk_size; y++)
                    {
                        var pvalue = noise_plane.GetValue((chunk_x + x) / 200.0, (chunk_z + z) / 200.0);
                        var height = 15  + pvalue * 10;

                        BlockType t;
                        if (height > (chunk_y + y))
                        {
                            t = BlockType.Snow;
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
            if (chunk_x > 3 || chunk_z > 3) { return; }

            Random random = new Random( (int)( 10000* noise_plane.GetValue((chunk_x) / 200.0, (chunk_z) / 200.0)));

            for (int i = 0; i <3; i++)
            {
                int tree_x = random.Next(0, 15);
                int tree_z = random.Next(0, 15);
                var tree_pvalue = noise_plane.GetValue((chunk_x + tree_x) / 200.0, (chunk_z + tree_z) / 200.0);
                var tree_height = (int)(Math.Floor(15 + tree_pvalue * 10));

                if (tree_height > chunk_y && (tree_height + 4) < chunk_y + MineGame.chunk_size)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        blocks[tree_x, tree_height - chunk_y + m, tree_z].type = BlockType.Oak;
                        blocks[tree_x, tree_height - chunk_y + m, tree_z].active = true;
                    }
                }
            }
        }

        public void Cull(){
            foreach(BlockType t in block_sets.Keys){
               for(int i = 0; i < 6; i++){
                   block_sets[t].vertex_counts[i] = 0;
               }
            }
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
                        var block_set = block_sets[cur.type];

                        cur.render_faces[Block.YPositive] = Convert.ToInt16(y == MineGame.chunk_size - 1 || !blocks[x, y + 1, z].active);
                        cur.render_faces[Block.XNegative] = Convert.ToInt16(x == 0 || !blocks[x - 1, y, z].active);
                        cur.render_faces[Block.XPositive] = Convert.ToInt16(x == MineGame.chunk_size - 1 || !blocks[x + 1, y, z].active);
                        cur.render_faces[Block.YNegative] = Convert.ToInt16(y == 0 || !blocks[x, y - 1, z].active);
                        cur.render_faces[Block.ZNegative] = Convert.ToInt16(z == 0 || !blocks[x, y, z - 1].active);
                        cur.render_faces[Block.ZPositive] = Convert.ToInt16(z == MineGame.chunk_size - 1 || !blocks[x, y, z + 1].active);

                        for(int i = 0; i < 6; i++){
                            if(block_set.textures[i] != null){
                                block_set.vertex_counts[i] += 6*cur.render_faces[i];
                            } else {
                                block_set.vertex_counts[0] += 6*cur.render_faces[i];
                            }
                        }
                    }
                }
            }
        }
        public void UpdateBuffer(GraphicsDevice graphics_device)
        {
            foreach(BlockType t in block_sets.Keys){
                var block_set = block_sets[t];
                for(int i = 0; i < 6; i++){
                    if(block_set.textures[i] != null){
                          int count =  block_set.vertex_counts[i];
                          if (count != 0)
                          {
                              block_set.block_vertices[i] = new VertexPositionNormalTexture[block_set.vertex_counts[i]];
                              block_set.block_vertex_index[i] = 0;
                              block_set.vertex_buffers[i] = new VertexBuffer(graphics_device, VertexPositionNormalTexture.VertexDeclaration, count, BufferUsage.WriteOnly);
                          }
                    } 
                }
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

                        Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
                        Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
                        Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f) ;
                        Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
                        Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
                        Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

                        Vector2 textureTopLeft = new Vector2(1.0f, 0.0f);
                        Vector2 textureTopRight = new Vector2(0.0f, 0.0f);
                        Vector2 textureBottomLeft = new Vector2(1.0f, 1.0f);
                        Vector2 textureBottomRight = new Vector2(0.0f, 1.0f);

                        var block_set = block_sets[cur.type];

                        if (cur.render_faces[Block.ZNegative] == 1) //front
                        {
                            int index = 0;
                            if(block_set.textures[Block.ZNegative] != null){
                                index = Block.ZNegative;
                            }
                            int offset = block_set.block_vertex_index[index];
                            block_set.block_vertices[index][offset + 0] = new VertexPositionNormalTexture(topLeftFront, frontNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 1] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 2] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);
                            block_set.block_vertices[index][offset + 3] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 4] = new VertexPositionNormalTexture(bottomRightFront, frontNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 5] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);
                            block_set.block_vertex_index[index] += 6;
                        }
                        if (cur.render_faces[Block.ZPositive] == 1) //back
                        {
                            int index = 0;
                            if (block_set.textures[Block.ZPositive] != null)
                            {
                                index = Block.ZPositive;
                            }
                            int offset = block_set.block_vertex_index[index];
                            block_set.block_vertices[index][offset + 0] = new VertexPositionNormalTexture(topLeftBack, backNormal, textureTopRight);
                            block_set.block_vertices[index][offset + 1] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 2] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 3] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 4] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 5] = new VertexPositionNormalTexture(bottomRightBack, backNormal, textureBottomLeft);

                            block_set.block_vertex_index[index] += 6;
                        }
                        if (cur.render_faces[Block.YPositive] == 1) //top
                        {
                            int index = 0;
                            if (block_set.textures[Block.YPositive] != null)
                            {
                                index = Block.YPositive;
                            }
                            int offset = block_set.block_vertex_index[index];
                            block_set.block_vertices[index][offset + 0] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 1] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);
                            block_set.block_vertices[index][offset + 2] = new VertexPositionNormalTexture(topLeftBack, topNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 3] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 4] = new VertexPositionNormalTexture(topRightFront, topNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 5] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);

                            block_set.block_vertex_index[index] += 6;
                        }
                        if (cur.render_faces[Block.YNegative] == 1) // bottom
                        {
                            int index = 0;
                            if (block_set.textures[Block.YNegative] != null)
                            {
                                index = Block.YNegative;
                            }
                            int offset = block_set.block_vertex_index[index];
                            block_set.block_vertices[index][offset + 0] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 1] = new VertexPositionNormalTexture(bottomLeftBack, bottomNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 2] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 3] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 4] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 5] = new VertexPositionNormalTexture(bottomRightFront, bottomNormal, textureTopRight);

                            block_set.block_vertex_index[index] += 6;
                        }
                        if (cur.render_faces[Block.XNegative] == 1) //left
                        {
                            int index = 0;
                            if (block_set.textures[Block.XNegative] != null)
                            {
                                index = Block.XNegative;
                            }
                            int offset = block_set.block_vertex_index[index];

                            block_set.block_vertices[index][offset + 0] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);
                            block_set.block_vertices[index][offset + 1] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 2] = new VertexPositionNormalTexture(bottomLeftFront, leftNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 3] = new VertexPositionNormalTexture(topLeftBack, leftNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 4] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 5] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);

                            block_set.block_vertex_index[index] += 6;
                        }
                        if (cur.render_faces[Block.XPositive] == 1) //right
                        {
                            int index = 0;
                            if (block_set.textures[Block.XPositive] != null)
                            {
                                index = Block.XPositive;
                            }
                            int offset = block_set.block_vertex_index[index];

                            block_set.block_vertices[index][offset + 0] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 1] = new VertexPositionNormalTexture(bottomRightFront, rightNormal, textureBottomLeft);
                            block_set.block_vertices[index][offset + 2] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);
                            block_set.block_vertices[index][offset + 3] = new VertexPositionNormalTexture(topRightBack, rightNormal, textureTopRight);
                            block_set.block_vertices[index][offset + 4] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
                            block_set.block_vertices[index][offset + 5] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);

                            block_set.block_vertex_index[index] += 6;
                        }
                    }
                }
            }

            foreach (BlockType t in block_sets.Keys)
            {
                var block_set = block_sets[t];
                for (int i = 0; i < 6; i++)
                {

                    if (block_set.vertex_buffers[i] != null)
                    {
                        block_set.vertex_buffers[i].SetData(block_set.block_vertices[i]);
                        block_set.block_vertices[i] = null;
                    }
                }
            }
        }
    }
}
