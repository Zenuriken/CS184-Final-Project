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
    [Tooltip("The angle offset of the player when facing forward in degrees.")]
    private float angleOffset;

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

    [SerializeField]
    [Tooltip("The number of shotgun pellets")]
    private int numberOfPellets;

    [SerializeField]
    [Tooltip("The spread limit")]
    private float spread;

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

    // Movement booleans
    private bool forwardPressed;
    private bool backwardPressed;
    private bool leftPressed;
    private bool rightPressed;
    private bool runPressed;

    // Velocity variables
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

    // Camera references
    private Vector3 camForward;
    private float camAngleFromZAxis;

    // Key Tracker
    private int keys; 

    // The animation for the shotgun
    private Animator shotgunAnim;

    // The Transform of the fire point of the shotgun
    private Transform firePoint;


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

        keys = 0;



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
        shotgunAnim = GameObject.FindGameObjectWithTag("Weapon").GetComponent<Animator>();
        firePoint = GameObject.FindGameObjectWithTag("FirePoint").GetComponent<Transform>();
    }
    #endregion

    #region Main Updates
    private void Update()
    {
        // How long the player is frozen in space after using an attack
        if (p_FrozenTimer > 0)
        {
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
                    //DecreaseHealth(attack.HealthCost);
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
        forwardPressed = Input.GetKey(KeyCode.W);
        backwardPressed = Input.GetKey(KeyCode.S);
        leftPressed = Input.GetKey(KeyCode.A);
        rightPressed = Input.GetKey(KeyCode.D);
        runPressed = Input.GetKey(KeyCode.LeftShift);

        // Set current maxVelocity depending on if runPressed is true.
        float currentMaxVelocity = 0;
        if (runPressed) {
            currentMaxVelocity = maximumRunVelocity;
        } else {
            currentMaxVelocity = maximumWalkVelocity;
        }

        // Set our animation variables
        cr_Anim.SetFloat(VelocityZHash, velocityZ);
        cr_Anim.SetFloat(VelocityXHash, velocityX);

        ChangeVelocity(forwardPressed, backwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);
        LockOrResetVelocity(forwardPressed, backwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);
    }

    // Use for code involving physics because frame rate can varying system to system.
    private void FixedUpdate()
    {   
        camForward = m_CameraTransform.forward;
        // If the player presses forward, then align the avatar's body to face the same direction as the camera + angleOffset. 
        if (forwardPressed || backwardPressed || leftPressed || rightPressed) {
            
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,(Camera.main.transform.eulerAngles.y) + angleOffset,0), 0.2f);

            // Get the angle of the camera from the z-axis, which will later be used to calculate the direction of the player's velocity
            //camForward = m_CameraTransform.forward;
            if (camForward.x < 0) {
                camAngleFromZAxis = Mathf.Deg2Rad * (-Vector3.SignedAngle(camForward, Vector3.forward, Vector3.forward));
            } else {
                camAngleFromZAxis = Mathf.Deg2Rad * (Vector3.SignedAngle(camForward, Vector3.forward, Vector3.forward));
            }
            //Debug.Log("Cam Angle from Z Axis: " + camAngleFromZAxis);
        }

        // Calculate the new player velocity based on camera direction if the player isn't currently frozen
        if (p_FrozenTimer == 0) {
            p_Velocity = new Vector2(velocityX * Mathf.Cos(camAngleFromZAxis) + velocityZ * Mathf.Sin(camAngleFromZAxis),
                             velocityX * -Mathf.Sin(camAngleFromZAxis) + velocityZ * Mathf.Cos(camAngleFromZAxis));
            Vector2 p_Velocity_norm = p_Velocity.normalized;
            cc_Rb.velocity = new Vector3(p_Velocity.x * m_Speed, 0, p_Velocity.y * m_Speed);
        } else {
            p_Velocity = Vector2.zero;
        }
    }
    #endregion

    #region Movement
    // Changes the velocity based on player input
    private void ChangeVelocity(bool forwardPressed, bool backwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity) {
        // If player presses forward, increase velocity in z-direction.
        if (forwardPressed && velocityZ < currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * acceleration;
        }

        // If player presses backward, decrease velocity in z-direction.
        if (backwardPressed && velocityZ > -currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * acceleration;
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

        // Decrease velocityZ if forward is not pressed and velocityZ > 0
        if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }

        // Increase velocityZ if backward is not pressed and velocityZ < 0
        if (!backwardPressed && velocityZ < 0.0f)
        {
            velocityZ += Time.deltaTime * deceleration;
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
    void LockOrResetVelocity(bool forwardPressed, bool backwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity) {
        // Reset velocityZ
        if (!forwardPressed && !backwardPressed && velocityZ != 0.0f && (velocityZ > -0.05f) && velocityZ < 0.05f)
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

        // Locking back movement
        if (backwardPressed && runPressed && velocityZ < -currentMaxVelocity) {
            velocityZ = -currentMaxVelocity;
        // Decelerate to the maximum walk velocity
        } else if (backwardPressed && velocityZ < -currentMaxVelocity) {
            velocityZ += Time.deltaTime * deceleration;
            // Round to the currentMaxVelocity if within offset
            if (velocityZ < -currentMaxVelocity && velocityZ > (-currentMaxVelocity - 0.05)) {
                velocityZ = -currentMaxVelocity;
            } 
        // round to the currentMaxVelocity if within offset
        } else if (backwardPressed && velocityZ > -currentMaxVelocity && velocityZ < (-currentMaxVelocity + 0.05f)) {
            velocityZ = -currentMaxVelocity;
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
        if (!forwardPressed && !backwardPressed) {
            cc_Rb.rotation = Quaternion.Euler(0, m_CameraTransform.eulerAngles.y + angleOffset, 0);
        }
        //cc_Rb.rotation = Quaternion.Euler(0, m_CameraTransform.eulerAngles.y + angleOffset, 0);
        // Call the animation for the attack
        cr_Anim.SetTrigger(attack.TriggerName);
        shotgunAnim.SetTrigger("Rotate");
        //IEnumerator toColor = ChangeColor(attack.AbilityColor, 10);
        //StartCoroutine(toColor);
        yield return new WaitForSeconds(attack.WindUpTime);

        //Vector3 offset = transform.forward * attack.Offset.z + transform.right * attack.Offset.x + transform.up * attack.Offset.y;

        for (int i = 0; i < numberOfPellets; i++) {
            // create a random left / right value
            Vector3 spreadAmount=new Vector3(0, Random.Range(-spread,spread), Random.Range(-spread,spread));
            // add it into the addForce
            //clone.AddForce((firepoint.up+spreadAmount) * bulletspeed, ForceMode2D.Impulse);
            GameObject go = Instantiate(attack.AbilityGO, firePoint.position, cc_Rb.rotation);
            Rigidbody bullet = go.GetComponent<Rigidbody>();
            bullet.AddForce((camForward + spreadAmount) * 20, ForceMode.Impulse);
        }

        //go.GetComponent<Ability>().Use(firePoint.position);

        //StopCoroutine(toColor);
        //StartCoroutine(ChangeColor(p_DefaultColor, 50));
        yield return new WaitForSeconds(attack.Cooldown);
        cr_Anim.ResetTrigger(attack.TriggerName);
        shotgunAnim.ResetTrigger("Rotate");
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

    #region Key Functions
    public void keys_increase()
    {
        keys++;
    }

    public int key_amount()
    {
        return keys;
    }

    public void key_decrease()
    {
        keys--;
    }
    #endregion
}
