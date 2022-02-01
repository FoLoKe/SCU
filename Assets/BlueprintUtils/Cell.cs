using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.BlueprintUtils
{
    public class Cell
    {
        private int _hp = 10;
        public int X;
        public int Y;
        public Color32 Color;
        //public Module module;

        ////////////////////////
        //  NEIGHBOURS STATE - 4-DIRECTIONAL -
        //          11
        //          5
        //      9   1  3
        //  13  8 cell 2  7  10 // all 15
        //      12  4  6 
        //          14  
        //
        //////////////////////////
        public int state;

        public Cell(int x, int y, Color32 color)
        {
            this.X = x;
            this.Y = y;
            this.Color = color;
        }

        public bool ApplyDamage(int amount)
        {
            _hp -= amount;
            return _hp <= 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Cell cell = obj as Cell;
            if (cell == null)
            {
                return false;
            }
            else
            {
                return cell.X == X && cell.Y == Y;
            }
        }
    }
}
