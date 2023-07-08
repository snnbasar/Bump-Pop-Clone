using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class Ball : MonoBehaviour {
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private EventTrigger _ballEventTrigger;
    [SerializeField] private GameObject _targetImage;
    [SerializeField] private TextMeshProUGUI _moneyText;

    public bool amIMultiplied;
    private bool _isGhost;

    private GameObject activeVisual;
    private Material myMaterial;
    private void Awake()
    {
        _ballEventTrigger.OnBallCollided += OnBallCollided;

        activeVisual = transform.GetChild(0).gameObject;
        myMaterial = activeVisual.GetComponent<Renderer>().material;
    }

    private void Start()
    {
        if (amIMultiplied)
            return;
        BallManager.Instance.ballsOnLevelDesign.Add(this);
        ChangeMyColor(BallManager.Instance.colors.GetRandomItem());
    }
    private void Update()
    {
        if (GameManager.Instance._gameState != GameManager.GameState.Started)
            return;
        if (transform.position.y <= -10f && !FinalManager.Instance.isFinalTriggered)
        {
            BallManager.Instance.RemoveMeFromInteractedBalls(this);
            gameObject.SetActive(false); // Return to the pool
        }
    }
    public void Init(Vector3 velocity, bool isGhost) {
        _isGhost = isGhost;
        _rb.AddForce(velocity, ForceMode.Impulse);
    }

    
    public void ResetVelocity()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnBallCollided(Ball ball)
    {
        if (ball.amIMultiplied)
            return;
        amIMultiplied = true;
        ball.amIMultiplied = true;

        Ball refBall = BallManager.Instance.ballsOnLevelDesign.Contains(this) ? this : ball; // 2 collisions happen on the same frame. This is for find the ball that can multiply

        //Set Lists
        BallManager.Instance.ballsOnLevelDesign.Remove(this);
        BallManager.Instance.ballsOnLevelDesign.Remove(ball);
        BallManager.Instance.AddMeToInteractedBalls(this);
        BallManager.Instance.AddMeToInteractedBalls(ball);

        BallManager.Instance.SpawnNewBalls(refBall);

    }


    public void DoMoneyAnim(int price)
    {
        TextMeshProUGUI text = _moneyText;
        text.gameObject.SetActive(true);
        text.text = price > 0 ? "" + price : price.ToString();
        Vector3 newPos = text.transform.localPosition;
        newPos.z = 0;
        text.transform.localPosition = newPos;
        CanvasGroup cG = text.GetComponent<CanvasGroup>();
        Sequence seq = DOTween.Sequence();
        float offset = 150f;
        float time = 0.5f;
        seq.Append(text.rectTransform.DOLocalMoveY(offset, time).SetRelative(true));
        seq.AppendInterval(time / 2);
        seq.Join(cG.DOFade(0, time));
        seq.OnComplete(() => text.gameObject.SetActive(false));
    }



    [Button]
    private void PrintVeloSqr() => print(this.GetRigidbody().velocity.sqrMagnitude);

    public void SetRenderer(bool sts) => activeVisual.SetActive(sts);
    public Vector3 GetVelocity() => _rb.velocity;
    public Rigidbody GetRigidbody() => _rb;
    public void LockYConstrain(bool sts) => _rb.constraints = sts ? RigidbodyConstraints.FreezePositionY & RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeRotation;
    public void ShowTargetImage(bool sts) => _targetImage.SetActive(sts);
    public void ChangeMyColor(Color color) => myMaterial.color = color;
    public Color GetMyColor() => myMaterial.color;
    public void SetKinematic(bool sts) => _rb.isKinematic = sts;

}
