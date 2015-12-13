using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        private static Field readFromStream(Stream s)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                int rows = int.Parse(sr.ReadLine());
                int cols = int.Parse(sr.ReadLine());
                Field f = new Field(rows, cols);
                for(int i=0;i< rows; ++i)
                {
                    for(int j=0;j< cols; ++j)
                    {
                        f.field[i, j] = Slot.MakeSlot(sr.ReadLine());
                    }
                }
                return f;
            }
        }

        public static Field DownloadField(string URL)
        {
            WebRequest req = HttpWebRequest.Create(URL);
            using (WebResponse resp = req.GetResponse())
            {
                return readFromStream(resp.GetResponseStream());
            }
        }

        public static Field MakeField(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                return readFromStream(fs);
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
                    //swap i & j because of swap from rows/cols to x/y
                    if (field[i, j].contains(x, y, j, i))
                    {
                        return new Tuple<int, int>(i, j);
                    }
                }
            }
            //nope
            return new Tuple<int, int>(-1,-1);
        }

        public bool divide(int row, int col)
        {
            Slot slot = field[row, col];
            if (!slot.Occupied)
            {
                return false;
            }
            bool divided = false;
            if (!slot.NorthWall)
            {
                //try row-1
                if (row > 0)
                {
                    //verify it doesn't have a wall (one way walls is icky)
                    if (!field[row - 1, col].SouthWall)
                    {
                        field[row - 1, col].Occupied = true;
                        divided = true;
                    }
                }
            }
            if (!slot.EastWall)
            {
                //try col+1
                if(col < Columns - 1)
                {
                    if (!field[row, col + 1].WestWall)
                    {
                        field[row, col + 1].Occupied = true;
                        divided = true;
                    }
                }
            }
            if (!slot.SouthWall)
            {
                //row+1
                if (row < Rows - 1)
                {
                    if (!field[row + 1, col].NorthWall)
                    {
                        field[row + 1, col].Occupied = true;
                        divided = true;
                    }
                }
            }
            if (!slot.WestWall)
            {
                if (col > 0)
                {
                    if (!field[row, col - 1].EastWall)
                    {
                        field[row, col - 1].Occupied = true;
                        divided = true;
                    }
                }
            }
            clear_surrounded();
            return divided;
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
