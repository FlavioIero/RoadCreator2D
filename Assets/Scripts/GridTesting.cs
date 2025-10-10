using UnityEngine;

public class GridTesting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject track;
    [SerializeField] private GameObject train;

    [Header("Grid Settings")]
    [SerializeField, Min(0)] int width = 3;
    [SerializeField, Min(0)] int height = 3;
    [SerializeField, Min(0)] int maxWidth = 50;
    [SerializeField, Min(0)] int maxHeight = 50;
    [SerializeField, Min(0)] float cellSize = 10f;
    [SerializeField, Min(2)] int newStationsPerTurn = 2;
    [SerializeField, Min(0f)] float gridMargin = 10f;

    [SerializeField] bool showDebug = true;
    [SerializeField, Min(0)] int fontSize = 30;

    [SerializeField, Min(0)] int gridExpansion = 1;

    private Grid grid;

    private PathBuilder pathBuilder;

    private void Start()
    {
        grid = new Grid(width, height, maxWidth, maxHeight, cellSize, newStationsPerTurn, gridMargin, showDebug, fontSize);
        pathBuilder = gameObject.AddComponent<PathBuilder>();
        pathBuilder.Initialize(grid, track, train);
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            grid.Reset();
        }
    }

}
