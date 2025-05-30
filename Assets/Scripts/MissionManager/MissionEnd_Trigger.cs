using UnityEngine;

public class MissionEnd_Trigger : MonoBehaviour
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

        // If current mission is Mission_Timer, mark as completed
        Mission_Timer timerMission = MissionManager.instance.currentMission as Mission_Timer;
        if (timerMission != null)
        {
            timerMission.MarkAsCompleted();
        }

        // If mission is completed, trigger game completion and reset
        if (MissionManager.instance.MissionCompleted())
        {
            GameManager.instance.CompleteGame();
            Debug.Log("Level completed!");
            MissionManager.instance.ResetAfterCompletion();
        }
    }
}

