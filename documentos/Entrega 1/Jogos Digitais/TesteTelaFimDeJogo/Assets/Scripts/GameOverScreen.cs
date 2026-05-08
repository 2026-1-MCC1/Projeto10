using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
}

public class GameOverScreen : MonoBehaviour
{
    [Header("Textos da Tela")]
    public string titleText = "Fim de Jogo";
    public string resultText = "Jogador 1 venceu!";

    [Header("Pontuações")]
    public List<ScoreEntry> scores = new List<ScoreEntry>();

    [Header("Referências de UI")]
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI resultLabel;
    public Transform scoresContainer;
    public GameObject scoreRowPrefab;

    [Header("Botões")]
    public Button restartButton;
    public Button quitButton;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (titleLabel != null) titleLabel.text = titleText;
        if (resultLabel != null) resultLabel.text = resultText;

        if (scoresContainer != null)
        {
            foreach (Transform child in scoresContainer)
                Destroy(child.gameObject);

            foreach (ScoreEntry entry in scores)
                CreateScoreRow(entry);
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
    }

    void CreateScoreRow(ScoreEntry entry)
    {
        if (scoreRowPrefab == null)
        {
            Debug.Log($"  {entry.playerName}: {entry.score} pts");
            return;
        }

        GameObject row = Instantiate(scoreRowPrefab, scoresContainer);
        TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length >= 2)
        {
            texts[0].text = entry.playerName;
            texts[1].text = entry.score + " pts";
        }
    }

    void OnRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void OnQuit()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameObject.SetActive(true);
            UpdateUI();
        }
    }
}