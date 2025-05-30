using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    private Transform playerTransform;

    [Header("Cover Points")]
    [SerializeField] private GameObject coverPointPrefab;
    [SerializeField] private List<CoverPoint> coverPoints = new List<CoverPoint>();
    [SerializeField] private float xOffSet = 1;
    [SerializeField] private float yOffSet = 0.2f;
    [SerializeField] private float zOffSet = 1;

    private void Start()
    {
        GenerateCoverPoints();
        playerTransform = GameObject.FindFirstObjectByType<Player>().transform;
    }

    // Generate cover points around this object
    private void GenerateCoverPoints()
    {
        Vector3[] localCoverPoints =
        {
            new Vector3(0, yOffSet, -zOffSet),  // Front
            new Vector3(0, yOffSet, zOffSet),   // Back
            new Vector3(-xOffSet, yOffSet, 0),  // Right
            new Vector3(xOffSet, yOffSet, 0)    // Left
        };

        foreach (Vector3 localPoint in localCoverPoints)
        {
            Vector3 worldPoint = transform.TransformPoint(localPoint);
            CoverPoint coverPoint = Instantiate(coverPointPrefab, worldPoint, Quaternion.identity, transform).GetComponent<CoverPoint>();
            coverPoints.Add(coverPoint);
        }
    }

    // Return all valid cover points for the given enemy
    public List<CoverPoint> GetValidCoverPoints(Transform enemyTransform)
    {
        List<CoverPoint> validCoverPoints = new List<CoverPoint>();
        foreach (CoverPoint coverPoint in coverPoints)
        {
            if (IsValidCoverPoint(coverPoint, enemyTransform))
                validCoverPoints.Add(coverPoint);
        }
        return validCoverPoints;
    }

    // Check if the cover point is valid for use
    private bool IsValidCoverPoint(CoverPoint coverPoint, Transform enemyTransform)
    {
        if (coverPoint.isOccupied) // Other enemy is using this cover point
            return false;

        if (!IsFarthestFromPlayer(coverPoint))
            return false;

        if (IsCoverBehindPlayer(coverPoint, enemyTransform))
            return false;

        if (IsCoverCloseToPlayer(coverPoint))
            return false;

        if (IsCoverCloseToLastCover(coverPoint, enemyTransform))
            return false;

        return true;
    }

    // True if cover point is behind the player (enemy cannot use this one)
    private bool IsCoverBehindPlayer(CoverPoint coverPoint, Transform enemyTransform)
    {
        float distanceToPlayer = Vector3.Distance(coverPoint.transform.position, playerTransform.position);
        float distanceToEnemy = Vector3.Distance(coverPoint.transform.position, enemyTransform.position);

        return distanceToPlayer < distanceToEnemy;
    }

    // True if player is too close to the cover point
    private bool IsCoverCloseToPlayer(CoverPoint coverPoint)
    {
        float distanceToPlayer = Vector3.Distance(coverPoint.transform.position, playerTransform.position);
        return distanceToPlayer < 2f;
    }

    // True if cover point is too close to the last cover used by this enemy
    private bool IsCoverCloseToLastCover(CoverPoint coverPoint, Transform enemyTransform)
    {
        CoverPoint lastCover = enemyTransform.GetComponent<Enemy_Range>().currentCover;
        return lastCover != null &&
            Vector3.Distance(coverPoint.transform.position, lastCover.transform.position) < 5;
    }

    // True if this is the farthest cover point from the player
    private bool IsFarthestFromPlayer(CoverPoint coverPoint)
    {
        CoverPoint farthestPoint = null;
        float farthestDistance = 0;

        foreach (CoverPoint point in coverPoints)
        {
            float distance = Vector3.Distance(point.transform.position, playerTransform.position);
            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestPoint = point;
            }
        }
        return farthestPoint == coverPoint;
    }
}
