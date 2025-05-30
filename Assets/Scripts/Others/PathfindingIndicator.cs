using UnityEngine;
using UnityEngine.AI;

public class PathfindingIndicator : MonoBehaviour
{
    private Transform target; // Current navigation target
    public LineRenderer lineRenderer; // Draws the path
    public float updateInterval = 0; // Path update interval
    public float heightOffset = 0.5f; // Path height above ground

    private NavMeshPath path; // Calculated path
    private float timer; // Timer for path updates

    #region Unity Methods

    private void Start()
    {
        // Initialize path and line renderer
        path = new NavMeshPath();
        lineRenderer.positionCount = 0;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCapVertices = 5;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval && target != null)
        {
            timer = 0f;
            UpdatePath();
        }
    }

    #endregion

    #region Pathfinding Logic

    // Set a new navigation target and update path immediately
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            Debug.Log("PathfindingIndicator: Target set to " + target.name);
            UpdatePath();
        }
        else
        {
            Debug.LogWarning("PathfindingIndicator: Target is null!");
            lineRenderer.positionCount = 0;
        }
    }

    // Calculate and draw the path to the target
    private void UpdatePath()
    {
        if (target == null)
        {
            Debug.LogWarning("PathfindingIndicator: Target is null in UpdatePath!");
            lineRenderer.positionCount = 0;
            return;
        }
        if (NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path))
        {
            Vector3[] smoothedPoints = SmoothPath(path.corners);
            lineRenderer.positionCount = smoothedPoints.Length;
            for (int i = 0; i < smoothedPoints.Length; i++)
            {
                lineRenderer.SetPosition(i, smoothedPoints[i] + Vector3.up * heightOffset);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    // Smooth the path by adding midpoints between corners
    private Vector3[] SmoothPath(Vector3[] corners)
    {
        if (corners.Length < 2) return corners;

        Vector3[] smoothedPoints = new Vector3[corners.Length * 2 - 1];
        smoothedPoints[0] = corners[0];

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];
            smoothedPoints[i * 2] = start;
            if (i < corners.Length - 1)
            {
                smoothedPoints[i * 2 + 1] = Vector3.Lerp(start, end, 0.5f);
            }
        }
        smoothedPoints[smoothedPoints.Length - 1] = corners[corners.Length - 1];

        return smoothedPoints;
    }

    // Reset the indicator state and clear the path
    public void Reset()
    {
        target = null;
        timer = 0f;
        lineRenderer.positionCount = 0;
        Debug.Log("PathfindingIndicator: Reset state.");
    }

    #endregion
}
