using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    //public UnityEvent OnTriggerStayEvent;
    //public UnityEvent OnTriggerEnterEvent;
    //public UnityEvent OnTriggerExitEvent;
    //public static float giveTime = 0.1f;

    public event Action<Ball> OnBallCollided;
    public event Action<Ball> OnBallEnterTrigger;
    public event Action<Ball> OnBallStayTrigger;

    //float time;

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("Player"))
        //{
        //    OnTriggerEnterEvent?.Invoke();
        //}
        if (other.TryGetComponent(out Ball ball))
        {
            OnBallEnterTrigger?.Invoke(ball);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Ball ball))
        {
            OnBallStayTrigger?.Invoke(ball);
        }
    }

    //private void OnTriggerExit(Collider other)
    //{

    //}

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out Ball ball))
        {
            OnBallCollided?.Invoke(ball);
        }
    }
}
