using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LudumDare
{
    /// <summary>
    /// A Field is where the Slots live. It is the game state
    /// </summary>
    class Field
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public int Width {
            get
            {
                return Columns * Slot.SLOT_SIZE;
            }
        }
        public int Height
        {
            get
            {
                return Rows * Slot.SLOT_SIZE;
            }
        }

        private Slot[,] field;

        private Field(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            field = new Slot[Rows,Columns];
        }

        public void clearField()
        {
            for(int i=0;i< Rows; ++i)
            {
                for(int j = 0; j < Columns; ++j)
                {
                    field[i, j].Occupied = false;
                }
            }
        }

        public static Field MakeField(string file)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                int rows = br.ReadByte();
                int cols = br.ReadByte();

                Field f = new Field(rows, cols);
                for (int i = 0; i < rows; ++i)
                {
                    for (int j = 0; j < cols; ++j)
                    {
                        f.field[i, j] = Slot.MakeSlot(br.ReadByte());
                    }
                }
                return f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, int xOffset = 0, int yOffset = 0)
        {
            spriteBatch.Begin();
            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    field[i, j].Draw(spriteBatch, i,j, xOffset,yOffset);
                }
            }
            spriteBatch.End();
        }

        public Slot GetSlot(int row, int col)
        {
            return field[row, col];
        }

        public bool IsWin()
        {
            for(int i=0;i< Rows; ++i)
            {
                for(int j = 0; j < Columns; ++j)
                {
                    if(field[i,j].Occupied && !field[i, j].Winning)
                    {
                        return false;
                    }
                    if(!field[i,j].Occupied && field[i, j].Winning)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Tuple<int,int> GetContainer(int x, int y)
        {
            for(int i=0;i< Rows; ++i)
            {
                for(int j = 0; j < Columns; ++j)
                {
                    if (field[i, j].contains(x, y, i, j))
                    {
                        return new Tuple<int, int>(i, j);
                    }
                }
            }
            //nope
            return new Tuple<int, int>(-1,-1);
        }

        public void divide(int row, int col)
        {
            Slot slot = field[row, col];
            if (!slot.Occupied)
            {
                return;
            }
            if (!slot.NorthWall)
            {
                //try row-1
                if (row > 0)
                {
                    field[row - 1, col].Occupied = true;
                }
            }
            if (!slot.EastWall)
            {
                //try col+1
                if(col < Columns - 1)
                {
                    field[row, col + 1].Occupied = true;
                }
            }
            if (!slot.SouthWall)
            {
                //row+1
                if (row < Rows - 1)
                {
                    field[row + 1, col].Occupied = true;
                }
            }
            if (!slot.WestWall)
            {
                if (col > 0)
                {
                    field[row, col - 1].Occupied = true;
                }
            }
            clear_surrounded();
        }

        private int count_neighbors(int i, int j)
        {
            int neighbor_count = 0;
            //if cell has 8 neighbors, it goes away
            if (i > 0)
            {
                if (field[i - 1, j].Occupied)
                {
                    neighbor_count++;
                }
                if (j > 0)
                {
                    if (field[i - 1, j - 1].Occupied)
                    {
                        neighbor_count++;
                    }
                }
                if (j < Columns - 1)
                {
                    if (field[i - 1, j + 1].Occupied)
                    {
                        neighbor_count++;
                    }
                }
            }
            if (i < Rows - 1)
            {
                if (field[i + 1, j].Occupied)
                {
                    neighbor_count++;
                }
                if (j > 0)
                {
                    if (field[i + 1, j - 1].Occupied)
                    {
                        neighbor_count++;
                    }
                }
                if (j < Columns - 1)
                {
                    if (field[i + 1, j + 1].Occupied)
                    {
                        neighbor_count++;
                    }
                }
            }
            if (j > 0)
            {
                if (field[i, j - 1].Occupied)
                {
                    neighbor_count++;
                }
            }
            if (j < Columns - 1)
            {
                if (field[i, j + 1].Occupied)
                {
                    neighbor_count++;
                }
            }
            return neighbor_count;
        }

        private void clear_surrounded()
        {
            List<Tuple<int, int>> unoccupy = new List<Tuple<int, int>>();
            for(int i=0;i< Rows; ++i)
            {
                for(int j = 0; j < Columns; ++j)
                {
                    if (count_neighbors(i, j) >= 8)
                    {
                        unoccupy.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            //wait till all calculations are done before un-occupying
            foreach(Tuple<int,int> t in unoccupy)
            {
                field[t.Item1, t.Item2].Occupied = false;
            }
        }
    }
}
