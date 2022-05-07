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

    [SerializeField]
    [Tooltip("The detection range of the enemy")]
    private float detectionRange;

    [SerializeField]
    [Tooltip("The shooting range of the enemy")]
    private float shootingRange;

    [SerializeField]
    [Tooltip("The number of bullets per burst")]
    private float numBullets;

    [SerializeField]
    [Tooltip("The bullet prefab")]
    private GameObject bulletPrefab;

    [SerializeField]
    [Tooltip("The delay between bullets within a burst")]
    private float bulletDelay;

    [SerializeField]
    [Tooltip("The delay between bursts")]
    private float burstDelay;

    [SerializeField]
    [Tooltip("The fire point of the enemy gun")]
    private Transform firePoint;

    [SerializeField]
    [Tooltip("The rifle gameObject")]
    private GameObject rifle;

    [SerializeField]
    [Tooltip("The spawn offset for the healthPill")]
    private Vector3 pillSpawnOffset;

    [SerializeField]
    [Tooltip("The duration enemy is stunned for after being punched")]
    private float stunnedDur;

    #endregion
    
    #region Private Variables
    private float p_curHealth;
    private bool isAlerted;
    private bool seesPlayer;
    private float distToPlayer;
    private float burstTimer;
    private Animator rifleAnimator;
    private Punch punchScript;
    private bool isStunned;
    #endregion

    #region Cached Components
    private Rigidbody cc_Rb;
    private Animator cc_Anim;
    private Collider col;
    #endregion

    #region Cached References
    private Transform cr_Player;
    #endregion

    #region Initialization
    private void Awake() {
        isAlerted = false;
        seesPlayer = false;
        p_curHealth = m_MaxHealth;
        burstTimer = burstDelay;
        cc_Rb = GetComponent<Rigidbody>();
        cc_Anim = GetComponent<Animator>();
        rifleAnimator = rifle.GetComponent<Animator>();
        col = GetComponent<Collider>();
    }

    private void Start() {
        cr_Player = FindObjectOfType<PlayerController>().transform;
        punchScript = GameObject.Find("Punch Range").GetComponent<Punch>();
    }
    #endregion

    #region Main Updates
    private void Update() {

        if (burstTimer > 0) {
            burstTimer -= Time.deltaTime;
        }

    }

    private void FixedUpdate() {
        Vector3 dir = cr_Player.position - transform.position;
        dir.Normalize();
        
        RaycastHit hit;
        
        // If Player is in Enemy's line of sight, then the enemy will pursue the player.
        if (Physics.Raycast(transform.position, dir, out hit, detectionRange)) {
            if (hit.transform.tag == "Player") {
                //Debug.Log("Hit Player");
                distToPlayer = (hit.point - transform.position).magnitude;
                isAlerted = true;
                seesPlayer = true;
            } else {
                seesPlayer = false;
                //Debug.Log("Hit wall");
            }
        }

        // If the Enemy is alerted of the Player's presence, then it will attack/pursue the player depending on their distance
        if (isAlerted && seesPlayer && distToPlayer <= shootingRange && burstTimer <= 0 && !isStunned) {
            float angleToRotateEnemy = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            transform.Rotate(new Vector3(0, angleToRotateEnemy + angleOffset, 0), Space.Self);
            cc_Anim.SetBool("isMoving", false);
            StartCoroutine("Shoot");

        // Move towards player if we cannot see them or they are too far
        } else if (isAlerted && (!seesPlayer || distToPlayer > shootingRange) && !isStunned) {
            float angleToRotateEnemy = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            transform.Rotate(new Vector3(0, angleToRotateEnemy + angleOffset, 0), Space.Self);
            cc_Rb.MovePosition(cc_Rb.position + dir * m_speed * Time.fixedDeltaTime);
            cc_Anim.SetBool("isMoving", true);

        // Default to Idle position
        } else if (isAlerted && !isStunned) {
            float angleToRotateEnemy = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            transform.Rotate(new Vector3(0, angleToRotateEnemy + angleOffset, 0), Space.Self);
            cc_Anim.SetBool("isMoving", false);
            //cc_Anim.SetBool("isShooting", false);
        } else if (isStunned) {
            Vector3 stunnedDir = transform.position - cr_Player.position;
            stunnedDir.Normalize(); 
            StartCoroutine("Stunned", stunnedDir);
        }
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

    #region Shooting Methods
    IEnumerator Shoot() {
        burstTimer = burstDelay;
        Vector3 shootDir = new Vector3(cr_Player.position.x - firePoint.position.x, 0, cr_Player.position.z - firePoint.position.z);
        rifleAnimator.SetBool("Rotate", true);
        cc_Anim.SetBool("isShooting", true);
        //Debug.Log("Shooting");
        for (int i = 0; i < numBullets; i++) {
            GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody bullet = go.GetComponent<Rigidbody>();
            bullet.AddForce(shootDir * 10, ForceMode.Impulse);
            yield return new WaitForSeconds(bulletDelay);
        }

        cc_Anim.SetBool("isShooting", false);
        rifleAnimator.SetBool("Rotate", false);
    }
    #endregion

    #region Health Methods
    public void DecreaseHealth(float amount) {
        p_curHealth -= amount;
        if (p_curHealth <= 0) {
            //ScoreManager.singleton.IncreaseScore(m_Score);
            if (Random.value < m_HealthPillDropRate) {
                Instantiate(m_HealthPill, transform.position + pillSpawnOffset, Quaternion.identity);
            }
            Instantiate(m_DeathExplosion, transform.position, Quaternion.identity);
            punchScript.RemoveEnemy(col);
            Destroy(gameObject);
        }
        
    }

    public float GetHealth() {
        return p_curHealth;
    }
    #endregion


    IEnumerator Stunned(Vector3 dir) {
        cc_Anim.SetBool("isShooting", false);
        cc_Anim.SetBool("isMoving", false);
        cc_Anim.SetBool("isStunned", true);


        cc_Rb.MovePosition(cc_Rb.position + dir * m_speed * 8 * Time.fixedDeltaTime);

        yield return new WaitForSeconds(stunnedDur);
        cc_Anim.SetBool("isStunned", false);
        isStunned = false;
    }

    public void SetIsStunned() {
        isStunned = true;
    }
}
