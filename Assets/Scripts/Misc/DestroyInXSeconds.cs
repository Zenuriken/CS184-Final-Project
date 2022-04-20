using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyInXSeconds : MonoBehaviour
{
   #region Editor Variables
   [SerializeField]
   [Tooltip("How long before this game object is destroyed.")]
   private float m_TimeToDestruction;
   #endregion

   #region Initialization
   private void Awake()
   {
      Destroy(gameObject, m_TimeToDestruction);
   }
   #endregion
}
