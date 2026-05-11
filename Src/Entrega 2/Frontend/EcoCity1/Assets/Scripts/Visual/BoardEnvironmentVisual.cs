using System.Collections.Generic;
using UnityEngine;

public class BoardEnvironmentVisual : MonoBehaviour
{
    private enum DistrictTheme
    {
        Residential,
        Commercial,
        Civic,
        Eco
    }

    [Header("Prefabs Decorativos (Opcionais)")]
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject lamppostPrefab;
    [SerializeField] private GameObject lowBuildingPrefab;
    [SerializeField] private GameObject tallBuildingPrefab;
    [SerializeField] private GameObject benchPrefab;

    [Header("Colecoes de Prefabs da Cidade")]
    [SerializeField] private GameObject[] lowRiseBuildingPrefabs;
    [SerializeField] private GameObject[] midRiseBuildingPrefabs;
    [SerializeField] private GameObject[] tallRiseBuildingPrefabs;
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] streetDetailPrefabs;

    [Header("Prefabs de Rua (Opcionais)")]
    [SerializeField] private GameObject roadStraightPrefab;
    [SerializeField] private GameObject roadTurnPrefab;
    [SerializeField] private GameObject roadTJunctionPrefab;
    [SerializeField] private GameObject roadAllDirectionsPrefab;
    [SerializeField] private GameObject roadCrosswalkPrefab;

    [Header("Layout da Cidade")]
    [SerializeField] private float outerMargin = 8f;
    [SerializeField] private float cleanMarginAroundBoard = 14f;
    [SerializeField] private float cityExtent = 44f;
    [SerializeField] private float roadWidth = 3f;
    [SerializeField] private float blockSize = 10f;
    [SerializeField] private float lotInset = 1.2f;
    [SerializeField] private float detailOffset = 2f;
    [SerializeField] private float roadSurfaceY = -0.53f;
    [SerializeField] private float lotSurfaceY = -0.28f;

    [Header("Variacao da Cidade")]
    [SerializeField] private int detailsPerSide = 5;
    [SerializeField] private float firstDistrictDistance = 18f;
    [SerializeField] private float secondDistrictDistance = 28f;
    [SerializeField] private float thirdDistrictDistance = 40f;
    [SerializeField] private float lowDistrictMaxHeight = 4.5f;
    [SerializeField] private float midDistrictMaxHeight = 6.5f;
    [SerializeField] private float highDistrictMaxHeight = 7.5f;

    [Header("Skyline e Landmarks")]
    [SerializeField] private int skylineBands = 3;
    [SerializeField] private float skylineStartDistance = 52f;
    [SerializeField] private float skylineBandSpacing = 12f;
    [SerializeField] private int skylineBuildingsPerBand = 10;
    [SerializeField] private float landmarkScaleBoost = 1.25f;
    [SerializeField] private float landmarkDistanceFromBoard = 24f;

    [Header("Atmosfera")]
    [SerializeField] private bool applyAtmosphere = true;
    [SerializeField] private Color directionalLightColor = new Color(1f, 0.90f, 0.78f, 1f);
    [SerializeField] private float directionalLightIntensity = 1.28f;
    [SerializeField] private Vector3 directionalLightEuler = new Vector3(36f, -28f, 0f);
    [SerializeField] private Color fogColor = new Color(0.73f, 0.79f, 0.85f, 1f);
    [SerializeField] private float fogDensity = 0.0042f;
    [SerializeField] private Color ambientLightColor = new Color(0.72f, 0.76f, 0.82f, 1f);

    [Header("Cores Fallback")]
    [SerializeField] private Color boardBaseColor = new Color(0.10f, 0.13f, 0.16f, 1f);
    [SerializeField] private Color cityGroundColor = new Color(0.17f, 0.19f, 0.22f, 1f);
    [SerializeField] private Color roadColor = new Color(0.11f, 0.12f, 0.14f, 1f);
    [SerializeField] private Color laneColor = new Color(0.82f, 0.78f, 0.52f, 1f);
    [SerializeField] private Color sidewalkColor = new Color(0.26f, 0.29f, 0.33f, 1f);
    [SerializeField] private Color buildingBaseColor = new Color(0.48f, 0.57f, 0.66f, 1f);
    [SerializeField] private Color foliageColor = new Color(0.25f, 0.55f, 0.28f, 1f);
    [SerializeField] private Color woodColor = new Color(0.45f, 0.33f, 0.22f, 1f);
    [SerializeField] private Color lampColor = new Color(0.78f, 0.82f, 0.88f, 1f);

    private GameObject environmentRoot;
    private Material boardBaseMaterial;
    private Material cityGroundMaterial;
    private Material roadMaterial;
    private Material laneMaterial;
    private Material sidewalkMaterial;
    private Material buildingMaterial;
    private Material foliageMaterial;
    private Material woodMaterial;
    private Material lampMaterial;

    [ContextMenu("Rebuild Environment From Board")]
    private void RebuildEnvironmentFromBoard()
    {
        BoardManager boardManager = FindFirstObjectByType<BoardManager>();

        if (boardManager == null)
        {
            Debug.LogWarning("Nenhum BoardManager foi encontrado para reconstruir o cenario.", this);
            return;
        }

        boardManager.RefreshEnvironment();
    }

    /// <summary>
    /// Gera uma cidade de fundo organizada em quadras, mantendo o tabuleiro limpo.
    /// </summary>
    public void Initialize(List<Tile> tiles, float tileSpacing)
    {
        if (tiles == null || tiles.Count == 0)
        {
            return;
        }

        EnsureMaterials();
        ClearPreviousEnvironment();

        Bounds boardBounds = CalculateBoardBounds(tiles, tileSpacing);
        environmentRoot = new GameObject("BoardEnvironmentRoot");
        environmentRoot.transform.SetParent(transform, false);

        ApplySceneAtmosphere();
        CreateBoardBase(boardBounds);
        CreateCityGround(boardBounds);
        CreateRoadGrid(boardBounds);
        CreateCityBlocks(boardBounds);
        CreateLandmarks(boardBounds);
        CreateSkylineBands(boardBounds);
    }

    /// <summary>
    /// Calcula os limites gerais do tabuleiro.
    /// </summary>
    private Bounds CalculateBoardBounds(List<Tile> tiles, float tileSpacing)
    {
        Vector3 min = tiles[0].transform.position;
        Vector3 max = tiles[0].transform.position;

        foreach (Tile tile in tiles)
        {
            Vector3 position = tile.transform.position;
            min = Vector3.Min(min, position);
            max = Vector3.Max(max, position);
        }

        Vector3 center = (min + max) * 0.5f;
        Vector3 size = new Vector3((max.x - min.x) + tileSpacing, 0.1f, (max.z - min.z) + tileSpacing);
        return new Bounds(center, size);
    }

    /// <summary>
    /// Cria uma base visual limpa sob o tabuleiro.
    /// </summary>
    private void CreateBoardBase(Bounds boardBounds)
    {
        GameObject boardBase = CreatePrimitive("BoardBase", PrimitiveType.Cube, environmentRoot.transform);
        boardBase.transform.position = new Vector3(boardBounds.center.x, -0.36f, boardBounds.center.z);
        boardBase.transform.localScale = new Vector3(boardBounds.size.x + outerMargin, 0.24f, boardBounds.size.z + outerMargin);
        ApplyMaterial(boardBase, boardBaseMaterial);
    }

    /// <summary>
    /// Cria a base ampla da cidade ao fundo.
    /// </summary>
    private void CreateCityGround(Bounds boardBounds)
    {
        GameObject cityGround = CreatePrimitive("CityGround", PrimitiveType.Cube, environmentRoot.transform);
        cityGround.transform.position = new Vector3(boardBounds.center.x, -0.58f, boardBounds.center.z);
        cityGround.transform.localScale = new Vector3(
            boardBounds.size.x + cityExtent * 2f,
            0.10f,
            boardBounds.size.z + cityExtent * 2f);
        ApplyMaterial(cityGround, cityGroundMaterial);
    }

    /// <summary>
    /// Cria uma grade de ruas retas e limpas ao redor do tabuleiro.
    /// </summary>
    private void CreateRoadGrid(Bounds boardBounds)
    {
        float cityMinX = boardBounds.center.x - (boardBounds.size.x * 0.5f + cityExtent);
        float cityMaxX = boardBounds.center.x + (boardBounds.size.x * 0.5f + cityExtent);
        float cityMinZ = boardBounds.center.z - (boardBounds.size.z * 0.5f + cityExtent);
        float cityMaxZ = boardBounds.center.z + (boardBounds.size.z * 0.5f + cityExtent);

        float clearMinX = boardBounds.min.x - cleanMarginAroundBoard;
        float clearMaxX = boardBounds.max.x + cleanMarginAroundBoard;
        float clearMinZ = boardBounds.min.z - cleanMarginAroundBoard;
        float clearMaxZ = boardBounds.max.z + cleanMarginAroundBoard;

        for (float x = cityMinX; x <= cityMaxX; x += blockSize + roadWidth)
        {
            if (x + roadWidth > clearMinX && x < clearMaxX)
            {
                continue;
            }

            CreateRoadStrip(
                "VerticalRoad_" + x.ToString("F1"),
                new Vector3(x + roadWidth * 0.5f, -0.31f, boardBounds.center.z),
                new Vector3(roadWidth, 0.03f, cityMaxZ - cityMinZ),
                false);
        }

        for (float z = cityMinZ; z <= cityMaxZ; z += blockSize + roadWidth)
        {
            if (z + roadWidth > clearMinZ && z < clearMaxZ)
            {
                continue;
            }

            CreateRoadStrip(
                "HorizontalRoad_" + z.ToString("F1"),
                new Vector3(boardBounds.center.x, -0.31f, z + roadWidth * 0.5f),
                new Vector3(cityMaxX - cityMinX, 0.03f, roadWidth),
                true);
        }

        CreateRoadIntersections(cityMinX, cityMaxX, cityMinZ, cityMaxZ, clearMinX, clearMaxX, clearMinZ, clearMaxZ);
    }

    /// <summary>
    /// Preenche as quadras da cidade com prédios, árvores e detalhes urbanos.
    /// </summary>
    private void CreateCityBlocks(Bounds boardBounds)
    {
        float cityMinX = boardBounds.center.x - (boardBounds.size.x * 0.5f + cityExtent);
        float cityMaxX = boardBounds.center.x + (boardBounds.size.x * 0.5f + cityExtent);
        float cityMinZ = boardBounds.center.z - (boardBounds.size.z * 0.5f + cityExtent);
        float cityMaxZ = boardBounds.center.z + (boardBounds.size.z * 0.5f + cityExtent);

        float clearMinX = boardBounds.min.x - cleanMarginAroundBoard;
        float clearMaxX = boardBounds.max.x + cleanMarginAroundBoard;
        float clearMinZ = boardBounds.min.z - cleanMarginAroundBoard;
        float clearMaxZ = boardBounds.max.z + cleanMarginAroundBoard;

        for (float startX = cityMinX + roadWidth; startX < cityMaxX - blockSize; startX += blockSize + roadWidth)
        {
            for (float startZ = cityMinZ + roadWidth; startZ < cityMaxZ - blockSize; startZ += blockSize + roadWidth)
            {
                float lotCenterX = startX + blockSize * 0.5f;
                float lotCenterZ = startZ + blockSize * 0.5f;
                float lotMaxX = startX + blockSize;
                float lotMaxZ = startZ + blockSize;

                bool overlapsCleanZone = startX < clearMaxX
                    && lotMaxX > clearMinX
                    && startZ < clearMaxZ
                    && lotMaxZ > clearMinZ;

                if (overlapsCleanZone)
                {
                    continue;
                }

                Vector3 lotCenter = new Vector3(lotCenterX, -0.12f, lotCenterZ);
                int districtLevel = GetDistrictLevel(boardBounds.center, lotCenter);
                DistrictTheme theme = GetDistrictTheme(boardBounds.center, lotCenter);

                CreateLotBase(lotCenter);

                if (ShouldCreatePocketPark(theme, districtLevel))
                {
                    CreatePocketPark(lotCenter, theme);
                }
                else
                {
                    SpawnMainBuilding(lotCenter, districtLevel, theme);
                }

                CreateLotDetails(lotCenter, theme);
            }
        }
    }

    /// <summary>
    /// Determina o nível do distrito conforme a distância ao tabuleiro.
    /// </summary>
    private int GetDistrictLevel(Vector3 boardCenter, Vector3 lotCenter)
    {
        float distance = Vector3.Distance(new Vector3(boardCenter.x, 0f, boardCenter.z), new Vector3(lotCenter.x, 0f, lotCenter.z));

        if (distance < firstDistrictDistance)
        {
            return 0;
        }

        if (distance < secondDistrictDistance)
        {
            return 1;
        }

        return 2;
    }

    /// <summary>
    /// Define o tema do bairro com base no quadrante da cidade.
    /// </summary>
    private DistrictTheme GetDistrictTheme(Vector3 boardCenter, Vector3 lotCenter)
    {
        bool east = lotCenter.x >= boardCenter.x;
        bool north = lotCenter.z >= boardCenter.z;

        if (!east && north)
        {
            return DistrictTheme.Residential;
        }

        if (east && north)
        {
            return DistrictTheme.Commercial;
        }

        if (!east && !north)
        {
            return DistrictTheme.Civic;
        }

        return DistrictTheme.Eco;
    }

    /// <summary>
    /// Cria a base visual de uma quadra.
    /// </summary>
    private void CreateLotBase(Vector3 lotCenter)
    {
        GameObject lotBase = CreatePrimitive("CityLotBase", PrimitiveType.Cube, environmentRoot.transform);
        lotBase.transform.position = new Vector3(lotCenter.x, -0.29f, lotCenter.z);
        lotBase.transform.localScale = new Vector3(blockSize - lotInset, 0.02f, blockSize - lotInset);
        ApplyMaterial(lotBase, sidewalkMaterial);
    }

    /// <summary>
    /// Cria um pequeno parque em uma quadra.
    /// </summary>
    private void CreatePocketPark(Vector3 lotCenter, DistrictTheme theme)
    {
        GameObject parkBase = CreatePrimitive("PocketPark", PrimitiveType.Cube, environmentRoot.transform);
        parkBase.transform.position = new Vector3(lotCenter.x, -0.27f, lotCenter.z);
        parkBase.transform.localScale = new Vector3(blockSize - lotInset - 1.2f, 0.03f, blockSize - lotInset - 1.2f);
        ApplyMaterial(parkBase, foliageMaterial);

        SpawnTree(lotCenter + new Vector3(-2f, 0f, -1.5f), Random.Range(0.9f, 1.2f));
        SpawnTree(lotCenter + new Vector3(2f, 0f, 1.5f), Random.Range(0.9f, 1.2f));
        SpawnTree(lotCenter + new Vector3(-1.2f, 0f, 2f), Random.Range(0.9f, 1.1f));

        if (theme == DistrictTheme.Eco || theme == DistrictTheme.Residential)
        {
            SpawnTree(lotCenter + new Vector3(2.2f, 0f, -1.8f), Random.Range(0.9f, 1.2f));
            SpawnStreetDetail(lotCenter + new Vector3(0.8f, 0f, -2.2f), Quaternion.Euler(0f, 15f, 0f), 0.9f);
        }
        else
        {
            SpawnStreetDetail(lotCenter + new Vector3(1.6f, 0f, -2f), Quaternion.Euler(0f, 35f, 0f), 0.95f);
        }
    }

    /// <summary>
    /// Instancia o prédio principal da quadra.
    /// </summary>
    private void SpawnMainBuilding(Vector3 lotCenter, int districtLevel, DistrictTheme theme)
    {
        bool preferTall = theme == DistrictTheme.Commercial
            ? districtLevel >= 2 && Random.value > 0.68f
            : theme == DistrictTheme.Civic
                ? districtLevel >= 2 && Random.value > 0.74f
                : districtLevel >= 2 && Random.value > 0.90f;

        float scale = theme == DistrictTheme.Commercial
            ? districtLevel == 0 ? Random.Range(0.86f, 0.98f) : districtLevel == 1 ? Random.Range(0.96f, 1.10f) : Random.Range(1.02f, 1.14f)
            : theme == DistrictTheme.Residential
                ? districtLevel == 0 ? Random.Range(0.76f, 0.90f) : districtLevel == 1 ? Random.Range(0.84f, 0.98f) : Random.Range(0.92f, 1.04f)
                : theme == DistrictTheme.Civic
                    ? districtLevel == 0 ? Random.Range(0.84f, 0.98f) : districtLevel == 1 ? Random.Range(0.92f, 1.06f) : Random.Range(0.98f, 1.10f)
                    : districtLevel == 0 ? Random.Range(0.78f, 0.92f) : districtLevel == 1 ? Random.Range(0.88f, 1.00f) : Random.Range(0.94f, 1.06f);

        GameObject prefab = ChooseBuildingPrefab(preferTall, districtLevel, theme);
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (prefab != null)
        {
            GameObject building = Instantiate(prefab, lotCenter, rotation, environmentRoot.transform);
            float maxHeight = districtLevel == 0 ? lowDistrictMaxHeight : districtLevel == 1 ? midDistrictMaxHeight : highDistrictMaxHeight;
            building.transform.localScale *= scale;
            FitInstanceToFootprint(building, blockSize - lotInset - 1.8f, blockSize - lotInset - 1.8f, maxHeight);
            AlignInstanceBaseToY(building, lotSurfaceY);
            DisableColliders(building);
            return;
        }

        CreateFallbackBuilding(lotCenter, preferTall, scale, rotation.eulerAngles.y);
    }

    /// <summary>
    /// Adiciona árvores, postes, bancos ou outros detalhes urbanos leves.
    /// </summary>
    private bool ShouldCreatePocketPark(DistrictTheme theme, int districtLevel)
    {
        float chance = theme switch
        {
            DistrictTheme.Eco => districtLevel == 0 ? 0.55f : districtLevel == 1 ? 0.42f : 0.30f,
            DistrictTheme.Residential => districtLevel == 0 ? 0.34f : districtLevel == 1 ? 0.20f : 0.10f,
            DistrictTheme.Civic => districtLevel == 0 ? 0.18f : districtLevel == 1 ? 0.14f : 0.08f,
            DistrictTheme.Commercial => districtLevel == 0 ? 0.12f : districtLevel == 1 ? 0.08f : 0.04f,
            _ => 0.10f
        };

        return Random.value < chance;
    }

    /// <summary>
    /// Adiciona árvores, postes, bancos e microdetalhes urbanos de acordo com o bairro.
    /// </summary>
    private void CreateLotDetails(Vector3 lotCenter, DistrictTheme theme)
    {
        switch (theme)
        {
            case DistrictTheme.Residential:
                SpawnTree(lotCenter + new Vector3(-detailOffset, 0f, detailOffset), Random.Range(0.85f, 1.05f));
                SpawnTree(lotCenter + new Vector3(detailOffset, 0f, -detailOffset), Random.Range(0.80f, 1.00f));
                SpawnTree(lotCenter + new Vector3(detailOffset + 0.6f, 0f, detailOffset + 0.4f), Random.Range(0.78f, 0.96f));
                if (Random.value > 0.45f)
                {
                    SpawnStreetDetail(lotCenter + new Vector3(0f, 0f, -detailOffset - 0.5f), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), 0.85f);
                }
                break;

            case DistrictTheme.Commercial:
                SpawnTree(lotCenter + new Vector3(-detailOffset - 0.4f, 0f, detailOffset + 0.2f), Random.Range(0.70f, 0.90f));
                SpawnStreetDetail(lotCenter + new Vector3(detailOffset, 0f, -detailOffset), Quaternion.Euler(0f, 90f, 0f), 0.90f);
                if (Random.value > 0.55f)
                {
                    SpawnStreetDetail(lotCenter + new Vector3(-detailOffset, 0f, -detailOffset), Quaternion.Euler(0f, 0f, 0f), 0.90f);
                }
                break;

            case DistrictTheme.Civic:
                SpawnTree(lotCenter + new Vector3(-detailOffset - 0.2f, 0f, detailOffset), Random.Range(0.78f, 0.95f));
                SpawnTree(lotCenter + new Vector3(detailOffset + 0.2f, 0f, detailOffset), Random.Range(0.78f, 0.95f));
                SpawnStreetDetail(lotCenter + new Vector3(0f, 0f, -detailOffset - 0.6f), Quaternion.identity, 0.95f);
                break;

            case DistrictTheme.Eco:
                SpawnTree(lotCenter + new Vector3(-detailOffset, 0f, detailOffset), Random.Range(0.95f, 1.15f));
                SpawnTree(lotCenter + new Vector3(detailOffset, 0f, detailOffset), Random.Range(0.95f, 1.15f));
                SpawnTree(lotCenter + new Vector3(0f, 0f, -detailOffset - 0.6f), Random.Range(0.85f, 1.05f));
                SpawnTree(lotCenter + new Vector3(-detailOffset - 0.8f, 0f, -detailOffset + 0.2f), Random.Range(0.88f, 1.08f));
                if (Random.value > 0.40f)
                {
                    SpawnStreetDetail(lotCenter + new Vector3(-0.7f, 0f, -detailOffset), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), 0.80f);
                }
                break;
        }
    }

    /// <summary>
    /// Cria uma faixa de rua com uma linha central simples.
    /// </summary>
    private void CreateRoadStrip(string name, Vector3 position, Vector3 scale, bool horizontal)
    {
        GameObject road = CreatePrimitive(name, PrimitiveType.Cube, environmentRoot.transform);
        road.transform.position = position;
        road.transform.localScale = scale;
        ApplyMaterial(road, roadMaterial);

        GameObject lane = CreatePrimitive(name + "_Lane", PrimitiveType.Cube, environmentRoot.transform);
        lane.transform.position = position + new Vector3(0f, 0.02f, 0f);
        lane.transform.localScale = horizontal
            ? new Vector3(scale.x * 0.85f, 0.005f, 0.12f)
            : new Vector3(0.12f, 0.005f, scale.z * 0.85f);
        ApplyMaterial(lane, laneMaterial);
    }

    /// <summary>
    /// Cria interseções de rua organizadas usando prefab dedicado quando disponível.
    /// </summary>
    private void CreateRoadIntersections(float cityMinX, float cityMaxX, float cityMinZ, float cityMaxZ, float clearMinX, float clearMaxX, float clearMinZ, float clearMaxZ)
    {
        for (float x = cityMinX; x <= cityMaxX; x += blockSize + roadWidth)
        {
            if (x + roadWidth > clearMinX && x < clearMaxX)
            {
                continue;
            }

            for (float z = cityMinZ; z <= cityMaxZ; z += blockSize + roadWidth)
            {
                if (z + roadWidth > clearMinZ && z < clearMaxZ)
                {
                    continue;
                }

                Vector3 intersectionPosition = new Vector3(x + roadWidth * 0.5f, -0.305f, z + roadWidth * 0.5f);
                GameObject intersection = CreatePrimitive("RoadIntersection", PrimitiveType.Cube, environmentRoot.transform);
                intersection.transform.position = intersectionPosition;
                intersection.transform.localScale = new Vector3(roadWidth, 0.031f, roadWidth);
                ApplyMaterial(intersection, roadMaterial);

                GameObject laneX = CreatePrimitive("RoadIntersectionLaneX", PrimitiveType.Cube, environmentRoot.transform);
                laneX.transform.position = intersectionPosition + new Vector3(0f, 0.02f, 0f);
                laneX.transform.localScale = new Vector3(roadWidth * 0.85f, 0.005f, 0.12f);
                ApplyMaterial(laneX, laneMaterial);

                GameObject laneZ = CreatePrimitive("RoadIntersectionLaneZ", PrimitiveType.Cube, environmentRoot.transform);
                laneZ.transform.position = intersectionPosition + new Vector3(0f, 0.02f, 0f);
                laneZ.transform.localScale = new Vector3(0.12f, 0.005f, roadWidth * 0.85f);
                ApplyMaterial(laneZ, laneMaterial);
            }
        }
    }

    /// <summary>
    /// Escolhe o prefab adequado para um distrito.
    /// </summary>
    private GameObject ChooseBuildingPrefab(bool preferTall, int districtLevel, DistrictTheme theme)
    {
        string[] themeKeywords = GetThemeKeywords(theme);

        if (districtLevel == 0)
        {
            GameObject lowOnlyPrefab = ChoosePrefabByTheme(lowRiseBuildingPrefabs, themeKeywords);

            if (lowOnlyPrefab != null)
            {
                return lowOnlyPrefab;
            }
        }

        if (districtLevel == 1)
        {
            if (Random.value > 0.45f)
            {
                GameObject midOnlyPrefab = ChoosePrefabByTheme(midRiseBuildingPrefabs, themeKeywords);

                if (midOnlyPrefab != null)
                {
                    return midOnlyPrefab;
                }
            }

            GameObject lowFallbackPrefab = ChoosePrefabByTheme(lowRiseBuildingPrefabs, themeKeywords);

            if (lowFallbackPrefab != null)
            {
                return lowFallbackPrefab;
            }
        }

        if (preferTall)
        {
            GameObject tallPrefab = ChoosePrefabByTheme(tallRiseBuildingPrefabs, themeKeywords);

            if (tallPrefab != null)
            {
                return tallPrefab;
            }

            if (tallBuildingPrefab != null)
            {
                return tallBuildingPrefab;
            }
        }

        if (districtLevel >= 2 && Random.value > 0.35f)
        {
            GameObject midPriorityPrefab = ChoosePrefabByTheme(midRiseBuildingPrefabs, themeKeywords);

            if (midPriorityPrefab != null)
            {
                return midPriorityPrefab;
            }
        }

        if (districtLevel >= 1)
        {
            GameObject midPrefab = ChoosePrefabByTheme(midRiseBuildingPrefabs, themeKeywords);

            if (midPrefab != null)
            {
                return midPrefab;
            }
        }

        GameObject lowPrefab = ChoosePrefabByTheme(lowRiseBuildingPrefabs, themeKeywords);

        if (lowPrefab != null)
        {
            return lowPrefab;
        }

        if (lowBuildingPrefab != null)
        {
            return lowBuildingPrefab;
        }

        return tallBuildingPrefab;
    }

    /// <summary>
    /// Retorna palavras-chave de apoio para dar identidade aos bairros.
    /// </summary>
    private string[] GetThemeKeywords(DistrictTheme theme)
    {
        return theme switch
        {
            DistrictTheme.Residential => new[] { "house", "res", "home", "cute", "villa" },
            DistrictTheme.Commercial => new[] { "office", "shop", "store", "block", "tower", "commercial" },
            DistrictTheme.Civic => new[] { "city", "hall", "school", "hospital", "post", "civic" },
            DistrictTheme.Eco => new[] { "eco", "green", "solar", "park", "house" },
            _ => new string[0]
        };
    }

    /// <summary>
    /// Escolhe um prefab tentando respeitar o tema do bairro antes de cair no sorteio geral.
    /// </summary>
    private GameObject ChoosePrefabByTheme(GameObject[] prefabs, string[] keywords)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        List<GameObject> keywordMatches = new List<GameObject>();
        List<GameObject> validPrefabs = new List<GameObject>();

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            validPrefabs.Add(prefab);

            if (keywords == null || keywords.Length == 0)
            {
                continue;
            }

            string prefabName = prefab.name.ToLowerInvariant();

            foreach (string keyword in keywords)
            {
                if (prefabName.Contains(keyword))
                {
                    keywordMatches.Add(prefab);
                    break;
                }
            }
        }

        if (keywordMatches.Count > 0)
        {
            return keywordMatches[Random.Range(0, keywordMatches.Count)];
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    /// <summary>
    /// Aplica atmosfera base de luz, neblina e ambiente para a cidade parecer mais viva.
    /// </summary>
    private void ApplySceneAtmosphere()
    {
        if (!applyAtmosphere)
        {
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientLightColor;

        Light directionalLight = FindPrimaryDirectionalLight();

        if (directionalLight != null)
        {
            directionalLight.color = directionalLightColor;
            directionalLight.intensity = directionalLightIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(directionalLightEuler);
        }
    }

    /// <summary>
    /// Cria alguns marcos urbanos grandes e afastados para dar identidade à cidade.
    /// </summary>
    private void CreateLandmarks(Bounds boardBounds)
    {
        Vector3[] landmarkPositions =
        {
            boardBounds.center + new Vector3(-(boardBounds.extents.x + landmarkDistanceFromBoard), 0f, boardBounds.extents.z + landmarkDistanceFromBoard),
            boardBounds.center + new Vector3(boardBounds.extents.x + landmarkDistanceFromBoard, 0f, boardBounds.extents.z + landmarkDistanceFromBoard),
            boardBounds.center + new Vector3(-(boardBounds.extents.x + landmarkDistanceFromBoard), 0f, -(boardBounds.extents.z + landmarkDistanceFromBoard)),
            boardBounds.center + new Vector3(boardBounds.extents.x + landmarkDistanceFromBoard, 0f, -(boardBounds.extents.z + landmarkDistanceFromBoard))
        };

        DistrictTheme[] landmarkThemes =
        {
            DistrictTheme.Residential,
            DistrictTheme.Commercial,
            DistrictTheme.Civic,
            DistrictTheme.Eco
        };

        for (int index = 0; index < landmarkPositions.Length; index++)
        {
            Vector3 position = landmarkPositions[index];
            DistrictTheme theme = landmarkThemes[index];

            GameObject plaza = CreatePrimitive("LandmarkPlaza_" + theme, PrimitiveType.Cube, environmentRoot.transform);
            plaza.transform.position = new Vector3(position.x, -0.30f, position.z);
            plaza.transform.localScale = new Vector3(8.5f, 0.06f, 8.5f);
            ApplyMaterial(plaza, sidewalkMaterial);

            if (theme == DistrictTheme.Eco)
            {
                CreatePocketPark(position, theme);
                continue;
            }

            GameObject prefab = ChooseBuildingPrefab(true, 2, theme);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            if (prefab != null)
            {
                GameObject landmark = Instantiate(prefab, position, rotation, environmentRoot.transform);
                landmark.transform.localScale *= landmarkScaleBoost;
                FitInstanceToFootprint(landmark, 7f, 7f, highDistrictMaxHeight + 2f);
                AlignInstanceBaseToY(landmark, lotSurfaceY);
                DisableColliders(landmark);
            }
            else
            {
                CreateFallbackBuilding(position, true, landmarkScaleBoost, rotation.eulerAngles.y);
            }

            SpawnTree(position + new Vector3(-2.8f, 0f, -2.4f), 1.0f);
            SpawnTree(position + new Vector3(2.8f, 0f, 2.4f), 1.0f);
            SpawnStreetDetail(position + new Vector3(2.3f, 0f, -2.2f), Quaternion.Euler(0f, 45f, 0f), 0.95f);
        }
    }

    /// <summary>
    /// Cria camadas distantes de skyline para preencher o horizonte sem poluir perto do tabuleiro.
    /// </summary>
    private void CreateSkylineBands(Bounds boardBounds)
    {
        for (int band = 0; band < skylineBands; band++)
        {
            float distance = skylineStartDistance + band * skylineBandSpacing;
            float ringWidth = boardBounds.size.x + distance * 2f;
            float ringDepth = boardBounds.size.z + distance * 2f;
            int buildingsPerSide = Mathf.Max(3, skylineBuildingsPerBand / 4);

            PlaceSkylineSide(boardBounds, band, buildingsPerSide, -1f, 0f, ringWidth, ringDepth, true);
            PlaceSkylineSide(boardBounds, band, buildingsPerSide, 1f, 0f, ringWidth, ringDepth, true);
            PlaceSkylineSide(boardBounds, band, buildingsPerSide, 0f, -1f, ringWidth, ringDepth, false);
            PlaceSkylineSide(boardBounds, band, buildingsPerSide, 0f, 1f, ringWidth, ringDepth, false);
        }

        CreateEdgeGreenery(boardBounds);
    }

    /// <summary>
    /// Distribui uma fileira de prédios de skyline em um dos lados da cidade.
    /// </summary>
    private void PlaceSkylineSide(Bounds boardBounds, int band, int buildingsPerSide, float sideX, float sideZ, float ringWidth, float ringDepth, bool verticalSide)
    {
        float sideLength = verticalSide ? ringDepth : ringWidth;
        float step = sideLength / buildingsPerSide;
        float centralBias = Mathf.Lerp(0.38f, 0.18f, band / Mathf.Max(1f, skylineBands - 1f));

        for (int index = 0; index < buildingsPerSide; index++)
        {
            float normalized = (index + 0.5f) / buildingsPerSide;
            float centered = Mathf.Lerp(-1f, 1f, normalized);
            float compressed = centered * (0.55f + Mathf.Abs(centered) * centralBias);
            float offset = compressed * sideLength * 0.5f;
            Vector3 position = verticalSide
                ? new Vector3(boardBounds.center.x + sideX * ringWidth * 0.5f, 0f, boardBounds.center.z + offset)
                : new Vector3(boardBounds.center.x + offset, 0f, boardBounds.center.z + sideZ * ringDepth * 0.5f);

            position += new Vector3(Random.Range(-1.0f, 1.0f), 0f, Random.Range(-1.0f, 1.0f));

            DistrictTheme theme = GetDistrictTheme(boardBounds.center, position);
            GameObject skylinePrefab = band == skylineBands - 1
                ? ChoosePrefabByTheme(tallRiseBuildingPrefabs, GetThemeKeywords(theme))
                : Random.value > 0.45f
                    ? ChoosePrefabByTheme(midRiseBuildingPrefabs, GetThemeKeywords(theme))
                    : ChoosePrefabByTheme(tallRiseBuildingPrefabs, GetThemeKeywords(theme));

            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            if (skylinePrefab != null)
            {
                GameObject skylineBuilding = Instantiate(skylinePrefab, position, rotation, environmentRoot.transform);
                skylineBuilding.transform.localScale *= Random.Range(0.85f, 1.1f);
                FitInstanceToFootprint(skylineBuilding, 5.5f, 5.5f, highDistrictMaxHeight + 4f + band * 1.25f);
                AlignInstanceBaseToY(skylineBuilding, lotSurfaceY);
                DisableColliders(skylineBuilding);
            }
            else
            {
                CreateFallbackBuilding(position, true, 1.05f + band * 0.08f, rotation.eulerAngles.y);
            }
        }
    }

    /// <summary>
    /// Cria vegetação de borda distante para suavizar o horizonte sem poluir o tabuleiro.
    /// </summary>
    private void CreateEdgeGreenery(Bounds boardBounds)
    {
        float greenDistance = skylineStartDistance - 10f;
        int treeGroupsPerSide = 8;

        for (int side = 0; side < 4; side++)
        {
            bool vertical = side < 2;
            float sideSign = side % 2 == 0 ? -1f : 1f;
            float sideLength = vertical ? boardBounds.size.z + greenDistance * 1.7f : boardBounds.size.x + greenDistance * 1.7f;
            float step = sideLength / treeGroupsPerSide;

            for (int index = 0; index < treeGroupsPerSide; index++)
            {
                float offset = -sideLength * 0.5f + step * (index + 0.5f);
                Vector3 groupCenter = vertical
                    ? new Vector3(boardBounds.center.x + sideSign * (boardBounds.extents.x + greenDistance), 0f, boardBounds.center.z + offset)
                    : new Vector3(boardBounds.center.x + offset, 0f, boardBounds.center.z + sideSign * (boardBounds.extents.z + greenDistance));

                int treeCount = Random.Range(2, 5);

                for (int tree = 0; tree < treeCount; tree++)
                {
                    Vector3 jitter = new Vector3(Random.Range(-2.4f, 2.4f), 0f, Random.Range(-2.4f, 2.4f));
                    SpawnTree(groupCenter + jitter, Random.Range(0.88f, 1.18f));
                }
            }
        }
    }

    /// <summary>
    /// Procura a luz direcional principal da cena.
    /// </summary>
    private Light FindPrimaryDirectionalLight()
    {
        Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Light currentLight in lights)
        {
            if (currentLight != null && currentLight.type == LightType.Directional)
            {
                return currentLight;
            }
        }

        return null;
    }

    /// <summary>
    /// Instancia uma árvore usando variações quando existirem.
    /// </summary>
    private void SpawnTree(Vector3 position, float scaleFactor)
    {
        GameObject selectedTree = ChoosePrefab(treePrefabs);

        if (selectedTree == null)
        {
            selectedTree = treePrefab;
        }

        if (selectedTree != null)
        {
            GameObject tree = Instantiate(selectedTree, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), environmentRoot.transform);
            tree.transform.localScale *= scaleFactor;
            DisableColliders(tree);
            return;
        }

        GameObject trunk = CreatePrimitive("FallbackTreeTrunk", PrimitiveType.Cylinder, environmentRoot.transform);
        trunk.transform.position = position + new Vector3(0f, 0.48f, 0f);
        trunk.transform.localScale = new Vector3(0.14f, 0.5f, 0.14f) * scaleFactor;
        ApplyMaterial(trunk, woodMaterial);

        GameObject crown = CreatePrimitive("FallbackTreeCrown", PrimitiveType.Sphere, environmentRoot.transform);
        crown.transform.position = position + new Vector3(0f, 1.15f, 0f);
        crown.transform.localScale = new Vector3(0.82f, 0.82f, 0.82f) * scaleFactor;
        ApplyMaterial(crown, foliageMaterial);
    }

    /// <summary>
    /// Instancia um detalhe urbano leve.
    /// </summary>
    private void SpawnStreetDetail(Vector3 position, Quaternion rotation, float scaleFactor)
    {
        GameObject detailPrefab = ChooseStreetDetailPrefab();

        if (detailPrefab != null)
        {
            GameObject detail = Instantiate(detailPrefab, position, rotation, environmentRoot.transform);
            detail.transform.localScale *= scaleFactor;
            DisableColliders(detail);
            return;
        }

        if (Random.value > 0.5f)
        {
            SpawnBench(position, rotation);
        }
        else
        {
            SpawnLamppost(position, scaleFactor);
        }
    }

    /// <summary>
    /// Escolhe um detalhe urbano evitando prefabs que claramente sejam peças de rua.
    /// </summary>
    private GameObject ChooseStreetDetailPrefab()
    {
        if (streetDetailPrefabs == null || streetDetailPrefabs.Length == 0)
        {
            return null;
        }

        List<GameObject> validPrefabs = new List<GameObject>();

        foreach (GameObject prefab in streetDetailPrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            string prefabName = prefab.name.ToLowerInvariant();

            if (prefabName.Contains("road") || prefabName.Contains("street") || prefabName.Contains("crossing"))
            {
                continue;
            }

            validPrefabs.Add(prefab);
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    /// <summary>
    /// Instancia um poste decorativo ou cria um fallback geométrico simples.
    /// </summary>
    private void SpawnLamppost(Vector3 position, float scaleFactor)
    {
        if (lamppostPrefab != null)
        {
            GameObject lamp = Instantiate(lamppostPrefab, position, Quaternion.identity, environmentRoot.transform);
            lamp.transform.localScale *= scaleFactor;
            DisableColliders(lamp);
            return;
        }

        GameObject pole = CreatePrimitive("FallbackLampPole", PrimitiveType.Cylinder, environmentRoot.transform);
        pole.transform.position = position + new Vector3(0f, 1.0f, 0f);
        pole.transform.localScale = new Vector3(0.08f, 1.0f, 0.08f) * scaleFactor;
        ApplyMaterial(pole, lampMaterial);

        GameObject light = CreatePrimitive("FallbackLampLight", PrimitiveType.Sphere, environmentRoot.transform);
        light.transform.position = position + new Vector3(0f, 2.12f, 0f);
        light.transform.localScale = new Vector3(0.20f, 0.20f, 0.20f) * scaleFactor;
        ApplyMaterial(light, CreateRuntimeMaterial(new Color(1f, 0.92f, 0.62f, 1f), true));
    }

    /// <summary>
    /// Instancia um banco decorativo ou cria uma versão simples com cubos.
    /// </summary>
    private void SpawnBench(Vector3 position, Quaternion rotation)
    {
        if (benchPrefab != null)
        {
            GameObject bench = Instantiate(benchPrefab, position, rotation, environmentRoot.transform);
            bench.transform.localScale *= 1.05f;
            DisableColliders(bench);
            return;
        }

        GameObject seat = CreatePrimitive("FallbackBenchSeat", PrimitiveType.Cube, environmentRoot.transform);
        seat.transform.SetPositionAndRotation(position + new Vector3(0f, 0.34f, 0f), rotation);
        seat.transform.localScale = new Vector3(0.9f, 0.08f, 0.28f);
        ApplyMaterial(seat, woodMaterial);

        GameObject back = CreatePrimitive("FallbackBenchBack", PrimitiveType.Cube, environmentRoot.transform);
        back.transform.SetPositionAndRotation(position + new Vector3(0f, 0.62f, -0.12f), rotation);
        back.transform.localScale = new Vector3(0.9f, 0.08f, 0.24f);
        ApplyMaterial(back, woodMaterial);
    }

    /// <summary>
    /// Cria um prédio simples quando não houver prefab adequado.
    /// </summary>
    private void CreateFallbackBuilding(Vector3 position, bool tall, float scaleMultiplier, float yaw)
    {
        GameObject baseBlock = CreatePrimitive(tall ? "FallbackTallBuilding" : "FallbackLowBuilding", PrimitiveType.Cube, environmentRoot.transform);
        float height = (tall ? Random.Range(5.6f, 7.4f) : Random.Range(3.4f, 4.6f)) * scaleMultiplier;
        float width = (tall ? Random.Range(1.8f, 2.5f) : Random.Range(1.6f, 2.2f)) * scaleMultiplier;
        float depth = (tall ? Random.Range(1.6f, 2.2f) : Random.Range(1.4f, 2.0f)) * scaleMultiplier;
        baseBlock.transform.position = position + new Vector3(0f, height * 0.5f, 0f);
        baseBlock.transform.localScale = new Vector3(width, height, depth);
        baseBlock.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        ApplyMaterial(baseBlock, buildingMaterial);

        GameObject capBlock = CreatePrimitive("FallbackBuildingCap", PrimitiveType.Cube, environmentRoot.transform);
        capBlock.transform.position = baseBlock.transform.position + new Vector3(0f, height * 0.44f, 0f);
        capBlock.transform.localScale = Vector3.Scale(baseBlock.transform.localScale, new Vector3(0.78f, 0.08f, 0.78f));
        capBlock.transform.rotation = baseBlock.transform.rotation;
        ApplyMaterial(capBlock, cityGroundMaterial);
    }

    /// <summary>
    /// Escolhe um prefab aleatório válido.
    /// </summary>
    private GameObject ChoosePrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        List<GameObject> validPrefabs = new List<GameObject>();

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    /// <summary>
    /// Garante que os materiais básicos existam para os elementos de fallback.
    /// </summary>
    private void FitInstanceToFootprint(GameObject instance, float targetWidth, float targetDepth, float maxHeight)
    {
        if (instance == null)
        {
            return;
        }

        Bounds bounds = GetCombinedBounds(instance);

        if (bounds.size.x <= 0.001f || bounds.size.y <= 0.001f || bounds.size.z <= 0.001f)
        {
            return;
        }

        float widthScale = targetWidth / bounds.size.x;
        float depthScale = targetDepth / bounds.size.z;
        float heightScale = maxHeight / bounds.size.y;
        float uniformScale = Mathf.Min(widthScale, depthScale, heightScale);

        if (uniformScale <= 0f)
        {
            return;
        }

        instance.transform.localScale *= uniformScale;
    }

    private void AlignInstanceBaseToY(GameObject instance, float targetBaseY)
    {
        if (instance == null)
        {
            return;
        }

        Bounds bounds = GetCombinedBounds(instance);
        float offsetY = targetBaseY - bounds.min.y;
        instance.transform.position += new Vector3(0f, offsetY, 0f);
    }

    private Bounds GetCombinedBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return new Bounds(target.transform.position, Vector3.one);
        }

        Bounds combinedBounds = renderers[0].bounds;

        for (int index = 1; index < renderers.Length; index++)
        {
            combinedBounds.Encapsulate(renderers[index].bounds);
        }

        return combinedBounds;
    }

    private void EnsureMaterials()
    {
        boardBaseMaterial ??= CreateRuntimeMaterial(boardBaseColor);
        cityGroundMaterial ??= CreateRuntimeMaterial(cityGroundColor);
        roadMaterial ??= CreateRuntimeMaterial(roadColor);
        laneMaterial ??= CreateRuntimeMaterial(laneColor);
        sidewalkMaterial ??= CreateRuntimeMaterial(sidewalkColor);
        buildingMaterial ??= CreateRuntimeMaterial(buildingBaseColor);
        foliageMaterial ??= CreateRuntimeMaterial(foliageColor);
        woodMaterial ??= CreateRuntimeMaterial(woodColor);
        lampMaterial ??= CreateRuntimeMaterial(lampColor);
    }

    /// <summary>
    /// Cria um material URP/Lit simples em runtime.
    /// </summary>
    private Material CreateRuntimeMaterial(Color color, bool emission = false)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;

        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.75f);
        }

        return material;
    }

    /// <summary>
    /// Limpa o cenário anterior antes de reconstruir os elementos decorativos.
    /// </summary>
    private void ClearPreviousEnvironment()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child == null || child == transform)
            {
                continue;
            }

            if (!child.name.StartsWith("BoardEnvironmentRoot"))
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        environmentRoot = null;
    }

    /// <summary>
    /// Cria um primitivo com colisor removido para uso puramente visual.
    /// </summary>
    private GameObject CreatePrimitive(string objectName, PrimitiveType primitiveType, Transform parent)
    {
        GameObject primitive = GameObject.CreatePrimitive(primitiveType);
        primitive.name = objectName;
        primitive.transform.SetParent(parent, false);

        Collider collider = primitive.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        return primitive;
    }

    /// <summary>
    /// Aplica um material ao renderer principal do objeto.
    /// </summary>
    private void ApplyMaterial(GameObject target, Material material)
    {
        if (target == null || material == null)
        {
            return;
        }

        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    /// <summary>
    /// Desliga colisores de um prefab decorativo para não interferir na jogabilidade.
    /// </summary>
    private void DisableColliders(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);

        foreach (Collider currentCollider in colliders)
        {
            currentCollider.enabled = false;
        }
    }
}
