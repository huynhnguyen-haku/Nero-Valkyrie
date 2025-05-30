using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;
    public Mission currentMission;
    private bool isMissionActive = false;
    private bool hasSetFinalTarget = false; // Prevents setting target multiple times

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Update mission logic if active
        if (isMissionActive)
            currentMission?.UpdateMission();

        // Set pathfinding target to mission complete zone after mission completion
        PathfindingIndicator pathfindingIndicator = FindObjectOfType<PathfindingIndicator>();
        if (pathfindingIndicator != null && currentMission != null && currentMission.MissionCompleted() && !hasSetFinalTarget)
        {
            GameObject missionCompleteZone = GameObject.Find("MissionComplete_Zone");
            if (missionCompleteZone != null)
            {
                pathfindingIndicator.SetTarget(missionCompleteZone.transform);
                hasSetFinalTarget = true;
                Debug.Log("MissionManager: Set PathfindingIndicator target to MissionComplete_Zone after mission completion.");
            }
        }
    }

    #endregion

    #region Mission Logic

    // Set the current mission and reset state
    public void SetCurrentMission(Mission newMission)
    {
        currentMission = newMission;
        hasSetFinalTarget = false;

        // Specific reset for LastDefense mission
        if (currentMission is Mission_LastDefense lastDefenseMission)
        {
            lastDefenseMission.ResetMissionState();
        }

        // Specific reset for CarDelivery mission
        if (currentMission is Mission_CarDelivery carDeliveryMission)
        {
            carDeliveryMission.ResetMissionState();
        }
    }


    // Start the current mission
    public void StartMission()
    {
        isMissionActive = true;
        currentMission.StartMission();
    }

    // Check if the mission is completed and handle reward
    public bool MissionCompleted()
    {
        if (currentMission != null && currentMission.MissionCompleted())
        {
            GameManager.instance.AddMoney(currentMission.reward);
            Debug.Log($"Mission '{currentMission.missionName}' completed! Reward: {currentMission.reward} golds.");
            return true;
        }
        return false;
    }

    // Reset mission state after completion
    public void ResetAfterCompletion()
    {
        currentMission = null;
        isMissionActive = false;
        hasSetFinalTarget = false;
        Debug.Log("MissionManager: Reset after mission completion.");
    }

    #endregion
}

