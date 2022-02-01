
using System;

namespace Assets.BlueprintUtils
{
    [Serializable]
    public class Blueprint
    {
        public string Name;
        public Cell[,] cells = null;
        public int sizeX = 8;
        public int sizeY = 8;

        public Blueprint()
        {
            Name = "Unnamed";
            cells = new Cell[sizeX, sizeY];
        }

        public Blueprint(string name, Cell[,] cells)
        {
            Name = name;
            this.cells = cells;
            sizeX = cells.GetLength(0);
            sizeY = cells.GetLength(1);
        }

    }
}