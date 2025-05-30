using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance;

    [SerializeField] private NavMeshSurface navMeshSurface;

    // Level parts
    [SerializeField] private List<Transform> levelParts;
    [SerializeField] private Transform lastLevelPart;
    private List<Transform> currentLevelParts;
    private List<Transform> generatedLevelParts = new List<Transform>();

    // Snap points
    [SerializeField] private SnapPoint nextSnapPoint;
    private SnapPoint defaultSnapPoint;

    // Generation state
    [Space]
    [SerializeField] private float generationCooldown;
    private bool generationOver = true;
    private float cooldownTimer;

    // Enemies
    private List<Enemy> enemyList;

    // Pathfinding Indicator
    private PathfindingIndicator pathfindingIndicator;

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        enemyList = new List<Enemy>();
        defaultSnapPoint = nextSnapPoint;
        pathfindingIndicator = FindObjectOfType<PathfindingIndicator>();
    }

    private void Update()
    {
        if (generationOver)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            if (currentLevelParts.Count > 0)
            {
                cooldownTimer = generationCooldown;
                GenerateNextLevelPart();
            }
            else if (!generationOver)
            {
                FinishGeneration();
            }
        }
    }

    #endregion

    #region Generation Logic

    // Start or restart level generation
    [ContextMenu("Restart Generation")]
    public void InitializeGeneration()
    {
        nextSnapPoint = defaultSnapPoint;
        generationOver = false;

        // Filter level parts by mission type
        MissionType currentMissionType = MissionManager.instance.currentMission.GetMissionType();
        currentLevelParts = new List<Transform>();

        foreach (Transform part in levelParts)
        {
            LevelPart levelPartScript = part.GetComponent<LevelPart>();
            if (levelPartScript != null && levelPartScript.missionTypes.Contains(currentMissionType))
            {
                currentLevelParts.Add(part);
            }
        }

        Debug.Log($"Filtered LevelParts for MissionType {currentMissionType}: {currentLevelParts.Count} parts found.");

        ClearGeneratedLevelParts();
    }

    // Destroy all generated parts and enemies
    private void ClearGeneratedLevelParts()
    {
        foreach (Enemy enemy in enemyList)
            Destroy(enemy.gameObject);

        foreach (Transform part in generatedLevelParts)
            Destroy(part.gameObject);

        generatedLevelParts = new List<Transform>();
        enemyList = new List<Enemy>();
    }

    // Finalize level, build navmesh, and activate enemies
    private void FinishGeneration()
    {
        generationOver = true;
        GenerateNextLevelPart();

        navMeshSurface.BuildNavMesh();

        foreach (Enemy enemy in enemyList)
        {
            enemy.transform.parent = null;
            enemy.gameObject.SetActive(true);
        }

        GameObject missionCompleteZone = GameObject.Find("MissionComplete_Zone");
        if (missionCompleteZone == null)
        {
            Debug.LogWarning("LevelGenerator: MissionComplete_Zone not found! Please ensure it's spawned in lastLevelPart.");
        }
        else if (pathfindingIndicator != null && MissionManager.instance.currentMission.GetMissionType() != MissionType.LastDefense)
        {
            pathfindingIndicator.SetTarget(missionCompleteZone.transform);
            Debug.Log("LevelGenerator: Set PathfindingIndicator target to MissionComplete_Zone.");
        }

        MissionManager.instance.StartMission();
    }

    // Instantiate and align the next level part
    [ContextMenu("Generate Next Level Part")]
    private void GenerateNextLevelPart()
    {
        Transform newPart = null;

        if (generationOver)
            newPart = Instantiate(lastLevelPart);
        else
            newPart = Instantiate(ChooseRandomPart());

        if (newPart == null)
            return;

        generatedLevelParts.Add(newPart);

        LevelPart levelPartScript = newPart.GetComponent<LevelPart>();
        levelPartScript.SnapAndAlignPartTo(nextSnapPoint);

        if (levelPartScript.IntersectionDetected())
        {
            InitializeGeneration();
            return;
        }

        nextSnapPoint = levelPartScript.GetExitPoint();
        enemyList.AddRange(levelPartScript.GetEnemies());
    }

    // Randomly select and remove a level part from the pool
    private Transform ChooseRandomPart()
    {
        if (currentLevelParts.Count == 0)
            return null;

        int randomIndex = Random.Range(0, currentLevelParts.Count);
        Transform chosenPart = currentLevelParts[randomIndex];
        currentLevelParts.RemoveAt(randomIndex);

        return chosenPart;
    }

    #endregion

    #region Enemy Access

    public Enemy GetRandomEnemy()
    {
        int randomIndex = Random.Range(0, enemyList.Count);
        return enemyList[randomIndex];
    }

    public List<Enemy> GetEnemyList() => enemyList;

    #endregion
}
