using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HUDController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("The part of the the health that decreases")]
    private RectTransform m_HealthBar;

    [SerializeField]
    [Tooltip("The text displaying the score of the player.")]
    private GameObject m_CurrentScore;

    [SerializeField]
    [Tooltip("The text displaying the number of enemies left.")]
    private GameObject m_EnemiesLeft;
    #endregion

    #region Private Variables
    private float p_HealthBarOrigWidth;
    private int p_Score;
    private int p_EnemiesLeft;
    private TextMeshProUGUI currScoreTxt;
    private TextMeshProUGUI enemiesLeftTxt;
    private string sceneName;
    #endregion

    #region Initialization
    private void Awake() {
        p_HealthBarOrigWidth = m_HealthBar.sizeDelta.x;
        currScoreTxt = m_CurrentScore.GetComponent<TextMeshProUGUI>();
        enemiesLeftTxt = m_EnemiesLeft.GetComponent<TextMeshProUGUI>();
        UpdateCurrentScore(0);
    }
    private void Start() {
        p_Score = ScoreManager.singleton.GetCurrentScore();
        UpdateCurrentScore(0);

        sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Arena") {
            p_EnemiesLeft = 30;
        } else if (sceneName == "Level 2") {
            p_EnemiesLeft = 60;
        }
        UpdateEnemiesRemaining(0);
    }
    #endregion

    #region Update Health Bar
    public void UpdateHealth(float percent) {
        m_HealthBar.sizeDelta = new Vector2(p_HealthBarOrigWidth * percent, m_HealthBar.sizeDelta.y);
    }
    #endregion

    #region Update Current Score Text
    public void UpdateCurrentScore(int amount) {
        p_Score += amount;
        currScoreTxt.text = "Score: " + p_Score.ToString();
    }
    #endregion

    #region Update Enemies Left Text
    public void UpdateEnemiesRemaining(int number) {
        if (number != 0) {
            p_EnemiesLeft -= number;
        }
        enemiesLeftTxt.text = "Enemies Remaining: " + p_EnemiesLeft.ToString();

        if (p_EnemiesLeft <= 0 && sceneName == "Arena") {
            SceneManager.LoadScene("Level 2");
        } else if (p_EnemiesLeft <= 0 && sceneName == "Level 2") {
            SceneManager.LoadScene("MainMenu");
        }
    }
    #endregion
}