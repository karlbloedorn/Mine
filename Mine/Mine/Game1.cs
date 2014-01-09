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

        GraphicsDeviceManager graphics;
        SpriteBatch sprite_batch;
        List<Chunk> loaded_chunks = new List<Chunk>();
        public Dictionary<BlockType, Texture2D> block_textures;
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
            Simplex noisemodule = new Simplex();
            SharpNoise.Models.Plane p = new SharpNoise.Models.Plane(noisemodule);

      
            for (int x = -10; x <10; x++)
            {
                for (int z = -10; z < 10; z++)
                {
                    for (int y = 0; y < 3; y++)
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
           // rasterizerState.CullMode = CullMode.None;
            rasterizerState.MultiSampleAntiAlias = true;
            GraphicsDevice.RasterizerState = rasterizerState;

            this.IsMouseVisible = false;
            base.Initialize();
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
            block_textures = new Dictionary<BlockType, Texture2D>();
            block_textures.Add(BlockType.DiamondOre, LoadTexture("diamond_ore"));
            block_textures.Add(BlockType.Cobblestone, LoadTexture("cobblestone"));
            block_textures.Add(BlockType.Dirt, LoadTexture("dirt"));
            block_textures.Add(BlockType.Grass, LoadTexture("grass"));
            block_textures.Add(BlockType.Sand, LoadTexture("sand"));
            block_textures.Add(BlockType.Oak, LoadTexture("log_big_oak"));
            block_textures.Add(BlockType.Oak_Top, LoadTexture("log_oak_top"));
            block_textures.Add(BlockType.Snow, LoadTexture("grass_side_snowed"));
            block_textures.Add(BlockType.Snow_Top, LoadTexture("snow"));
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
            if (block_textures.Count < 5)
            {
                return;
            }
            GraphicsDevice.Clear(Color.SkyBlue);

            cubeEffect.View = View;

            foreach (var chunk in loaded_chunks)
            {
                if(!chunk.active){
                    continue;
                }
                foreach (BlockType t in chunk.block_sets.Keys)
                {
                    var block_set = chunk.block_sets[t];
                    for (int i = 0; i < 6; i++)
                    {
                        if (block_set.vertex_buffers[i] != null)
                        {
                            cubeEffect.Texture = block_textures[(BlockType)block_set.textures[i]];
                            cubeEffect.CurrentTechnique.Passes[0].Apply();
                            GraphicsDevice.SetVertexBuffer(block_set.vertex_buffers[i]);
                            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, block_set.vertex_counts[i]);
                        }
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}
