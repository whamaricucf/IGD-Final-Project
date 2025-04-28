using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved at " + SavePath);
    }

    public static SaveData Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("Game Loaded.");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load save data. Creating new data. Error: {ex.Message}");
                return new SaveData();
            }
        }
        else
        {
            Debug.LogWarning("Save file not found, creating new data.");
            return new SaveData();
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }


    public static Dictionary<string, int> ConvertListToDict(List<KeyValue> list)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        foreach (var kv in list)
        {
            if (!dict.ContainsKey(kv.key))
                dict.Add(kv.key, kv.value);
        }
        return dict;
    }

    public static List<KeyValue> ConvertDictToList(Dictionary<string, int> dict)
    {
        List<KeyValue> list = new List<KeyValue>();
        foreach (var kv in dict)
        {
            list.Add(new KeyValue { key = kv.Key, value = kv.Value });
        }
        return list;
    }
}
