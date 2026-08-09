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
    public GameObject startPrefab;
    public GameObject endPrefab;

    [Header("Grid Reference")]
    public Grid grid;

    [Header("Camera Framing")]
    [SerializeField, Range(1f, 1.5f)] private float cameraPadding = 1.08f;

    [Header("Follow Camera")]
    [SerializeField] private bool followCharacterCamera = false;
    [SerializeField, Min(0.5f)] private float followDistance = 6f;
    [SerializeField, Min(0.5f)] private float followHeight = 6f;
    [SerializeField, Min(0f)] private float followLookHeight = 0.75f;
    [SerializeField, Min(0.1f)] private float followSmoothSpeed = 6f;
    [SerializeField, Range(30f, 90f)] private float followFieldOfView = 60f;

    public MapData MapData { get; private set; }
    public int MinX => 0;
    public int MinY => 0;

    private GameObject currentStartMarker;
    private GameObject currentEndMarker;
    private GameObject generatedGround;
    private GridCharacterMovement characterController;
    private float hWallOffset = 0.5f;
    private float vWallOffset = 0.5f;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool previousFollowCharacterCamera;

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

        if (followCharacterCamera != previousFollowCharacterCamera)
        {
            previousFollowCharacterCamera = followCharacterCamera;
            ApplyCameraMode(true);
        }

        if (!followCharacterCamera && MapData != null &&
            (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight))
        {
            FitCameraToMap();
        }
    }

    private void LateUpdate()
    {
        if (followCharacterCamera)
            UpdateFollowCamera(false);
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
        GenerateGround();
        RefreshQuestMarkers();

        characterController = FindAnyObjectByType<GridCharacterMovement>();
        if (characterController != null)
            characterController.InitializeCharacterAtStart();

        previousFollowCharacterCamera = followCharacterCamera;
        ApplyCameraMode(true);

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

    private void GenerateGround()
    {
        if (generatedGround != null)
            Destroy(generatedGround);

        float worldWidth = MapData.width * grid.cellSize;
        float worldHeight = MapData.height * grid.cellSize;

        generatedGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        generatedGround.name = "Generated Ground";
        generatedGround.transform.position = new Vector3(
            worldWidth * 0.5f,
            grid.gridHeight - 0.01f,
            worldHeight * 0.5f
        );
        generatedGround.transform.localScale = new Vector3(
            worldWidth / 10f,
            1f,
            worldHeight / 10f
        );
    }

    private void ApplyCameraMode(bool snap)
    {
        if (followCharacterCamera)
            UpdateFollowCamera(snap);
        else
            FitCameraToMap();
    }

    private void FitCameraToMap()
    {
        Camera mapCamera = Camera.main;
        if (mapCamera == null || MapData == null || grid == null)
            return;

        float worldWidth = MapData.width * grid.cellSize;
        float worldHeight = MapData.height * grid.cellSize;
        float aspect = Mathf.Max(0.01f, mapCamera.aspect);

        mapCamera.orthographic = true;

        float verticalHalfSize = worldHeight * 0.5f;
        float horizontalHalfSizeRequired = (worldWidth * 0.5f) / aspect;
        mapCamera.orthographicSize = Mathf.Max(verticalHalfSize, horizontalHalfSizeRequired) * cameraPadding;

        float cameraHeight = Mathf.Max(20f, Mathf.Max(worldWidth, worldHeight) * 0.25f);
        mapCamera.transform.position = new Vector3(
            worldWidth * 0.5f,
            grid.gridHeight + cameraHeight,
            worldHeight * 0.5f
        );
        mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        mapCamera.nearClipPlane = 0.1f;
        mapCamera.farClipPlane = cameraHeight + 100f;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void UpdateFollowCamera(bool snap)
    {
        Camera mapCamera = Camera.main;
        Transform target = GetFollowTarget();
        if (mapCamera == null || target == null)
            return;

        mapCamera.orthographic = false;
        mapCamera.fieldOfView = followFieldOfView;
        mapCamera.nearClipPlane = 0.1f;
        mapCamera.farClipPlane = Mathf.Max(1000f, followDistance + followHeight + 100f);

        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 desiredPosition = target.position - forward * followDistance + Vector3.up * followHeight;
        Vector3 lookTarget = target.position + Vector3.up * followLookHeight;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - desiredPosition, Vector3.up);

        if (snap)
        {
            mapCamera.transform.position = desiredPosition;
            mapCamera.transform.rotation = desiredRotation;
            return;
        }

        float blend = 1f - Mathf.Exp(-followSmoothSpeed * Time.deltaTime);
        mapCamera.transform.position = Vector3.Lerp(mapCamera.transform.position, desiredPosition, blend);
        mapCamera.transform.rotation = Quaternion.Slerp(mapCamera.transform.rotation, desiredRotation, blend);
    }

    private Transform GetFollowTarget()
    {
        if (characterController == null)
            characterController = FindAnyObjectByType<GridCharacterMovement>();

        if (characterController == null || characterController.transform.childCount == 0)
            return null;

        return characterController.transform.GetChild(characterController.transform.childCount - 1);
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
}
