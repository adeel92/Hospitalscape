using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

namespace Arc
{
    public class UnityGameServices : MonoBehaviour
    {
        public static event Action OnInitializationSuccess;  

        private string environment = "production";

        async void Start()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);
                OnInitializationSuccess?.Invoke();
            }
            catch (Exception exception)
            {
                // An error occurred during services initialization.
                Debug.Log(exception.Message);
            }
        }
    }
}
