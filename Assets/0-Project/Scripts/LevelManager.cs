using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public LevelSettings currentLevel;
    public int currentLevelIndex;

    private void Awake()
    {
        Instance = this;

        //Load Level
        int level = SaveManager.Load("currentLevel", 0);
        currentLevelIndex = level % transform.childCount;

        foreach (Transform lvl in transform)
        {
            lvl.gameObject.SetActive(false);
        }

        GameObject levelGO = transform.GetChild(currentLevelIndex).gameObject;
        levelGO.SetActive(true);
        this.currentLevel = levelGO.GetComponent<LevelSettings>();
    }

    
    [Button]
    public void GameOver()
    {
        if (GameManager.Instance._gameState != GameManager.GameState.Started)
            return;

        GameManager.Instance.ChangeGameState(GameManager.GameState.GameOver);
        UIManager.Instance.OpenLosePanel();
    }

    [Button]
    public void LevelCompleted()
    {
        if (GameManager.Instance._gameState != GameManager.GameState.Started)
            return;

        GameManager.Instance.ChangeGameState(GameManager.GameState.Win);

        int level = SaveManager.Load("currentLevel", 0);
        SaveManager.Save(level + 1, "currentLevel");

        UIManager.Instance.OpenWinPanel();
    }


}
