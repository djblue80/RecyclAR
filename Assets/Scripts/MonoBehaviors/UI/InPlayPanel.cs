using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI InPlay Panel control
/// </summary>
public class InPlayPanel : MonoBehaviour {

    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject controlGuideObj;
    [SerializeField] private GameObject showGuideButton;

    public void SetActiveGuide(bool active)
    {
        controlGuideObj.SetActive(active);
        showGuideButton.SetActive(!active);
        SettingsManager.Instance.ShowInGameGuide = active;
    }


    private void OnEnable()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnScoreUpdated += OnGameScoreUpdated;
            scoreText.text = GameManager.Instance.Score.ToString();
        }
        SetActiveGuide(SettingsManager.Instance.ShowInGameGuide);
    }

    private void OnDisable()
    {
        if (GameManager.Instance) GameManager.Instance.OnScoreUpdated -= OnGameScoreUpdated;
    }

    private void OnGameScoreUpdated(long newScore)
    {
        scoreText.text = newScore.ToString();
    }
}
