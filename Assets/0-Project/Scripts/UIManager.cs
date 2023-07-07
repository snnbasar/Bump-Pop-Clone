using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject mainMenu;
    public GameObject gameMenu;
    public GameObject losePanel;
    public GameObject winPanel;

    [Header("Money Area")]
    public GameObject moneyGO;
    public Transform moneyParent;
    public TextMeshProUGUI moneyText;

    [Header("Level Area")]
    public TextMeshProUGUI levelText;

    [Header("Win Panel")]
    public TextMeshProUGUI scoreText;

    [Header("Game Area")]
    public GameObject incrementalMenu;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateLevelText();
    }


    public void RestartTheScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenWinPanel()
    {
        //levelProgressArea.SetActive(false);

        //winEmoji.sprite = winSprites[Random.Range(0, winSprites.Length)];

        winPanel.SetActive(true);
        winPanel.GetComponent<CanvasGroup>().alpha = 0;
        winPanel.GetComponent<CanvasGroup>().DOFade(1f, 1f).SetUpdate(true);
    }

    public void OpenLosePanel()
    {
        //levelProgressArea.SetActive(false);

        //loseEmoji.sprite = loseSprites[Random.Range(0, loseSprites.Length)];

        losePanel.SetActive(true);
        losePanel.GetComponent<CanvasGroup>().alpha = 0;
        losePanel.GetComponent<CanvasGroup>().DOFade(1f, 1f).SetUpdate(true);
    }

    public void PlayArea()
    {
        mainMenu.SetActive(false);
        gameMenu.SetActive(true);
        //GameManager.Instance.ChangeGameState(GameManager.GameState.Started);

    }

    public void UpdateMoneyText(int amount)
    {
        moneyText.text = Extensions.KMBMaker(amount);
        AnimateScaleOfMoney();
    }
    public void UpdateScoreText(int score) => scoreText.text = score.ToString();

    public void AnimateScaleOfMoney()
    {
        if (DOTween.IsTweening(moneyParent))
            return;
        float time = 0.25f;
        float scaleMultiplier = 1.1f;
        Vector3 scale = Vector3.one;
        Sequence moneyScaleAnimSeq = DOTween.Sequence();
        moneyScaleAnimSeq.Append(moneyParent.DOScale(scale * scaleMultiplier, time / 2));
        moneyScaleAnimSeq.Append(moneyParent.DOScale(scale, time / 2));
    }

    public void UpdateLevelText()
    {
        int level = 0;
        for (int i = 0; i < LevelManager.Instance.transform.childCount; i++)
        {
            if (LevelManager.Instance.transform.GetChild(i).gameObject.activeSelf)
                level = i;
        }
        levelText.text = "Level " + (level + 1).ToString();
    }


    public void SetActiveIncrementalMenu(bool sts) => incrementalMenu.SetActive(sts);
}
