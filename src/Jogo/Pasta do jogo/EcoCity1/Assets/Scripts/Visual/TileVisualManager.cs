using System.Collections.Generic;
using UnityEngine;

public class TileVisualManager : MonoBehaviour
{
    [Header("Materiais por Tipo")]
    [SerializeField] private Material matStart;
    [SerializeField] private Material matFactory;
    [SerializeField] private Material matResidential;
    [SerializeField] private Material matPark;
    [SerializeField] private Material matCommercial;
    [SerializeField] private Material matSpecial;

    [Header("Materiais do Tabuleiro")]
    [SerializeField] private Material matBoardEdgeWood;
    [SerializeField] private Material matBoardFloor;

    [Header("Estados Visuais")]
    [SerializeField] private Material matHighlight;
    [SerializeField] private Material matPurchased;

    [Header("Borda Dourada (tile atual)")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMin = 0.7f;
    [SerializeField] private float pulseMax = 1.0f;

    private readonly Dictionary<int, Material> originalMaterials = new Dictionary<int, Material>();
    private readonly Dictionary<int, Material> purchasedMaterials = new Dictionary<int, Material>();
    private readonly Dictionary<int, GameObject> purchasedIcons = new Dictionary<int, GameObject>();
    private readonly List<Tile> allTiles = new List<Tile>();

    private int currentTileIndex = -1;
    private Renderer currentHighlight;
    private GameObject boardVisualRoot;

    /// <summary>
    /// Inicializa o visual de todos os tiles e cria a base visual do tabuleiro.
    /// </summary>
    public void Initialize(List<Tile> tiles)
    {
        allTiles.Clear();
        originalMaterials.Clear();
        purchasedMaterials.Clear();
        currentTileIndex = -1;
        currentHighlight = null;

        if (tiles == null || tiles.Count == 0)
        {
            return;
        }

        allTiles.AddRange(tiles);

        foreach (Tile tile in allTiles)
        {
            ApplyTypeMaterial(tile);
        }

        BuildBoardVisuals();
    }

    /// <summary>
    /// Atualiza o brilho pulsante da casa atual do jogador.
    /// </summary>
    private void Update()
    {
        if (currentHighlight == null)
        {
            return;
        }

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(pulseMin, pulseMax, t);
        currentHighlight.material.SetColor("_EmissionColor", Color.yellow * intensity);
    }

    /// <summary>
    /// Aplica o material base correto de acordo com o tipo da casa.
    /// </summary>
    private void ApplyTypeMaterial(Tile tile)
    {
        if (tile == null || tile.Data == null)
        {
            return;
        }

        Renderer tileRenderer = tile.GetComponent<Renderer>();

        if (tileRenderer == null)
        {
            return;
        }

        Material baseMaterial = GetMaterialForType(tile.Data.Type);

        if (baseMaterial == null)
        {
            return;
        }

        tileRenderer.material = new Material(baseMaterial);
        originalMaterials[tile.TileIndex] = tileRenderer.material;
    }

    /// <summary>
    /// Destaca a casa atual do jogador e restaura a casa anterior.
    /// </summary>
    public void SetCurrentTile(int index)
    {
        if (allTiles.Count == 0 || index < 0 || index >= allTiles.Count)
        {
            return;
        }

        if (currentTileIndex >= 0 && currentTileIndex < allTiles.Count)
        {
            RestoreTileMaterial(currentTileIndex);
        }

        currentTileIndex = index;
        currentHighlight = allTiles[index].GetComponent<Renderer>();

        if (currentHighlight == null || matHighlight == null)
        {
            return;
        }

        currentHighlight.material = new Material(matHighlight);
        currentHighlight.material.EnableKeyword("_EMISSION");
    }

    /// <summary>
    /// Marca visualmente uma casa como comprada e cria um icone acima dela.
    /// </summary>
    public void MarkAsPurchased(int index)
    {
        if (index < 0 || index >= allTiles.Count)
        {
            return;
        }

        Tile tile = allTiles[index];
        Renderer tileRenderer = tile.GetComponent<Renderer>();

        if (tileRenderer == null)
        {
            return;
        }

        if (!originalMaterials.TryGetValue(index, out Material baseMaterial) || baseMaterial == null)
        {
            return;
        }

        Material purchasedMaterial;

        if (matPurchased != null)
        {
            purchasedMaterial = new Material(matPurchased);
            purchasedMaterial.color = baseMaterial.color * 0.6f;
        }
        else
        {
            purchasedMaterial = new Material(baseMaterial);
            purchasedMaterial.color = baseMaterial.color * 0.6f;
        }

        purchasedMaterials[index] = purchasedMaterial;

        if (currentTileIndex != index)
        {
            tileRenderer.material = purchasedMaterial;
        }

        SpawnPurchasedIcon(tile);
    }

    /// <summary>
    /// Seleciona o material visual adequado para cada tipo de casa.
    /// </summary>
    private Material GetMaterialForType(TileType type)
    {
        switch (type)
        {
            case TileType.Start:
                return matStart;
            case TileType.Factory:
                return matFactory;
            case TileType.Residential:
                return matResidential;
            case TileType.Park:
                return matPark;
            case TileType.Shopping:
            case TileType.FoodCourt:
                return matCommercial;
            case TileType.Empty:
            case TileType.TreatmentPlant:
            case TileType.School:
            case TileType.Hospital:
            case TileType.SolarPlant:
            default:
                return matSpecial;
        }
    }

    /// <summary>
    /// Restaura o material base ou comprado de um tile que deixou de ser o atual.
    /// </summary>
    private void RestoreTileMaterial(int index)
    {
        if (index < 0 || index >= allTiles.Count)
        {
            return;
        }

        Renderer tileRenderer = allTiles[index].GetComponent<Renderer>();

        if (tileRenderer == null)
        {
            return;
        }

        if (allTiles[index].IsPurchased && purchasedMaterials.TryGetValue(index, out Material purchasedMaterial))
        {
            tileRenderer.material = purchasedMaterial;
            return;
        }

        if (originalMaterials.TryGetValue(index, out Material originalMaterial))
        {
            tileRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// Cria um pequeno marcador acima de uma casa comprada.
    /// </summary>
    private void SpawnPurchasedIcon(Tile tile)
    {
        if (tile == null || purchasedIcons.ContainsKey(tile.TileIndex))
        {
            return;
        }

        GameObject icon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        icon.name = $"PurchasedIcon_{tile.TileIndex:D2}";
        icon.transform.SetParent(tile.transform, true);
        icon.transform.position = tile.transform.position + Vector3.up * 0.8f;
        icon.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        Renderer iconRenderer = icon.GetComponent<Renderer>();

        if (iconRenderer != null)
        {
            Material iconMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            iconMaterial.color = new Color(0.18f, 0.52f, 0.22f, 0.85f);
            iconRenderer.material = iconMaterial;
        }

        Collider iconCollider = icon.GetComponent<Collider>();

        if (iconCollider != null)
        {
            Destroy(iconCollider);
        }

        purchasedIcons[tile.TileIndex] = icon;
    }

    /// <summary>
    /// Cria o piso e a moldura de madeira ao redor do tabuleiro.
    /// </summary>
    private void BuildBoardVisuals()
    {
        if (boardVisualRoot != null)
        {
            if (Application.isPlaying)
            {
                Destroy(boardVisualRoot);
            }
            else
            {
                DestroyImmediate(boardVisualRoot);
            }
        }

        boardVisualRoot = new GameObject("BoardVisualRoot");
        boardVisualRoot.transform.SetParent(transform, false);

        Vector3 min = allTiles[0].transform.position;
        Vector3 max = allTiles[0].transform.position;

        foreach (Tile tile in allTiles)
        {
            Vector3 position = tile.transform.position;
            min = Vector3.Min(min, position);
            max = Vector3.Max(max, position);
        }

        Vector3 center = (min + max) * 0.5f;
        float width = (max.x - min.x) + 3f;
        float depth = (max.z - min.z) + 3f;

        CreateBoardFloor(center, width, depth);
        CreateBoardFrame(center, width, depth);
    }

    /// <summary>
    /// Cria o piso escuro abaixo das casas do tabuleiro.
    /// </summary>
    private void CreateBoardFloor(Vector3 center, float width, float depth)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "BoardFloorVisual";
        floor.transform.SetParent(boardVisualRoot.transform, false);
        floor.transform.position = new Vector3(center.x, center.y - 0.12f, center.z);
        floor.transform.localScale = new Vector3(width, 0.08f, depth);

        ApplyMaterialToPrimitive(floor, matBoardFloor);
    }

    /// <summary>
    /// Cria uma moldura de madeira ao redor do tabuleiro.
    /// </summary>
    private void CreateBoardFrame(Vector3 center, float width, float depth)
    {
        CreateFramePiece("BoardFrame_North", new Vector3(center.x, center.y + 0.05f, center.z - depth * 0.5f), new Vector3(width, 0.2f, 0.3f));
        CreateFramePiece("BoardFrame_South", new Vector3(center.x, center.y + 0.05f, center.z + depth * 0.5f), new Vector3(width, 0.2f, 0.3f));
        CreateFramePiece("BoardFrame_East", new Vector3(center.x + width * 0.5f, center.y + 0.05f, center.z), new Vector3(0.3f, 0.2f, depth));
        CreateFramePiece("BoardFrame_West", new Vector3(center.x - width * 0.5f, center.y + 0.05f, center.z), new Vector3(0.3f, 0.2f, depth));
    }

    /// <summary>
    /// Cria uma unica peca da moldura externa do tabuleiro.
    /// </summary>
    private void CreateFramePiece(string pieceName, Vector3 position, Vector3 scale)
    {
        GameObject framePiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        framePiece.name = pieceName;
        framePiece.transform.SetParent(boardVisualRoot.transform, false);
        framePiece.transform.position = position;
        framePiece.transform.localScale = scale;

        ApplyMaterialToPrimitive(framePiece, matBoardEdgeWood);
    }

    /// <summary>
    /// Aplica um material a um objeto primitivo, quando disponivel.
    /// </summary>
    private void ApplyMaterialToPrimitive(GameObject target, Material material)
    {
        if (target == null)
        {
            return;
        }

        Renderer targetRenderer = target.GetComponent<Renderer>();

        if (targetRenderer != null && material != null)
        {
            targetRenderer.material = new Material(material);
        }
    }
}
