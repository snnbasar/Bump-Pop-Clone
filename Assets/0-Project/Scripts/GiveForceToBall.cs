using UnityEngine;

public class GiveForceToBall : MonoBehaviour
{
    public ForceMode forceMode;
    public bool lookDirection;
    public Vector3 forceDirection;
    public float force;

    private EventTrigger trigger;
    private void Awake()
    {
        trigger = GetComponentInChildren<EventTrigger>();
        trigger.OnBallEnterTrigger += OnBallEnterTrigger;
        trigger.OnBallStayTrigger += OnBallStayTrigger;
    }


    private void OnBallEnterTrigger(Ball ball)
    {
        if (forceMode == ForceMode.Acceleration || forceMode == ForceMode.Force)
            return;
        GiveForce(ball);
    }


    private void OnBallStayTrigger(Ball ball)
    {
        if (forceMode == ForceMode.Impulse || forceMode == ForceMode.VelocityChange)
            return;
        GiveForce(ball);
    }

    private void GiveForce(Ball ball)
    {
        ball.GetRigidbody().AddForce((lookDirection ? transform.forward : forceDirection) * force, forceMode);
    }
}