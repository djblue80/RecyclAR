using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Manages persistent game data and user preference settings.
/// </summary>
public class SettingsManager
{
    /// <summary>
    /// Singleton reference to SettingsManager.
    /// </summary>
    private static SettingsManager s_Instance;
    public static SettingsManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new SettingsManager();
            }
            return s_Instance;
        }

        set
        {
            s_Instance = value;
        }
    }

    /// <summary>
    /// Maximum number of highscores to be saved
    /// </summary>
    private const int maxNumHighscores = 10;


    public SettingsManager()
    {
        m_CurrentPlayerData = LoadSavedPlayerData();
        if (m_CurrentPlayerData == null) Save();
    }

    /// <summary>
    /// setting to show in-game guide during gameplay
    /// </summary>
    public bool ShowInGameGuide = true; 

    /// <summary>
    /// Saved player data loaded into memory
    /// </summary>
    private PlayerData m_CurrentPlayerData;

    public List<long> GetHighscores()
    {
        return m_CurrentPlayerData.highscores;
    }

    /// <summary>
    /// Checks if the new score is a new score.If so, add to the highscore record and save.
    /// </summary>
    /// <param name="newScore"></param>
    public void AddHighscore(long newScore)
    {
        List<long> highscores = m_CurrentPlayerData.highscores;
        if(highscores.Count == 0)
        {
            highscores.Add(newScore);
            SettingsManager.Instance.Save();
            return;
        }
        //make sure highscore list is less than maxNumHighScores
        while (highscores.Count >= maxNumHighscores)
        {
            if(highscores.Count == maxNumHighscores)
            {
                //check if the new score qualifies to enter the high score list
                if (highscores[maxNumHighscores - 1] > newScore) return;
            }
            highscores.RemoveAt(highscores.Count - 1);
        }
        highscores.Add(newScore);
        highscores.Sort();
        highscores.Reverse();
        SettingsManager.Instance.Save();
    }

    /// <summary>
    /// Serialize player data and save to a persistent data path.
    /// </summary>
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        if (m_CurrentPlayerData == null)
        {
            m_CurrentPlayerData = new PlayerData();
        }

        bf.Serialize(file, new SimplePlayerData(JsonUtility.ToJson(m_CurrentPlayerData)));
        file.Close();
    }

    /// <summary>
    /// Load previously saved data. 
    /// </summary>
    /// <returns></returns>
    public PlayerData LoadSavedPlayerData()
    {
        if (!File.Exists(Application.persistentDataPath + "/playerInfo.dat")) return null;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
        PlayerData data;

        //use try and catch to prevent any conflicts between versions of the saved data
        try
        {
            SimplePlayerData sData = (SimplePlayerData)bf.Deserialize(file);
            data = JsonUtility.FromJson<PlayerData>(sData.JSONData);
            file.Close();
        }
        catch (System.Exception)
        {
            file.Close();
            Save();
            data = m_CurrentPlayerData;
            Debug.LogWarning("Error loading data. Overwriting save file");
        }

        Debug.Log("Loaded data: " + JsonUtility.ToJson(data));
        //load data to script
        return data;
    }

}

/// <summary>
/// This is a simplified player data class that only holds the JSON form of PlayerData, this makes playerData a portable data and version proof
/// </summary>
[System.Serializable]
public class SimplePlayerData
{
    public string JSONData;

    public SimplePlayerData(string JSONString)
    {
        JSONData = JSONString;
    }
}

[System.Serializable]
public class PlayerData
{
    public List<long> highscores;

    public PlayerData()
    {
        highscores = new List<long>();
    }

    public static PlayerData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<PlayerData>(jsonString);
    }

    public string ConvertToJson()
    {
        return JsonUtility.ToJson(this);
    }
}


