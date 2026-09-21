using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    [Header("Board Size Settings")]
    public int BaseWidth = 8;
    public int BaseHeight = 8;
    public int MaxWidth = 20;
    public int MaxHeight = 20;

    [HideInInspector] public int Width;
    [HideInInspector] public int Height;

    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    [Header("Exit Cell Settings")]
    public ExitCellObject ExitCellPrefab;

    [Header("Wall Settings")]
    public WallObject[] WallPrefabs;

    [Header("Food Spawning Settings")]
    public FoodObject[] FoodPrefabs;
    public int MinFoodCount = 3;
    public int MaxFoodCount = 8;

    [Header("Enemy Spawning Settings")]
    public Enemy[] EnemyPrefabs;
    public int MinEnemyCount = 1;
    public int MaxEnemyCount = 2;

    [Header("Item Spawning Settings")]
    public CellObject[] ItemPrefabs;
    public int MinItemCount = 0;
    public int MaxItemCount = 2;

    public void Init(int level)
    {
        Width = Mathf.Min(BaseWidth + ((level - 1) / 2) * 2, MaxWidth);
        Height = Mathf.Min(BaseHeight + ((level - 1) / 2) * 2, MaxHeight);

        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;

                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateWall(level);
        GenerateFood(level);
        GenerateEnemy(level);
        GenerateItems(level);
    }

    public void Clean()
    {
        if (m_BoardData == null)
            return;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];
                if (cellData != null && cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }
                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    void GenerateWall(int level)
    {
        if (WallPrefabs == null || WallPrefabs.Length == 0) return;

        int minWalls = Width + level;
        int maxWalls = (Width * 2) + (level * 2);
        int wallCount = Random.Range(minWalls, maxWalls);

        for (int i = 0; i < wallCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            WallObject selectedPrefab = WallPrefabs[Random.Range(0, WallPrefabs.Length)];
            WallObject newWall = Instantiate(selectedPrefab);
            AddObject(newWall, coord);
        }
    }

    void GenerateFood(int level)
    {
        if (FoodPrefabs == null || FoodPrefabs.Length == 0) return;

        int maxFood = Mathf.Max(1, MaxFoodCount - (level / 2));
        int minFood = Mathf.Clamp(MinFoodCount - (level / 3), 1, maxFood);
        int foodCount = Random.Range(minFood, maxFood + 1);

        for (int i = 0; i < foodCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            FoodObject selectedPrefab = FoodPrefabs[Random.Range(0, FoodPrefabs.Length)];
            FoodObject newFood = Instantiate(selectedPrefab);
            AddObject(newFood, coord);
        }
    }

    void GenerateEnemy(int level)
    {
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0) return;

        int minEnemies = MinEnemyCount + ((level - 1) / 2);
        int maxEnemies = MaxEnemyCount + (level - 1);
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

        for (int i = 0; i < enemyCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            Enemy selectedPrefab = EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)];
            Enemy newEnemy = Instantiate(selectedPrefab);
            AddObject(newEnemy, coord);
        }
    }

    void GenerateItems(int level)
    {
        if (ItemPrefabs == null || ItemPrefabs.Length == 0) return;

        int itemCount = Random.Range(MinItemCount, MaxItemCount + 1);

        for (int i = 0; i < itemCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

           
            CellObject selectedPrefab = ItemPrefabs[Random.Range(0, ItemPrefabs.Length)];
            CellObject newItem = Instantiate(selectedPrefab);
            AddObject(newItem, coord);
        }
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }
}