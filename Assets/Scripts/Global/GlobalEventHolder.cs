using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Isometric.Data;
using Isometric.Tutorial;

namespace Isometric
{
    public class GlobalEventHolder
    {
        //---Currency---
        public static Action<int> OnCoinCurrencyUpdate;
        public static Action<int> OnGemCurrencyUpdate;
        public static Action<int> OnStarCurrencyUpdate;
        public static Action<int> OnKeyCurrencyUpdate;
        public static Action<int> OnHeartCurrencyUpdate;
        public static Action<double> OnHeartTimeCurrencyUpdate;

        //---Gameplay---
        public static Action<int> OnCoinsCollected;

        public static Action OnCustomerEntered;
        public static Action OnCustomerServed;
        public static Action OnCustomerLost;

        //Sends current value and the target value
        public static Action<int, int> OnTargetValueUpdate;
        public static Action<int, int> OnTimeConstraintValueUpdate;
        public static Action<int> OnCustomerConstraintValueUpdate;

        public static Action OnGameplayPause;
        public static Action OnGameplayUnause;

        public static Action OnGameWon;
        public static Action OnGameLost;

        //---Workers---
        public static Action<DataConsumable> OnAllWorkerBusy;
        public static Action<DataConsumable> OnNotAllWorkerBusy;

        //---Booster---
        //sends on or off value
        public static Action<bool> OnTimeFrozeBooster;
        //sends on or off value
        public static Action<bool> OnWaitressSpeedBooster;
        public static Action OnInstanceOrderFillBooster;

        //---Patience---
        //sends on or off value
        public static Action<bool> OnPatienceSunRays;

        //---Shop---
        /// <summary>
        /// Bool is on menu
        /// </summary>
        public static Action<bool> OnOpenShop;

        //---Tutorial---
        public static Action<DataConsumable> OnHintByConsumable;
        public static Action<HintKey> OnHintByKey;
        //True for Freeze and False for UnFreeze
        public static Action<bool> OnCustomerWaitFreeze;

        //---Task---
        public static Action OnTaskAssigned;
        //Event to cancle the Current Task by the player or varient if exists
        public static Action OnCurrentTaskTargetCancle;
        public static Action OnCurrentTaskTargetCancleImmediately;

        //---Achievement---
        public static Action<CustomerId> OnNewCustomerUnlocked;
        public static Action OnLevelComplete;
        public static Action OnKeyCollected;
        public static Action<int> OnPlayerWalkDistance;
        public static Action OnHasNotLostACustomerOnLevel;
        public static Action OnCustomerSitHappy;
        public static Action OnCounterCustomerServedHappy;
        public static Action OnCustomerOrderServed;
        public static Action<int> OnNewCoinCurrencyAdded;
        public static Action<int> OnCoinCurrencySpendOnUpgrade;
        public static Action OnPlayerStartedMoving;
        public static Action OnPlayerStopMoving;
        public static Action OnWorkerServesOrder;
        public static Action OnACustomersWaitFrozen;

        //---Ads Purchase---
        public static Action OnNoAdsPurchaseSuccessful;
    }
}
