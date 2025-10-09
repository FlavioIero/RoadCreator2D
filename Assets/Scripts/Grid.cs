using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grid
{
    private int width;
    private int height;
    private int maxWidth;
    private int maxHeight;
    private float cellSize;
    private int newStationsPerTurn = 2;
    private float gridMargin = 10f;

    private int[,] grid;

    // Grid debug
    private bool showDebug = false;
    private TextMesh[,] debugTextGrid;
    private int fontSize;
    private Color textColor = Color.white;
    private Color lineColor = Color.white;
    private float lineDuration = 100f;
    
    // Events
    public event Action OnMaxSizeReached;
    public event Action OnStationSpawnFailed;

    #region public methods

    public Grid(int width, int height, int maxWidth, int maxHeight, float cellSize, int newStationsPerTurn, float gridMargin = 10f, bool showDebug = false, int fontSize = 40)
    {
        this.width = width;
        this.height = height;
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;
        this.cellSize = cellSize;
        this.newStationsPerTurn = newStationsPerTurn;
        this.gridMargin = gridMargin;
        this.fontSize = fontSize;
        this.showDebug = showDebug;
        

        grid = new int[width, height];
        if (showDebug)
            debugTextGrid = new TextMesh[width, height];

        UpdateGrid();
        SpawnNewStations();
    }

    public void ExpandGrid(int increase)
    {
        Debug.Log("Expanding grid by: " + increase);

        if (increase <= 0) return;

        if (width >= maxWidth && height >= maxHeight)
        {
            OnMaxSizeReached?.Invoke();
            Debug.Log("Max grid size reached!");
            return;
        }

        int newWidth = Mathf.Min(maxWidth, width + increase * 2);
        int newHeight = Mathf.Min(maxHeight, height + increase * 2);
        int offsetX = (newWidth - width) / 2;
        int offsetY = (newHeight - height) / 2;

        int[,] newGrid = new int[newWidth, newHeight];
        TextMesh[,] newDebugTextGrid = new TextMesh[newWidth, newHeight];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                newGrid[x + offsetX, y + offsetY] = grid[x, y];
                if (showDebug)
                    newDebugTextGrid[x + offsetX, y + offsetY] = debugTextGrid[x, y];
            }
        }

        width = newWidth;
        height = newHeight;
        grid = newGrid;
        if (showDebug)
            debugTextGrid = newDebugTextGrid;
        Debug.Log("Grid expanded to X:" + grid.GetLength(0) + " Y: " + grid.GetLength(1));
        //Debug.Log("showDebug: " + showDebug);


        UpdateGrid();
        SpawnNewStations(offsetX, offsetY);

        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;

        if (GetGridWorldHeight() > camHeight - gridMargin * 2)
            ExpandCamera((GetGridWorldHeight() - camHeight) / 2f + gridMargin * 2);

    }

    public bool HasTypeAround(int x, int y, int type)
    {
        Vector2Int[] dirs = new Vector2Int[] {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var d in dirs)
        {
            int nx = x + d.x;
            int ny = y + d.y;
            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                continue;
            if (grid[nx, ny] == type)
                return true;
        }
        return false;
    }

    public void SpawnNewStations()
    {
        int stationsSpawned = 0;
        int stationHash = (int)Tile.TileType.Station;
        int emptyHash = (int)Tile.TileType.Empty;

        while (stationsSpawned < newStationsPerTurn)
        {
            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height);

            if (grid[x, y] != emptyHash)
                continue;

            grid[x, y] = stationHash;
            if (showDebug)
            {
                debugTextGrid[x, y].text = stationHash.ToString();
                debugTextGrid[x, y].color = Color.green;
            }

            stationsSpawned++;
        }
    }

    public void SpawnNewStations(int offsetX, int offsetY)
    {
        int stationHash = (int)Tile.TileType.Station;
        int stationsSpawned = 0;
        List<Vector2Int> possiblePoints = GetPossibleStationSpawnPoints(offsetX, offsetY);

        if (possiblePoints.Count == 0)
        {
            // Game over?
            Debug.LogWarning("No more space to spawn new stations!");
            OnStationSpawnFailed?.Invoke();
            return;
        }

        while (stationsSpawned < newStationsPerTurn)
        {
            int index = UnityEngine.Random.Range(0, possiblePoints.Count);
            Vector2Int point = possiblePoints[index];

            grid[point.x, point.y] = stationHash;
            if (showDebug)
            {
                debugTextGrid[point.x, point.y].text = stationHash.ToString();
                debugTextGrid[point.x, point.y].color = Color.green;
            }

            stationsSpawned++;
            possiblePoints.RemoveAt(index);
        }

    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            grid[x, y] = value;

            if (showDebug && debugTextGrid[x, y] != null)
            {
                debugTextGrid[x, y].text = grid[x, y].ToString();

                if (value == (int)Tile.TileType.Station)
                    debugTextGrid[x, y].color = Color.green;
                else if (value == (int)Tile.TileType.HorTrack || value == (int)Tile.TileType.VertTrack)
                    debugTextGrid[x, y].color = Color.yellow;
                else
                    debugTextGrid[x, y].color = Color.white;
            }
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return grid[x, y];
        return -1;
    }

    public int GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    public float GetGridWorldWidth()
    {
        return width * cellSize;
    }

    public float GetGridWorldHeight()
    {
        return height * cellSize;
    }

    public void CenterGridView()
    {
        Camera.main.transform.position = new Vector3(width * cellSize / 2f, height * cellSize / 2f, -10f);
    }

    #endregion

    #region private methods
    private void UpdateGrid()
    {
        DestroyWorldTexts();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (showDebug)
                    debugTextGrid[x, y] = Utils.CreateWorldText(grid[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, fontSize, textColor, TextAnchor.MiddleCenter);
                else
                    Utils.CreateWorldText(grid[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, fontSize, textColor, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), lineColor, lineDuration);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), lineColor, lineDuration);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), lineColor, lineDuration);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), lineColor, lineDuration);

        CenterGridView();
    }

    private void DestroyWorldText(int x, int y)
    {
        if (debugTextGrid[x, y] != null)
            GameObject.Destroy(debugTextGrid[x, y].gameObject);
    }

    private void DestroyWorldTexts()
    {
        if (debugTextGrid == null) return;
        for (int x = 0; x < debugTextGrid.GetLength(0); x++)
        {
            for (int y = 0; y < debugTextGrid.GetLength(1); y++)
            {
                DestroyWorldText(x, y);
            }
        }
    }

    private void ExpandCamera(float increase)
    {
        Camera.main.orthographicSize += increase;
    }

    private List<Vector2Int> GetPossibleStationSpawnPoints(int offsetX, int offsetY)
    {
        List<Vector2Int> possiblePoints = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsNewTile(x, y, offsetX, offsetY))
                    continue;

                if (grid[x, y] != (int)Tile.TileType.Empty)
                    continue;

                if (HasTypeAround(x, y, (int)Tile.TileType.Empty))
                    possiblePoints.Add(new Vector2Int(x, y));
            }
        }

        return possiblePoints;

    }

    private bool IsNewTile(int x, int y, int offsetX, int offsetY)
    {
        return x < offsetX || 
               x >= width - offsetX ||
               y < offsetY || 
               y >= height - offsetY;
    }

    public void Reset()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = (int)Tile.TileType.Empty;
                if (showDebug && debugTextGrid[x, y] != null)
                {
                    debugTextGrid[x, y].text = grid[x, y].ToString();
                    debugTextGrid[x, y].color = Color.white;
                }
            }
        }
        UpdateGrid();
        SpawnNewStations(0, 0);
    }

    #endregion

}
