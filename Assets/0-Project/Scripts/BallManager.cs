using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

    public Ball mainBall;
    public float ballThrowForce = 10f; // Player throw force
    public float forwardMultiplierOnBallSpawn = 35f; // Force for balls that newly spawned
    public bool canMove; // Global move bool
    public bool isAbleToMove; // Local move bool
    public float rotateSpeed = 10f; // MainBall's rotate speed for player

    private List<Ball> interactedBalls = new List<Ball>(); // A list for keep tracking balls we interacted so we can check for camera and next move
    public List<Ball> ballsOnLevelDesign = new List<Ball>(); // A list for keep tracking balls that can spawn new balls
    public Color[] colors; // Available colors

    private int _totalBallSpawnCount;
    private int totalBallSpawnCount { get => _totalBallSpawnCount; set
        {
            _totalBallSpawnCount = value;
            UIManager.Instance.UpdateBallText(_totalBallSpawnCount);
        } }

    Quaternion lookRot;
    Vector2 lastMousePos;

    int beforeMoveBallSpawnTriggeredCount;
    int curBallSpawnTriggeredCount;

    private void Awake() => Instance = this;

    private IEnumerator Start()
    {
        //Set Main ball
        mainBall = LevelManager.Instance.currentLevel.mainBall;
        GameManager.Instance.camera_CM.Follow = mainBall.transform;
        GameManager.Instance.camera_CM.LookAt = mainBall.transform;

        isAbleToMove = true;
        AddMeToInteractedBalls(mainBall);

        //Wait 2 frame for every ball to register itself to ballsOnLevelDesign list so we can remove mainBall
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

        if (FinalManager.Instance && FinalManager.Instance.isFinalTriggered)
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
        // Rotate main ball according to player's input
        mainBall.transform.rotation = Quaternion.Slerp(mainBall.transform.rotation, lookRot, Time.deltaTime * rotateSpeed);

        if (Input.GetMouseButtonUp(0))
        {
            // Throw main ball and start movement
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
        if(CheckIfEveryBallStopped() && waitTimer >= waitTime)
        {
            OnMoveEnded();
            waitTimer = 0;
        }
    }
    // Function to check if there is any ball still moving
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

        // Check if we did not hit any ball and final is not triggered
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
            //Get Random Pos
            float randomMultiplier = 0.5f;
            Vector3 randomPos = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            randomPos *= randomMultiplier;

            //Create ball Instance, set color and initialize it
            GameObject ballGO = GameManager.Instance.ballPool.PullGameObject(refBall.transform.position + randomPos);
            Ball ball = ballGO.GetComponent<Ball>();
            ball.amIMultiplied = true;
            ball.ChangeMyColor(refBall.GetMyColor());

            AddMeToInteractedBalls(ball);
            AddForceToSpawnedBalls(refBall, ball);
            
            //Give income for spawned balls
            GiveIncome(ball);

            totalBallSpawnCount++;
        }
        //Give income for ball that we hit
        GiveIncome(refBall);

        void GiveIncome(Ball ball)
        {
            ball.DoMoneyAnim(GameManager.Instance.oneBallIncome);
            GameManager.Instance.MoneyAdd(GameManager.Instance.oneBallIncome);
        }
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
    //Function to find the farest ball in every interacted balls
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


    public void AddMeToInteractedBalls(Ball ball)
    {
        if (!interactedBalls.Contains(ball))
            interactedBalls.Add(ball);
    }

    public void RemoveMeFromInteractedBalls(Ball ball) => interactedBalls.Remove(ball);

    public void SetCanMove(bool sts) => canMove = sts;
}

