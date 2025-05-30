using System.Collections.Generic;
using UnityEngine;

public class LevelPart : MonoBehaviour
{
    [Header("Mission Compatibility")]
    public List<MissionType> missionTypes;

    [Header("Intersection Check")]
    [SerializeField] private LayerMask intersectionLayer;
    [SerializeField] private Collider[] intersectionCheckColliders;
    [SerializeField] private Transform intersectionCheckParent;

    #region Unity Methods

    private void Start()
    {
        if (intersectionCheckColliders.Length <= 0)
        {
            intersectionCheckColliders = intersectionCheckParent.GetComponentsInChildren<Collider>();
        }
    }

    #endregion

    #region Intersection Logic

    // Check if this part intersects with others in the intersection layer
    public bool IntersectionDetected()
    {
        Physics.SyncTransforms();

        foreach (var collider in intersectionCheckColliders)
        {
            Collider[] hitColliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, Quaternion.identity, intersectionLayer);

            foreach (var hit in hitColliders)
            {
                IntersectionCheck intersectionCheck = hit.GetComponentInParent<IntersectionCheck>();
                if (intersectionCheck != null && intersectionCheckParent != intersectionCheck.transform)
                {
                    return true;
                }
            }
        }
        return false;
    }

    #endregion

    #region Snap & Align

    // Snap and align this part to a target snap point
    public void SnapAndAlignPartTo(SnapPoint targetSnapPoint)
    {
        SnapPoint entrancePoint = GetEntrancePoint();
        AlignTo(entrancePoint, targetSnapPoint);
        SnapTo(entrancePoint, targetSnapPoint);
    }

    // Move this part so its snap point matches the target
    private void SnapTo(SnapPoint ownSnapPoint, SnapPoint targetSnapPoint)
    {
        var offset = transform.position - ownSnapPoint.transform.position;
        var newPosition = targetSnapPoint.transform.position + offset;
        transform.position = newPosition;
    }

    // Align this part's rotation to the target snap point
    private void AlignTo(SnapPoint ownSnapPoint, SnapPoint targetSnapPoint)
    {
        var rotationOffset = ownSnapPoint.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y;
        transform.rotation = targetSnapPoint.transform.rotation;
        transform.Rotate(0, 180, 0);
        transform.Rotate(0, -rotationOffset, 0);
    }

    #endregion

    #region Snap Point Access

    public SnapPoint GetEntrancePoint()
    {
        return GetSnapPointOfType(SnapPointType.Enter);
    }

    public SnapPoint GetExitPoint()
    {
        return GetSnapPointOfType(SnapPointType.Exit);
    }

    private SnapPoint GetSnapPointOfType(SnapPointType pointType)
    {
        SnapPoint[] snapPoints = GetComponentsInChildren<SnapPoint>();
        List<SnapPoint> filteredSnapPoints = new List<SnapPoint>();

        foreach (SnapPoint snapPoint in snapPoints)
        {
            if (snapPoint.pointType == pointType)
            {
                filteredSnapPoints.Add(snapPoint);
            }
        }

        if (filteredSnapPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, filteredSnapPoints.Count);
            return filteredSnapPoints[randomIndex];
        }

        return null;
    }

    #endregion

    #region Enemy Access

    public Enemy[] GetEnemies() => GetComponentsInChildren<Enemy>(true);

    #endregion
}
