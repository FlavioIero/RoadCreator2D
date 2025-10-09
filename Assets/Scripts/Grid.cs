using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using System;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private int maxWidth;
    private int maxHeight;
    private float stationsSpawnChance = 0.1f;
    private int fontSize;
    private int[,] grid;
    
    private float gridMargin = 10f;

    private const int NEW_STATIONS_PER_ROUND = 2;

    // Grid debug
    private bool showDebug = false;
    private TextMesh[,] debugTextGrid;
    private Color textColor = Color.white;
    private Color lineColor = Color.white;
    private float lineDuration = 100f;
    private int level = 1;
    
    // Events
    
    public event Action OnMaxSizeReached;


    public Grid(int width, int height, float cellSize, int maxWidth, int maxHeight, float stationsSpawnChance, int fontSize, bool showDebug = false, float gridMargin = 10f)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;
        this.stationsSpawnChance = stationsSpawnChance;
        this.fontSize = fontSize;
        this.showDebug = showDebug;
        this.gridMargin = gridMargin;

        grid = new int[width, height];
        if (showDebug)
            debugTextGrid = new TextMesh[width, height];

        DrawDebug();
        //SpawnNewStations(0, 0);
    }

    private void DrawDebug()
    {
        DeleteWorldTexts();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (showDebug)
                    debugTextGrid[x, y] = UtilsClass.CreateWorldText(grid[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, fontSize, textColor, TextAnchor.MiddleCenter);
                else
                    UtilsClass.CreateWorldText(grid[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, fontSize, textColor, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), lineColor, lineDuration);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), lineColor, lineDuration);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), lineColor, lineDuration);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), lineColor, lineDuration);

        CenterGridView();
    }

    private void DeleteWorldTexts()
    {
        if (debugTextGrid == null) return;
        for (int x = 0; x < debugTextGrid.GetLength(0); x++)
        {
            for (int y = 0; y < debugTextGrid.GetLength(1); y++)
            {
                DeleteWorldText(x, y);
            }
        }
    }

    private void DeleteWorldText(int x, int y)
    {
        if (debugTextGrid[x, y] != null)
            GameObject.Destroy(debugTextGrid[x, y].gameObject);
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

        
        DrawDebug();
        SpawnNewStations(offsetX, offsetY); 

        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        if (GetGridWorldHeight() > camHeight - gridMargin * 2) 
            ExpandCamera((GetGridWorldHeight() - camHeight) / 2f + gridMargin * 2);
        if (GetGridWorldWidth() > camWidth - gridMargin * 2)
            ExpandCamera((GetGridWorldWidth() - camWidth) / 2f + gridMargin);

        level++;
        Debug.Log("Level: " + level + " ---- Size: " + width + " x " + height);
    }

    private void ExpandCamera(float increase)
    {
        Camera.main.orthographicSize += increase;
    }

    private void SpawnNewStations(int offsetX, int offsetY)
    {
        int stationsSpawned = 0;
        int stationHash = (int)Tile.TileType.Station;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Debug.Log($"Checking cell ({x},{y}) for new station...");
                if (!IsNewZone(x, y, offsetX, offsetY)) 
                    continue;

                if (grid[x, y] != (int)Tile.TileType.Empty || UnityEngine.Random.value >= stationsSpawnChance)
                    continue;

                grid[x, y] = stationHash;
                if (showDebug)
                {
                    debugTextGrid[x, y].text = stationHash.ToString();
                    debugTextGrid[x, y].color = Color.green;
                }
                stationsSpawned++;
                if (stationsSpawned >= NEW_STATIONS_PER_ROUND)
                    return;
            }
        }

        // TEMPORARY
        // Causa Stack Overflow :)
        if (stationsSpawned < NEW_STATIONS_PER_ROUND)
            SpawnNewStations(offsetX, offsetY);
    }

    private Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, y) * cellSize;
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

    public void GetXY(Vector3 worldPosition, out int x, out int y) {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
    }

    public int GetValue(int x, int y) {
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

    public void CenterGridView()
    {
        Camera.main.transform.position = new Vector3(width * cellSize / 2f, height * cellSize / 2f, -10f);
    }

    public float GetGridWorldWidth() {
        return width * cellSize;
    }

    public float GetGridWorldHeight() {
        return height * cellSize;
    }

    private bool IsNewZone(int x, int y, int offsetX, int offsetY)
    {
        return x < offsetX ||                   // bordo sinistro
               x >= width - offsetX ||          // bordo destro
               y < offsetY ||                   // bordo basso
               y >= height - offsetY;
    }

}
