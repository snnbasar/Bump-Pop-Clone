using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class SaveManager
{
    public static SaveData saveData;
    public static string savePath = Application.dataPath + "/save.json";
    public static void Save_SaveDataIntoFile()
    {
#if UNITY_EDITOR
        string file = JsonConvert.SerializeObject(saveData);
        File.WriteAllText(savePath, file);
#endif
    }
    public static void Load_SaveDataFromFile()
    {
#if UNITY_EDITOR
        string file = File.ReadAllText(savePath);
        SaveData data = JsonConvert.DeserializeObject<SaveData>(file);
        saveData = data;
        UpdatePlayerPrefs();
#endif
    }

    private static void UpdatePlayerPrefs()
    {
        foreach (var data in saveData.data)
        {
            PlayerPrefs.SetString(data.Key, data.Value);
        }
    }
    public static void SaveToDictionary<T, L>(T veriId, L veri, string saveName)
    {
        Dictionary<T, L> saveData = LoadDictionary<T, L>(saveName);
        if (saveData == null) saveData = new Dictionary<T, L>();

        if (saveData.ContainsKey(veriId))
            saveData[veriId] = veri;
        else
            saveData.Add(veriId, veri);

        string data = JsonConvert.SerializeObject(saveData);

        PlayerPrefs.SetString(saveName, data);
        SaveToSaveData(saveName, data);
    }
    public static L LoadFromDictionary<T, L>(T t, string saveName, L defaultValue = default)
    {
        L veri = defaultValue;
        string data = PlayerPrefs.GetString(saveName, "");
        if (data == "")
            return veri;

        Dictionary<T, L> dic = JsonConvert.DeserializeObject<Dictionary<T, L>>(data);
        if (dic.ContainsKey(t))
            veri = dic[t];
        return veri;

    }
    public static void SaveDictionary<T, L>(Dictionary<T, L> dic, string saveName)
    {
        Dictionary<T, L> saveData = dic;
        string data = JsonConvert.SerializeObject(saveData);

        PlayerPrefs.SetString(saveName, data);
        SaveToSaveData(saveName, data);
    }

    public static Dictionary<T, L> LoadDictionary<T, L>(string saveName)
    {
        string data = PlayerPrefs.GetString(saveName, "");
        if (data == "")
            return null;

        return JsonConvert.DeserializeObject<Dictionary<T, L>>(data);

    }

    public static void Save<T>(T veri, string saveName)
    {
        string save = JsonConvert.SerializeObject(veri);
        PlayerPrefs.SetString(saveName, save);
        SaveToSaveData(saveName, save);
    }

    public static T Load<T>(string saveName, T defaultValue = default)
    {
        string save = PlayerPrefs.GetString(saveName, "");
        if (save == "")
            return (T)defaultValue;
        return JsonConvert.DeserializeObject<T>(save);

    }

    public static void SaveToSaveData(string key, string value)
    {
        bool saveDataActive = true;
        if (!saveDataActive)
            return;
        if (saveData == null)
            saveData = new SaveData();
        saveData.Add(key, value);
    }
}

[System.Serializable]
public class SaveData
{
    public Dictionary<string, string> data;

    public SaveData()
    {
        data = new Dictionary<string, string>();
    }

    public void Add(string key, string value)
    {
        if (data.ContainsKey(key))
        {
            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }
    }
}