using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the AR game.
/// 1.Keeps track of Global game states
/// 2.Transitioning between global game states
/// 3.Sets up game level
/// 4.Adjusts difficulty
/// 5.Keeps track of score
/// </summary>
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour {

    /// <summary>
    /// List of possible Application States.
    /// </summary>
    public enum AppState { Menu, Preparation, InPlay, Results }

    /// <summary>
    /// Singleton reference to GameManager.
    /// </summary>
    private static GameManager m_Instance;
    public static GameManager Instance
    {
        get
        {
            if (!m_Instance)
            {
                GameObject go = GameObject.Find("GameManager");
                if (!go)
                {
                    Debug.LogError("Cannot find GameManager");
                    return null;
                }
                return go.GetComponent<GameManager>();
            }
            return m_Instance;
        }

        set
        {
            m_Instance = value;
        }
    }
    public delegate void AppStateChangeDelegate(AppState state);
    public event AppStateChangeDelegate OnGameStateChanged;

    public Transform playerTransform;
    private Transform playerColliderTransform;

    /// <summary>
    /// Current state of the game. 
    /// Game starts at Menu state.
    /// </summary>
    private AppState m_CurrentState = AppState.Menu;
    public AppState CurrentState
    {
        get { return m_CurrentState;  }
        set
        {
            m_CurrentState = value;
            if (OnGameStateChanged != null) OnGameStateChanged(m_CurrentState);
        }
    }

    /// <summary>
    /// Current level of difficulty of gameplay. 
    /// Default value set to 1 to make convenient for UI display.
    /// </summary>
    [HideInInspector] public int level = 1;

    /// <summary>
    /// 
    /// </summary>
    private SpawnedItem[] m_SpawnedItemDataCatalog;

    /// <summary>
    /// Current pool of spawnable objects based on level
    /// </summary>
    public List<GameObject> currentSpawnPool = new List<GameObject>();

    /// <summary>
    /// Item Spawn frequency control variables. Scales based on level.
    /// </summary>
    public const float MinSpawnWait = 1f;
    public const float MaxSpawnWait = 5f;
    public const int MaxSpawnWaitLvl = 10;
    public float currentMaxSpawnWait;

    /// <summary>
    /// Item movement speed limit factor
    /// </summary>
    public const float MinItemMoveSpeed = 0.1f;
    public const float MaxItemMoveSpeed = 1f;
    public const int MaxSpeedLevel = 8;

    /// <summary>
    /// A scale factor that is applied to all GameObjects/Prefabs with a unit scale (Vector3.one)
    /// </summary>
    public static float UnitScaleMultiplier = 0.3f;

    /// <summary>
    /// Instantiated at the center of a detected plane
    /// </summary>
    [SerializeField] private GameObject itemSpawnerPrefab;

    /// <summary>
    /// Parent transform for all GameObjects that spawns in a game session.
    /// </summary>
    [HideInInspector] public Transform inGameObjectRoot;

    /// <summary>
    /// Catalog of all SpawnedItems' meshes. Useful for randomizing appearance for each type of waste
    /// </summary>
    public Dictionary<SpawnedItem.SpawnType, GameObject[]> spawnedItemModelCatalog = new Dictionary<SpawnedItem.SpawnType, GameObject[]>();

    /// <summary>
    /// Score variables and events
    /// </summary>
    private long m_Score;
    public long Score { get { return m_Score; } }
    public delegate void ScoreUpdateDelegate(long newScore);
    public event ScoreUpdateDelegate OnScoreUpdated;

    /// <summary>
    /// Current Score threshold required to pass to next level
    /// </summary>
    private long m_ScoreThreshold;

    /// <summary>
    /// Default Base score threshold required to pass to next level
    /// </summary>
    private const long k_BaseScoreThreshold = 5;

    /// <summary>
    /// Number of anchors required for AR gameplay is based on current level
    /// </summary>
    public int numAnchorsRequired = DefaultNumAnchorsRequired;
    public const int DefaultNumAnchorsRequired = 1;

    /// <summary>
    /// Anchor Increment Factor is used to calculate the next level where number of spawnable anchors will increase
    /// </summary>
    private const int k_DefaultAnchorIncFactor = 2;
    int anchorIncFactor = k_DefaultAnchorIncFactor;
    int nextAnchorIncLevel;

    /// <summary>
    /// Initialize all game data to start the game. (start level 1)
    /// </summary>
    public void StartGame()
    {
        UpdateSpawnPool();
        UpdateSpawnAnchors();
        UpdateScoreThreshold();
        if (level >= MaxSpawnWaitLvl) currentMaxSpawnWait = MinSpawnWait;
        else currentMaxSpawnWait = ((float)level).Remap(1, MaxSpawnWaitLvl, MaxSpawnWait, MinSpawnWait);
        CurrentState = AppState.InPlay;
    }

    /// <summary>
    /// Update attributes for next level.
    /// </summary>
    public void ProceedToNextLevel()
    {
        level++;
        ClearInGameObjects();
        UpdateNumAnchorsRequired();
        CurrentState = AppState.Preparation;
    }

    /// <summary>
    /// Resets all game-related data.
    /// </summary>
    public void ResetGame()
    {
        level = 1;
        m_Score = 0;
        m_ScoreThreshold = 0;
        anchorIncFactor = k_DefaultAnchorIncFactor;
        nextAnchorIncLevel = level + anchorIncFactor;
        numAnchorsRequired = DefaultNumAnchorsRequired;
        currentSpawnPool.Clear();
    }

    /// <summary>
    /// Transition to Results state.
    /// </summary>
    public void LoseGame()
    {
        ClearInGameObjects();
        SettingsManager.Instance.AddHighscore(m_Score);
        CurrentState = AppState.Results;
    }

    /// <summary>
    /// Score threshold is calculated based on the current level, the base threshold, and the number of anchors that exist
    /// Assume X = baseScoreThreshold*level, X is assigned to the first anchor, then 80% of X is assigned to the subsequent anchors
    /// </summary>
    private void UpdateScoreThreshold()
    {
        m_ScoreThreshold = m_Score + (long)((k_BaseScoreThreshold * level) * ((numAnchorsRequired-1)*0.8f + 1));
    }

    /// <summary>
    /// Unity Awake function.
    /// </summary>
    private void Awake()
    {
        m_Instance = this;
        inGameObjectRoot = new GameObject("InGameObjectRoot").transform;
        LoadResources();
        playerColliderTransform = playerTransform.GetComponentInChildren<Collider>().transform;
        playerColliderTransform.localScale = Vector3.one * UnitScaleMultiplier;
    }

    /// <summary>
    /// Unity Start function.
    /// </summary>
    private void Start()
    {
        //Optimized experience for mobile devices
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        nextAnchorIncLevel = level + anchorIncFactor;
    }

    /// <summary>
    /// Loads game data from Resources folder into memory
    /// </summary>
    private void LoadResources()
    {
        m_SpawnedItemDataCatalog = Resources.LoadAll<SpawnedItem>(ResourcesDirectoryNames.SpawnedItemData);
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.ElectronicWaste, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.ElectronicWasteModels));
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.PlasticWaste, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.PlasticWasteModels));
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.MetalWaste, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.MetalWasteModels));
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.OrganicWaste, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.OrganicWasteModels));
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.HazardousWaste, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.HazardousWasteModels));
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.InertWaste, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.InertWasteModels));
        spawnedItemModelCatalog.Add(SpawnedItem.SpawnType.Animal, Resources.LoadAll<GameObject>(ResourcesDirectoryNames.AnimalModels));
        foreach(var entry in spawnedItemModelCatalog)
        {
            if(entry.Value.Length == 0)
            {
                Debug.Log(entry.Key.ToString() + " has no models");
            }
        }
    }

    /// <summary>
    /// Updates the pool of spawnable objects
    /// </summary>
    private void UpdateSpawnPool()
    {
        currentSpawnPool.Clear();
        foreach(SpawnedItem item in m_SpawnedItemDataCatalog)
        {
            if (item.minLevel <= level) currentSpawnPool.Add(item.prefab);
        }
        Debug.Log("Current spawn pool size: " + currentSpawnPool.Count);
    }

    /// <summary>
    /// TODO: shuffle the anchors  and randomly select a required number of anchors to spawn wastes.
    /// </summary>
    private void UpdateSpawnAnchors()
    {
        for (int i = 0; i < ARManager.Instance.anchors.Count; ++i)
        {
            //check if enough anchors are spawnable
            if ((i+1) > numAnchorsRequired) break;
            Instantiate(itemSpawnerPrefab,
                ARManager.Instance.anchors[i].CenterPose.position,
                ARManager.Instance.anchors[i].CenterPose.rotation, inGameObjectRoot);
        }
    }

    /// <summary>
    /// Cleans up all objects instantiated during a gameplay session.
    /// </summary>
    private void ClearInGameObjects()
    {
        //for (int i = 0; i < m_CurrentItemSpawners.Count; i++)
        //    Destroy(m_CurrentItemSpawners[i]);
        //m_CurrentItemSpawners.Clear();
        inGameObjectRoot.DestroyAllChildren();
    }

    /// <summary>
    /// Increments score based on waste type
    /// </summary>
    public void AddScore(SpawnedItem.SpawnType spawnType)
    {
        int incAmt = 0;
        switch (spawnType)
        {
            case SpawnedItem.SpawnType.PlasticWaste:
                incAmt += 1;
                break;
            case SpawnedItem.SpawnType.OrganicWaste:
                incAmt += 1;
                break;
            case SpawnedItem.SpawnType.MetalWaste:
                incAmt += 1;
                break;
            case SpawnedItem.SpawnType.ElectronicWaste:
                incAmt += 1;
                break;
            case SpawnedItem.SpawnType.HazardousWaste:
                incAmt += 2;
                break;
            case SpawnedItem.SpawnType.Animal:
                incAmt += 3;
                break;
        }
        m_Score += incAmt;
        if (OnScoreUpdated != null) OnScoreUpdated(m_Score);
        if (m_Score >= m_ScoreThreshold) ProceedToNextLevel();
    }

    /// <summary>
    /// Increasing the number of anchors required in order to increase game difficulty
    /// </summary>
    public void UpdateNumAnchorsRequired()
    {
        if (level < nextAnchorIncLevel) return;
        ++anchorIncFactor;
        ++numAnchorsRequired;
        nextAnchorIncLevel = level + anchorIncFactor;
    }
}
