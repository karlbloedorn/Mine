using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine.Voxels
{
    public class BlockSet
    {
        public BlockType?[] textures = new BlockType?[6];
        public int[] vertex_counts = new int[6];
        public VertexBuffer[] vertex_buffers = new VertexBuffer[6];
        public VertexPositionNormalTexture[][] block_vertices = new VertexPositionNormalTexture[6][];
        public int[] block_vertex_index = new int[6];

        public BlockSet(BlockType? all, BlockType? top)
        {
            textures[0] = all;
            textures[Block.YPositive] = top;
        }
    }
}
