using UnityEngine;

[CreateAssetMenu(fileName = "Car Delivery Mission", menuName = "Mission/Car Delivery - Mission")]
public class Mission_CarDelivery : Mission
{
    private bool isCarDelivered;

    #region Unity Methods

    private void OnEnable()
    {
        isCarDelivered = false;
    }

    #endregion

    #region Mission Logic

    public override void StartMission()
    {
        if (isCarDelivered)
            return;

        // Activate delivery zone and set pathfinding target
        FindObjectOfType<MissionObject_CarDeliveryZone>(true).gameObject.SetActive(true);
        MissionObject_CarDeliveryZone deliveryZone = FindObjectOfType<MissionObject_CarDeliveryZone>();
        if (deliveryZone != null)
        {
            PathfindingIndicator pathfindingIndicator = FindObjectOfType<PathfindingIndicator>();
            if (pathfindingIndicator != null)
            {
                pathfindingIndicator.SetTarget(deliveryZone.transform);
                Debug.Log("Mission_CarDelivery: Set PathfindingIndicator target to MissionObject_CarDeliveryZone.");
            }
            else
            {
                Debug.LogWarning("Mission_CarDelivery: PathfindingIndicator not found in scene!");
            }

            string missionText = "Find a functional car";
            string missionDetails = "Get to the car and drive it to the specified parking area";
            UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);

            // Register event for car delivery (unregistered in CompleteCarDelivery to prevent duplicate calls)
            MissionObject_Car.OnCarDelivery += CompleteCarDelivery;


            // Attach MissionObject_Car to all cars
            Car_Controller[] cars = Object.FindObjectsByType<Car_Controller>(FindObjectsSortMode.None);
            foreach (var car in cars)
            {
                if (car != null)
                {
                    car.gameObject.AddComponent<MissionObject_Car>();
                }
            }
        }
    }

    // Return true if the car is delivered
    public override bool MissionCompleted()
    {
        return isCarDelivered;
    }

    // Resets the mission state
    public void ResetMissionState()
    {
        isCarDelivered = false;
    }


    // Complete the mission when the car is delivered
    private void CompleteCarDelivery()
    {
        isCarDelivered = true;
        MissionObject_Car.OnCarDelivery -= CompleteCarDelivery;

        string missionText = "Car delivered.";
        string missionDetails = "Now go to the airplane to complete the mission.";
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    public override MissionType GetMissionType()
    {
        return MissionType.CarDelivery;
    }

    #endregion
}
