using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.BlueprintUtils
{
    public class BlueprintComponent : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public Debris debrisPref = null;
        public Texture2D texture;

        public Blueprint bp;

        private List<Vector2> path = new List<Vector2>();
        public int sizeX = 0;
        public int sizeY = 0;

        private float scale = 0.01f;
        private float offset = 0.3f;

        // Start is called before the first frame update
        void Start()
        {
            if (SpriteRenderer.sprite == null)
            {
                if (bp == null)
                {
                    texture = new Texture2D(8, 8);
                    texture.filterMode = FilterMode.Point;
                    GetComponent<Renderer>().material.mainTexture = texture;

                    for (int y = 0; y < texture.height; y++)
                    {
                        for (int x = 0; x < texture.width; x++)
                        {
                            Color color = ((x & y) != 0 ? Color.black : Color.gray);
                            texture.SetPixel(x, y, color);
                        }
                    }

                    texture.Apply();
                    TextureToCells();
                }
                else
                {
                    CellsToTexture();
                }

            }
            else // DEBUG ONLY (CROP)
            {
                Sprite sprite = SpriteRenderer.sprite;

                sizeX = (int)SpriteRenderer.sprite.textureRect.width;
                sizeY = (int)SpriteRenderer.sprite.textureRect.height;

                texture = new Texture2D(sizeX, sizeY);
                texture.filterMode = FilterMode.Point;

                var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                             (int)sprite.textureRect.y,
                                             (int)sprite.textureRect.width,
                                             (int)sprite.textureRect.height);
                texture.SetPixels(pixels);
                texture.Apply();
                TextureToCells();
            }

            SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            UpdateCollider();

            //foreach (Module module in Module.GetValues(typeof(Module)))
            //{
            //    modules.Add(module, new List<List<Cell>>());
            //}
        }

        // DEBUG ONLY
        private void TextureToCells()
        {
            sizeX = (int)texture.width;
            sizeY = (int)texture.height;

            Cell[,] cells = new Cell[sizeX, sizeY];
            var pixelsData = texture.GetPixels32(0);

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    var pixelData = pixelsData[x + y * sizeX];
                    if (pixelData.a != 0)
                    {
                        cells[x, y] = new Cell(x, y, pixelData);
                    }
                }
            }

            bp = new Blueprint("debug", cells);
            //setting states TODO: negate on destroy
            //for (int y = 0; y < sizeY; y++)
            //{
            //    for (int x = 0; x < sizeX; x++)
            //    {
            //        if (cells[x, y] != null)
            //        {
            //            int i = 0;
            //            foreach (var n in neighbours)
            //            {
            //                TryGetCell(new Vector2Int(x, y) + n, out var cell);

            //                if (cell != null)
            //                {
            //                    Debug.Log(cell.X + " " + cell.Y + " " + i + " " + Mathf.Pow(2, i) + " " + n);
            //                    cells[x, y].state += (int)Mathf.Pow(2, i);
            //                }

            //                i++;
            //            }

            //            Debug.Log(x + " " + y + " " + cells[x, y].state);
            //        }
            //    }
            //}
        }

        private void CellsToTexture()
        {

            texture = new Texture2D(bp.sizeX, bp.sizeY);
            texture.filterMode = FilterMode.Point;
            GetComponent<Renderer>().material.mainTexture = texture;

            for (int y = 0; y < bp.sizeY; y++)
            {
                for (int x = 0; x < bp.sizeX; x++)
                {
                    if (bp.cells[x, y] != null)
                    {
                        texture.SetPixel(x, y, bp.cells[x, y].Color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, s_transparent);
                    }
                }
            }

            texture.Apply();
        }

        //TODO: MOVE TO THE SHIP 
        private void UpdateCollider()
        {
            // for (int y = 0; y < sizeY; y++) 
            // {
            //     for (int x = 0; x < sizeX; x++) 
            //     {
            //         if (cells[x, y] != null) 
            //         {
            //             path = getPath(x, y);
            //             GetComponent<PolygonCollider2D>().SetPath(0, path.ToArray());
            //             return;
            //         }
            //     }
            // }
        }

        private List<Vector2> getPath(int x, int y)
        {
            var newPath = new List<Vector2>();

            Vector2 firstNode = new Vector2((x - sizeX / 2f + 0.5f - offset) * scale, (y - sizeY / 2f + 0.5f - offset) * scale);

            newPath.Add(firstNode);
            int lastSubGrid = 4;
            Vector2Int lastCell = new Vector2Int(x, y);
            int maxIterations = 1000;
            int iteration = 0;

            while ((lastSubGrid = findNext(lastSubGrid, lastCell, out var last, out var next)) > 0 && next != firstNode && iteration < maxIterations)
            {
                if (newPath.Count > 1)
                {
                    var first = newPath[newPath.Count - 2];
                    var middle = newPath[newPath.Count - 1];
                    var y1 = first.y;
                    var y2 = middle.y;
                    var y3 = next.y;
                    var x1 = first.x;
                    var x2 = middle.x;
                    var x3 = next.x;

                    if (((y1 - y2) * (x1 - x3) - (y1 - y3) * (x1 - x2)) == 0)
                    {
                        newPath[newPath.Count - 1] = next;
                    }
                    else
                    {
                        newPath.Add(next);
                    }
                }
                else
                {
                    newPath.Add(next);
                }

                lastCell = last;
                iteration++;
            }

            return newPath;
        }

        ///////////////
        /// CELL
        /// 1 | 2     1 -> next ?4 or 1 -> 2
        /// -----     2 -> next ?1 or 2 -> 3
        /// 4 | 3     3 -> next ?2 or 3 -> 4
        ///           4 -> next ?3 or 4 -> 1 
        ///////////////

        private int findNext(int lastSubGrid, Vector2Int lastCell, out Vector2Int newCell, out Vector2 point)
        {
            int next = 0;
            newCell = new Vector2Int();
            point = new Vector2();
            float offsetX = offset;
            float offsetY = offset;

            if (lastSubGrid == 4)
            {
                if (lastCell.x - 1 >= 0 && bp.cells[lastCell.x - 1, lastCell.y] != null)
                {
                    newCell.Set(lastCell.x - 1, lastCell.y);
                    offsetY = -offsetY;
                    next = 3;
                }
                else
                {
                    newCell.Set(lastCell.x, lastCell.y);
                    offsetX = -offsetX;
                    next = 1;
                }
            }
            else if (lastSubGrid == 3)
            {
                if (lastCell.y - 1 >= 0 && bp.cells[lastCell.x, lastCell.y - 1] != null)
                {
                    newCell.Set(lastCell.x, lastCell.y - 1);
                    next = 2;
                }
                else
                {
                    newCell.Set(lastCell.x, lastCell.y);
                    offsetX = -offsetX;
                    offsetY = -offsetY;
                    next = 4;
                }
            }
            else if (lastSubGrid == 2)
            {
                if (lastCell.x + 1 < sizeX && bp.cells[lastCell.x + 1, lastCell.y] != null)
                {
                    newCell.Set(lastCell.x + 1, lastCell.y);
                    offsetX = -offsetX;
                    next = 1;
                }
                else
                {
                    newCell.Set(lastCell.x, lastCell.y);
                    offsetY = -offsetY;
                    next = 3;
                }
            }
            else if (lastSubGrid == 1)
            {
                if (lastCell.y + 1 < sizeY && bp.cells[lastCell.x, lastCell.y + 1] != null)
                {
                    newCell.Set(lastCell.x, lastCell.y + 1);
                    offsetX = -offsetX;
                    offsetY = -offsetY;
                    next = 4;
                }
                else
                {
                    newCell.Set(lastCell.x, lastCell.y);
                    next = 2;
                }
            }

            point.Set((newCell.x - sizeX / 2f + 0.5f + offsetX) * scale, (newCell.y - sizeY / 2f + 0.5f + offsetY) * scale);
            return next;
        }

        // Update is called once per frame
        void Update()
        {
            //Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //if (Input.GetMouseButtonDown(0))
            //{
            //Debug.Log(point);
            //Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);

            //if (targetObject)
            //{
            //    selectedObject = targetObject.transform.gameObject;
            //    mouseOffset = selectedObject.transform.position - mousePosition;
            //}
            //}

            // if (selectedObject)
            // {
            //     selectedObject.transform.position = mousePosition + mouseOffset;
            // }

            // if (Input.GetMouseButtonUp(0) && selectedObject)
            // {
            //     selectedObject = null;
            // }
        }

        private static Color s_transparent = new Color(0, 0, 0, 0);
        private void OnMouseDown()
        {
            Camera cam = Camera.main;
            Vector3 point = new Vector3();
            Vector2 mousePos = new Vector2();

            mousePos.x = Input.mousePosition.x;
            mousePos.y = Input.mousePosition.y;

            point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z));

            var bp = WorldPointToBlueprint(point);
            //ApplyDamage(bp.x, bp.y, 100);
        }

        public Vector2Int WorldPointToBlueprint(Vector3 point)
        {
            var rigidbody2D = GetComponent<Rigidbody2D>();

            Vector3 local;
            if (rigidbody2D != null)
            {
                local = transform.InverseTransformPoint(point + (Vector3)rigidbody2D.velocity * Time.fixedDeltaTime);
            }
            else
            {
                local = transform.InverseTransformPoint(point);
            }

            var bp = Vector3.Scale(local, transform.lossyScale * 100f);
            bp.x += sizeX / 2f;
            bp.y += sizeY / 2f;

            Debug.Log(transform.lossyScale);

            return new Vector2Int((int)bp.x, (int)bp.y);
        }

        public Vector3 BlueprintPointToWorld(float x, float y)
        {
            x -= sizeX / 2f - 0.5f;
            y -= sizeY / 2f - 0.5f;

            var local = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(1f / transform.lossyScale.x, 1f / transform.lossyScale.y, 1f));
            var world = transform.TransformPoint(local);

            return world;
        }

        private void ApplyDamage(int x, int y, int amount)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY && bp.cells[x, y] != null)
            {
                if (bp.cells[x, y].ApplyDamage(amount))
                {
                    spawnDebris(x, y);

                    bp.cells[x, y] = null;
                    TryChunks(x, y);


                    texture.SetPixel(x, y, s_transparent);
                    texture.Apply();
                }
            }
        }

        private void spawnDebris(int x, int y)
        {
            var worldPoint = BlueprintPointToWorld(x, y);
            var debris = Instantiate(debrisPref, worldPoint, transform.rotation);
            debris.transform.Rotate(-90, 0, 0);
            var rigidbody2D = GetComponent<Rigidbody2D>();
            debris.Inertia = rigidbody2D.velocity * Time.fixedDeltaTime + new Vector2(Random.Range(-0.00025f, 0.00025f), Random.Range(-0.001f, 0.001f));
            debris.Torgue = Random.Range(-0.05f, 0.05f);
            debris.GetComponent<Renderer>().material.SetColor("_Color", bp.cells[x, y].Color);
        }

        private static List<Vector2Int> neighbours = new List<Vector2Int>()
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

        private void TryChunks(int x, int y)
        {
            var cellPoint = new Vector2Int(x, y);
            var chunks = new List<List<Cell>>();

            foreach (var n in neighbours)
            {
                var testPoint = n + cellPoint;

                if (TryGetCell(testPoint, out var testCell))
                {
                    var tested = false;

                    foreach (var list in chunks)
                    {
                        if (testCell == null || list.Contains(testCell))
                        {
                            tested = true;
                        }
                    }

                    if (tested)
                    {
                        continue;
                    }

                    var chunk = FloodFill(testPoint);

                    if (chunk != null)
                    {
                        chunks.Add(chunk);
                    }
                }
            }

            //TODO: DELETE ON 0 CHUNKS?
            if (chunks.Count > 0)
            {
                //TODO: with largest bridge
                chunks.Sort(delegate (List<Cell> x, List<Cell> y)
                    {
                        return x.Count < y.Count ? 1 : -1;
                    }
                );

                var mainChunk = chunks[0];
                chunks.Remove(mainChunk);

                foreach (var chunk in chunks)
                {
                    if (chunk.Count < 9)
                    {
                        foreach (var cell in chunk)
                        {
                            texture.SetPixel(cell.X, cell.Y, s_transparent);
                            spawnDebris(cell.X, cell.Y);
                            bp.cells[cell.X, cell.Y] = null;
                        }
                        continue;
                    }

                    var chunkStartX = sizeX - 1; // GOING BACKWARDS
                    var chunkEndX = 0;
                    var chunkStartY = sizeY - 1;
                    var chunkEndY = 0;

                    foreach (var cell in chunk)
                    {
                        texture.SetPixel(cell.X, cell.Y, s_transparent);

                        if (chunkStartX > cell.X)
                        {
                            chunkStartX = cell.X;
                        }

                        if (chunkEndX < cell.X)
                        {
                            chunkEndX = cell.X;
                        }

                        if (chunkStartY > cell.Y)
                        {
                            chunkStartY = cell.Y;
                        }

                        if (chunkEndY < cell.Y)
                        {
                            chunkEndY = cell.Y;
                        }
                    }

                    //TODO: TRUE POSITION
                    var chunkPrefab = Instantiate(this, transform.position, transform.rotation);
                    chunkPrefab.bp = new Blueprint(bp.Name + " debris", new Cell[chunkEndX - chunkStartX + 1, chunkEndY - chunkStartY + 1]);

                    foreach (var cell in chunk)
                    {
                        chunkPrefab.bp.cells[cell.X - chunkStartX, cell.Y - chunkStartY] = cell;
                        bp.cells[cell.X, cell.Y] = null;
                    }

                    chunkPrefab.SpriteRenderer.sprite = null;
                    chunkPrefab.texture = null;
                }
            }

            UpdateCollider();
        }

        public bool TryGetCell(Vector2Int point, out Cell? cell)
        {
            return TryGetCell(point.x, point.y, out cell);
        }

        public bool TryGetCell(int x, int y, out Cell? cell)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                cell = bp.cells[x, y];
                if (cell != null)
                {
                    return true;
                }
            }

            cell = null;
            return false;
        }

        public void SetCell(int x, int y, Color32 color)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                if (bp.cells[x, y] == null) // CREATE NEW
                {
                    bp.cells[x, y] = new Cell(x, y, color);
                }
                else // PAINT
                {
                    bp.cells[x, y].Color = color;
                }

                texture.SetPixel(x, y, color);
                texture.Apply();
            }
        }

        public void RemoveCell(int x, int y)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                if (bp.cells[x, y] != null)
                {
                    // TODO: Destroy module

                    bp.cells[x, y] = null;

                    texture.SetPixel(x, y, s_transparent);
                    texture.Apply();
                }
            }
        }

        private List<Cell>? FloodFill(Vector2Int point, Module? module = null)
        {
            if (!TryGetCell(point, out var cell))
            {
                return null;
            }

            var chunk = new List<Cell>();
            var queue = new Stack<Cell>();

            queue.Push(cell);

            while (queue.Count > 0)
            {
                cell = queue.Pop();

                if (cell != null && !chunk.Contains(cell))
                {
                    foreach (var n in neighbours)
                    {
                        if (TryGetCell(n + new Vector2Int(cell.X, cell.Y), out var next)
                            //&& (module == null || module == next.module)
                            && !chunk.Contains(next)
                            && !queue.Contains(next))
                        {
                            queue.Push(next);
                        }
                    }

                    chunk.Add(cell);
                }
            }

            return chunk;
        }

        private void OnDrawGizmos()
        {
            // Gizmos.color = new Color(1, 0, 0, 0.5f);
            // foreach (var item in chunk)
            // {
            //     Gizmos.DrawCube(transform.position + new Vector3((item.X - sizeX / 2f + 0.5f) * scale * 10f, (item.Y - sizeY / 2f + 0.5f) * scale * 10f, 0), new Vector3(0.1f, 0.1f, 0));
            // }
        }

        public void SelectRange(Vector3 world1, Vector3 world2, Module module)
        {
            var min = new Vector2Int();
            var max = new Vector2Int(sizeX - 1, sizeY - 1);

            var start = WorldPointToBlueprint(world1);
            var end = WorldPointToBlueprint(world2);

            if (start.x > end.x)
            {
                var a = start.x;
                start.x = end.x;
                end.x = a;
            }

            if (start.y > end.y)
            {
                var a = start.y;
                start.y = end.y;
                end.y = a;
            }

            start.Clamp(min, max);
            end.Clamp(min, max);

            var selected = new List<Cell>();

            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    var cell = bp.cells[x, y];
                    if (cell != null /**&& cell.module != module**/)
                    {
                        selected.Add(cell);
                    }
                }
            }

            //foreach (var cell in selected)
            //{
            //    var prev = cell.module;
            //    cell.module = module;
            //    CalcModule(cell, prev);
            //}

            texture.Apply();
        }

        private float cargo = 0f;
        private float cargoBonus = 1f;

        private void CalcModule(Cell cell, Module previousModule)
        {
            Vector2Int bpPos = new Vector2Int(cell.X, cell.Y);

            //switch (previousModule)
            //{
            //    case Module.Cargo:
            //        cargo -= cargoBonus;

            //        foreach (var n in neighbours)
            //        {
            //            if (TryGetCell(n + bpPos, out var next) && next.module == Module.Cargo)
            //            {
            //                cargo -= cargoBonus / 4f;
            //            }
            //        }

            //        break;
            //}

            //switch (bp.cell.module)
            //{
            //    case Module.Cargo:
            //        cargo += cargoBonus;

            //        foreach (var n in neighbours)
            //        {
            //            if (TryGetCell(n + bpPos, out var next) && next.module == Module.Cargo)
            //            {
            //                cargo += cargoBonus / 4f;
            //            }
            //        }

            //        break;
            //}

            //Debug.Log("CARGO: " + cargo);
        }

        public void Save()
        {

        }

        //private Dictionary<Module, List<Cell>> modules = new Dictionary<Module, List<Cell>>();

        public enum Module
        {
            None,
            Cargo,
            Engine,
            Bridge
        }

        private class Engine
        {
            List<Cell> thruster;
            List<Cell> engine;


        }
    }
}
