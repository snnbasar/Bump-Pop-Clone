using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData",menuName = "GameData")]
public class GameData : SingletonScriptableObject<GameData>
{
    [BoxGroup("General Settings")]
    public float timeScale = 1f;
    [BoxGroup("General Settings")]
    public float targetFPS = 60f;

    [BoxGroup("Level Settings")]
    public int firstLevelAfterFinish = 0;


    public GameObject ballPrefab;

}