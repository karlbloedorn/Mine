﻿using Microsoft.Xna.Framework;
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
        Cobblestone,
        DiamondOre,
        Grass,
        Sand,
        Oak,
        Oak_Top,
        Snow,
        Snow_Top
    }

    public class Block
    {
        public const int XNegative = 0;
        public const int XPositive = 1;
        public const int YNegative = 2;
        public const int YPositive = 3;
        public const int ZNegative = 4;
        public const int ZPositive = 5;

        public Vector3 shapePosition;
        public BlockType type;
        public bool active;

        public short[] render_faces = new short[6];

        public Block(float x, float y, float z, BlockType t)
        {
            this.shapePosition = new Vector3(x, y, z);
            this.type = t;
        }
        public int renderedVerticeCount()
        {
            return render_faces[0] + render_faces[1] + render_faces[2] + render_faces[3] + render_faces[4] + render_faces[5];
        }
    }
}
