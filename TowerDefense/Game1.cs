using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TowerDefense 
{
    public enum Side { Player, Enemy }

    public class Unit
    {
        public Vector2 Position;
        public Side Side;
        public float Speed = 80f;
        public int Health = 50;
        public int Damage = 15;
        
        // cooldown
        private float _attackTimer = 0;
        private float _attackSpeed = 1.0f; // 1 punch in second

        public bool IsDead => Health <= 0;
        public bool IsAttacking = false;

        public Unit(Vector2 startPos, Side side)
        {
            Position = startPos;
            Side = side;
        }

        public void Update(GameTime gameTime, List<Unit> targets, ref int baseHealth)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IsAttacking = false;

            // test for crush with enemy
            foreach (var target in targets)
            {
                if (target.Side != this.Side && Vector2.Distance(this.Position, target.Position) < 35)
                {
                    IsAttacking = true;
                    PerformAttack(target, dt);
                    break; 
                }
            }

            // test for crush with tower
            // if player come to tower and if enemy come to tower
            if (!IsAttacking)
            {
                if ((Side == Side.Player && Position.X < 70) || (Side == Side.Enemy && Position.X > 730))
                {
                    IsAttacking = true;
                    baseHealth -= Damage; // damage to tower
                    Health = 0; // unit dies after attack
                }
            }

            // moving if there is no enemy
            if (!IsAttacking)
            {
                Position.X += (Side == Side.Enemy ? 1 : -1) * Speed * dt;
            }
        }

        private void PerformAttack(Unit target, float dt)
        {
            _attackTimer += dt;
            if (_attackTimer >= _attackSpeed)
            {
                target.Health -= this.Damage;
                _attackTimer = 0;
            }
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        
        private List<Unit> _units = new List<Unit>();
        
        // resources and economy
        private float _gold = 100;
        private float _goldPassiveRate = 5f; // 5 gold per sec
        private int _unitCost = 30;
        
        // towers health
        private int _playerBaseHp = 500;
        private int _enemyBaseHp = 500;

        private float _enemySpawnTimer = 0;
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
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (newState.IsKeyDown(Keys.Escape)) Exit();

            // passive economy
            _gold += _goldPassiveRate * dt;

            // unit buy
            if (newState.IsKeyDown(Keys.D1) && _oldState.IsKeyUp(Keys.D1) && _gold >= _unitCost)
            {
                _units.Add(new Unit(new Vector2(730, 350), Side.Player));
                _gold -= _unitCost;
            }

            // spawn of enemy 
            _enemySpawnTimer += dt;
            if (_enemySpawnTimer > 4.0f)
            {
                _units.Add(new Unit(new Vector2(70, 350), Side.Enemy));
                _enemySpawnTimer = 0;
            }

            // refresh for units
            for (int i = 0; i < _units.Count; i++)
            {
                // we choose whose health to reduce if unit reaches base
                ref int targetBaseHp = ref (_units[i].Side == Side.Player ? ref _enemyBaseHp : ref _playerBaseHp);
                _units[i].Update(gameTime, _units, ref targetBaseHp);
            }

            // accrual of gold for killed enemies before their removal
            foreach (var u in _units)
            {
                if (u.IsDead && u.Side == Side.Enemy) _gold += 15; 
            }

            _units.RemoveAll(u => u.IsDead);
            _oldState = newState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            // ground
            _spriteBatch.Draw(_pixel, new Rectangle(0, 380, 800, 100), Color.DarkOliveGreen);

            // towers (height depends on hp + visual effect)
            int pBaseHeight = (int)(100 * (_playerBaseHp / 500f));
            int eBaseHeight = (int)(100 * (_enemyBaseHp / 500f));

            _spriteBatch.Draw(_pixel, new Rectangle(0, 380 - eBaseHeight, 70, eBaseHeight), Color.Maroon); // База врага
            _spriteBatch.Draw(_pixel, new Rectangle(730, 380 - pBaseHeight, 70, pBaseHeight), Color.DarkGreen); // База игрока

            // units
            foreach (var unit in _units)
            {
                Color c = (unit.Side == Side.Player) ? Color.LimeGreen : Color.HotPink;
                // when a unit attacks it will bounce slightly or change color
                if (unit.IsAttacking) c = Color.White;
                
                _spriteBatch.Draw(_pixel, new Rectangle((int)unit.Position.X, (int)unit.Position.Y, 30, 30), c);
            }

            // ui gold stripe
            _spriteBatch.Draw(_pixel, new Rectangle(10, 10, (int)_gold, 20), Color.Gold);

            _spriteBatch.End();
            base.Update(gameTime);
        }
    }
}
