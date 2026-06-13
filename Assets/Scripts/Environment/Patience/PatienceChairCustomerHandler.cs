using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment
{
    public class PatienceChairCustomerHandler : MonoBehaviour
    {
        [SerializeField] PatienceChairController m_PatienceChairController;
        public PatienceChairController PatienceChairController => m_PatienceChairController;
    }
}
