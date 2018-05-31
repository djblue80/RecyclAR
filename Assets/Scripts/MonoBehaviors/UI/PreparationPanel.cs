using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI preparation phase panel control
/// </summary>
public class PreparationPanel : MonoBehaviour {

    /// <summary>
    /// UI display text for number of tracked anchors.
    /// </summary>
    [SerializeField] private Text trackableCntText;

    /// <summary>
    /// UI display GO for searching snackbar.
    /// </summary>
    [SerializeField] private GameObject snackBar;

    /// <summary>
    /// UI display components for progression to new level
    /// </summary>
    [SerializeField] private GameObject newLevelIntro;
    [SerializeField] private Text newLevelText;
    [SerializeField] private Text numAnchorRequiredText;

    /// <summary>
    /// Count down UI display GO
    /// </summary>
    [SerializeField] private GameObject countDown;

    /// <summary>
    /// Count down UI text
    /// </summary>
    [SerializeField] private Text countDownText;

    /// <summary>
    /// True if tracking is valid
    /// </summary>
    private bool m_IsPrepReady = false;

    /// <summary>
    /// Cached objects for coroutines
    /// </summary>
    private WaitForSeconds m_ReadyWait = new WaitForSeconds(3);
    private WaitForSeconds m_StartWait = new WaitForSeconds(1);
    private WaitForSeconds m_LevelIntroWait = new WaitForSeconds(3);

    /// <summary>
    /// True if this panel needs to show progression to new level
    /// </summary>
    private bool m_ShowNewLevel = false;

    /// <summary>
    /// Unity OnEnable function.
    /// </summary>
    private void OnEnable()
    {
        m_IsPrepReady = false;
        snackBar.SetActive(false);
        countDown.SetActive(false);
        m_ShowNewLevel = false;
        newLevelIntro.SetActive(false);

        if(GameManager.Instance != null && (GameManager.Instance.level > 1))
        {
            m_ShowNewLevel = true;
            StartCoroutine(_EnterNewLevel());
        }
    }

    /// <summary>
    /// Unity Update function.
    /// </summary>
    private void Update()
    {
        if (m_ShowNewLevel) return;
        if (m_IsPrepReady) return;

        int anchorCount = ARManager.Instance.CurrentTrackedCount;

        trackableCntText.text = anchorCount.ToString();
        snackBar.SetActive(!(anchorCount >= GameManager.Instance.numAnchorsRequired));
        if (anchorCount >= GameManager.Instance.numAnchorsRequired)
        {
            m_IsPrepReady = true;
            countDown.SetActive(true);
            StartCoroutine(_StartGame());
        }
    }

    /// <summary>
    /// Coroutine to countdown and start game
    /// </summary>
    /// <returns></returns>
    private IEnumerator _StartGame()
    {
        countDownText.text = "Ready...";
        yield return m_ReadyWait;
        countDownText.text = "Start!";
        yield return m_StartWait;
        GameManager.Instance.StartGame();
    }

    /// <summary>
    /// Coroutine to display new level UI.
    /// </summary>
    /// <returns></returns>
    private IEnumerator _EnterNewLevel()
    {
        newLevelIntro.SetActive(true);
        newLevelText.text = "Level " + GameManager.Instance.level;
        numAnchorRequiredText.text = "Waste Sources Required: " + GameManager.Instance.numAnchorsRequired;
        yield return m_LevelIntroWait;
        newLevelIntro.SetActive(false);
        m_ShowNewLevel = false;
    }

    
}
