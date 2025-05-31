using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private CinemachineVirtualCamera virtualCamera;
    private Cinemachine3rdPersonFollow thirdPersonFollow;

    [Header("Camera Settings")]
    [SerializeField] private bool canChangeCameraDistance;
    [SerializeField] private float targetCameraDistance;
    [SerializeField] private float distanceChangeRate;

    #region Unity Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    private void Update()
    {
        UpdateCameraDistance();
    }

    #endregion

    #region Camera Logic

    // Smoothly update camera distance if allowed
    private void UpdateCameraDistance()
    {
        if (!canChangeCameraDistance)
            return;

        float currentDistance = thirdPersonFollow.CameraDistance;
        if (Mathf.Abs(targetCameraDistance - currentDistance) < 0.1f)
            return;

        thirdPersonFollow.CameraDistance = Mathf.Lerp(currentDistance, targetCameraDistance, distanceChangeRate * Time.deltaTime);
    }

    // Set new camera distance and change rate
    public void ChangeCameraDistance(float distance, float newChangeRate = 0.25f)
    {
        distanceChangeRate = newChangeRate;
        targetCameraDistance = distance;
    }

    // Change camera follow target and optionally distance/lookahead
    public void ChangeCameraTarget(Transform target, float cameraDistance = 10)
    {
        virtualCamera.Follow = target;
        ChangeCameraDistance(cameraDistance);
    }

    #endregion
}
