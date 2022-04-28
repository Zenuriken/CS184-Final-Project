using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float RotationSpeed = 1;
    public Transform Target, Player;
    float mouseX, mouseY;
    
    public Transform Obstruction;
    float zoomSpeed = 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        // Set the default obstruction object to the player
        Obstruction = Target;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate() {
        CamControl();
        ViewObstructed();
    }

    void CamControl() {

        // Control left and right movement
        mouseX += Input.GetAxis("Mouse X") * RotationSpeed;
        // Control up and down camera movement
        mouseY -= Input.GetAxis("Mouse Y") * RotationSpeed;
        // Prevent the camera from flipping entirely around
        mouseY = Mathf.Clamp(mouseY, -35, 60);
        // Locks the camera to look at the player
        transform.LookAt(Target);
        // Rotates the camera based off of mouse movement
        Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);

        // if (Input.GetKey(KeyCode.LeftControl)) {
        //     Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        // } else {
        //     Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        //     Player.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        // }
    }

    void ViewObstructed() {
        RaycastHit hit;
        // If the raycast hits an object
        if (Physics.Raycast(transform.position, Target.position - transform.position, out hit, 2.0f)) {
            if (hit.collider.gameObject.tag != "Player") {
                Obstruction = hit.transform;
                // Hides the wall from view while also keeping the shadows casted by the wall
                Obstruction.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                // Checks if the camera should zoom into the wall/player
                if (Vector3.Distance(Obstruction.position, transform.position) >= 1.5f && Vector3.Distance(transform.position, Target.position) >= 1.0f) {
                    transform.Translate(Vector3.forward * zoomSpeed * Time.deltaTime);
                }

            } else if (Obstruction.gameObject.tag != "Player") {
                // Unhide the wall from view
                Obstruction.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                // Zoom the camera back to the normal position.
                if (Vector3.Distance(transform.position, Target.position) < 2.0f) {
                    transform.Translate(Vector3.back * zoomSpeed * Time.deltaTime);
                }
            }
        }
    }
}
