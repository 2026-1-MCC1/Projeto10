using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertySpawner : MonoBehaviour
{
    [Header("Prefabs por Tipo")]
    [SerializeField] private GameObject prefabFactory;
    [SerializeField] private GameObject prefabResidential;
    [SerializeField] private GameObject prefabPark;
    [SerializeField] private GameObject prefabCommercial;
    [SerializeField] private GameObject prefabTreatmentPlant;
    [SerializeField] private GameObject prefabSchool;
    [SerializeField] private GameObject prefabHospital;
    [SerializeField] private GameObject prefabSolarPlant;
    [SerializeField] private GameObject prefabFoodCourt;
    [SerializeField] private GameObject prefabSpecial;

    [Header("Posições")]
    [SerializeField] private Transform boardCenter;

    [Header("Animação")]
    [SerializeField] private float riseDuration = 0.7f;
    [SerializeField] private float bounceAmount = 0.12f;
    [SerializeField] private float fallDuration = 0.4f;
    [SerializeField] private float previewRotationSpeed = 28f;

    [Header("Escala Automática")]
    [SerializeField] private float previewFootprint = 1.45f;
    [SerializeField] private float previewMaxHeight = 1.7f;
    [SerializeField] private float tileFootprint = 0.62f;
    [SerializeField] private float tileMaxHeight = 0.82f;
    [SerializeField] private float tilePlacementHeight = 0.26f;
    [SerializeField] private TileType debugPreviewType = TileType.SolarPlant;
    [SerializeField] private float solarPanelScale = 0.18f;

    private readonly Dictionary<TileType, GameObject> prefabMap = new Dictionary<TileType, GameObject>();
    private readonly Dictionary<int, GameObject> placedBuildings = new Dictionary<int, GameObject>();

    private GameObject currentBuilding;
    private Tile currentTile;
    private bool buildingPurchased;
    private Vector3 currentFinalScale = Vector3.one;
    private Vector3 placedFinalScale = Vector3.one;

    /// <summary>
    /// Exibe a propriedade configurada para debug diretamente no centro do tabuleiro.
    /// </summary>
    [ContextMenu("Preview Debug Property")]
    public void PreviewDebugProperty()
    {
        ShowPropertyByType(debugPreviewType, debugPreviewType.ToString());
    }

    /// <summary>
    /// Exibe rapidamente a Usina Solar para teste visual.
    /// </summary>
    [ContextMenu("Preview Solar Plant")]
    public void PreviewSolarPlant()
    {
        ShowPropertyByType(TileType.SolarPlant, "Usina Solar");
    }

    /// <summary>
    /// Inicializa o mapeamento de prefabs por tipo de propriedade.
    /// </summary>
    private void Awake()
    {
        BuildPrefabMap();
    }

    /// <summary>
    /// Mantem a construcao de preview girando suavemente enquanto ela esta no centro.
    /// </summary>
    private void Update()
    {
        if (currentBuilding == null || buildingPurchased)
        {
            return;
        }

        currentBuilding.transform.Rotate(Vector3.up, previewRotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Exibe a construção candidata no centro do tabuleiro para a propriedade atual.
    /// </summary>
    public void ShowProperty(Tile tile)
    {
        if (tile == null || tile.Data == null || boardCenter == null)
        {
            return;
        }

        if (!tile.CanBePurchased() || tile.IsPurchased)
        {
            return;
        }

        currentTile = tile;
        ShowPropertyByType(tile.Data.Type, tile.Data.Name, tile);
    }

    /// <summary>
    /// Exibe uma construção apenas pelo tipo, útil para preview e debug.
    /// </summary>
    public void ShowPropertyByType(TileType type, string displayName = null, Tile tile = null)
    {
        if (boardCenter == null)
        {
            return;
        }

        currentTile = tile;
        buildingPurchased = false;

        if (currentBuilding != null)
        {
            Destroy(currentBuilding);
            currentBuilding = null;
        }

        currentBuilding = CreateBuildingForType(type);

        if (currentBuilding == null)
        {
            return;
        }

        currentBuilding.transform.SetParent(transform, true);
        currentBuilding.transform.position = boardCenter.position;
        currentBuilding.name = $"Preview_{(string.IsNullOrWhiteSpace(displayName) ? type.ToString() : displayName)}";
        currentFinalScale = CalculateFittedScale(currentBuilding, previewFootprint, previewMaxHeight);
        placedFinalScale = CalculateFittedScale(currentBuilding, tileFootprint, tileMaxHeight);
        currentBuilding.transform.localScale = Vector3.zero;
        StartCoroutine(RiseAnimation());
    }

    /// <summary>
    /// Confirma a compra e move a construção do centro para a casa correspondente.
    /// </summary>
    public void ConfirmPurchase()
    {
        if (currentBuilding == null || currentTile == null)
        {
            return;
        }

        buildingPurchased = true;
        StartCoroutine(MoveToTile());
    }

    /// <summary>
    /// Cancela a compra e faz a construção sumir de volta no chão.
    /// </summary>
    public void CancelPurchase()
    {
        if (currentBuilding == null)
        {
            return;
        }

        StartCoroutine(SinkAnimation());
    }

    /// <summary>
    /// Anima a construção surgindo do chão com leve bounce.
    /// </summary>
    private IEnumerator RiseAnimation()
    {
        if (currentBuilding == null)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            float curve = EaseOutBack(t);
            float extraBounce = Mathf.Sin(t * Mathf.PI) * bounceAmount;
            currentBuilding.transform.localScale = currentFinalScale * (curve + extraBounce * (1f - t));
            yield return null;
        }

        currentBuilding.transform.localScale = currentFinalScale;
    }

    /// <summary>
    /// Move a construção comprada do centro do tabuleiro para cima do tile em arco.
    /// </summary>
    private IEnumerator MoveToTile()
    {
        if (currentBuilding == null || currentTile == null)
        {
            yield break;
        }

        Vector3 startPos = currentBuilding.transform.position;
        Vector3 endPos = currentTile.transform.position + Vector3.up * tilePlacementHeight;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            float arc = Mathf.Sin(t * Mathf.PI) * 1.5f;
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += arc;
            currentBuilding.transform.position = pos;
            currentBuilding.transform.localScale = Vector3.Lerp(currentFinalScale, placedFinalScale, t);
            yield return null;
        }

        currentBuilding.transform.position = endPos;
        currentBuilding.transform.localScale = placedFinalScale;
        placedBuildings[currentTile.TileIndex] = currentBuilding;
        currentBuilding = null;
        currentTile = null;
    }

    /// <summary>
    /// Faz a construção afundar e desaparecer quando o jogador decide não comprar.
    /// </summary>
    private IEnumerator SinkAnimation()
    {
        if (currentBuilding == null)
        {
            yield break;
        }

        Vector3 startScale = currentBuilding.transform.localScale;
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            currentBuilding.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(currentBuilding);
        currentBuilding = null;
        currentTile = null;
        buildingPurchased = false;
    }

    /// <summary>
    /// Retorna uma curva de saída com ultrapassagem para gerar o bounce.
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// Escolhe o prefab correto de acordo com o tipo da propriedade.
    /// </summary>
    private GameObject GetPrefabForType(TileType type)
    {
        if (prefabMap.Count == 0)
        {
            BuildPrefabMap();
        }

        prefabMap.TryGetValue(type, out GameObject prefab);
        return prefab;
    }

    /// <summary>
    /// Atualiza o dicionário de prefabs por tipo disponível.
    /// </summary>
    private void BuildPrefabMap()
    {
        prefabMap.Clear();
        prefabMap[TileType.Factory] = prefabFactory;
        prefabMap[TileType.Residential] = prefabResidential;
        prefabMap[TileType.Park] = prefabPark;
        prefabMap[TileType.Shopping] = prefabCommercial;
        prefabMap[TileType.FoodCourt] = prefabFoodCourt != null ? prefabFoodCourt : prefabCommercial;
        prefabMap[TileType.TreatmentPlant] = prefabTreatmentPlant;
        prefabMap[TileType.School] = prefabSchool != null ? prefabSchool : prefabSpecial;
        prefabMap[TileType.Hospital] = prefabHospital != null ? prefabHospital : prefabSpecial;
        prefabMap[TileType.SolarPlant] = prefabSolarPlant;
    }

    /// <summary>
    /// Cria a construcao apropriada para o tipo informado, usando prefab quando existir e fallback procedural quando necessario.
    /// </summary>
    private GameObject CreateBuildingForType(TileType type)
    {
        if (type == TileType.SolarPlant)
        {
            return CreateSolarPlantPreview();
        }

        if (type == TileType.TreatmentPlant && prefabTreatmentPlant == null)
        {
            return CreatePrimitiveBuilding(type);
        }

        GameObject prefab = GetPrefabForType(type);

        if (prefab != null)
        {
            return Instantiate(prefab, boardCenter.position, Quaternion.identity);
        }

        return CreatePrimitiveBuilding(type);
    }

    /// <summary>
    /// Cria a melhor versão possível da Usina Solar com base no prefab disponível.
    /// </summary>
    private GameObject CreateSolarPlantPreview()
    {
        if (prefabSolarPlant == null)
        {
            return CreatePrimitiveBuilding(TileType.SolarPlant);
        }

        return CreateSolarPanelCluster();
    }

    /// <summary>
    /// Monta um pequeno conjunto de paineis solares quando so existe um prefab unitario.
    /// </summary>
    private GameObject CreateSolarPanelCluster()
    {
        GameObject root = new GameObject("Prop_SolarPlantCluster");
        root.transform.SetParent(transform, false);

        Vector3[] localPositions =
        {
            new Vector3(-0.18f, 0f, -0.04f),
            new Vector3(0f, 0.01f, 0.04f),
            new Vector3(0.18f, 0f, -0.04f)
        };

        float[] yRotations = { -8f, 0f, 8f };

        for (int index = 0; index < localPositions.Length; index++)
        {
            GameObject panel = Instantiate(prefabSolarPlant, root.transform);
            panel.name = $"SolarPanel_{index + 1}";
            panel.transform.localPosition = localPositions[index];
            panel.transform.localRotation = Quaternion.Euler(0f, yRotations[index], 0f);
            panel.transform.localScale = Vector3.one * solarPanelScale;
        }

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.015f, 0f), new Vector3(0.62f, 0.03f, 0.42f), new Color(0.42f, 0.62f, 0.36f));
        return root;
    }

    /// <summary>
    /// Cria uma construção simples com primitivos quando ainda não existe prefab configurado.
    /// </summary>
    private GameObject CreatePrimitiveBuilding(TileType type)
    {
        GameObject root = new GameObject($"Prop_{type}");
        root.transform.SetParent(transform, false);
        root.transform.localScale = Vector3.one;

        switch (type)
        {
            case TileType.Factory:
                BuildFactory(root.transform);
                break;
            case TileType.Residential:
                BuildResidential(root.transform);
                break;
            case TileType.Park:
                BuildPark(root.transform);
                break;
            case TileType.Shopping:
            case TileType.FoodCourt:
                BuildCommercial(root.transform);
                break;
            case TileType.TreatmentPlant:
                BuildTreatmentPlant(root.transform);
                break;
            case TileType.School:
                BuildSchool(root.transform);
                break;
            case TileType.Hospital:
                BuildHospital(root.transform);
                break;
            case TileType.SolarPlant:
                BuildSolarPlant(root.transform);
                break;
            default:
                BuildSpecial(root.transform);
                break;
        }

        return root;
    }

    /// <summary>
    /// Calcula uma escala uniforme para caber no espaço desejado sem estourar o centro ou o tile.
    /// </summary>
    private Vector3 CalculateFittedScale(GameObject target, float maxFootprint, float maxHeight)
    {
        if (target == null)
        {
            return Vector3.one;
        }

        Bounds bounds = GetCombinedBounds(target);
        float width = Mathf.Max(bounds.size.x, 0.01f);
        float depth = Mathf.Max(bounds.size.z, 0.01f);
        float height = Mathf.Max(bounds.size.y, 0.01f);

        float footprintScale = maxFootprint / Mathf.Max(width, depth);
        float heightScale = maxHeight / height;
        float uniformScale = Mathf.Min(footprintScale, heightScale);
        return Vector3.one * uniformScale;
    }

    /// <summary>
    /// Obtém o volume total ocupado pelos renderers da construção.
    /// </summary>
    private Bounds GetCombinedBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Bounds combinedBounds = new Bounds(target.transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer childRenderer in renderers)
        {
            if (!hasBounds)
            {
                combinedBounds = childRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(childRenderer.bounds);
            }
        }

        return combinedBounds;
    }

    /// <summary>
    /// Monta uma fábrica simples com cubos e cilindros.
    /// </summary>
    private void BuildFactory(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.7f, 0.4f, 0.7f), new Color(0.36f, 0.25f, 0.22f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.6f, 0.5f), new Color(0.47f, 0.34f, 0.28f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(0.15f, 1f, 0f), new Vector3(0.08f, 0.4f, 0.08f), new Color(0.26f, 0.26f, 0.26f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(-0.15f, 0.95f, 0f), new Vector3(0.08f, 0.3f, 0.08f), new Color(0.26f, 0.26f, 0.26f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.84f, 0f), new Vector3(0.55f, 0.08f, 0.55f), new Color(0.31f, 0.2f, 0.18f));
    }

    /// <summary>
    /// Monta uma residência simples com base, paredes e telhado.
    /// </summary>
    private void BuildResidential(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(0.65f, 0.35f, 0.65f), new Color(0.74f, 0.74f, 0.74f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0f), new Vector3(0.5f, 0.45f, 0.5f), new Color(0.93f, 0.93f, 0.93f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.65f, 0f), new Vector3(0.6f, 0.05f, 0.6f), new Color(0.9f, 0.22f, 0.21f), new Vector3(30f, 0f, 0f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.18f, 0.26f), new Vector3(0.1f, 0.18f, 0.02f), new Color(0.36f, 0.25f, 0.22f));
    }

    /// <summary>
    /// Monta um pequeno parque com gramado, arvores e banco.
    /// </summary>
    private void BuildPark(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.8f, 0.04f, 0.8f), new Color(0.4f, 0.73f, 0.42f));
        BuildTree(root, new Vector3(0f, 0f, 0f), 0.25f);
        BuildTree(root, new Vector3(0.2f, 0f, 0.2f), 0.25f);
        BuildTree(root, new Vector3(-0.2f, 0f, -0.15f), 0.2f);
        CreatePart(root, PrimitiveType.Cube, new Vector3(0.18f, 0.12f, -0.22f), new Vector3(0.3f, 0.04f, 0.08f), new Color(0.63f, 0.53f, 0.5f));
    }

    /// <summary>
    /// Monta um prédio comercial simples com vitrine e placa.
    /// </summary>
    private void BuildCommercial(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), new Vector3(0.7f, 0.08f, 0.7f), new Color(1f, 0.65f, 0.15f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), new Vector3(0.55f, 0.55f, 0.55f), new Color(1f, 0.8f, 0.01f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0.28f), new Vector3(0.4f, 0.12f, 0.02f), new Color(0.9f, 0.22f, 0.21f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0.28f), new Vector3(0.35f, 0.2f, 0.02f), new Color(0.7f, 0.9f, 0.99f));
    }

    /// <summary>
    /// Monta uma estacao de tratamento simples com tanques, bloco tecnico e tubulacoes.
    /// </summary>
    private void BuildTreatmentPlant(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), new Vector3(0.9f, 0.08f, 0.75f), new Color(0.56f, 0.67f, 0.72f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(-0.22f, 0.26f, 0f), new Vector3(0.2f, 0.22f, 0.2f), new Color(0.72f, 0.8f, 0.84f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(0.22f, 0.26f, 0f), new Vector3(0.2f, 0.22f, 0.2f), new Color(0.72f, 0.8f, 0.84f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.34f, -0.18f), new Vector3(0.28f, 0.36f, 0.2f), new Color(0.43f, 0.53f, 0.57f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(0f, 0.38f, 0.06f), new Vector3(0.06f, 0.3f, 0.06f), new Color(0.35f, 0.4f, 0.43f), new Vector3(90f, 0f, 0f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(0.22f, 0.55f, 0f), new Vector3(0.04f, 0.16f, 0.04f), new Color(0.27f, 0.31f, 0.33f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(-0.32f, 0.08f, 0.24f), new Vector3(0.14f, 0.08f, 0.14f), new Color(0.2f, 0.55f, 0.68f));
    }

    /// <summary>
    /// Monta uma pequena escola com predio central e entrada destacada.
    /// </summary>
    private void BuildSchool(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.82f, 0.1f, 0.7f), new Color(0.94f, 0.84f, 0.52f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), new Vector3(0.6f, 0.45f, 0.48f), new Color(0.96f, 0.92f, 0.74f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.62f, 0f), new Vector3(0.66f, 0.05f, 0.54f), new Color(0.8f, 0.25f, 0.21f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.18f, 0.25f), new Vector3(0.14f, 0.2f, 0.04f), new Color(0.33f, 0.21f, 0.16f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.48f, 0.25f), new Vector3(0.3f, 0.12f, 0.04f), new Color(0.45f, 0.67f, 0.85f));
    }

    /// <summary>
    /// Monta um pequeno hospital com bloco principal e cruz frontal.
    /// </summary>
    private void BuildHospital(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.84f, 0.1f, 0.72f), new Color(0.88f, 0.9f, 0.94f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.38f, 0f), new Vector3(0.58f, 0.55f, 0.52f), new Color(0.97f, 0.98f, 0.99f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0f), new Vector3(0.64f, 0.05f, 0.58f), new Color(0.64f, 0.72f, 0.8f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0.27f), new Vector3(0.1f, 0.28f, 0.03f), new Color(0.9f, 0.24f, 0.21f));
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0.27f), new Vector3(0.26f, 0.1f, 0.03f), new Color(0.9f, 0.24f, 0.21f));
    }

    /// <summary>
    /// Monta uma pequena usina solar com varios paineis sobre uma base de gramado tecnico.
    /// </summary>
    private void BuildSolarPlant(Transform root)
    {
        CreatePart(root, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.95f, 0.04f, 0.72f), new Color(0.4f, 0.62f, 0.36f));
        BuildSolarPanel(root, new Vector3(-0.26f, 0.12f, 0f), -18f);
        BuildSolarPanel(root, new Vector3(0f, 0.15f, 0.08f), 0f);
        BuildSolarPanel(root, new Vector3(0.26f, 0.12f, 0f), 18f);
    }

    /// <summary>
    /// Cria um painel solar simples inclinado sobre dois suportes.
    /// </summary>
    private void BuildSolarPanel(Transform root, Vector3 basePosition, float yRotation)
    {
        CreatePart(root, PrimitiveType.Cylinder, basePosition + new Vector3(-0.08f, 0.09f, 0f), new Vector3(0.015f, 0.1f, 0.015f), new Color(0.45f, 0.49f, 0.52f));
        CreatePart(root, PrimitiveType.Cylinder, basePosition + new Vector3(0.08f, 0.09f, 0f), new Vector3(0.015f, 0.1f, 0.015f), new Color(0.45f, 0.49f, 0.52f));
        CreatePart(root, PrimitiveType.Cube, basePosition + new Vector3(0f, 0.2f, 0f), new Vector3(0.28f, 0.02f, 0.18f), new Color(0.21f, 0.44f, 0.72f), new Vector3(25f, yRotation, 0f));
    }

    /// <summary>
    /// Monta um monumento especial com base, pilares e orbe.
    /// </summary>
    private void BuildSpecial(Transform root)
    {
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(0.4f, 0.04f, 0.4f), new Color(0.61f, 0.15f, 0.69f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(0.2f, 0.5f, 0f), new Vector3(0.05f, 0.5f, 0.05f), new Color(0.81f, 0.58f, 0.85f));
        CreatePart(root, PrimitiveType.Cylinder, new Vector3(-0.2f, 0.5f, 0f), new Vector3(0.05f, 0.5f, 0.05f), new Color(0.81f, 0.58f, 0.85f));

        GameObject orb = CreatePart(root, PrimitiveType.Sphere, new Vector3(0f, 0.6f, 0f), new Vector3(0.2f, 0.2f, 0.2f), new Color(0.88f, 0.25f, 0.98f));
        Renderer orbRenderer = orb.GetComponent<Renderer>();

        if (orbRenderer != null)
        {
            orbRenderer.material.EnableKeyword("_EMISSION");
            orbRenderer.material.SetColor("_EmissionColor", new Color(0.88f, 0.25f, 0.98f) * 0.8f);
        }
    }

    /// <summary>
    /// Cria uma arvore simples para o parque.
    /// </summary>
    private void BuildTree(Transform root, Vector3 basePosition, float crownSize)
    {
        CreatePart(root, PrimitiveType.Cylinder, basePosition + new Vector3(0f, 0.17f, 0f), new Vector3(0.05f, 0.3f, 0.05f), new Color(0.36f, 0.25f, 0.22f));
        CreatePart(root, PrimitiveType.Sphere, basePosition + new Vector3(0f, 0.45f, 0f), new Vector3(crownSize, crownSize, crownSize), new Color(0.18f, 0.49f, 0.2f));
    }

    /// <summary>
    /// Cria uma peça primitiva com cor simples e remove colisor desnecessário.
    /// </summary>
    private GameObject CreatePart(Transform parent, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color, Vector3? localEuler = null)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        if (localEuler.HasValue)
        {
            part.transform.localRotation = Quaternion.Euler(localEuler.Value);
        }

        Renderer renderer = part.GetComponent<Renderer>();

        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.material = material;
        }

        Collider collider = part.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        return part;
    }
}
