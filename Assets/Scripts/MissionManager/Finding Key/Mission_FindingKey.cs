using UnityEngine;

[CreateAssetMenu(fileName = "New Key Find Mission", menuName = "Mission/Key Find Mission")]
public class Misson_KeyFind : Mission
{
    [SerializeField] private GameObject key;
    public bool isKeyFound;

    private void OnEnable()
    {
        isKeyFound = false;
    }

    public override void StartMission()
    {
        string missionText = "Find the enemy and retrieve the core.";
        string missionDetails = "Tip: The enemy who has core installed is much bigger than normal one.";
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);

        MissionObject_Key.OnKeyPickedUp += PickupKey;

        // Give the key to a random enemy
        // Also make the enemy bigger
        Enemy enemy = LevelGenerator.instance.GetRandomEnemy();
        enemy.GetComponent<Enemy_DropController>()?.GiveKey(key);
        enemy.MakeEnemyStronger();
    }

    public override bool MissionCompleted()
    {
        return isKeyFound;
    }

    private void PickupKey()
    {
        isKeyFound = true;
        MissionObject_Key.OnKeyPickedUp -= PickupKey;

        string missionText = "Core Found!";
        string missionDetails = "Now go to the airplane to complete the mission.";
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    public override MissionType GetMissionType()
    {
        return MissionType.FindingKey;
    }
}
