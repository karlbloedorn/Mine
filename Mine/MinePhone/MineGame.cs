using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Mine;
//using SharpNoise;
//using SharpNoise.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mine
{
    public class MineGame : Game
    {
        public static float gravity = 0f;
        public static int chunk_size =16 ;
        public static int chunk_height = 32;

        private Vector3 player_speed = Vector3.Zero;
        private Planet planet;
        private int limit_latitude = 80;
        const float tex_w = 256;
        const float tex_h = 272;
        const float tile_w = 16;
        const float tile_h = 16;
        const float w_factor = tile_w / tex_w;
        const float h_factor = tile_h / tex_h;
        int frameCounter = 0;
        int frameRate = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        Texture2D stitched_blocks;
        Texture2D stitched_items;
        public Dictionary<BlockType, Vector2[,]> texture_coordinates;
        public Dictionary<Coordinate, Task<Quadrangle>> waiting_for_load;

        float latitude = 0;
        float longitude = 50;
        float height =0;
        GraphicsDeviceManager graphics;
        SpriteBatch sprite_batch;
        private SpriteFont Font1;
        private BasicEffect cubeEffect;
        private MouseState prevMouseState;
        private Vector3 mouseRotationBuffer;   
        private Vector3 cameraPosition;
        private Vector3 cameraRotation;
        private Vector3 cameraLookAt;
        private float cameraSpeed = 39.0f;
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
        protected override void Initialize()
        {
            texture_coordinates = new Dictionary<BlockType, Vector2[,]>();
            waiting_for_load = new Dictionary<Coordinate, Task<Quadrangle>>();
            Font1 = Content.Load<SpriteFont>("Courier New");
            stitched_blocks = LoadTexture("stitched_blocks");
            stitched_items = LoadTexture("stitched_items");
            AddTextureCoordinates(BlockType.Dirt, 6, 14, 6, 14, 6, 14);
            AddTextureCoordinates(BlockType.Grass, 7, 14, 6, 12, 6, 14);
            AddTextureCoordinates(BlockType.Snow, 8, 14, 8, 13, 6, 14);
            AddTextureCoordinates(BlockType.Stone, 10, 6);
            AddTextureCoordinates(BlockType.Cobblestone, 11, 6);
            AddTextureCoordinates(BlockType.Gravel, 11, 5);
            AddTextureCoordinates(BlockType.Coal, 7, 8);
            AddTextureCoordinates(BlockType.IronOre, 8, 8);
            AddTextureCoordinates(BlockType.GoldOre, 9, 8);
            AddTextureCoordinates(BlockType.DiamondOre, 10, 8);
            AddTextureCoordinates(BlockType.Sand, 15, 7);
            AddTextureCoordinates(BlockType.Oak_Wood, 1, 14, 1, 15, 1, 15);
            AddTextureCoordinates(BlockType.Oak_Leaves, 1, 10);
            AddTextureCoordinates(BlockType.Birch_Wood, 2, 14, 2, 15, 2, 15);
            AddTextureCoordinates(BlockType.Birch_Leaves, 2, 10);
            AddTextureCoordinates(BlockType.Crafting_Table, 4, 9, 3, 8, 4, 13);

            cubeEffect = new BasicEffect(GraphicsDevice);
            cubeEffect.Texture = stitched_blocks;
            cubeEffect.LightingEnabled = false;
            cubeEffect.TextureEnabled = true;

            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio,
                50.05f,
                1000000.0f);
            cubeEffect.Projection = Projection;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            cubeEffect.VertexColorEnabled = true;
            MoveTo(new Vector3(0f,40f, 0f), new Vector3(0f, 0f, 0f));
            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;
            Mouse.SetPosition(centerX, centerY);
            prevMouseState = Mouse.GetState();
            this.IsMouseVisible = false;
            planet = new Planet(20000f);
            planet.game = this;
            planet.loaded_quadrangles = new Dictionary<Coordinate, Quadrangle>();
            planet.requested_quadrangles = new Dictionary<Coordinate, Quadrangle>();
            height = planet.radial_distance + planet.height_step * 100;
            base.Initialize();
        }
        private void AddTextureCoordinates(BlockType t, short x, short y)
        {
          AddTextureCoordinates(t, x, y, x, y, x, y);
        }
        private void AddTextureCoordinates(BlockType t, short sides_x, short sides_y, short top_x, short top_y, short bottom_x, short bottom_y)
        {
          short[,] faces = new short[6, 2];
          faces[Block.west_face, 0] = sides_x;
          faces[Block.east_face, 0] = sides_x;
          faces[Block.bottom_face, 0] = bottom_x;
          faces[Block.top_face , 0] = top_x;
          faces[Block.north_face, 0] = sides_x;
          faces[Block.south_face, 0] = sides_x;
          faces[Block.east_face, 1] = sides_y;
          faces[Block.west_face, 1] = sides_y;
          faces[Block.bottom_face, 1] = bottom_y;
          faces[Block.top_face, 1] = top_y;
          faces[Block.north_face, 1] = sides_y;
          faces[Block.south_face, 1] = sides_y;

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
            return Content.Load<Texture2D>("stitched_blocks");
        }
        protected override void LoadContent()
        {
            sprite_batch = new SpriteBatch(GraphicsDevice);
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
          elapsedTime += gameTime.ElapsedGameTime;
          if (elapsedTime > TimeSpan.FromSeconds(1))
          {
            elapsedTime -= TimeSpan.FromSeconds(1);
            frameRate = frameCounter;
            frameCounter = 0;
          }
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //player_speed = new Vector3(player_speed.X, player_speed.Y, player_speed.Z -= 200*gravity*dt);
            //height -= player_speed.Z * dt;


          var gesture = default(GestureSample);

    
            var touchCol = TouchPanel.GetState();

            foreach (var touch in touchCol)
            {
              // You're looking for when they finish a drag, so only check
              // released touches.
              //if (touch.State != TouchLocationState.Released)
              //  continue;

              TouchLocation prevLoc;

              // Sometimes TryGetPreviousLocation can fail. Bail out early if this happened
              // or if the last state didn't move
              if (!touch.TryGetPreviousLocation(out prevLoc) || prevLoc.State != TouchLocationState.Moved)
                continue;

              // get your delta
              var delta = touch.Position - prevLoc.Position;

              if (delta.X < 0 || delta.Y < 0)

                if (delta.X != 0 || delta.Y != 0)
                {

                  latitude += dt * cameraSpeed/5.0f * delta.Y;
                  longitude += dt * cameraSpeed/5.0f  * delta.X;
                }

            

            }

            if (this.IsActive)
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.T))
                   latitude += dt * cameraSpeed * 1;
                if (ks.IsKeyDown(Keys.G))
                  latitude -= dt * cameraSpeed * 1;
                if (ks.IsKeyDown(Keys.R))
                  height += dt * cameraSpeed * 214;
                if (ks.IsKeyDown(Keys.V))
                  height -= dt * cameraSpeed * 214;
                if (ks.IsKeyDown(Keys.H))
                  longitude -= dt * cameraSpeed * 1;
                if (ks.IsKeyDown(Keys.F))
                  longitude += dt * cameraSpeed * 1;

                if (latitude < -limit_latitude)
                {
                  latitude = -limit_latitude;
                }
                if (latitude > limit_latitude)
                {
                  latitude = limit_latitude;
                }
                longitude = longitude % 360;
                if (longitude < 0)
                {
                  longitude = longitude + 360;
                }

                if (ks.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }
                if (ks.IsKeyDown(Keys.OemMinus) || ks.IsKeyDown(Keys.OemPlus))
                {
                    int x = (int)Math.Round(Position.X/2.0);
                    int z = (int)Math.Round(Position.Z/2.0);
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
                        MathHelper.ToRadians(90.0f)), MathHelper.WrapAngle(mouseRotationBuffer.X),0 );

                    deltaX = 0;
                    deltaY = 0;
                }
                Mouse.SetPosition(centerX, centerY);
                prevMouseState = currentMouseState;
            }
           
            List<Coordinate> finishedTasks = new List<Coordinate>();

            int count = 0;
            foreach (var task in waiting_for_load)
            {
              if (task.Value.IsCompleted)
              {
                Quadrangle c = task.Value.Result;
                if (c.vertex_count > 0)
                {
                  count++;
                  c.vertex_buffer = new VertexBuffer(GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, c.vertex_count, BufferUsage.WriteOnly);
                  c.vertex_buffer.SetData(c.block_vertices);
                  c.block_vertices = null;
                }
                c.active = true;
                planet.loaded_quadrangles.Add(task.Key, c);
                finishedTasks.Add(task.Key);
                if (count > 4)
                {
                  break;
                }
              }
            }
            foreach( var task in finishedTasks)
            {
                 waiting_for_load.Remove(task);
            }
            finishedTasks.Clear();

            var nearest = planet.Near(latitude, longitude, 205);
            var nearest_for_delete = planet.Near(latitude, longitude, 405);

            //var nearest_for_delete = world.Near(this.Position, 10);
            //var nearest = this.planet.requested_quadrangles.Keys.ToList();
            nearest.ForEach(x =>
            {
              if (Math.Abs(x.latitude) > limit_latitude)
              {
                return;
              }
              if (waiting_for_load.Count() > 5 ||  planet.loaded_quadrangles.ContainsKey(x) || waiting_for_load.ContainsKey(x))
              {
                return;
              }
              var tcs = new TaskCompletionSource<Quadrangle>();
              ThreadPool.QueueUserWorkItem(_ =>
              {
                try
                {
                  Quadrangle q = new Quadrangle(x.latitude, x.longitude, x.radial_distance);
                  q.planet = planet;
                  q.Generate();
                  q.Cull(false);
                  q.UpdateBuffer();
                  tcs.SetResult(q);
                }
                catch (Exception exc) { tcs.SetException(exc); }
              });
              waiting_for_load.Add(x, tcs.Task);
            });

            List<Coordinate> quad_keys = new List<Coordinate>();
            foreach (var quad_key in planet.loaded_quadrangles.Keys)
            {
              quad_keys.Add(quad_key);
            }         
          
           foreach (var quad_key in quad_keys)
            {
              if (!nearest_for_delete.Contains(quad_key))
              {
                Quadrangle matching = null;
                planet.loaded_quadrangles.TryGetValue(quad_key, out matching);
                if (matching != null)
                {
                  if (matching.vertex_count > 0)
                  {
                    matching.vertex_buffer.Dispose();
                  }
                  planet.loaded_quadrangles.Remove(quad_key);
                }
              }
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
            Matrix rotationMatrix =  Matrix.CreateRotationX(cameraRotation.X) * Matrix.CreateRotationY(cameraRotation.Y);
            //Build look at offset vector
            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
            //Update our camera's look at vector
            cameraLookAt = cameraPosition + lookAtOffset;
        }
        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;
            GraphicsDevice.Clear(Color.SkyBlue);
            //GraphicsDevice.BlendState = BlendState.NonPremultiplied;  need to draw transparent stuff later.
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressW = TextureAddressMode.Wrap;

            RasterizerState rasterizerState = new RasterizerState();
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Space))
            {
              rasterizerState.FillMode = FillMode.WireFrame;
              rasterizerState.CullMode = CullMode.None;
            }
            else
            {
              rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            }

            graphics.SynchronizeWithVerticalRetrace = true;
            rasterizerState.MultiSampleAntiAlias = true;
            GraphicsDevice.RasterizerState = rasterizerState;

            if (this.stitched_blocks == null || this.Font1 == null) {
              base.Draw(gameTime);
              return;
            }
           
            int vertices = 0;
            //cubeEffect.View = View;

            var up = new Vector3(0,1,0);
            var position = planet.ToCartesian(height, latitude, longitude);
            var looking_at = Vector3.Zero;
            cubeEffect.View = Matrix.CreateLookAt(position, looking_at, up);

            //up.Normalize();
            //cubeEffect.View = Matrix.CreateLookAt(new Vector3(950, 0, 0), new Vector3(950,0, 1), new Vector3(1, 0,0)     );
            
            foreach (var quadrangle in planet.loaded_quadrangles.Values)
            {
              if (!quadrangle.active || quadrangle.vertex_count == 0)
              {
                continue;
              }
              vertices+= quadrangle.vertex_count;
              cubeEffect.CurrentTechnique.Passes[0].Apply();
              GraphicsDevice.SetVertexBuffer(quadrangle.vertex_buffer);
              GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, quadrangle.vertex_count);
            }
           
            sprite_batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            sprite_batch.DrawString(Font1, string.Format("fps: {0} mem : {1} MB", frameRate, GC.GetTotalMemory(false)/ 0x100000  ), new Vector2(10, 10), Color.Gray);
            sprite_batch.DrawString(Font1, string.Format("latitude: {0} longitude: {1}", latitude, 180-longitude), new Vector2(10, 80), Color.Gray);
            sprite_batch.DrawString(Font1, string.Format("quads: {0} vertices : {1} ", planet.loaded_quadrangles.Count, vertices) , new Vector2(10, 30), Color.Gray);
           // sprite_batch.Draw(this.stitched_items, new Rectangle(300, 300, 128, 128), new Rectangle(16 * 5, 16 * 6, 16, 16), Color.White);
            //sprite_batch.Draw(this.texture, new Vector2(10, 100), Color.White);
            sprite_batch.End();

            base.Draw(gameTime);
        }
    }
}
