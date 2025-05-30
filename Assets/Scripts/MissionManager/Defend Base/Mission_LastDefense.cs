using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Last Defence Mission", menuName = "Mission/Last Defence - Mission")]
public class Mission_LastDefense : Mission
{
    public bool isDefenceStarted = false;
    public bool isMissionCompleted = false;

    [Header("Mission Details")]
    public float defenseDuration = 120;
    public float timeBetweenWaves = 15;

    private float defenseTimer;
    private float waveTimer;

    [Header("Respawn Details")]
    public int numberOfRespawnPoints = 2;
    public List<Transform> respawnPoints;
    private Vector3 defensePoint;

    [Space]
    public int numberOfEnemiesPerWave;
    public GameObject[] enemyPrefabs;
    private string defenceTimerText;

    #region Unity Methods

    private void OnEnable()
    {
        isDefenceStarted = false;
        isMissionCompleted = false;
    }

    #endregion

    #region Mission Logic

    public override void StartMission()
    {
        if (isMissionCompleted)
            return;

        // Set defense point and pathfinding target
        MissionObject_BaseToDefend baseToDefend = FindObjectOfType<MissionObject_BaseToDefend>();
        if (baseToDefend != null)
        {
            defensePoint = baseToDefend.transform.position;
            PathfindingIndicator pathfindingIndicator = FindObjectOfType<PathfindingIndicator>();
            if (pathfindingIndicator != null)
            {
                pathfindingIndicator.SetTarget(baseToDefend.transform);
            }
        }
        else
        {
            Debug.LogWarning("Mission_LastDefense: MissionObject_BaseToDefend not found in scene!");
        }

        respawnPoints = new List<Transform>(ClosestPoints(numberOfRespawnPoints));

        string missionText = "Head to the enemy main base to activate the virus.";
        string missionDetails = "Tip: There are ammo boxes and powerful guns in nearby abandoned town.";
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    public override bool MissionCompleted()
    {
        return isMissionCompleted;
    }

    // Resets the mission state
    public void ResetMissionState()
    {
        isDefenceStarted = false;
        isMissionCompleted = false;
        defenseTimer = 0f;
        waveTimer = 0f;
        respawnPoints = null;
        defenceTimerText = "";
    }


    public override void UpdateMission()
    {
        if (!isDefenceStarted || isMissionCompleted) return;
        waveTimer -= Time.deltaTime;

        if (defenseTimer > 0)
            defenseTimer -= Time.deltaTime;

        if (defenseTimer <= 0)
        {
            EndDefenseEvent();
            return;
        }

        // Spawn enemies every timeBetweenWaves seconds (new wave)
        if (waveTimer < 0)
        {
            CreateNewEnemies(numberOfEnemiesPerWave);
            waveTimer = timeBetweenWaves;
        }

        defenceTimerText = System.TimeSpan.FromSeconds(defenseTimer).ToString("mm':'ss");
        string missionText = "Activating. Please standby...";
        string missionDetails = "Time Left: " + defenceTimerText;
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    public void StartDefenseEvent()
    {
        waveTimer = 0.5f;
        defenseTimer = defenseDuration;
        isDefenceStarted = true;
    }

    private void EndDefenseEvent()
    {
        isDefenceStarted = false;
        HealthController.muteDeathSound = true;

        // Destroy all enemies in the scene
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in allEnemies)
        {
            HealthController healthController = enemy.GetComponent<HealthController>();
            if (healthController != null && !healthController.isDead)
            {
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null && agent.enabled)
                {
                    healthController.SetHealthToZero();
                }
            }
        }
        isMissionCompleted = true;
        HealthController.muteDeathSound = false;

        string missionText = "The virus has been installed.";
        string missionDetails = "Now go to the airplane to complete the mission.";
        UI.instance.inGameUI.UpdateMissionUI(missionText, missionDetails);
    }

    // Spawns enemies at respawn points
    private void CreateNewEnemies(int number)
    {
        if (number != respawnPoints.Count)
            return;

        for (int i = 0; i < respawnPoints.Count; i++)
        {
            Transform spawnPoint = respawnPoints[i];
            GameObject enemyPrefab = enemyPrefabs[i % enemyPrefabs.Length];
            GameObject spawnedEnemy = ObjectPool.instance.GetObject(enemyPrefab, spawnPoint);
            Enemy enemyComponent = spawnedEnemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.arrgresssionRange = 100;
            }
        }
    }

    // Finds the closest respawn points to the defense point
    // Because maybe there are more than required spawn points in the map
    private List<Transform> ClosestPoints(int number)
    {
        List<Transform> closestPoints = new List<Transform>();
        List<MissionObject_EnemyRespawnPoint> allPoints =
            new List<MissionObject_EnemyRespawnPoint>(FindObjectsOfType<MissionObject_EnemyRespawnPoint>());

        while (closestPoints.Count < number && allPoints.Count > 0)
        {
            float shortestDistance = float.MaxValue;
            MissionObject_EnemyRespawnPoint closestPoint = null;

            foreach (var point in allPoints)
            {
                float distance = Vector3.Distance(defensePoint, point.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPoint = point;
                }
            }

            if (closestPoint != null)
            {
                closestPoints.Add(closestPoint.transform);
                allPoints.Remove(closestPoint);
            }
        }

        return closestPoints;
    }

    public override MissionType GetMissionType()
    {
        return MissionType.LastDefense;
    }

    #endregion
}
