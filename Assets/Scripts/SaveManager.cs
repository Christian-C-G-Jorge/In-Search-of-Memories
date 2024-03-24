using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public GameObject playerObject;

    void Start()
    {
        Debug.Log(Application.persistentDataPath);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Save();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
    }

    private void Save()
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + "SaveTest.dat", FileMode.OpenOrCreate);
            SaveData data = new SaveData();
            SavePlayerPosition(data); // Save player's position
            bf.Serialize(file, data);
            file.Close();
            Debug.Log("Game saved.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving game: " + ex.Message);
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + "SaveTest.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/" + "SaveTest.dat", FileMode.Open);
                SaveData data = (SaveData)bf.Deserialize(file);
                file.Close();
                LoadPlayerPosition(data); // Load player's position
                Debug.Log("Game loaded.");
            }
            else
            {
                Debug.LogWarning("No save file found.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error loading game: " + ex.Message);
        }
    }

    private void SavePlayerPosition(SaveData data)
    {
        if (playerObject != null)
        {
            data.playerPosition = new SerializableVector3(playerObject.transform.position);
        }
        else
        {
            Debug.LogWarning("Player GameObject reference is null. Cannot save player position.");
        }
    }

    private void LoadPlayerPosition(SaveData data)
{
    if (playerObject != null)
    {
        Vector3 loadedPosition = data.playerPosition.ToVector3();
        Debug.Log("Loaded player position: " + loadedPosition);
        playerObject.transform.position = loadedPosition;
        Debug.Log("Player moved to loaded position.");
    }
    else
    {
        Debug.LogWarning("Player GameObject reference is null. Cannot load player position.");
    }
}


    [System.Serializable]
    public class SaveData
    {
        public SerializableVector3 playerPosition;
    }

    [System.Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
