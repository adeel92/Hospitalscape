using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Isometric
{
    [RequireComponent(typeof(Collider2D))]
    public class HitBox2D : MonoBehaviour
    {
        private enum HitEvent { OnEnter, OnStay, OnExit }
        private enum HitType { OnTrigger, OnCollision}

        [Serializable]
        private class Interaction
        {
            public HitEvent InteractionEvent;
            public UnityEvent<GameObject> Callback;
        }

        [Header("---Parameters---")]
        [SerializeField] private HitType m_InteractionType;
        [SerializeField] private List<Interaction> m_Interactions;

        public event Action<GameObject> OnEnter;
        public event Action<GameObject> OnStay;
        public event Action<GameObject> OnExit;

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnEnter?.Invoke(other.gameObject);
            if (m_InteractionType == HitType.OnTrigger)
            {
                foreach (var interaction in m_Interactions)
                {
                    if(interaction.InteractionEvent == HitEvent.OnEnter)
                    {
                        interaction.Callback?.Invoke(other.gameObject);
                    }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            OnStay?.Invoke(other.gameObject);

            if (m_InteractionType == HitType.OnTrigger)
            {
                foreach (var interaction in m_Interactions)
                {
                    if (interaction.InteractionEvent == HitEvent.OnStay)
                    {
                        interaction.Callback?.Invoke(other.gameObject);
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            OnExit?.Invoke(other.gameObject);

            if (m_InteractionType == HitType.OnTrigger)
            {
                foreach (var interaction in m_Interactions)
                {
                    if (interaction.InteractionEvent == HitEvent.OnExit)
                    {
                        interaction.Callback?.Invoke(other.gameObject);
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnEnter?.Invoke(collision.gameObject);


            if (m_InteractionType == HitType.OnCollision)
            {
                foreach (var interaction in m_Interactions)
                {
                    if (interaction.InteractionEvent == HitEvent.OnEnter)
                    {
                        interaction.Callback?.Invoke(collision.gameObject);
                    }
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            OnStay?.Invoke(collision.gameObject);

            if (m_InteractionType == HitType.OnCollision)
            {
                foreach (var interaction in m_Interactions)
                {
                    if (interaction.InteractionEvent == HitEvent.OnStay)
                    {
                        interaction.Callback?.Invoke(collision.gameObject);
                    }
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            OnExit?.Invoke(collision.gameObject);

            if (m_InteractionType == HitType.OnCollision)
            {
                foreach (var interaction in m_Interactions)
                {
                    if (interaction.InteractionEvent == HitEvent.OnExit)
                    {
                        interaction.Callback?.Invoke(collision.gameObject);
                    }
                }
            }
        }

        
    }
}

