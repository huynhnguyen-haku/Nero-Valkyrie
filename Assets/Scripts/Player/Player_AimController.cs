﻿using Cinemachine;
using UnityEngine;

public class Player_AimController : MonoBehaviour
{
    public static Player_AimController instance;

    private Player player;
    private PlayerControls controls;

    [Header("Aim Visual - Laser")]
    public LineRenderer aimLaser;
    public GameObject aimTarget;

    [Header("Camera Controls")]
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private CinemachineVirtualCamera zoomVirtualCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float topClamp = 90f;
    [SerializeField] private float bottomClamp = -90f;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    [Header("Aim Settings")]
    [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayer;
    public bool allowCameraLook = true;
    public bool allowZoom = true;
    [SerializeField] private float maxAimDistance = 50f; // Khoảng cách tối đa cho raycast

    private Vector2 mouseInput;
    private RaycastHit lastKnownMouseHit;

    private void Start()
    {
        instance = this;
        player = GetComponent<Player>();
        AssignInputEvents();
        if (aimTarget != null)
            aimTarget.SetActive(false);
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void Update()
    {
        if (!allowCameraLook)
            return;

        if (player.health.playerIsDead)
            return;

        aimVirtualCamera.gameObject.SetActive(true);
        UpdateAimVisual();
        UpdateAimPosition();
    }

    private void LateUpdate()
    {
        if (!allowCameraLook)
            return;

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
        RaycastHit hitInfo = GetAimHitInfo();
        if (Physics.Raycast(Camera.main.transform.position, (hitInfo.point - Camera.main.transform.position).normalized, out RaycastHit forwardHit, maxAimDistance, aimLayer))
        {
            aim.position = forwardHit.point;
        }
        else
        {
            aim.position = Camera.main.transform.position + Camera.main.transform.forward * maxAimDistance;
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
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxAimDistance, aimLayer))
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

    #region Input Events

    private void AssignInputEvents()
    {
        controls = player.controls;
        controls.Character.Aim.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        controls.Character.Aim.canceled += ctx => mouseInput = Vector2.zero;

        controls.Character.Zoom.performed += ctx =>
        {
            if (allowZoom)
                zoomVirtualCamera.gameObject.SetActive(true);
        };

        controls.Character.Zoom.canceled += ctx =>
        {
            if (allowZoom)
                zoomVirtualCamera.gameObject.SetActive(false);
        };
    }

    #endregion
}