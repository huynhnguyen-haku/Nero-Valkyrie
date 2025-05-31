using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera carVirtualCamera;

    #region Unity Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }


    #endregion

    #region Camera Logic

    public void ChangeToCarVirtualCamera()
    {
        carVirtualCamera.gameObject.SetActive(true);
    }

    public void ChangeToPlayerVirtualCamera()
    {
        carVirtualCamera.gameObject.SetActive(false);
    }

    #endregion
}
