using System.Collections.Generic;
using UnityEngine;

public class PathBuilder : MonoBehaviour
{
    private Grid grid;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private bool isBuilding = false;
    private Vector2Int lastDir;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Utils.GetMouseWorldPosition();
            grid.GetXY(worldPos, out int x, out int y);

            if (grid.GetValue(x, y) == (int)Tile.TileType.Station)
            {
                StartPath(new Vector2Int(x, y));
            }
        }

        if (Input.GetMouseButton(0) && isBuilding)
        {
            Vector3 worldPos = Utils.GetMouseWorldPosition();
            grid.GetXY(worldPos, out int x, out int y);
            Vector2Int pos = new Vector2Int(x, y);

            if (!currentPath.Contains(pos))
                TryAddToPath(pos);
        }

        if (Input.GetMouseButtonUp(0) && isBuilding)
        {
            EndPath();
        }
    }

    private void StartPath(Vector2Int start)
    {
        currentPath.Clear();
        currentPath.Add(start);
        isBuilding = true;
    }

    private void TryAddToPath(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return;
        Debug.Log("Pos: " + pos);

        Vector2Int prev = currentPath[currentPath.Count - 1];
        Vector2Int dir = pos - prev;

        Debug.Log("dir: " + dir + " lastDir: " + lastDir);
        if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) != 1)
            return;

        Debug.Log("Direction: " + dir);
        if (currentPath.Count >= 2 && dir == -lastDir)
        {
            Debug.Log("Direction: Removing last");
            Vector2Int last = currentPath[currentPath.Count - 1];
            grid.SetValue(last.x, last.y, (int)Tile.TileType.Empty);
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        if (currentPath.Count >= 2)
        {
            Vector2Int forward = lastDir;
            Vector2Int right = new Vector2Int(forward.y, -forward.x);
            Vector2Int left = new Vector2Int(-forward.y, forward.x);

            if (dir != forward && dir != right && dir != left)
                return;
        }

        currentPath.Add(pos);
        lastDir = dir;

        Tile.TileType trackType = dir.x != 0 ? Tile.TileType.HorTrack : Tile.TileType.VertTrack;

        if (grid.GetValue(pos.x, pos.y) != (int)Tile.TileType.Station)
            grid.SetValue(pos.x, pos.y, (int)trackType);
    }

    private void EndPath()
    {
        isBuilding = false;

        int stationCount = 0;
        foreach (var p in currentPath)
        {
            if (grid.GetValue(p.x, p.y) == (int)Tile.TileType.Station)
                stationCount++;
        }

        if (stationCount < 2)
        {
            foreach (var p in currentPath)
            {
                if (grid.GetValue(p.x, p.y) != (int)Tile.TileType.Station)
                    grid.SetValue(p.x, p.y, (int)Tile.TileType.Empty);
            }
        }

        currentPath.Clear();
    }

}

