using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("How much health this enemy has")]
    private int m_MaxHealth;

    [SerializeField]
    [Tooltip("How fast the enemy can move")]
    private float m_speed;

    [SerializeField]
    [Tooltip("Approximate amount of damage health per frame")]
    private float m_Damage;

    [SerializeField]
    [Tooltip("The explosion that occurs when this enemy dies")]
    private ParticleSystem m_DeathExplosion;

    [SerializeField]
    [Tooltip("The probability that that this enemy drops a health pill")]
    private float m_HealthPillDropRate;

    [SerializeField]
    [Tooltip("The type of health pill this enemy drops")]
    private GameObject m_HealthPill;

    [SerializeField]
    [Tooltip("How many points killing this enemy provides")]
    private int m_Score;

    [SerializeField]
    [Tooltip("The angle offset of the enemy to the player in degrees")]
    private int angleOffset;
    #endregion
    
    #region Private Variables
    private float p_curHealth;

    // Our Velocity variables
    float velocityZ;
    float velocityX;

    // increase performance
    int VelocityZHash;
    int VelocityXHash;

    #endregion

    #region Cached Components
    private Rigidbody cc_Rb;

    private Animator cc_Anim;
    private Quaternion rotToPlayer;
    #endregion

    #region Cached References
    private Transform cr_Player;
    private Transform lookAtTarget;
    #endregion

    #region Initialization
    private void Awake() {
        p_curHealth = m_MaxHealth;

        cc_Rb = GetComponent<Rigidbody>();
        cc_Anim = GetComponent<Animator>();

        rotToPlayer = new Quaternion();

        // VelocityZHash = Animator.StringToHash("Velocity Z");
        // VelocityXHash = Animator.StringToHash("Velocity X");
    }

    private void Start() {
        cr_Player = FindObjectOfType<PlayerController>().transform;
        lookAtTarget = GameObject.Find("LookAtTarget").GetComponent<Transform>();
    }
    #endregion

    #region Main Updates
    private void Update() {
        // Set our animation variables

        cc_Anim.SetBool("isMoving", true);
        // cc_Anim.SetFloat(VelocityZHash, velocityZ);
        // cc_Anim.SetFloat(VelocityXHash, velocityX);    
    }

    private void FixedUpdate() {
        Vector3 dir = cr_Player.position - transform.position;
        dir.Normalize();

        //float degsToRot = 0;

        float angleToRotateEnemy = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        
        //Debug.Log("Angle: " + angleToRotateEnemy + "    dirX: " + dir.x + "      dirY: " + dir.y);
        transform.Rotate(new Vector3(0, angleToRotateEnemy + angleOffset, 0), Space.Self);

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,angleToRotateEnemy,0), 0.2f);

        //if (dir)

        //cc_Rb.rotation = 

        // float playerAngleFromZAxis = 0;
        // if (playerForward.x < 0) {
        //     playerAngleFromZAxis = Mathf.Deg2Rad * -Vector3.SignedAngle(camForward, Vector3.forward, Vector3.forward);
        // } else {
        //     playerAngleFromZAxis = Mathf.Deg2Rad * Vector3.SignedAngle(camForward, Vector3.forward, Vector3.forward);
        // }
        // float angleToRotatePlayer = Vector3.SignedAngle(playerForward, camForward, Vector3.forward);
        // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,Camera.main.transform.eulerAngles.y,0), 0.2f);




        //transform.LookAt(cr_Player);
        cc_Rb.MovePosition(cc_Rb.position + dir * m_speed * Time.fixedDeltaTime);
    }
    #endregion

    #region Collision Methods
    private void OnCollisionStay(Collision collision) {
        GameObject other = collision.collider.gameObject;
        if (other.CompareTag("Player")) {
           other.GetComponent<PlayerController>().DecreaseHealth(m_Damage);
        }
    }
    #endregion

    #region Health Methods
    public void DecreaseHealth(float amount) {
        p_curHealth -= amount;
        if (p_curHealth <= 0) {
            //ScoreManager.singleton.IncreaseScore(m_Score);
            if (Random.value < m_HealthPillDropRate) {
                Instantiate(m_HealthPill, transform.position, Quaternion.identity);
            }
            Instantiate(m_DeathExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
    }

    public float GetHealth() {
        return p_curHealth;
    }
    #endregion
}
