using UnityEngine;

[CreateAssetMenu(fileName = "New Timer Mission", menuName = "Mission/Timer Mission")]
public class Mission_Timer : Mission
{
    public float time;
    private float currentTime;
    private bool isCompleted;

    private void OnEnable()
    {
        currentTime = time;
        isCompleted = false;
    }

    public override void StartMission()
    {
        currentTime = time;
        isCompleted = false;
        string missionText = "Get to the airplane before the time runs out.";
        string missionDetails = "Time Left: " + System.TimeSpan.FromSeconds(currentTime).ToString("mm':'ss");
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    // Continously update the mission UI with the remaining time
    // and check if the mission is completed (player reached the airplane)
    public override void UpdateMission()
    {
        if (isCompleted)
            return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            string timeText = System.TimeSpan.FromSeconds(currentTime).ToString("mm':'ss");
            string missionText = "Get to the airplane before the time runs out.";
            string missionDetails = "Time Left: " + timeText;
            UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
        }
        else
        {
            string missionText = "Time's up!";
            string missionDetails = "Mission failed. Return to main menu to try again.";
            UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
            GameManager.instance.GameOver();
        }
    }

    public override bool MissionCompleted()
    {
        return isCompleted && currentTime > 0;
    }

    // Called by MissionEnd_Trigger when player reaches the airplane
    public void MarkAsCompleted()
    {
        isCompleted = true;
        Debug.Log("Mission_Timer: Marked as completed.");
    }

    public override MissionType GetMissionType()
    {
        return MissionType.Default;
    }
}
