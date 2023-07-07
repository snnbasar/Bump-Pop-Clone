using Cinemachine;
using OWS.ObjectPooling;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        NotStarted,
        Started,
        GameOver,
        Win
    }

    public GameState _gameState;


    public int startMoney = 100;
    public int _collectedMoney;
    public int collectedMoney { get => _collectedMoney; set
        {
            _collectedMoney = value;
            UIManager.Instance.UpdateMoneyText(_collectedMoney);
            SaveManager.Save(_collectedMoney, "collectedMoney");
        } }
    public int oneBallIncome = 10;
    public int ballSpawnCount = 5;
    public int incomeIncrementalAmountPerLevel = 5;

    public CinemachineVirtualCamera camera_CM;
    public CinemachineImpulseSource ImpulseSource_CM;
    public ObjectPool<PoolObject> ballPool;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        Time.timeScale = GameData.Instance.timeScale;
        //Application.targetFrameRate = (int)GameData.Instance.targetFPS;



        ballPool = new ObjectPool<PoolObject>(GameData.Instance.ballPrefab);
    }

    private void Start()
    {
        collectedMoney = SaveManager.Load("collectedMoney", startMoney);

    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.B))
            Debug.Break();

        if (Input.GetKeyDown(KeyCode.M))
            UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
#endif

    }

    public void StartGame()
    {
        UIManager.Instance.PlayArea();
        ChangeGameState(GameState.Started);
    }

    public void ChangeGameState(GameState state)
    {
        _gameState = state;

        switch (_gameState)
        {
            case GameState.NotStarted:
                //FUNCTIONS
                break;
            case GameState.Started:
                BallManager.Instance.ShowTargetImages(true);
                UIManager.Instance.SetActiveIncrementalMenu(false);
                break;
            case GameState.GameOver:
                BallManager.Instance.ShowTargetImages(false);

                break;
            case GameState.Win:
                BallManager.Instance.ShowTargetImages(false);

                break;
            default:
                break;
        }
    }

    public bool DoIHaveEnoughMoney(int neededMoney)
    {
        return collectedMoney >= neededMoney;
    }

    public void MoneyAdd(int money)
    {
        collectedMoney += money;
    }


    public void SetCameraFollow(Transform t)
    {
        GameManager.Instance.camera_CM.Follow = t;
        GameManager.Instance.camera_CM.LookAt = t;
    }

    [Button]
    public void ShakeCam()
    {
        ImpulseSource_CM.GenerateImpulse();
    }
}
