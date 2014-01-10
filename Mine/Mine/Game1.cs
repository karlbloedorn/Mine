using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mine.Voxels;
using SharpNoise;
using SharpNoise.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Mine
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MineGame : Game
    {
        public const int chunk_size = 16;
        const float tex_w = 256;
        const float tex_h = 272;
        const float tile_w = 16;
        const float tile_h = 16;
        const float w_factor = tile_w / tex_w;
        const float h_factor = tile_h / tex_h;

        Texture2D texture;
        public Dictionary<BlockType, Vector2[,]> texture_coordinates;

        GraphicsDeviceManager graphics;
        SpriteBatch sprite_batch;
        List<Chunk> loaded_chunks = new List<Chunk>();
        private BasicEffect cubeEffect;
        private MouseState prevMouseState;
        private Vector3 mouseRotationBuffer;   
        private Vector3 cameraPosition;
        private Vector3 cameraRotation;
        private Vector3 cameraLookAt;
        private float cameraSpeed = 10.0f;
        public Vector3 Position
        {
            get { return cameraPosition; }
            set
            {
                cameraPosition = value;
                UpdateLookAt();
            }
        }
        public Vector3 Rotation
        {
            get { return cameraRotation; }
            set
            {
                cameraRotation = value;
                UpdateLookAt();
            }
        }
        public Matrix Projection
        {
            get;
            protected set;
        }
        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            }
        }

        public MineGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }
 

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            texture = LoadTexture("stitched_blocks");
            texture_coordinates = new Dictionary<BlockType, Vector2[,]>();

            AddTextureCoordinates(BlockType.Dirt, 6, 14, 6, 14, 6, 14);
            AddTextureCoordinates(BlockType.Grass, 7, 14, 6, 12, 6, 14);
            AddTextureCoordinates(BlockType.Snow, 8, 14, 8, 13, 6, 14);
            AddTextureCoordinates(BlockType.Stone, 10,6);
            AddTextureCoordinates(BlockType.Cobblestone, 11,6);
            AddTextureCoordinates(BlockType.Gravel, 11, 5);
            AddTextureCoordinates(BlockType.Coal,7,8 );
            AddTextureCoordinates(BlockType.IronOre,8, 8);
            AddTextureCoordinates(BlockType.GoldOre,9,8 );
            AddTextureCoordinates(BlockType.DiamondOre,10,8 );
            AddTextureCoordinates(BlockType.Sand, 15,7);
            AddTextureCoordinates(BlockType.Oak_Wood,1,14,1,15,1,15);
            AddTextureCoordinates(BlockType.Oak_Leaves,1,10);
            AddTextureCoordinates(BlockType.Birch_Wood,2,14, 2,15,2,15);
            AddTextureCoordinates(BlockType.Birch_Leaves,2,10);
            AddTextureCoordinates(BlockType.Crafting_Table, 4, 9, 3, 8, 4, 13);

            Simplex noisemodule = new Simplex();
            SharpNoise.Models.Plane p = new SharpNoise.Models.Plane(noisemodule);

            for (int x = -7; x < 7; x++)
            {
              for (int z = -7; z < 7; z++)
              {
                for (int y = 0; y < 4; y++)
                {
                  var chunk = new Chunk(this, x * chunk_size, y * chunk_size, z * chunk_size);
                  chunk.Generate(p);
                  loaded_chunks.Add(chunk);
                }
              }
            }

            cubeEffect = new BasicEffect(GraphicsDevice);
            cubeEffect.LightingEnabled = true;
            cubeEffect.AmbientLightColor = new Vector3(0.5f, 0.5f, 0.5f);
            cubeEffect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            cubeEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            cubeEffect.SpecularPower = 4.0f;
            cubeEffect.Alpha = 1.0f;
           
            if (cubeEffect.LightingEnabled)
            {
                cubeEffect.DirectionalLight1.Enabled = true;
                if (cubeEffect.DirectionalLight1.Enabled)
                {
                    // y direction
                    cubeEffect.DirectionalLight1.DiffuseColor = new Vector3(0.35f, 0.35f, 0.35f);
                    cubeEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
                    cubeEffect.DirectionalLight1.SpecularColor = Vector3.One;
                }
                cubeEffect.PreferPerPixelLighting = true;
            }
             
            cubeEffect.TextureEnabled = true;

            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio,
                0.05f,
                1000.0f);
            cubeEffect.Projection = Projection;

            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            MoveTo(new Vector3(30, 50, 30), new Vector3(0.0f, 0.0f, 0.0f));

            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;
            Mouse.SetPosition(centerX, centerY);

            prevMouseState = Mouse.GetState();

            RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.FillMode = FillMode.WireFrame;
            //rasterizerState.CullMode = CullMode.None;

            graphics.SynchronizeWithVerticalRetrace = true;
            rasterizerState.MultiSampleAntiAlias = true;
            GraphicsDevice.RasterizerState = rasterizerState;

            this.IsMouseVisible = false;
            base.Initialize();
        }


        private void AddTextureCoordinates(BlockType t, short x, short y)
        {
          AddTextureCoordinates(t, x, y, x, y, x, y);
        }
        private void AddTextureCoordinates(BlockType t, short sides_x, short sides_y, short top_x, short top_y, short bottom_x, short bottom_y)
        {
          short[,] faces = new short[6, 2];
          faces[Block.XNegative, 0] = sides_x;
          faces[Block.XPositive, 0] = sides_x;
          faces[Block.YNegative, 0] = bottom_x;
          faces[Block.YPositive, 0] = top_x;
          faces[Block.ZNegative, 0] = sides_x;
          faces[Block.ZPositive, 0] = sides_x;
          faces[Block.XNegative, 1] = sides_y;
          faces[Block.XPositive, 1] = sides_y;
          faces[Block.YNegative, 1] = bottom_y;
          faces[Block.YPositive, 1] = top_y;
          faces[Block.ZNegative, 1] = sides_y;
          faces[Block.ZPositive, 1] = sides_y;

          Vector2[,] cur_coordinates = new Vector2[6, 4];

          for (int i = 0; i < 6; i++)
          {
            short col = faces[i, 0];
            short row = faces[i, 1];
            float x_tex_beg = w_factor * (col - 1 + 0);
            float x_tex_end = w_factor * (col - 1 + 1);
            float y_tex_beg = h_factor * (row - 1 + 0);
            float y_tex_end = h_factor * (row - 1 + 1);
            cur_coordinates[i, Block.textureBottomLeft] = new Vector2(x_tex_beg, y_tex_end);
            cur_coordinates[i, Block.textureTopLeft] = new Vector2(x_tex_beg, y_tex_beg);
            cur_coordinates[i, Block.textureTopRight] = new Vector2(x_tex_end, y_tex_beg);
            cur_coordinates[i, Block.textureBottomRight] = new Vector2(x_tex_end, y_tex_end);
          }
          texture_coordinates.Add(t, cur_coordinates);
        }

        private Texture2D LoadTexture(string name)
        {
            return Content.Load<Texture2D>(name + ".png");
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sprite_batch = new SpriteBatch(GraphicsDevice);
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (this.IsActive)
            {
                foreach (var chunk in loaded_chunks)
                {
                    if(!chunk.active)
                    {
                        chunk.Cull();
                        chunk.UpdateBuffer(GraphicsDevice);
                        chunk.active = true;
                    }
                }
            
                KeyboardState ks = Keyboard.GetState();

                if (ks.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }

                if (ks.IsKeyDown(Keys.OemMinus) || ks.IsKeyDown(Keys.OemPlus))
                {
                    int x = (int)Math.Round(Position.X/2.0);
                    int z = (int)Math.Round(Position.Z/2.0);

                    /*
                    Debug.WriteLine(x);
                    if (x >= 0 && x < chunk_size && z >= 0 && z < chunk_size){
                        for (int y = chunk_size - 1; y >= 0; y--)
                        {
                            if (blocks[x, y, z].active)
                            {
                                if (ks.IsKeyDown(Keys.OemMinus) )
                                {
                                    if (y > 0)
                                    {
                                        blocks[x, y, z].active = false;
                                        blocks[x, y, z].type = BlockType.Air;
                                    }
                                }
                                else
                                
                                {
                                    if (y < chunk_size-2)
                                    {
                                        blocks[x, y + 1, z].active = true;
                                        blocks[x, y + 1, z].type = BlockType.Dirt;
                                    }
                                }
                                break;
                            }
                        }
                    }
                     blockBuffers = null;
                     */
                }

                Vector3 moveVector = Vector3.Zero;

          
                if (ks.IsKeyDown(Keys.Q))
                    moveVector.Y = 1;
                if (ks.IsKeyDown(Keys.Z))
                    moveVector.Y = -1;
                if (ks.IsKeyDown(Keys.W))
                    moveVector.Z = 1;
                if (ks.IsKeyDown(Keys.S))
                    moveVector.Z = -1;
                if (ks.IsKeyDown(Keys.A))
                    moveVector.X = 1;
                if (ks.IsKeyDown(Keys.D))
                    moveVector.X = -1;

                if (moveVector != Vector3.Zero)
                {
                    //normalize that vector
                    //so that we don't move faster diagonally
                    moveVector.Normalize();
                    //Now we add in smooth and speed
                    moveVector *= dt * cameraSpeed;

                    //Move camera
                    Move(moveVector);
                }

                var currentMouseState = Mouse.GetState();

                int centerX = GraphicsDevice.Viewport.Width / 2;
                int centerY = GraphicsDevice.Viewport.Height / 2;

                //Change in mouse position
                //x and y
                float deltaX;
                float deltaY;

                //Handle mouse movement
                if (currentMouseState != prevMouseState)
                {
                    //Get the change in mouse position
                    deltaX = Mouse.GetState().X - (centerX);
                    deltaY = Mouse.GetState().Y - (centerY);

                    //This is used to buffer against use input.
                    mouseRotationBuffer.X -= 0.05f * deltaX * dt;
                    mouseRotationBuffer.Y -= 0.05f * deltaY * dt;

                    if (mouseRotationBuffer.Y < MathHelper.ToRadians(-75.0f))
                        mouseRotationBuffer.Y = mouseRotationBuffer.Y - (mouseRotationBuffer.Y - MathHelper.ToRadians(-75.0f));
                    if (mouseRotationBuffer.Y > MathHelper.ToRadians(90.0f))
                        mouseRotationBuffer.Y = mouseRotationBuffer.Y - (mouseRotationBuffer.Y - MathHelper.ToRadians(90.0f));

                    Rotation = new Vector3(-MathHelper.Clamp(mouseRotationBuffer.Y, MathHelper.ToRadians(-75.0f),
                        MathHelper.ToRadians(90.0f)), MathHelper.WrapAngle(mouseRotationBuffer.X), 0);

                    deltaX = 0;
                    deltaY = 0;
                }
                Mouse.SetPosition(centerX, centerY);
                prevMouseState = currentMouseState;
            }
            base.Update(gameTime);
        }
        private void MoveTo(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }
        private Vector3 PreviewMove(Vector3 amount)
        {
            //Create a rotate matrix
            Matrix rotate = Matrix.CreateRotationY(cameraRotation.Y);
            //Create a movement vector
            Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
            movement = Vector3.Transform(movement, rotate);
            //Return the value of camera position + movement vector
            return cameraPosition + movement;
        }
        private void Move(Vector3 scale)
        {
            MoveTo(PreviewMove(scale), Rotation);
        }
        private void UpdateLookAt()
        {
            //Build a rotation matrix
            Matrix rotationMatrix = Matrix.CreateRotationX(cameraRotation.X) * Matrix.CreateRotationY(cameraRotation.Y);
            //Build look at offset vector
            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
            //Update our camera's look at vector
            cameraLookAt = cameraPosition + lookAtOffset;
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            if (this.texture == null) {
              return;
            }
            cubeEffect.Texture = texture;
            cubeEffect.View = View;
            foreach (var chunk in loaded_chunks)
            {
                if(!chunk.active || chunk.vertex_count ==0){
                    continue;
                }
                cubeEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.SetVertexBuffer(chunk.vertex_buffer);
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, chunk.vertex_count);
            }
            base.Draw(gameTime);
        }
    }
}
