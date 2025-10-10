using UnityEngine;
using UnityEngine.Splines;

public class Track : MonoBehaviour
{
    public SplineContainer spline;
    public GameObject train;
    public float speed = 10f;

    private float progress = 0f;
    private float trackLength = 0f;

    public void Initialize(Vector3 startPos)
    {
        transform.position = startPos;
    }

    private void Start()
    {
        if (spline != null)
            trackLength = spline.Spline.GetLength();
    }

    private void Update()
    {
        if (spline == null || train == null) return;

        float delta = speed / trackLength * Time.deltaTime;
        progress += delta;

        if (progress > 1f)
        {
            HandleEndedPath();
        }

        spline.Evaluate(progress, out var pos, out var tangent, out var up);
        train.transform.position = pos;
        train.transform.rotation = Quaternion.LookRotation(Vector3.forward, tangent);
    }

    private void HandleEndedPath()
    {
        progress = 0f;
    }

}
