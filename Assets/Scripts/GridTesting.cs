using UnityEngine;
using CodeMonkey.Utils;

public class GridTesting : MonoBehaviour
{
    [SerializeField, Min(0)] int width = 3;
    [SerializeField, Min(0)] int height = 3;
    [SerializeField, Min(0)] float cellSize = 10f;
    [SerializeField, Min(0)] int fontSize = 30;
    [SerializeField, Min(0f)] float gridMargin = 10f;
    [SerializeField, Min(0)] int maxWidth = 50;
    [SerializeField, Min(0)] int maxHeight = 50;
    [SerializeField, Range(0f, 1f)] float stationsSpawnChance = 0.05f;
    [SerializeField] bool showDebug = true;
    [SerializeField, Min(0)] int gridExpansion = 1;

    private Grid grid;

    private PathBuilder pathBuilder;

    private void Start()
    {
        grid = new Grid(width, height, cellSize, maxWidth, maxHeight, stationsSpawnChance, fontSize, showDebug, gridMargin);
        pathBuilder = gameObject.AddComponent<PathBuilder>();
        pathBuilder.Initialize(grid);
    }

    private void Update()
    {
        /*
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
            int value = grid.GetValue(mouseWorldPosition);
            Debug.Log("Value: " + value);
            grid.SetValue(mouseWorldPosition, value + 20);
        }
        */

        if (Input.GetKeyDown(KeyCode.C))
        {
            grid.ExpandGrid(gridExpansion);
        }
    }

}
