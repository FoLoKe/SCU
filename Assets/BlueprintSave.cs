using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintSave : MonoBehaviour
{
    public void Save() {
        BinaryFormatter bf = new BinaryFormatter(); 
        FileStream file = File.Create(Application.persistentDataPath 
            + "/MySaveData.dat"); 
        SaveData data = new SaveData();
        data.savedInt = 5;
        data.savedFloat = 25f;
        data.savedBool = true;
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game data saved!");
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath 
            + "/MySaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = 
            File.Open(Application.persistentDataPath 
            + "/MySaveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();
            int intToSave = data.savedInt;
            float floatToSave = data.savedFloat;
            bool boolToSave = data.savedBool;
            Debug.Log("Game data loaded!");
        }
        else
            Debug.LogError("There is no save data!");
    }
}

[Serializable]
class SaveData
{
  public int savedInt;
  public float savedFloat;
  public bool savedBool;
}
