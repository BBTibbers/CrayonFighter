using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : MonoBehaviour
{

    public GameObject[] HealthUI;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI RetryText;
    public TextMeshProUGUI GameOverText;
    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI ComboText;
    public GameObject GameOverUI;
    public Slider Slider;
    public GameObject SliderHandle;
    public GameObject SliderVfx;
    public Button RegameButton;


    public static UI_Game Instance;

    private bool _parry = false;

    private void Awake()
    {
        Instance = this;

        Slider.maxValue = 5f;
        SliderVfx.GetComponent<ParticleSystem>().Stop();
        RegameButton.onClick.AddListener(() =>
        {
            GameOverUI.SetActive(false);
            Player.Instance.Regame();
        });
    }




    public void UpdateHealth(int health)
    {
        for (int i = 0; i < HealthUI.Length; i++)
        {
            HealthUI[i].SetActive(i < health-1);
        }
    }
    public void UpdateScore(int score) 
    {
        ScoreText.text = $"Score : {score.ToString()}";  
    }

    public void SetSlider(float time) 
    {
        Slider.value = Mathf.Min(time,5f);
        if (time > 5f && _parry == false)
        {
            _parry = true;
            SliderVfx.GetComponent<ParticleSystem>().Play() ;
        }
        if (time < 5f&& _parry == true)
            _parry = false;
    }

    public void GameOver(int health)
    {
        if(health > 0)
        {
            return;
        }
        Player.Instance.KillBreath();
        RetryText.text = "Retry?";
        GameOverText.text = "Game Over";
        ScoreManager.Instance.CheckAndUpdateScore();
        GameOverUI.SetActive(true);
        Player.Instance.IsGameOver = true;
    }

    
    
}
