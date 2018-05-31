using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI control for Results Panel
/// </summary>
public class ResultsPanel : MonoBehaviour {

    [SerializeField] private Text scoreText;

    private void OnEnable()
    {
        scoreText.text = GameManager.Instance.Score.ToString();
    }
}
