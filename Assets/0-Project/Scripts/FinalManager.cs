using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FinalManager : MonoBehaviour
{
    public static FinalManager Instance;

    public int _totalScore;
    public int totalScore { get => _totalScore; set
        {
            _totalScore = value;
            UIManager.Instance.UpdateScoreText(_totalScore);
        } }

    public EventTrigger finalTrigger;
    public bool isFinalTriggered;

    [Header("Final Creation")]
    public int chunkCount;
    public GameObject chunkPrefab;
    public float chunkOffset = 40f;
    public List<GameObject> spawnedChunks = new List<GameObject>();

    private void Awake() => Instance = this;
    private void Start()
    {
        finalTrigger.OnBallEnterTrigger += FinalTrigger_OnBallEnterTrigger;
    }

    private void FinalTrigger_OnBallEnterTrigger(Ball ball)
    {
        if (isFinalTriggered)
            return;
        isFinalTriggered = true;

        StartCoroutine(SlowlyAdjustCamera());
    }

    IEnumerator SlowlyAdjustCamera()
    {
        CinemachineVirtualCamera cm = GameManager.Instance.camera_CM;
        cm.LookAt = null;
        Vector3 lookRot = cm.transform.rotation.eulerAngles;
        lookRot.x = 45f;
        lookRot.y = 0f;
        lookRot.z = 0f;

        CinemachineTransposer transposer = cm.GetCinemachineComponent<CinemachineTransposer>();
        DOTween.To(() => transposer.m_FollowOffset.y, x => transposer.m_FollowOffset.y = x, 25f, 1f);
        DOTween.To(() => transposer.m_YawDamping, x => transposer.m_YawDamping = x, 3f, 1f);
        //transposer.m_FollowOffset.y = 10f;
        //transposer.m_YawDamping = 3f;

        float rotateSpeed = 2f;
        float moveSpeed = 2f;

        Transform followT = cm.Follow;
        GameObject temp = new GameObject("Camera Fix Follow");
        temp.transform.position = followT.position;

        Vector3 tempNewPos = followT.position;
        tempNewPos.x = 0;
        temp.transform.position = tempNewPos;
        cm.Follow = temp.transform;

        while (Vector3.Distance(cm.transform.rotation.eulerAngles, lookRot) >= 0.1f || Mathf.Abs(cm.transform.position.x) >= 0.1f)
        {
            cm.transform.rotation = Quaternion.Slerp(cm.transform.rotation, Quaternion.Euler(lookRot), Time.deltaTime * rotateSpeed);
            //Vector3 newPos = cm.transform.position;
            //newPos.x = 0f;
            //cm.transform.position = Vector3.Lerp(cm.transform.position, newPos, Time.deltaTime * moveSpeed);

            tempNewPos = followT.position;
            tempNewPos.x = 0;
            temp.transform.position = Vector3.Lerp(temp.transform.position, tempNewPos, Time.deltaTime * moveSpeed);
            //temp.transform.position = tempNewPos;

            yield return null;
        }
        cm.Follow = followT;
        cm.gameObject.AddComponent<CinemachineLockXAxis>();

    }

    [Button]
    public void CreateChunks()
    {
#if UNITY_EDITOR
        int partCount = 0;
        for (int i = 0; i < chunkCount; i++)
        {
            GameObject chunk = PrefabUtility.InstantiatePrefab(chunkPrefab) as GameObject;
            chunk.transform.SetParent(this.transform);
            Vector3 pos = Vector3.zero;
            pos.z = i * chunkOffset + chunkOffset / 2;
            chunk.transform.localPosition = pos;
            chunk.transform.localRotation = Quaternion.identity;
            FinalPart[] finalparts = chunk.GetComponentsInChildren<FinalPart>();
            for (int a = 0; a < finalparts.Length; a++)
            {
                partCount++;
                FinalPart finalPart = finalparts[a];
                finalPart.maxScore = partCount * 5;
                EditorUtility.SetDirty(finalPart);
            }
            spawnedChunks.Add(chunk);
            EditorUtility.SetDirty(chunk);
        }
        EditorUtility.SetDirty(this);
#endif
    }

    [Button]
    public void DeleteChunks()
    {
#if UNITY_EDITOR
        foreach (var chunk in spawnedChunks)
        {
            DestroyImmediate(chunk, true);
        }
        spawnedChunks.Clear();
        EditorUtility.SetDirty(this);
#endif
    }
}
