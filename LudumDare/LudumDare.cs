using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif
using System;
using System.IO;
using System.Net;

namespace LudumDare
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class LudumDare : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private const string API_URL = "http://52.23.199.85:8000/";

        private const int BUTTON_WIDTH = 32;
        private const int BUTTON_HEIGHT = 32;
        private const int BUTTON_SPACER = 10;

        private const int TOP_LEFT_X = 20;
        private const int TOP_LEFT_Y = 20;

        private int LEVEL_COUNT = 0;

        private Field field;
        private bool clicked;
        private bool levelComplete;
        private bool loadNewLevel;
        private int level;
        private int record;
        private int moveCount;

        Texture2D restartTexture;
        Texture2D nextLevelTexture;

        Rectangle restartButton;
        Rectangle nextLevelButton;
        Vector2 statusTextLocation;
        string status;

        private string userid;

        SpriteFont font;
        
        public LudumDare()
        {
            graphics = new GraphicsDeviceManager(this);
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
            // TODO: Add your initialization logic here
            clicked = false;
            levelComplete = false;
            loadNewLevel = false;
            IsMouseVisible = true;
            level = 1;
            status = "Level: {0}\nMoves: {1}\nRecord: {2}";

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Slot.cell_texture = Content.Load<Texture2D>("cell");
            Slot.wall_texture = Content.Load<Texture2D>("wall");
            Slot.slot_texture = Content.Load<Texture2D>("slot");

            restartTexture = Content.Load<Texture2D>("restart");
            nextLevelTexture = Content.Load<Texture2D>("arrow");

            font = Content.Load<SpriteFont>("font");

            try {
                userid = GetUserId();

                LEVEL_COUNT = getLevelCount();
                field = loadLevel(level);
                moveButtons();
            }
            catch (Exception)
            {
                Console.WriteLine("You must be connected to the internet and the host server must be up!");
                Exit();
            }
        }

        private int getLevelCount()
        {
            WebRequest req = HttpWebRequest.Create(API_URL + "level");
            using (WebResponse resp = req.GetResponse())
            {
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    return int.Parse(sr.ReadToEnd());
                }
            }
        }

        private void submitScore(int level, string userid,int score)
        {
            string s = API_URL + "level/" + level + "/user/" + userid + "/score/" + score;
            //Console.WriteLine(s);
            WebRequest req = HttpWebRequest.Create(s);
            req.Method = "POST";
            WebResponse resp = req.GetResponse();
            resp.Close();
        }

        private int getLevelRecord(int level)
        {
            WebRequest req = HttpWebRequest.Create(API_URL + "level/"+level+"/record");
            using (WebResponse resp = req.GetResponse())
            {
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    return int.Parse(sr.ReadToEnd());
                }
            }
        }

        private string GetUserId()
        {
            //check file for userid
            if (File.Exists("userid"))
            {
                return File.ReadAllText("userid");
            }
            else
            {
                //if no userid
                WebRequest req = HttpWebRequest.Create(API_URL + "user");
                req.Method = "POST";
                using (WebResponse resp = req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        string uid = sr.ReadToEnd();
                        File.WriteAllText("userid", uid);
                        return uid;
                    }
                }
            }
        }

        private void moveButtons()
        {
            if (field == null)
            {
                return;
            }
            restartButton = new Rectangle(TOP_LEFT_X, field.Height + BUTTON_SPACER + TOP_LEFT_Y, BUTTON_WIDTH, BUTTON_HEIGHT);
            if (BUTTON_WIDTH + BUTTON_WIDTH / 2 > field.Width)
            {
                //if the button wont fit at the right, then draw it to the right of the first button and let it hang over the field
                nextLevelButton = new Rectangle(TOP_LEFT_X + BUTTON_WIDTH + BUTTON_WIDTH / 2, field.Height + BUTTON_SPACER + TOP_LEFT_Y, BUTTON_WIDTH, BUTTON_HEIGHT);
            }
            else
            {
                //draw button aligned with right of field
                nextLevelButton = new Rectangle(field.Width + TOP_LEFT_X - BUTTON_WIDTH, field.Height + BUTTON_SPACER + TOP_LEFT_Y, BUTTON_WIDTH, BUTTON_HEIGHT);
            }
            Vector2 stringSize = font.MeasureString(status);
            statusTextLocation = new Vector2(TOP_LEFT_X + field.Width - font.MeasureString(status).X / 2, field.Height + BUTTON_SPACER + TOP_LEFT_Y + BUTTON_HEIGHT + BUTTON_SPACER);
            
            graphics.PreferredBackBufferHeight = field.Height + TOP_LEFT_Y * 2 + BUTTON_HEIGHT + BUTTON_SPACER + (int) stringSize.Y;
            graphics.PreferredBackBufferWidth = field.Width + TOP_LEFT_X * 2 + BUTTON_SPACER ;
            graphics.ApplyChanges();
        }

        private Field loadLevel(int level)
        {
            if (level > LEVEL_COUNT)
            {
                Console.WriteLine("You won forever!");
                Exit();
                return null;
            }
            levelComplete = false;
            moveCount = 0;

            string s = API_URL + "level/" + level;
            //Console.WriteLine(s);
            Field field = Field.DownloadField(s);
            record = getLevelRecord(level);
            //string levelName = Content.RootDirectory + Path.DirectorySeparatorChar + "level" + level;
            //Field field = Field.MakeField(levelName);

            return field;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            int xInputLoc = -1;
            int yInputLoc = -1;
            bool processInput = false;
#if WINDOWS
            MouseState state = Mouse.GetState();
            if(state.LeftButton == ButtonState.Pressed && !clicked)
            {
                clicked = true;
                xInputLoc = state.Position.X;
                yInputLoc = state.Position.Y;
                processInput = true;
            }
            if(state.LeftButton == ButtonState.Released)
            {
                clicked = false;
            }
#endif
#if WINDOWS_PHONE
            TouchCollection tc = TouchPanel.GetState();
            if (tc.Count == 1)
            {
                if (tc[0].State == TouchLocationState.Pressed)
                {
                    processInput = true;
                    xInputLoc = (int)tc[0].Position.X;
                    yInputLoc = (int)tc[0].Position.Y;
                }
            }
#endif
            if (loadNewLevel)
            {
                loadNewLevel = false;
                field = loadLevel(level);
                moveButtons();
            }
            if (processInput)
            {
                //check if inside slot
                Tuple<int, int> inside = field.GetContainer(xInputLoc-TOP_LEFT_X, yInputLoc-TOP_LEFT_Y);
                if (inside.Item1 != -1 && inside.Item2 != -1 && !levelComplete)
                {
                    //divide this cell
                    if(field.divide(inside.Item1, inside.Item2))
                    {
                        moveCount++;
                    }
                    if (field.IsWin())
                    {
                        Console.WriteLine("You Win!");
                        levelComplete = true;
                    }
                } else if (restartButton.Contains(xInputLoc, yInputLoc) || loadNewLevel)
                {
                    //restart clicked
                    field = loadLevel(level);
                    moveButtons();
                } else if (nextLevelButton.Contains(xInputLoc, yInputLoc) && levelComplete)
                {
                    //next level clicked
                    loadNewLevel = true;
                    submitScore(level,userid,moveCount);
                    level++;
                }
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(restartTexture,restartButton, Color.White);
            spriteBatch.Draw(nextLevelTexture,nextLevelButton, levelComplete ? Color.White : new Color(Color.Black, 128));
            //center status text
            Vector2 size = font.MeasureString(String.Format(status, level, moveCount, record));
            //size.X/4 is a magic number.... it was /2 but that didn't work as intended
            statusTextLocation.X = field.Width / 2 - size.X /4;
            spriteBatch.DrawString(font, String.Format(status, level, moveCount,record), statusTextLocation, Color.Black);
            spriteBatch.End();

            field.Draw(spriteBatch, TOP_LEFT_X,TOP_LEFT_Y);

            base.Draw(gameTime);
        }
    }
}
