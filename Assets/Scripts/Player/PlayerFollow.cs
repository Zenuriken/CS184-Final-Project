using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("The player to follow")]
    private Transform m_PlayerTransform;

    [SerializeField]
    [Tooltip("The offset from the player's origin to the camera")]
    private Vector3 m_Offset;

    [SerializeField]
    [Tooltip("How quickly the player can rotate the camera to the left and the right")]
    private float m_RotationSpeed = 10;
    #endregion

    #region Main Updates
    private void LateUpdate() {
        Vector3 newPos = m_PlayerTransform.position + m_Offset;

        transform.position = Vector3.Slerp(transform.position, newPos, 1);

        float rotationAmount = m_RotationSpeed * Input.GetAxis("Mouse X");
        float rotationAmountY = m_RotationSpeed * Input.GetAxis("Mouse Y");

        transform.RotateAround(m_PlayerTransform.position, Vector3.up, rotationAmount);
        //transform.RotateAround(m_PlayerTransform.position, Vector3.left, rotationAmountY);
        transform.Rotate(rotationAmountY * Vector3.left, Space.Self);
        
        Vector3 currRotation = transform.localRotation.eulerAngles;
        float xRot = Mathf.Clamp(currRotation.x, -50f, 50f);
        //transform.localRotation = Quaternion.Euler(new Vector3(xRot, currRotation.y, currRotation.z));
        m_Offset = transform.position - m_PlayerTransform.position;


    }
    #endregion
}