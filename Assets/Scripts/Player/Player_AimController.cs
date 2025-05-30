using UnityEngine;
using Cinemachine;

public class Player_AimController : MonoBehaviour
{
    public static Player_AimController instance;

    private Player player;
    private PlayerControls controls;
    private CameraManager cameraManager;

    [Header("Aim Visual - Laser")]
    public LineRenderer aimLaser;
    public GameObject aimTarget;

    [Header("Camera Controls")]
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float topClamp = 90f;
    [SerializeField] private float bottomClamp = -90f;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    [Header("Aim Settings")]
    [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayer;
    [SerializeField] private LayerMask lockOnLayer;

    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnRadius = 2f;
    public Transform lockedEnemy;
    public bool isLockedOn;

    private Vector2 mouseInput;
    private RaycastHit lastKnownMouseHit;

    private void Start()
    {
        instance = this;
        cameraManager = CameraManager.instance;
        player = GetComponent<Player>();
        AssignInputEvents();
        if (aimTarget != null)
            aimTarget.SetActive(false);
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void Update()
    {
        if (player.health.playerIsDead || !player.controlsEnabled)
            return;

        aimVirtualCamera.gameObject.SetActive(true); // Bật camera khi cần
        UpdateAimVisual();
        UpdateAimPosition();
    }

    private void LateUpdate()
    {
        cinemachineTargetYaw += mouseInput.x * sensitivity;
        cinemachineTargetPitch -= mouseInput.y * sensitivity;
        cinemachineTargetPitch = Mathf.Clamp(cinemachineTargetPitch, bottomClamp, topClamp);
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0f);
    }

    public Transform Aim() => aim;

    #region Aim Logic

    private void UpdateAimVisual()
    {
        aim.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        aimLaser.enabled = player.weapon.WeaponReady();
        if (!aimLaser.enabled)
        {
            EnableAimTarget(false);
            return;
        }

        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();
        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);

        Transform gunPoint = player.weapon.GunPoint();
        float laserTipLength = 0.5f;
        float gunDistance = player.weapon.CurrentWeapon().laserDistance;

        Vector3 laserDirection = player.weapon.BulletDirection();
        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hitInfo, gunDistance))
        {
            endPoint = hitInfo.point;
            laserTipLength = 0;
        }

        aimLaser.SetPosition(0, gunPoint.position);
        aimLaser.SetPosition(1, endPoint);
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLength);

        EnableAimTarget(true);
    }

    private void UpdateAimPosition()
    {
        if (isLockedOn && lockedEnemy != null)
        {
            aim.position = lockedEnemy.position;
            aimTarget.GetComponent<SpriteRenderer>().color = Color.red;
            Vector3 direction = lockedEnemy.position - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            RaycastHit hitInfo = GetAimHitInfo();
            aim.position = hitInfo.point;
            aimTarget.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (aimTarget != null)
        {
            aimTarget.transform.position = aim.position;
            aimTarget.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    public void EnableLaserAim(bool enable) => aimLaser.enabled = enable;

    public void EnableAimTarget(bool enable)
    {
        if (aimTarget != null)
            aimTarget.SetActive(enable);
    }

    private RaycastHit GetAimHitInfo()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, aimLayer))
        {
            lastKnownMouseHit = hitInfo;
            return hitInfo;
        }
        return lastKnownMouseHit;
    }

    public Transform GetAimCameraTarget()
    {
        return cinemachineCameraTarget.transform;
    }

    #endregion

    #region Lock-On Logic

    private void ToggleLockOn(bool enable)
    {
        isLockedOn = enable;
        if (isLockedOn)
            CheckLockOn();
        else
            lockedEnemy = null;
    }

    private void CheckLockOn()
    {
        if (!isLockedOn)
            return;

        Collider[] enemies = Physics.OverlapSphere(aim.position, lockOnRadius, lockOnLayer);
        if (enemies.Length > 0)
        {
            Transform closestEnemy = null;
            float closestDistance = float.MaxValue;
            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(aim.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
            if (closestEnemy != null)
                lockedEnemy = closestEnemy;
        }
    }

    private void RotatePlayerTowardsLockedTarget()
    {
        if (lockedEnemy == null || player == null)
            return;

        Vector3 directionToTarget = lockedEnemy.position - player.transform.position;
        directionToTarget.y = 0;
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }
    }

    #endregion

    #region Input Events

    private void AssignInputEvents()
    {
        controls = player.controls;
        controls.Character.Aim.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        controls.Character.Aim.canceled += ctx => mouseInput = Vector2.zero;
        controls.Character.ToggleLockOn.performed += ctx => ToggleLockOn(true);
        controls.Character.ToggleLockOn.canceled += ctx => ToggleLockOn(false);
    }

    #endregion
}