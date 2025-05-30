using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission Hunting Target", menuName = "Mission/Hunting Target")]
public class Mission_HuntingTarget : Mission
{
    public int numberOfTarget;
    private int remainingTargets;
    public EnemyType enemyType;

    public override void StartMission()
    {
        remainingTargets = numberOfTarget;
        UpdateMissionUI();

        // Register event for target killed
        MissionObject_Target.OnTargetKilled -= ReduceRemainingTargets;
        MissionObject_Target.OnTargetKilled += ReduceRemainingTargets;

        List<Enemy> validEnemies = new List<Enemy>();

        // Get the list of enemies from the LevelGenerator
        // Find all enemies of the specified type, and add the MissionObject_Target component to them
        foreach (Enemy enemy in LevelGenerator.instance.GetEnemyList())
        {
            if (enemy.enemyType == enemyType)
            {
                validEnemies.Add(enemy);
            }
        }

        foreach (Enemy enemy in validEnemies)
        {
            if (enemy.GetComponent<MissionObject_Target>() == null)
            {
                enemy.gameObject.AddComponent<MissionObject_Target>();
            }
        }

        Debug.Log($"Total valid enemies with type {enemyType}: {validEnemies.Count}");
    }

    public override bool MissionCompleted()
    {
        return remainingTargets <= 0;
    }


    // This method is called when a target is killed
    // It reduces the number of remaining targets and updates the UI
    private void ReduceRemainingTargets()
    {
        remainingTargets--;
        UpdateMissionUI();

        if (remainingTargets <= 0)
        {
            string missionText = "Target eliminated.";
            string missionDetails = "Now go to the airplane to complete the mission.";
            UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);

            MissionObject_Target.OnTargetKilled -= ReduceRemainingTargets;
            Debug.Log("Mission completed!");
        }
    }

    private void UpdateMissionUI()
    {
        string missionText = "Eliminate " + numberOfTarget + " " + enemyType.ToString() + " enemies";
        string missionDetails = "Remaining: " + remainingTargets;
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    public override MissionType GetMissionType()
    {
        return MissionType.HuntingTarget;
    }
}
