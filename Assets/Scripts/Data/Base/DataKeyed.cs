using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Isometric.Data
{
    /// <summary>
    /// Use this class to give the ScriptableObject a unique key
    /// </summary>
    public abstract class DataKeyed : ScriptableObject
    {
        public string Key => m_Key;
        [SerializeField, EnableIf(nameof(m_EditKey))]
        protected string m_Key;
        [SerializeField]
        protected bool m_EditKey = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            /*if (!IsKeyUnique(m_Key))
            {
                m_Key = "";
                Debug.LogWarning("Key is not unique");
            }*/
        }


        private bool IsKeyUnique(string key)
        {
            // Find all assets of type DataKeyed (including derived types)
            string[] guids = AssetDatabase.FindAssets($"t:DataKeyed");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DataKeyed obj = AssetDatabase.LoadAssetAtPath<DataKeyed>(path);

                if (obj != this && obj.m_Key == key)
                {
                    return false;
                }
            }

            return true;
        }

#endif
    }
}
