using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Main Menu Panel control
/// </summary>
public class MainMenuPanel : MonoBehaviour {

    [SerializeField] private Text highscoreListText;

    public void DisplayHighscore()
    {
        List<long> highscores = SettingsManager.Instance.GetHighscores();
        if (highscores.Count == 0)
            highscoreListText.text = "---------------------";
        else
        {
            string newText = "";
            for(int i = 0; i < highscores.Count; ++i)
            {
                newText += string.Format("{0}{1}",highscores[i],
                    (i != highscores.Count - 1) ? "\r\n" : "");
            }
            highscoreListText.text = newText;
        }

    }

	
}
