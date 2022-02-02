using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BlueprintUtils
{
    public class Blueprint : ISerializationCallbackReceiver
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public int sizeX = 8;

        [SerializeField]
        public int sizeY = 8;

        [SerializeField] 
        private List<Cell> cellsSeria = null;

        [NonSerialized]
        public Cell[,] cells = null;

        [NonSerialized]
        private static Color s_transparent = new Color(0, 0, 0, 0);

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

        public void OnAfterDeserialize()
        {
            cells = new Cell[sizeX, sizeY];
            foreach (var cell in cellsSeria)
            {
                cells[cell.X, cell.Y] = cell;
            }

            cellsSeria = null;
        }

        public void OnBeforeSerialize()
        {
            cellsSeria = new List<Cell>();
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (cells[x, y] != null)
                    {
                        cellsSeria.Add(cells[x, y]);
                    }
                }
            }
        }

        public string GetSaveData()
        {
            return JsonUtility.ToJson(this);
        }

        internal Texture2D GetTexture()
        {
            Texture2D texture = new Texture2D(sizeX, sizeY);
            texture.filterMode = FilterMode.Point;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (cells[x, y] != null)
                    {
                        texture.SetPixel(x, y, cells[x, y].Color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, s_transparent);
                    }
                }
            }

            texture.Apply();

            return texture;
        }
    }
}