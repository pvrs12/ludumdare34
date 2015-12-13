using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LudumDare
{
    /// <summary>
    /// A location which is surrounded by walls and can be occupied by a cell
    /// Slots can either be winning or losing. A winning slot must have a cell in it and a losing slot must be empty
    /// </summary>
    class Slot 
    {
        public static Texture2D cell_texture;
        public static Texture2D wall_texture;
        public static Texture2D slot_texture;

        public bool NorthWall { get; private set; }
        public bool EastWall { get; private set; }
        public bool SouthWall { get; private set; }
        public bool WestWall { get; private set; }
        private bool _occupied;
        public bool Occupied {
            get
            {
                return _occupied;
            }
            set
            {
                _occupied = value;
            }
        }
        public bool Winning { get; private set; }

        public const int SLOT_SIZE = 40;
        public int WALL_WIDTH
        {
            get
            {
                return SLOT_SIZE / 10;
            }
        }
        public int CELL_SIZE
        {
            get
            {
                return SLOT_SIZE - WALL_WIDTH*2;
            }
        }

        private Slot(bool northWall, bool eastWall, bool southWall, bool westWall, bool occupied, bool winning)
        {
            NorthWall = northWall;
            EastWall = eastWall;
            SouthWall = southWall;
            WestWall = westWall;

            Occupied = occupied;
            Winning = winning;
        }

        internal void Draw(SpriteBatch spriteBatch, int i, int j, int xoffset,int yoffset)
        {
            //switch i & j because of switch from rows/cols to x/y
            Vector2 topLeft = new Vector2(j * SLOT_SIZE+xoffset, i * SLOT_SIZE+yoffset);
            spriteBatch.Draw(slot_texture, new Rectangle((int)topLeft.X, (int)topLeft.Y, SLOT_SIZE, SLOT_SIZE), Winning ? Color.White : new Color(Color.DarkRed, 200));
            if (NorthWall)
            {
                spriteBatch.Draw(wall_texture, new Rectangle((int)topLeft.X, (int)topLeft.Y, SLOT_SIZE, WALL_WIDTH), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);
            }
            if (EastWall)
            {
                spriteBatch.Draw(wall_texture, new Rectangle((int)topLeft.X+SLOT_SIZE, (int)topLeft.Y, SLOT_SIZE, WALL_WIDTH), null, Color.White, (float)Math.PI/2, Vector2.Zero, SpriteEffects.None, 0);
            }
            if (SouthWall)
            {
                spriteBatch.Draw(wall_texture, new Rectangle((int)topLeft.X, (int)topLeft.Y+SLOT_SIZE-WALL_WIDTH, SLOT_SIZE, WALL_WIDTH), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);
            }
            if (WestWall)
            {
                spriteBatch.Draw(wall_texture, new Rectangle((int)topLeft.X+WALL_WIDTH, (int)topLeft.Y, SLOT_SIZE, WALL_WIDTH), null, Color.White, (float)Math.PI / 2, Vector2.Zero, SpriteEffects.None, 0);
            }
            if (Occupied)
            {
                spriteBatch.Draw(cell_texture,new Rectangle((int)topLeft.X+WALL_WIDTH,(int)topLeft.Y+WALL_WIDTH,CELL_SIZE,CELL_SIZE),Color.White);
            }
        }

        public bool contains(int x, int y, int row, int col)
        {
            int topLeftX = row * SLOT_SIZE;
            int topLeftY = col * SLOT_SIZE;
            Rectangle container = new Rectangle(topLeftX + WALL_WIDTH, topLeftY + WALL_WIDTH, CELL_SIZE, CELL_SIZE);
            return container.Contains(x, y);
        }

        /// <summary>
        /// Converts the binary slot_data to an object
        /// </summary>
        /// <param name="slot_data">The integer representation of the slot encoded bit-wise
        ///     0x100110
        ///     NorthWall=true
        ///     EastWall=false
        ///     SouthWall=false
        ///     WestWall=true
        ///     Occupied=true
        ///     Winning=false
        /// </param>
        /// <returns></returns>
        public static Slot MakeSlot(int slot_data)
        {
            bool winning = (slot_data & 1)==1;
            slot_data >>= 1;
            bool occupied = (slot_data & 1) == 1;
            slot_data >>= 1;
            bool westwall = (slot_data & 1) == 1;
            slot_data >>= 1;
            bool southwall = (slot_data & 1) == 1;
            slot_data >>= 1;
            bool eastwall = (slot_data & 1) == 1;
            slot_data >>= 1;
            bool northwall = (slot_data & 1) == 1;

            Slot slot = new Slot(northwall,eastwall,southwall,westwall,occupied,winning);
            return slot;
        }

        public int WriteSlot()
        {
            int slot=0;
            slot += NorthWall ? 1 : 0;
            slot <<=1;
            slot += EastWall ? 1 : 0;
            slot <<= 1;
            slot += SouthWall ? 1 : 0;
            slot <<= 1;
            slot += WestWall ? 1 : 0;
            slot <<= 1;
            slot += Occupied ? 1 : 0;
            slot <<= 1;
            slot += Winning ? 1 : 0;
            return slot;
        }
    }
}
