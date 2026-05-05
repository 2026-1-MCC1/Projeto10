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
    [SerializeField] private TileVisualManager visualManager;

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

        visualManager?.Initialize(Tiles);
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
                purchasePrice = 0,
                rentPrice = 0,
                moneyImpact = 0,
                wellBeingImpact = 0,
                pollutionImpact = 0,
                propertyDescription = "Casa vazia sem efeitos."
            };
        }

        TileType type = pathTypes[index];

        return new TileData
        {
            Name = GetTileName(type, index),
            Type = type,
            purchasePrice = GetTilePrice(type),
            rentPrice = GetTileRent(type),
            moneyImpact = GetFinanceImpact(type),
            wellBeingImpact = GetWellBeingImpact(type),
            pollutionImpact = GetPollutionImpact(type),
            propertyDescription = GetTileDescription(type)
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
                return 90;
            case TileType.Residential:
                return 120;
            case TileType.School:
            case TileType.Hospital:
                return 170;
            case TileType.SolarPlant:
            case TileType.TreatmentPlant:
                return 190;
            case TileType.Factory:
            case TileType.Shopping:
            case TileType.FoodCourt:
                return 150;
            default:
                return 0;
        }
    }

    private int GetTileRent(TileType type)
    {
        switch (type)
        {
            case TileType.Start:
            case TileType.Empty:
                return 0;
            case TileType.Park:
                return 12;
            case TileType.Residential:
                return 18;
            case TileType.School:
            case TileType.Hospital:
                return 24;
            case TileType.SolarPlant:
            case TileType.TreatmentPlant:
                return 26;
            case TileType.Factory:
            case TileType.Shopping:
            case TileType.FoodCourt:
                return 28;
            default:
                return 0;
        }
    }

    private int GetFinanceImpact(TileType type)
    {
        switch (type)
        {
            case TileType.Start:
                return 60;
            case TileType.Factory:
                return 22;
            case TileType.Shopping:
                return 18;
            case TileType.FoodCourt:
                return 14;
            case TileType.SolarPlant:
                return 8;
            case TileType.Residential:
                return 10;
            case TileType.TreatmentPlant:
            case TileType.School:
            case TileType.Hospital:
            case TileType.Park:
                return -6;
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
                return 18;
            case TileType.School:
                return 16;
            case TileType.Hospital:
                return 22;
            case TileType.Residential:
                return 12;
            case TileType.TreatmentPlant:
                return 10;
            case TileType.SolarPlant:
                return 8;
            case TileType.Shopping:
            case TileType.FoodCourt:
                return 5;
            case TileType.Factory:
                return -10;
            default:
                return 0;
        }
    }

    private int GetPollutionImpact(TileType type)
    {
        switch (type)
        {
            case TileType.Factory:
                return 18;
            case TileType.Shopping:
                return 8;
            case TileType.FoodCourt:
                return 6;
            case TileType.Residential:
                return 4;
            case TileType.Park:
                return -12;
            case TileType.SolarPlant:
                return -10;
            case TileType.TreatmentPlant:
                return -9;
            case TileType.School:
            case TileType.Hospital:
                return -4;
            default:
                return 0;
        }
    }

    private string GetTileDescription(TileType type)
    {
        switch (type)
        {
            case TileType.Factory:
                return "Gera renda mas aumenta a poluicao da cidade.";
            case TileType.Residential:
                return "Moradia para a populacao com crescimento moderado.";
            case TileType.Park:
                return "Melhora a qualidade de vida e reduz a poluicao.";
            case TileType.Shopping:
                return "Movimenta a economia local com impacto urbano.";
            case TileType.School:
                return "Fortalece a educacao e o bem-estar coletivo.";
            case TileType.Hospital:
                return "Aumenta a saude publica e reduz riscos sociais.";
            case TileType.SolarPlant:
                return "Produz energia limpa com ganhos ambientais.";
            case TileType.TreatmentPlant:
                return "Trata residuos e ajuda no equilibrio ambiental.";
            case TileType.FoodCourt:
                return "Atrai consumo e renda com impacto moderado.";
            case TileType.Start:
                return "Casa inicial do percurso.";
            default:
                return "Casa especial sem descricao adicional.";
        }
    }
}
