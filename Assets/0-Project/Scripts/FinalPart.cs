using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalPart : MonoBehaviour
{
    public int maxScore;
    private int _curScore;
    private int curScore { get => _curScore; set
        {
            _curScore = value;

            int score = maxScore - _curScore >= 0 ? maxScore - curScore : 0;
            UpdateScoreText(score);
            CheckIfBallsCanPass();
        }
    }

    public float waitTime = 5f;
    float waitTimer = 0f;

    public TextMeshProUGUI scoreText;
    public GameObject col;
    public Renderer[] chainsRenderer;

    public GameObject chain;
    public GameObject leftChain;
    public GameObject rightChain;
    public EventTrigger ballTrigger;

    private List<Ball> enteredBalls = new List<Ball>();

    private bool isChainBreaked;

    private void Start()
    {
        ballTrigger.OnBallEnterTrigger += BallTrigger_OnBallEnterTrigger;
        UpdateScoreText(maxScore);

        //Change Chain color
        ChangeMyColor(BallManager.Instance.colors.GetRandomItem());
    }
    private void Update()
    {
        if (GameManager.Instance._gameState != GameManager.GameState.Started)
            return;
        if (isChainBreaked)
            return;
        if (curScore <= 0)
            return;
        waitTimer += Time.deltaTime;
        if(waitTimer >= waitTime)
        {
            LevelManager.Instance.LevelCompleted();
        }
    }
    private void UpdateScoreText(int amount)
    {
        scoreText.text = amount.ToString();
    }

    private void BallTrigger_OnBallEnterTrigger(Ball ball)
    {
        if (enteredBalls.Contains(ball))
            return;
        enteredBalls.Add(ball);
        curScore++;
        waitTimer = 0;
    }

    private void CheckIfBallsCanPass()
    {
        if (_curScore < maxScore)
            return;
        BreakChain();
    }

    private void BreakChain()
    {
        FinalManager.Instance.totalScore = maxScore;
        isChainBreaked = true;
        col.SetActive(false);
        this.gameObject.SetLayer(9, true);
        GameManager.Instance.ShakeCam();

        BreakChainVisual();

        float force = 10f;
        enteredBalls.ForEach(x => x.GetRigidbody().AddForce(FinalManager.Instance.transform.forward * force, ForceMode.Impulse));
    }

    [Button]
    public void BreakChainVisual()
    {
        
        List<Transform> bonesToGiveForce = new List<Transform>();
        for (int i = 0; i < chain.transform.GetChild(0).childCount; i++)
        {
           Transform bone = chain.transform.GetChild(0).GetChild(i);

            int index = i;
            Transform copyChain;
            if(i <= 5)
            {
                copyChain = leftChain.transform;
                
            }
            else
            {
                index = i - 6;
                copyChain = rightChain.transform;
            }
            Transform copyBone = copyChain.GetChild(0).GetChild(index);
            copyBone.position = bone.position;
            if (i == 5 || i == 6)
                bonesToGiveForce.Add(copyBone);
        }
        chain.SetActive(false);
        leftChain.SetActive(true);
        rightChain.SetActive(true);

        bonesToGiveForce.ForEach(x => x.GetComponent<Rigidbody>().AddForce(Vector3.forward * 50f, ForceMode.Impulse));
    }

    public void ChangeMyColor(Color color)
    {
        Array.ForEach(chainsRenderer, x => x.material.color = color);
    }
}
