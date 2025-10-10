using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PathBuilder : MonoBehaviour
{
    [SerializeField] private GameObject track;
    [SerializeField] private GameObject train;

    private Grid grid;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private bool isBuilding = false;
    private Vector2Int lastDir;

    public void Initialize(Grid grid, GameObject track, GameObject train)
    {
        this.grid = grid;
        this.track = track;
        this.train = train;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Utils.GetMouseWorldPosition();
            grid.GetXY(worldPos, out int x, out int y);
            Debug.Log($"Grid.GetValue = " + grid.GetValue(x, y));

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

    public Grid GetGrid() => grid;

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
        
        Vector2Int prev = currentPath[currentPath.Count - 1];
        Vector2Int dir = pos - prev;
        int tileValue = grid.GetValue(pos.x, pos.y);

        if (tileValue == -1)
        {
            Debug.Log("Out of bounds = val -1");
            return;
        }
            
        if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) != 1)
            return;

        if (currentPath.Count >= 2 && dir == -lastDir)
        {
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

        if (tileValue != (int)Tile.TileType.Station)
            grid.SetValue(pos.x, pos.y, (int)trackType);
        else if (tileValue == (int)Tile.TileType.Station)
            EndPath();
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
            currentPath.Clear();
            return;
        }

        CreateTrackFromPath(currentPath);
        currentPath.Clear();
    }

    private void CreateTrackFromPath(List<Vector2Int> pathCells)
    {
        // Crea l’oggetto Track
        GameObject trackObj = Instantiate(track);
        Track trackComp = trackObj.GetComponent<Track>();
        SplineContainer spline = trackComp.spline;

        spline.Spline.Clear();

        foreach (var cell in pathCells)
        {
            Vector3 worldPos = grid.GetWorldPosition(cell.x, cell.y);
            spline.Spline.Add(new BezierKnot(worldPos));
        }

        // Crea il treno
        GameObject trainObj = Instantiate(train);
        trackComp.train = trainObj;
        trainObj.transform.position = spline.Spline[0].Position;

        Vector3 startPos = new Vector3(grid.GetCellSize() * 0.5f, grid.GetCellSize() * 0.5f, 0f);
        trackComp.Initialize(startPos);

        Debug.Log($"Created track with {pathCells.Count} points");
    }
}
