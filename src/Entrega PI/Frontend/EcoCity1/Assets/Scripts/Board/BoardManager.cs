using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private const int GridSize = 6;
    private const int ExpectedTileCount = 20;

    [Header("Board Setup")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private float tileSpacing = 2.2f;
    [SerializeField] private float tileHeight = 0f;
    [SerializeField] private bool generateOnStart = true;

    public List<Tile> Tiles { get; } = new List<Tile>();

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateBoard();
        }
    }

    public Tile GetTileByIndex(int index)
    {
        if (index < 0 || index >= Tiles.Count)
        {
            return null;
        }

        return Tiles[index];
    }

    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("BoardManager precisa de um tilePrefab configurado.", this);
            return;
        }

        ClearBoard();
        List<Vector2Int> path = GenerateBoardPath();

        for (int index = 0; index < path.Count; index++)
        {
            Vector2Int gridPosition = path[index];
            Vector3 worldPosition = GetWorldPosition(gridPosition.x, gridPosition.y);

            Tile tileInstance = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
            TileData tileData = GenerateTileData(index);

            tileInstance.Initialize(tileData, index);
            Tiles.Add(tileInstance);
        }

        if (Tiles.Count != ExpectedTileCount)
        {
            Debug.LogError($"O tabuleiro deveria ter {ExpectedTileCount} tiles, mas gerou {Tiles.Count}.", this);
        }
    }

    private void ClearBoard()
    {
        Tiles.Clear();

        for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            GameObject child = transform.GetChild(childIndex).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private List<Vector2Int> GenerateBoardPath()
    {
        List<Vector2Int> path = new List<Vector2Int>(ExpectedTileCount);

        // 1. Comeca no canto inferior esquerdo e percorre a base para a direita.
        for (int x = 0; x < GridSize; x++)
        {
            path.Add(new Vector2Int(x, 0));
        }

        // 2. Sobe pela borda direita sem repetir o canto inferior direito.
        for (int z = 1; z < GridSize; z++)
        {
            path.Add(new Vector2Int(GridSize - 1, z));
        }

        // 3. Vai pela borda superior da direita para a esquerda.
        for (int x = GridSize - 2; x >= 0; x--)
        {
            path.Add(new Vector2Int(x, GridSize - 1));
        }

        // 4. Desce pela borda esquerda sem repetir os cantos.
        for (int z = GridSize - 2; z > 0; z--)
        {
            path.Add(new Vector2Int(0, z));
        }

        return path;
    }

    private Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x * tileSpacing, tileHeight, z * tileSpacing);
    }

    private TileData GenerateTileData(int index)
    {
        TileType[] pathTypes =
        {
            TileType.Start,
            TileType.Residential,
            TileType.Park,
            TileType.Factory,
            TileType.Shopping,
            TileType.School,
            TileType.Hospital,
            TileType.Factory,
            TileType.TreatmentPlant,
            TileType.Residential,
            TileType.Park,
            TileType.Shopping,
            TileType.SolarPlant,
            TileType.FoodCourt,
            TileType.Residential,
            TileType.Factory,
            TileType.School,
            TileType.Park,
            TileType.Hospital,
            TileType.Shopping
        };

        if (index < 0 || index >= pathTypes.Length)
        {
            Debug.LogWarning($"Indice de tile invalido: {index}. Um tile Empty sera usado.", this);

            return new TileData
            {
                Name = $"Empty_{index}",
                Type = TileType.Empty,
                Price = 0,
                FinanceImpact = 0,
                WellBeingImpact = 0,
                PollutionImpact = 0
            };
        }

        TileType type = pathTypes[index];

        return new TileData
        {
            Name = GetTileName(type, index),
            Type = type,
            Price = GetTilePrice(type),
            FinanceImpact = GetFinanceImpact(type),
            WellBeingImpact = GetWellBeingImpact(type),
            PollutionImpact = GetPollutionImpact(type)
        };
    }

    private string GetTileName(TileType type, int index)
    {
        if (type == TileType.Start)
        {
            return "Start";
        }

        return $"{type}_{index}";
    }

    private int GetTilePrice(TileType type)
    {
        switch (type)
        {
            case TileType.Start:
            case TileType.Empty:
                return 0;
            case TileType.Park:
                return 120;
            case TileType.Residential:
                return 160;
            case TileType.School:
            case TileType.Hospital:
                return 220;
            case TileType.SolarPlant:
            case TileType.TreatmentPlant:
                return 260;
            case TileType.Factory:
            case TileType.Shopping:
            case TileType.FoodCourt:
                return 200;
            default:
                return 0;
        }
    }

    private int GetFinanceImpact(TileType type)
    {
        switch (type)
        {
            case TileType.Start:
                return 100;
            case TileType.Factory:
                return 35;
            case TileType.Shopping:
                return 30;
            case TileType.FoodCourt:
                return 20;
            case TileType.SolarPlant:
                return 10;
            case TileType.Residential:
                return 5;
            case TileType.TreatmentPlant:
            case TileType.School:
            case TileType.Hospital:
            case TileType.Park:
                return -10;
            default:
                return 0;
        }
    }

    private int GetWellBeingImpact(TileType type)
    {
        switch (type)
        {
            case TileType.Start:
                return 0;
            case TileType.Park:
                return 25;
            case TileType.School:
                return 20;
            case TileType.Hospital:
                return 30;
            case TileType.Residential:
                return 10;
            case TileType.TreatmentPlant:
                return 15;
            case TileType.SolarPlant:
                return 8;
            case TileType.Shopping:
            case TileType.FoodCourt:
                return 5;
            case TileType.Factory:
                return -15;
            default:
                return 0;
        }
    }

    private int GetPollutionImpact(TileType type)
    {
        switch (type)
        {
            case TileType.Factory:
                return 30;
            case TileType.Shopping:
                return 12;
            case TileType.FoodCourt:
                return 8;
            case TileType.Residential:
                return 6;
            case TileType.Park:
                return -20;
            case TileType.SolarPlant:
                return -18;
            case TileType.TreatmentPlant:
                return -15;
            case TileType.School:
            case TileType.Hospital:
                return -5;
            default:
                return 0;
        }
    }
}
