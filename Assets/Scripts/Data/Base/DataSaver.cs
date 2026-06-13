using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Isometric.Data
{
    /// <summary>
    /// Use this class to save the data in xml
    /// </summary>
    public abstract class DataSaver : DataKeyed
    {
        abstract public void Setup();

        abstract public void SetDataToDefault();

        public void SaveData<T>(T data) where T : class
        {

            string filePath = GetFilePath(m_Key);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, data);
                    Debug.Log($"Data successfully saved to {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data: {ex.Message}");
            }
        }

        public T LoadData<T>() where T : class
        {
            string filePath = GetFilePath(m_Key);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return null;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    T data = (T)serializer.Deserialize(reader);
                    Debug.Log($"Data successfully loaded from {filePath}");
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
                return null;
            }
        }

        public bool FileExists()
        {
            string filePath = GetFilePath(m_Key);
            return File.Exists(filePath);
        }

        private string GetFilePath(string key)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "SavedData");
            return Path.Combine(folderPath, key + ".xml");
        }

        
    }

}
