using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject controlPanel;

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI currentPlayerText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("Références")]
    [SerializeField] private GameController gameController;

    void Start()
    {
        ShowInstructions("Scannez une surface et touchez pour placer le Tic Tac Toe");
        currentPlayerText.gameObject.SetActive(false);
        statusPanel.SetActive(false);
    }

    public void UpdateCurrentPlayer(bool isXTurn)
    {
        currentPlayerText.text = isXTurn ? "Tour de X" : "Tour de O";
        currentPlayerText.color = isXTurn ? Color.red : Color.blue;
    }

    public void ShowInstructions(string message)
    {
        instructionsText.text = message;
        instructionsText.gameObject.SetActive(true);
    }

    public void HideInstructions()
    {
        instructionsText.gameObject.SetActive(false);
    }

    public void ShowGameOver(string message)
    {
        gameOverPanel.SetActive(true);
        winnerText.text = message;
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowControls()
    {
        controlPanel.SetActive(true);
    }

    public void UpdateForBoardPlaced()
    {
        HideInstructions();
        ShowControls();
        statusPanel.SetActive(true);
        currentPlayerText.gameObject.SetActive(true);
        UpdateCurrentPlayer(true);
    }
}