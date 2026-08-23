using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalScore;
    public int stageScore;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;

    public Image[] UIHealth;
    public Text UIScore;
    public Text UIStage;
    public GameObject UIRestartButton;

    private void Update()
    {
        UIScore.text = (totalScore + stageScore).ToString();
    }

    public void NextStage()
    {
        // Change Stage
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        // Game Clear
        else
        {
            Time.timeScale = 0;
            Debug.Log("게임 클리어!");
            
            Text buttonText = UIRestartButton.GetComponentInChildren<Text>();
            buttonText.text = "Clear!";
            UIRestartButton.SetActive(true);
        }

        // Calculate Score
        totalScore += stageScore;
        stageScore = 0;
    }

    public void DecreaseHealth()
    {
        if(health > 1)
        {
            health--;
            UIHealth[health].color = new Color(1f, 0f, 0f, 0.4f);
        }
        else
        {
            // All Health UI Off
            UIHealth[0].color = new Color(1f, 0f, 0f, 0.4f);

            // Player Die
            player.OnDeath();

            // Result UI
            Debug.Log("플레이어가 죽었습니다!");

            // Retry Button UI
            UIRestartButton.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(health > 1)
            {
                PlayerReposition();
            }

            // Decrease Health
            DecreaseHealth();
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0f, 0f, 0f);
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
