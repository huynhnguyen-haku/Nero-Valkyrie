using UnityEngine;
using UnityEngine.AI;

public class Car_Interaction : Interactable
{
    private Car_HealthController carHealthController;
    private Car_Controller carController;
    private Transform player;
    private NavMeshObstacle carObstacle;

    private float defaultPlayerScale;

    [Header("Exit Points Settings")]
    [SerializeField] private Transform[] exitPoints;

    private void Start()
    {
        carHealthController = GetComponent<Car_HealthController>();
        carController = GetComponent<Car_Controller>();
        player = GameManager.instance.player.transform;
        carObstacle = GetComponent<NavMeshObstacle>();
    }

    #region Interaction Logic

    public override void Interact()
    {
        if (carHealthController.carBroken)
            return;

        base.Interact();
        EnterCar();
    }

    public override void Highlight(bool active)
    {
        if (carHealthController.carBroken)
            return;

        base.Highlight(active);
    }

    // Handle player entering the car
    private void EnterCar()
    {
        ControlsManager.instance.SwitchToCarControls();
        carController.ActivateCar(true);

        GameManager.instance.currentCar = carController;
        carHealthController.UpdateCarHealthUI();

        GameManager.instance.player.GetComponent<Player_Movement>().isInCar = true;

        defaultPlayerScale = player.localScale.x;
        player.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        player.transform.parent = transform;
        player.transform.localPosition = Vector3.up / 2;

        CameraManager.instance.ChangeToCarVirtualCamera();

        // Disable NavMeshObstacle when in car
        if (carObstacle != null)
        {
            carObstacle.enabled = false;
        }
    }

    // Handle player exiting the car
    public void ExitCar()
    {
        if (!carController.carActive)
            return;

        carController.ActivateCar(false);
        carController.DecelerateCar();

        GameManager.instance.currentCar = null;
        GameManager.instance.player.GetComponent<Player_Movement>().isInCar = false;

        player.parent = null;
        player.position = GetExitPoint();
        player.transform.localScale = new Vector3(defaultPlayerScale, defaultPlayerScale, defaultPlayerScale);

        ControlsManager.instance.SwitchToCharacterControls();
        Player_AimController aim = GameManager.instance.player.aim;

        CameraManager.instance.ChangeToPlayerVirtualCamera();

        var aimController = GameManager.instance.player.aim;
        if (aimController != null)
            aimController.allowCameraLook = true;

        // Enable NavMeshObstacle when out of car
        if (carObstacle != null)
        {
            carObstacle.enabled = true;
        }
    }

    // Get a valid exit point (not blocked)
    private Vector3 GetExitPoint()
    {
        foreach (var exitPoint in exitPoints)
        {
            var trigger = exitPoint.GetComponent<Door_ExitPoint>();
            if (trigger != null && !trigger.isBlocked)
                return exitPoint.position;
        }
        // Default exit if all are blocked
        return transform.position + transform.up * 2;
    }

    #endregion
}

