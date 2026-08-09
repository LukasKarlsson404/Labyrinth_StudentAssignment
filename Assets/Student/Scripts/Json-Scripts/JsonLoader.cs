using System.IO;
using UnityEngine;

public class JsonLoader : MonoBehaviour
{
    [Header("JSON Filename")]
    public string fileName;

    [Header("Quest Selection")]
    [Min(0)]
    [SerializeField] private int questIndex = 0;
    private int previousQuestIndex = -1;

    [Header("Prefabs")]
    public GameObject hwallPrefab;
    public GameObject vwallPrefab;
    public GameObject hWallLowPrefab;
    public GameObject vWallLowPrefab;
    public GameObject groundPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;

    [Header("Grid Reference")]
    public Grid grid;

    public MapData MapData { get; private set; }
    public int MinX => 0;
    public int MinY => 0;

    private GameObject currentStartMarker;
    private GameObject currentEndMarker;
    private float hWallOffset = 0.5f;
    private float vWallOffset = 0.5f;

    private void Start()
    {
        CacheWallOffsets();
        LoadJsonFile(fileName);
    }

    private void Update()
    {
        if (questIndex != previousQuestIndex)
        {
            previousQuestIndex = questIndex;
            RefreshQuestMarkers();
        }
    }

    private void CacheWallOffsets()
    {
        if (hwallPrefab != null)
        {
            Renderer renderer = hwallPrefab.GetComponentInChildren<Renderer>();
            if (renderer != null) hWallOffset = renderer.bounds.size.x / 2f;
        }

        if (vwallPrefab != null)
        {
            Renderer renderer = vwallPrefab.GetComponentInChildren<Renderer>();
            if (renderer != null) vWallOffset = renderer.bounds.size.z / 2f;
        }
    }

    public MapData GetMapData() => MapData;
    public int GetQuestIndex() => questIndex;
    public int GetMinX() => 0;
    public int GetMinY() => 0;

    public void LoadJsonFile(string jsonFileName)
    {
        if (string.IsNullOrWhiteSpace(jsonFileName))
        {
            Debug.LogError("No JSON filename assigned to JsonLoader.");
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Could not find file: " + path);
            return;
        }

        MapData = JsonHelper.FromJson<MapData>(File.ReadAllText(path));
        if (!ValidateMapData()) return;

        grid.xSize = MapData.width;
        grid.zSize = MapData.height;
        grid.GenerateMesh();

        questIndex = Mathf.Clamp(questIndex, 0, Mathf.Max(0, MapData.quests.Length - 1));
        previousQuestIndex = questIndex;

        SpawnWalls();
        SpawnGroundTiles();
        RefreshQuestMarkers();

        GridCharacterMovement character = FindAnyObjectByType<GridCharacterMovement>();
        if (character != null) character.InitializeCharacterAtStart();

        Debug.Log($"Map loaded: {jsonFileName} ({MapData.width}x{MapData.height}, {MapData.jumps.Length} jumps)");
    }

    private bool ValidateMapData()
    {
        if (MapData == null)
        {
            Debug.LogError("JSON could not be parsed as MapData.");
            return false;
        }

        if (MapData.width <= 0 || MapData.height <= 0)
        {
            Debug.LogError($"Invalid map size {MapData.width}x{MapData.height}.");
            return false;
        }

        if (grid == null)
        {
            Debug.LogError("JsonLoader is missing its Grid reference.");
            return false;
        }

        return true;
    }

    private void SpawnWalls()
    {
        foreach (Wall wall in MapData.hwalls)
        {
            if (wall == null) continue;
            Vector3 pos = new Vector3(wall.x + hWallOffset, 0, wall.y);
            SpawnWallForCost(wall.cost, hwallPrefab, hWallLowPrefab, pos);
        }

        foreach (Wall wall in MapData.vwalls)
        {
            if (wall == null) continue;
            Vector3 pos = new Vector3(wall.x, 0, wall.y + vWallOffset);
            SpawnWallForCost(wall.cost, vwallPrefab, vWallLowPrefab, pos);
        }

        SpawnOuterBoundary();
    }

    private static void SpawnWallForCost(float cost, GameObject solidPrefab, GameObject traversablePrefab, Vector3 position)
    {
        GameObject prefab = cost >= float.MaxValue ? solidPrefab : traversablePrefab;
        if (prefab == null) prefab = solidPrefab;
        if (prefab != null) Instantiate(prefab, position, prefab.transform.rotation);
    }

    private void SpawnOuterBoundary()
    {
        if (hwallPrefab != null)
        {
            for (int x = 0; x < grid.xSize; x++)
            {
                Instantiate(hwallPrefab, new Vector3(x + hWallOffset, 0, 0), hwallPrefab.transform.rotation);
                Instantiate(hwallPrefab, new Vector3(x + hWallOffset, 0, grid.zSize), hwallPrefab.transform.rotation);
            }
        }

        if (vwallPrefab != null)
        {
            for (int y = 0; y < grid.zSize; y++)
            {
                Instantiate(vwallPrefab, new Vector3(0, 0, y + vWallOffset), vwallPrefab.transform.rotation);
                Instantiate(vwallPrefab, new Vector3(grid.xSize, 0, y + vWallOffset), vwallPrefab.transform.rotation);
            }
        }
    }

    private void RefreshQuestMarkers()
    {
        if (currentStartMarker != null) Destroy(currentStartMarker);
        if (currentEndMarker != null) Destroy(currentEndMarker);
        SpawnQuestMarkers();
    }

    private void SpawnQuestMarkers()
    {
        if (MapData?.quests == null || MapData.quests.Length == 0) return;
        if (questIndex < 0 || questIndex >= MapData.quests.Length) return;

        Quest quest = MapData.quests[questIndex];
        if (quest?.from == null || quest.to == null) return;

        if (startPrefab != null)
            currentStartMarker = Instantiate(startPrefab, grid.GetCellCenter(quest.from.x, quest.from.y), Quaternion.identity);
        if (endPrefab != null)
            currentEndMarker = Instantiate(endPrefab, grid.GetCellCenter(quest.to.x, quest.to.y), Quaternion.identity);
    }

    public Vector2Int GetStartGridPosition()
    {
        if (MapData?.quests == null || MapData.quests.Length == 0) return Vector2Int.zero;
        Quest quest = MapData.quests[Mathf.Clamp(questIndex, 0, MapData.quests.Length - 1)];
        return quest?.from == null ? Vector2Int.zero : new Vector2Int(quest.from.x, quest.from.y);
    }

    private void SpawnGroundTiles()
    {
        if (groundPrefab == null) return;

        long tileCount = (long)grid.xSize * grid.zSize;
        if (tileCount > 50000)
            Debug.LogWarning($"Map has {tileCount} cells; one ground GameObject per cell may be expensive.");

        for (int x = 0; x < grid.xSize; x++)
        {
            for (int y = 0; y < grid.zSize; y++)
                Instantiate(groundPrefab, grid.GetCellCenter(x, y), Quaternion.identity);
        }
    }
}
