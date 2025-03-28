using UnityEngine;
using System.IO;

public class ScoreData
{
    public int highScore;
}
public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Instance;

    private string savePath;
    private ScoreData scoreData = new ScoreData();
    public int Combo = 0;

    private void Awake()
    {
        Instance = this;
        savePath = Application.persistentDataPath + "/score.json";
        LoadHighScore();
    }

    private int _score = 0;
    private float _nextDiscount = 0f;
    private float _discountCooltime = 0.1f;

    void Update()
    {
        if(Player.Instance.IsGameOver) return;
        TimeDiscount();
    }

    public void AddScore(int add)
    {
        _score+=add;
        UI_Game.Instance.UpdateScore(_score);
    }

    private void TimeDiscount()
    {
        if (_score > 0 && Time.time > _nextDiscount)
        {
            _score -= 1;

            _nextDiscount = Time.time+_discountCooltime;
        }

        UI_Game.Instance.UpdateScore(_score);
    }

    public void Regame()
    {
        _score = 0;
        UI_Game.Instance.UpdateScore(_score);
    }

    public void CheckAndUpdateScore()
    {
        if (_score > scoreData.highScore)
        {
            scoreData.highScore = _score;
            SaveHighScore();
            UI_Game.Instance.HighScoreText.text = $"New Record!! \n {_score}";
            
        }
        else
            UI_Game.Instance.HighScoreText.text = $"High Score : {scoreData.highScore}\n Score : {_score}";

    }

    void SaveHighScore()
    {
        string json = JsonUtility.ToJson(scoreData);
        File.WriteAllText(savePath, json);
    }

    void LoadHighScore()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            scoreData = JsonUtility.FromJson<ScoreData>(json);
        }
        else
        {
            scoreData.highScore = 0;
        }
    }

    public int GetHighScore()
    {
        return scoreData.highScore;
    }

    public void AddCombo()
    {
        Combo++;
        if (Combo > 1)
        {
            UI_Game.Instance.ComboText.text = $"{Combo} Combo!!";
        }
    }

    public void ResetCombo()
    {
        Combo = 0;
        UI_Game.Instance.ComboText.text = "Keep Focus -";
    }

}
