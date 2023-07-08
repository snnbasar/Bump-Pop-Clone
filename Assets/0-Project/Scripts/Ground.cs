using System;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public Renderer groundRenderer;
    public Renderer[] groundSidesRenderer;

    private void Start()
    {
        groundRenderer.sharedMaterial = LevelManager.Instance.currentLevel.levelMaterials[0];
        Array.ForEach(groundSidesRenderer, x => x.sharedMaterial = LevelManager.Instance.currentLevel.levelMaterials[1]);
    }
}
