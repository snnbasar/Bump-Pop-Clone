using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextCameraLook : MonoBehaviour
{
    private Transform camera;
    public float damping = 40f;
    public bool lockY;
    void Start()
    {
        camera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        var lookPos = camera.position - transform.position;
        if(lockY) lookPos.x = 0;
        var rotation = Quaternion.LookRotation(-lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
    }
}
