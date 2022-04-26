using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("How fast the player should move when running around.")]
    private float m_Speed;

    [SerializeField]
    [Tooltip("The transform of the camera following the player")]
    private Transform m_CameraTransform;

    [SerializeField]
    [Tooltip("A list of all attacks and information about them")]
    private PlayerAttackInfo[] m_Attacks;

    [SerializeField]
    [Tooltip("Amount of health that the player starts with")]
    private int m_MaxHealth;

    [SerializeField]
    [Tooltip("The HUD script")]
    private HUDController m_HUD;

    #endregion

    #region Cached References
    private Animator cr_Anim;
    private Renderer cr_Renderer;
    #endregion

    #region Cached Components
    private Rigidbody cc_Rb;
    #endregion

    #region Private Variables
    // The current move direction of the player.. Does NOT include magnitude
    private Vector2 p_Velocity;

    private float velocityZ = 0.0f;
    private float velocityX = 0.0f;
    public float acceleration = 2.0f;
    public float deceleration = 2.0f;
    public float maximumWalkVelocity = 0.5f;
    public float maximumRunVelocity = 2.0f;

    // increase performance
    int VelocityZHash;
    int VelocityXHash;

    // In order to do anything, we cannot be frozen (timer must be 0)
    private float p_FrozenTimer;

    // The default color. Cached so we can switch between colors
    private Color p_DefaultColor;

    // Current amount of health that the player has
    private float p_CurHealth;
    #endregion

    #region Initialization
    private void Awake()
    {
        p_Velocity = Vector2.zero;
        cc_Rb = GetComponent<Rigidbody>();
        cr_Anim = GetComponent<Animator>();
        cr_Renderer = GetComponentInChildren<Renderer>();
        p_DefaultColor = cr_Renderer.material.color;

        VelocityZHash = Animator.StringToHash("Velocity Z");
        VelocityXHash = Animator.StringToHash("Velocity X");



        p_FrozenTimer = 0;
        p_CurHealth = m_MaxHealth;

        for (int i = 0; i < m_Attacks.Length; i++)
        {
            PlayerAttackInfo attack = m_Attacks[i];
            attack.Cooldown = 0;

            if (attack.WindUpTime > attack.FrozenTime)
            {
                Debug.LogError(attack.AttackName + " has a wind up time that is larger than the amount of time that the player is frozen for");
            }
        }
    }

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Main Updates
    private void Update()
    {
        // How long the player is frozen in space after using an attack
        if (p_FrozenTimer > 0)
        {
            p_Velocity = Vector2.zero;
            p_FrozenTimer -= Time.deltaTime;
            return;
        }
        else
        {
            p_FrozenTimer = 0;
        }

        // Ability use
        for (int i = 0; i < m_Attacks.Length; i++)
        {
            PlayerAttackInfo attack = m_Attacks[i];

            if (attack.IsReady())
            {
                if (Input.GetButtonDown(attack.Button))
                {
                    p_FrozenTimer = attack.FrozenTime;
                    DecreaseHealth(attack.HealthCost);
                    StartCoroutine(UseAttack(attack));
                    break;
                }
            }
            else if (attack.Cooldown > 0)
            {
                attack.Cooldown -= Time.deltaTime;
            }
        }

        // Input for movement
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        // Set current maxVelocity depending on if runPressed is true.
        float currentMaxVelocity = runPressed ? maximumRunVelocity : maximumWalkVelocity;

        // Set our animation variables
        cr_Anim.SetFloat(VelocityZHash, velocityZ);
        cr_Anim.SetFloat(VelocityXHash, velocityX);

        ChangeVelocity(forwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);
        LockOrResetVelocity(forwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);

        //    // Set how hard the player is pressing movement buttons
        //    float forward = Input.GetAxis("Vertical");
        //    float right = Input.GetAxis("Horizontal");

        //    // Updating the animation
        //    cr_Anim.SetFloat("Speed", Mathf.Clamp01(Mathf.Abs(forward) + Mathf.Abs(right)));

        //    // Updating velocity
        //    float moveThreshold = 0.3f;

        //    if (forward > 0 && forward < moveThreshold) {
        //        forward = 0;
        //    } else if (forward < 0 && forward > -moveThreshold) {
        //        forward = 0;
        //    }
        //    if (right > 0 && right < moveThreshold) {
        //        right = 0;
        //    }
        //    if (right < 0 && right > -moveThreshold) {
        //        right = 0;
        //    }
        //    p_Velocity.Set(right, forward);

        //Vector3 camForward = m_CameraTransform.forward;


        // Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
        // Vector3 right = -left;

        cc_Rb.velocity = new Vector3(velocityX * m_Speed, 0, velocityZ * m_Speed);
    }

    // Use for code involving physics because frame rate can varying system to system.
    private void FixedUpdate()
    {   
        // p_Velocity = new Vector2(velocityX, velocityZ);
        
        // // Update the rotation of the player
        // cc_Rb.angularVelocity = Vector3.zero;

        // if (p_Velocity.sqrMagnitude > 0) {
            

        //     // float angleToRotCam = Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, p_Velocity);
        //     // Vector3 camForward = m_CameraTransform.forward; // Returns a normalized vector of direction camera is facing.
        //     // Vector3 newRot = new Vector3(Mathf.Cos(angleToRotCam) * camForward.x - Mathf.Sin(angleToRotCam) * camForward.z, 0,
        //     //                 Mathf.Cos(angleToRotCam) * camForward.z + Mathf.Sin(angleToRotCam) * camForward.x);
        //     // float theta = Vector3.SignedAngle(transform.forward, newRot, Vector3.up);
        //     // cc_Rb.rotation = Quaternion.Slerp(cc_Rb.rotation, cc_Rb.rotation * Quaternion.Euler(0, theta, 0), 0.2f);

        //     //Update the position of the player
        //     //cc_Rb.MovePosition(cc_Rb.position + m_Speed * Time.fixedDeltaTime * transform.forward * p_Velocity.magnitude);
        // }
    }
    #endregion

    #region Movement
    // Changes the velocity based on player input
    private void ChangeVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity) {
         // If player presses forward, increase velocity in z-direction.
        if (forwardPressed && velocityZ < currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * acceleration;
        }

        // If player presses left, increase velocity in left direction.
        if (leftPressed && velocityX > -currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * acceleration;
        }

        // If player presses right, increase velocity in right direction.
        if (rightPressed && velocityX < currentMaxVelocity)
        {
            velocityX += Time.deltaTime * acceleration;
        }

        // Decreases velocityZ
        if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }

         // Increase velocityX if left is not pressed and velocityX < 0
        if (!leftPressed && velocityX < 0.0f) {
            velocityX += Time.deltaTime * deceleration;
        }

        // Decrease velocityX if right is not pressed and velocityX > 0
        if (!rightPressed && velocityX > 0.0f) {
            velocityX -= Time.deltaTime * deceleration;
        }
    }

    // Locks or resets velocity based on current velocity and inputs
    void LockOrResetVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity) {
        // Reset velocityZ
        if (!forwardPressed && velocityZ < 0.0f)
        {
            velocityZ = 0.0f;
        }

        // Reset velocityX
        if (!leftPressed && !rightPressed && velocityX != 0.0f && (velocityX > -0.05f && velocityX < 0.05f))
        {
            velocityX = 0.0f;
        }

        // Locks the running movement from exceeding the maximum velocity
        if (forwardPressed && runPressed && velocityZ > currentMaxVelocity) {
            velocityZ = currentMaxVelocity;
        // Decelerate to the maximum walk velocity
        } else if (forwardPressed && velocityZ > currentMaxVelocity) {
            velocityZ -= Time.deltaTime * deceleration;
            // Round to the currentMaxVelocity if within offset
            if (velocityZ > currentMaxVelocity && velocityZ < (currentMaxVelocity + 0.05)) {
                velocityZ = currentMaxVelocity;
            } 
        // round to the currentMaxVelocity if within offset
        } else if (forwardPressed && velocityZ < currentMaxVelocity && velocityZ > (currentMaxVelocity - 0.05f)) {
            velocityZ = currentMaxVelocity;
        }

         // Locking left movement
        if (leftPressed && runPressed && velocityX < -currentMaxVelocity) {
            velocityX = -currentMaxVelocity;
        // Decelerate to the maximum walk velocity
        } else if (leftPressed && velocityX < -currentMaxVelocity) {
            velocityX += Time.deltaTime * deceleration;
            // Round to the currentMaxVelocity if within offset
            if (velocityX < -currentMaxVelocity && velocityX > (-currentMaxVelocity - 0.05)) {
                velocityX = -currentMaxVelocity;
            } 
        // round to the currentMaxVelocity if within offset
        } else if (leftPressed && velocityX > -currentMaxVelocity && velocityX < (-currentMaxVelocity + 0.05f)) {
            velocityX = -currentMaxVelocity;
        }

        // Locks right movement
        if (rightPressed && runPressed && velocityZ > currentMaxVelocity) {
            velocityX = currentMaxVelocity;
        // Decelerate to the maximum walk velocity
        } else if (rightPressed && velocityX > currentMaxVelocity) {
            velocityX -= Time.deltaTime * deceleration;
            // Round to the currentMaxVelocity if within offset
            if (velocityX > currentMaxVelocity && velocityX < (currentMaxVelocity + 0.05)) {
                velocityX = currentMaxVelocity;
            } 
        // round to the currentMaxVelocity if within offset
        } else if (rightPressed && velocityX < currentMaxVelocity && velocityX > (currentMaxVelocity - 0.05f)) {
            velocityX = currentMaxVelocity;
        }
    }
    #endregion

    #region Health/Dying Methods
    public void DecreaseHealth(float amount)
    {
        p_CurHealth -= amount;
        m_HUD.UpdateHealth(1.0f * p_CurHealth / m_MaxHealth);
        if (p_CurHealth <= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void IncreaseHealth(int amount)
    {
        p_CurHealth += amount;
        if (p_CurHealth > m_MaxHealth)
        {
            p_CurHealth = m_MaxHealth;
        }
        m_HUD.UpdateHealth(1.0f * p_CurHealth / m_MaxHealth);
    }
    #endregion

    #region Attack Methods
    private IEnumerator UseAttack(PlayerAttackInfo attack)
    {
        // Set the player to the current direction of the camera
        cc_Rb.rotation = Quaternion.Euler(0, m_CameraTransform.eulerAngles.y, 0);
        // Call the animation for the attack
        cr_Anim.SetTrigger(attack.TriggerName);
        IEnumerator toColor = ChangeColor(attack.AbilityColor, 10);
        StartCoroutine(toColor);
        yield return new WaitForSeconds(attack.WindUpTime);

        Vector3 offset = transform.forward * attack.Offset.z + transform.right * attack.Offset.x + transform.up * attack.Offset.y;
        GameObject go = Instantiate(attack.AbilityGO, transform.position + offset, cc_Rb.rotation);
        go.GetComponent<Ability>().Use(transform.position + offset);

        StopCoroutine(toColor);
        StartCoroutine(ChangeColor(p_DefaultColor, 50));
        yield return new WaitForSeconds(attack.Cooldown);

        attack.ResetCooldown();
    }
    #endregion

    #region Misc Methods
    private IEnumerator ChangeColor(Color newColor, float speed)
    {
        Color curColor = cr_Renderer.material.color;
        while (curColor != newColor)
        {
            curColor = Color.Lerp(curColor, newColor, speed / 100);
            cr_Renderer.material.color = curColor;
            yield return null;
        }
    }
    #endregion

    #region Collision Methods
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HealthPill"))
        {
            IncreaseHealth(other.GetComponent<HealthPill>().HealthGain);
            Destroy(other.gameObject);
        }
    }
    #endregion
}
