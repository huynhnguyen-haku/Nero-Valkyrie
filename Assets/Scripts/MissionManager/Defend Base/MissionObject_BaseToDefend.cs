using UnityEngine;

public class MissionObject_BaseToDefend : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player)
            return;

        // Start defense event if mission is LastDefense and not started
        var mission = MissionManager.instance.currentMission as Mission_LastDefense;
        if (mission != null && !mission.isDefenceStarted)
        {
            mission.StartDefenseEvent();
        }
    }
}
