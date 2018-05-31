using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Strings that serve as IDs for UI panels
/// </summary>
public class UIPanelIDs
{
    public static string MainMenu = "MainMenu";
    public static string ResultsPanel = "ResultsPanel";
    public static string PreparationPanel = "PreparationPanel";
    public static string InPlayPanel = "InPlayPanel";
}

/// <summary>
/// Manages the game's UI system.
/// </summary>
[DisallowMultipleComponent]
public class UIManager : MonoBehaviour {

    /// <summary>
    /// UI Panel catalog will be filled with children of this canvas object.
    /// </summary>
    public Transform UICanvas;

    /// <summary>
    /// Catalog of UI panels with key as their in-scene GameObject names and value as the GameObject reference.
    /// </summary>
    public Dictionary<string, GameObject> UICatalog = new Dictionary<string, GameObject>();

    /// <summary>
    /// Sets one panel active while hiding the rest of the panels in UICatalog.
    /// </summary>
    /// <param name="targetName">Target panel name.</param>
    public void SetPanelActive(string targetName)
    {
        foreach(var entry in UICatalog)
        {
            entry.Value.SetActive(entry.Key == targetName);
        }
    }

    /// <summary>
    /// UI function to start the game.
    /// </summary>
    public void StartGame()
    {
        GameManager.Instance.CurrentState = GameManager.AppState.Preparation;
        //GameManager.Instance.StartGame();
    }

    /// <summary>
    /// UI function to return to menu state.
    /// </summary>
    public void ReturnToMenu()
    {
        GameManager.Instance.ResetGame();
        GameManager.Instance.CurrentState = GameManager.AppState.Menu;
    }

    /// <summary>
    /// UI function to restart the game.
    /// </summary>
    public void RestartGame()
    {
        GameManager.Instance.ResetGame();
        StartGame();
    }

    /// <summary>
    /// Unity Awake function.
    /// </summary>
    private void Awake()
    {
        foreach (Transform panel in UICanvas)
        {
            UICatalog.Add(panel.name, panel.gameObject);
        }

        string defaultUI = UIPanelIDs.MainMenu;
        SetPanelActive(defaultUI);
    }

    /// <summary>
    /// Unity Start function.
    /// </summary>
    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    /// <summary>
    /// Callback function to GameManager's OnGameStateChanged event.
    /// </summary>
    private void OnGameStateChanged(GameManager.AppState state)
    {
        SetPanelActive(UIPanelIDs.InPlayPanel);
        switch (state)
        {
            case GameManager.AppState.Menu:
                SetPanelActive(UIPanelIDs.MainMenu);
                break;
            case GameManager.AppState.Preparation:
                SetPanelActive(UIPanelIDs.PreparationPanel);
                break;
            case GameManager.AppState.InPlay:
                SetPanelActive(UIPanelIDs.InPlayPanel);
                break;
            case GameManager.AppState.Results:
                SetPanelActive(UIPanelIDs.ResultsPanel);
                break;
        }
    }
}
