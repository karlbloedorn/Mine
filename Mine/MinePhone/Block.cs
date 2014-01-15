using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
    public enum BlockType
    {
        Air,
        Dirt,
        Snow,
        Grass,
        Stone,
        Cobblestone,
        IronOre,
        DiamondOre,
        GoldOre,
        Coal,
        Gravel,
        Sand,
        Crafting_Table,
        Oak_Wood,
        Oak_Leaves,
        Birch_Wood,
        Birch_Leaves,
    }

    public class Block
    {
        public const int north_face = 0;
        public const int south_face = 1;
        public const int bottom_face = 2;
        public const int top_face = 3;
        public const int east_face = 4;
        public const int west_face = 5;
        public const int textureBottomLeft = 0;
        public const int textureTopLeft = 1;
        public const int textureTopRight = 2;
        public const int textureBottomRight = 3;
        public float latitude = 0;
        public float longitude = 0;
        public float radial_distance = 0;
        public BlockType type;
        public bool active;
        public short[] render_faces = new short[6];
        public bool combine_primary;
        public int combine_height;
        
        public Block(float latitude, float longitude, float radial_distance, BlockType t)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.radial_distance = radial_distance;
            this.type = t;
        }
        public int rendered_vertice_count()
        {
            return render_faces[0] + render_faces[1] + render_faces[2] + render_faces[3] + render_faces[4] + render_faces[5];
        }

        public bool are_combinable(Block other)
        {
          return  this.type == other.type && Enumerable.SequenceEqual( this.render_faces ,other.render_faces);
        }

    }
}
