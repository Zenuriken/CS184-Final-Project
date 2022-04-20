using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LookAtObject : MonoBehaviour
{
   #region Editor Variables
   [SerializeField]
   [Tooltip("The transform of the object that this object will constantly look at.")]
   private Transform m_LookAtObject;
   #endregion

   #region Main Updates
   private void Update()
   {
      transform.LookAt(m_LookAtObject);
   }
   #endregion
}
