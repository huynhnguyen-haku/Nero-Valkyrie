using UnityEngine;

public abstract class Mission : ScriptableObject
{
    public string missionName;

    [TextArea]
    public string missionDescription;

    [TextArea]
    public string missionObjective;

    public int reward;

    [Header("Mission Preview")]
    public Sprite missionPreview;

    // Start the mission logic
    public abstract void StartMission();

    // Check if the mission is completed
    public abstract bool MissionCompleted();

    // Optional: update mission progress each frame
    public virtual void UpdateMission() { }

    // Get the mission type (for filtering level parts, etc.)
    public abstract MissionType GetMissionType();
}

