using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TD
{
    public enum Side { Player, Enemy }

    public class Unit
    {
        public Vector2 Position;
        public Side Side;
        public float Speed = 100f;
        public int Health = 50;
        public bool IsDead => Health <= 0;

        public Unit(Vector2 startPos, Side side)
        {
            Position = startPos;
            Side = side;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position.X += (Side == Side.Enemy ? 1 : -1) * Speed * dt;
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        private List<Unit> _units = new List<Unit>();
        private int _gold = 100;
        private float _enemySpawnTimer = 0;
        
        // to prevent keys from sticking
        private KeyboardState _oldState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // we create a texture programmatically so as not to depend on files
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // exit
            if (newState.IsKeyDown(Keys.Escape)) Exit();

            // for unit buy - 1
            if (newState.IsKeyDown(Keys.D1) && _oldState.IsKeyUp(Keys.D1) && _gold >= 20)
            {
                _units.Add(new Unit(new Vector2(750, 350), Side.Player));
                _gold -= 20;
            }

            // enemy spawn
            _enemySpawnTimer += dt;
            if (_enemySpawnTimer > 3.0f)
            {
                _units.Add(new Unit(new Vector2(50, 350), Side.Enemy));
                _enemySpawnTimer = 0;
            }

            // movement and fight logic
            for (int i = 0; i < _units.Count; i++)
            {
                _units[i].Update(gameTime);
                
                for (int j = i + 1; j < _units.Count; j++)
                {
                    if (_units[i].Side != _units[j].Side && 
                        Vector2.Distance(_units[i].Position, _units[j].Position) < 30)
                    {
                        _units[i].Health = 0;
                        _units[j].Health = 0;
                        _gold += 10;
                    }
                }
            }

            _units.RemoveAll(u => u.IsDead);
            _oldState = newState; // save keyboard staterment

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // ground
            _spriteBatch.Draw(_pixel, new Rectangle(0, 380, 800, 100), Color.DarkGray);

            // towers
            _spriteBatch.Draw(_pixel, new Rectangle(0, 280, 60, 100), Color.Red); // Враг
            _spriteBatch.Draw(_pixel, new Rectangle(740, 280, 60, 100), Color.Green); // Игрок

            // units
            foreach (var unit in _units)
            {
                Color c = (unit.Side == Side.Player) ? Color.LightGreen : Color.Pink;
                _spriteBatch.Draw(_pixel, new Rectangle((int)unit.Position.X, (int)unit.Position.Y, 25, 25), c);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}