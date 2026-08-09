using System.Collections.Generic;
using UnityEngine;

public class GridCharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Grid grid;
    public float moveDelay = 0.5f;

    [Header("Character Setup")]
    [SerializeField] private GameObject charPrefab;

    [Header("Wall Detection")]
    [SerializeField] private JsonLoader jsonLoader;

    [Header("Path Following")]
    public bool manualInputEnabled = false;

    private int currentGridX;
    private int currentGridZ;
    private float lastMoveTime;
    private GameObject spawnedCharacter;
    private List<Vector2Int> currentPath;
    private int pathIndex;
    private bool isFollowingPath;
    private bool pathCompleted;
    private IMapData mapAdapter;

    public event System.Action OnPathCompleted;

    private void Start()
    {
        if (grid == null) grid = FindAnyObjectByType<Grid>();
        if (jsonLoader == null) jsonLoader = FindAnyObjectByType<JsonLoader>();
        RefreshMapAdapter();
        MoveToCurrentCell();
    }

    private void Update()
    {
        if (Time.time - lastMoveTime < moveDelay) return;
        if (manualInputEnabled) HandleInput();
        else if (isFollowingPath && !pathCompleted) FollowPath();
    }

    private void RefreshMapAdapter()
    {
        MapData mapData = jsonLoader != null ? jsonLoader.GetMapData() : null;
        mapAdapter = mapData != null ? new MapDataAdapter(mapData) : null;
    }

    public void SetPath(List<Vector2Int> newPath)
    {
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("Invalid path provided!");
            return;
        }

        currentPath = new List<Vector2Int>(newPath);
        pathIndex = 0;
        isFollowingPath = true;
        pathCompleted = false;
        SetGridPosition(currentPath[0].x, currentPath[0].y);
        pathIndex = 1;
    }

    private void FollowPath()
    {
        if (pathIndex >= currentPath.Count)
        {
            isFollowingPath = false;
            pathCompleted = true;
            OnPathCompleted?.Invoke();
            return;
        }

        Vector2Int nextPosition = currentPath[pathIndex];
        if (TryMove(nextPosition.x, nextPosition.y)) pathIndex++;
        else
        {
            Debug.LogError($"Path blocked at ({nextPosition.x}, {nextPosition.y})!");
            isFollowingPath = false;
        }
    }

    private void HandleInput()
    {
        int newGridX = currentGridX;
        int newGridZ = currentGridZ;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) newGridZ++;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) newGridZ--;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) newGridX--;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) newGridX++;
        else return;

        TryMove(newGridX, newGridZ);
    }

    public bool TryMove(int targetX, int targetZ)
    {
        if (Time.time - lastMoveTime < moveDelay) return false;
        if (!grid.IsValidGridPosition(targetX, targetZ)) return false;
        if (IsMovementBlocked(currentGridX, currentGridZ, targetX, targetZ)) return false;

        FaceDirection(new Vector2Int(currentGridX, currentGridZ), new Vector2Int(targetX, targetZ));
        currentGridX = targetX;
        currentGridZ = targetZ;
        MoveToCurrentCell();
        lastMoveTime = Time.time;
        return true;
    }

    private void MoveToCurrentCell()
    {
        if (spawnedCharacter != null)
            spawnedCharacter.transform.position = grid.GetCellCenter(currentGridX, currentGridZ);
    }

    private bool IsMovementBlocked(int fromX, int fromZ, int toX, int toZ)
    {
        return mapAdapter != null && PathfindingAlgorithm.IsMovementBlocked(new Vector2Int(fromX, fromZ), new Vector2Int(toX, toZ), mapAdapter);
    }

    public Vector2Int CurrentGridPosition => new Vector2Int(currentGridX, currentGridZ);

    public void SetGridPosition(int gridX, int gridZ)
    {
        if (!grid.IsValidGridPosition(gridX, gridZ)) return;
        currentGridX = gridX;
        currentGridZ = gridZ;
        MoveToCurrentCell();
    }

    public Grid GetGrid() => grid;

    [ContextMenu("Toggle Manual Input")]
    public void ToggleManualControl()
    {
        manualInputEnabled = !manualInputEnabled;
        if (manualInputEnabled) isFollowingPath = false;
    }

    public void InitializeCharacterAtStart()
    {
        RefreshMapAdapter();
        Vector2Int startGridPos = jsonLoader.GetStartGridPosition();
        currentGridX = startGridPos.x;
        currentGridZ = startGridPos.y;

        if (spawnedCharacter != null) Destroy(spawnedCharacter);
        if (charPrefab == null) return;

        spawnedCharacter = Instantiate(charPrefab, grid.GetCellCenter(currentGridX, currentGridZ), Quaternion.identity);
        spawnedCharacter.transform.SetParent(transform);
    }

    public int GetCurrentMoveNumber() => !isFollowingPath || currentPath == null ? 0 : pathIndex;
    public int GetTotalMoves() => currentPath == null || currentPath.Count == 0 ? 0 : currentPath.Count - 1;
    public bool IsFollowingPath() => isFollowingPath;
    public bool IsPathCompleted() => pathCompleted;

    private void FaceDirection(Vector2Int from, Vector2Int to)
    {
        if (spawnedCharacter == null) return;
        Vector3 direction = (grid.GetCellCenter(to.x, to.y) - grid.GetCellCenter(from.x, from.y)).normalized;
        if (direction != Vector3.zero) spawnedCharacter.transform.rotation = Quaternion.LookRotation(direction);
    }
}
