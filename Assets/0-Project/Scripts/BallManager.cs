using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

    public Ball mainBall;
    public float ballThrowForce = 10f;
    public float forwardMultiplierOnBallSpawn = 35f;
    public bool canMove;
    public bool isAbleToMove;
    public float rotateSpeed = 10f;

    private List<Ball> interactedBalls = new List<Ball>();
    public List<Ball> ballsOnLevelDesign = new List<Ball>();
    public Color[] colors;

    Quaternion lookRot;
    Vector2 lastMousePos;

    int beforeMoveBallSpawnTriggeredCount;
    int curBallSpawnTriggeredCount;

    private void Awake() => Instance = this;

    private IEnumerator Start()
    {
        isAbleToMove = true;
        AddMeToInteractedBalls(mainBall);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        ballsOnLevelDesign.Remove(mainBall);

    }

    

    private void Update()
    {
        if (GameManager.Instance._gameState != GameManager.GameState.Started)
            return;

        if (!canMove)
            return;

        if (FinalManager.Instance.isFinalTriggered)
            return;

        if (isAbleToMove)
        {
            HandleControls();
            Projection.Instance.HandleSimilation(mainBall, ballThrowForce);
        }

        CheckIfMoveEnded();

        GetMainBall();
    }

    private void HandleControls()
    {
        if (Input.GetKey(KeyCode.A))
        {
            lookRot.eulerAngles += Vector3.down * rotateSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            lookRot.eulerAngles += Vector3.up * rotateSpeed * Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 curMousePos = Input.mousePosition;
            Vector2 deltaPos = curMousePos - lastMousePos;
            lookRot.eulerAngles += deltaPos.x * Vector3.up * rotateSpeed * Time.deltaTime;
            lastMousePos = curMousePos;
        }
        mainBall.transform.rotation = Quaternion.Slerp(mainBall.transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        //Vector3 velocity = Vector3.zero;
        //mainBall.transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(mainBall.transform.rotation.eulerAngles, lookRot.eulerAngles, ref velocity, 0.3f));

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonUp(0))
        {
            mainBall.Init(mainBall.transform.forward * ballThrowForce, false);
            OnMoveStarted();
        }
    }

    float waitTime = 1f;
    float waitTimer = 0f;
    private void CheckIfMoveEnded()
    {
        if (isAbleToMove)
            return;

        waitTimer += Time.deltaTime;
        //if(mainBall.GetVelocity().sqrMagnitude <= 0.01f && waitTimer >= waitTime)
        if(CheckIfEveryBallStopped() && waitTimer >= waitTime)
        {
            OnMoveEnded();
            waitTimer = 0;
        }
    }
    private bool CheckIfEveryBallStopped()
    {
        bool sts = true;
        foreach (var ball in interactedBalls)
        {
            if (ball.GetVelocity().sqrMagnitude > 0.01f)
                sts = false;
        }
        if (mainBall.GetVelocity().sqrMagnitude > 0.01f)
            sts = false;

        return sts;
    }

    private void OnMoveStarted()
    {
        Projection.Instance.OnProjectionEnd();
        isAbleToMove = false;
        mainBall.LockYConstrain(false);

        ShowTargetImages(false);
        beforeMoveBallSpawnTriggeredCount = curBallSpawnTriggeredCount;
    }

    private void OnMoveEnded()
    {
        mainBall.ResetVelocity();
        mainBall.transform.rotation = Quaternion.identity;
        lookRot = Quaternion.identity;
        mainBall.LockYConstrain(true);

        isAbleToMove = true;
        ShowTargetImages(true);

        if(beforeMoveBallSpawnTriggeredCount == curBallSpawnTriggeredCount && !FinalManager.Instance.isFinalTriggered)
        {
            //GameOver
            print("Game Over");
            LevelManager.Instance.GameOver();
        }
    }

    public void SpawnNewBalls(Ball refBall)
    {
        curBallSpawnTriggeredCount++;
        int ballSpawnCount = GameManager.Instance.ballSpawnCount;
        for (int i = 0; i < ballSpawnCount; i++)
        {
            float randomMultiplier = 0.5f;
            Vector3 randomPos = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            randomPos *= randomMultiplier;

            GameObject ballGO = GameManager.Instance.ballPool.PullGameObject(refBall.transform.position + randomPos);
            Ball ball = ballGO.GetComponent<Ball>();
            ball.amIMultiplied = true;
            ball.ChangeMyColor(refBall.GetMyColor());

            AddMeToInteractedBalls(ball);
            AddForceToSpawnedBalls(refBall, ball);

            GiveIncome(ball);
        }
        GiveIncome(refBall);

        void GiveIncome(Ball ball)
        {
            ball.DoMoneyAnim(GameManager.Instance.oneBallIncome);
            GameManager.Instance.MoneyAdd(GameManager.Instance.oneBallIncome);
        }
    }

    void GiveIncome(Ball refBall)
    {
        refBall.DoMoneyAnim(10);
        GameManager.Instance.MoneyAdd(10);
    }

    private void AddForceToSpawnedBalls(Ball refBall, Ball ball)
    {
        
        Vector3 newVelo = refBall.GetVelocity();
        newVelo += Vector3.forward * forwardMultiplierOnBallSpawn;

        Vector3 velo = ball.GetRigidbody().velocity;
        velo += newVelo;
        velo.y = -10f;
        ball.GetRigidbody().velocity = velo;
    }

    public void AddMeToInteractedBalls(Ball ball)
    {
        if (!interactedBalls.Contains(ball))
            interactedBalls.Add(ball);
    }
    public void RemoveMeFromInteractedBalls(Ball ball) => interactedBalls.Remove(ball);



    private void GetMainBall()
    {
        if (isAbleToMove)
            return;
        Ball farest = GetFarestBall();
        if(mainBall != farest && farest != null)
        {
            mainBall = farest;
            GameManager.Instance.SetCameraFollow(mainBall.transform);
        }
    }
    private Ball GetFarestBall()
    {
        float zMax = 0;
        Ball farest = null;
        foreach (var ball in interactedBalls)
        {
            if (ball.transform.position.z > zMax)
            {
                zMax = ball.transform.position.z;
                farest = ball;
            }
        }

        return farest;
    }


    public void ShowTargetImages(bool sts)
    {
        ballsOnLevelDesign.ForEach(x =>
        {
            if (!x.amIMultiplied)
                x.ShowTargetImage(sts);
            else
                x.ShowTargetImage(false);
        });
    }

    public void SetCanMove(bool sts) => canMove = sts;
}

