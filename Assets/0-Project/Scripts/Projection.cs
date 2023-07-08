using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Projection : MonoBehaviour {
    public static Projection Instance;
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations = 100;
    [SerializeField] private float _physicsStep = 0.02f;

    public float simulationTime = 0.1f;
    float simulationTimer = 0;

    private Scene _simulationScene;
    private PhysicsScene _physicsScene;

    private Ball ghostObj;

    private void Awake()
    {
        Instance = this;
    }
    private void Start() {

        CreatePhysicsScene(LevelManager.Instance.currentLevel.transform);
    }

    public void CreatePhysicsScene(Transform parent) {
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulationScene.GetPhysicsScene();

        foreach (Transform obj in parent) {

            if (obj.GetComponent<Ball>()) continue;
            if (obj.GetComponentInParent<FinalManager>()) continue;

            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);

            if(ghostObj.TryGetComponent(out Renderer ghostObjRenderer))
            {
                ghostObjRenderer.enabled = false;
            }
            //if (ghostObj.TryGetComponent(out Ball ghostObjBall))
            //{
            //    ghostObjBall.SetRenderer(false);
            //}

            SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
        }
    }

    public void DestroyPhysicsScene()
    {
        SceneManager.UnloadSceneAsync(_simulationScene);
        OnProjectionEnd();
    }

    public void OnProjectionEnd()
    {
        _line.positionCount = 0;
    }

    public void HandleSimilation(Ball mainBall, float ballThrowForce)
    {
        if (!BallManager.Instance.isAbleToMove)
            return;
        simulationTimer += Time.deltaTime;
        if (simulationTimer >= simulationTime)
        {
            simulationTimer = 0;
            Projection.Instance.SimulateTrajectory(mainBall, mainBall.transform.position, mainBall.transform.forward * ballThrowForce);
        }
    }

    private void SimulateTrajectory(Ball ballPrefab, Vector3 pos, Vector3 velocity) {

        ghostObj = Instantiate(ballPrefab, pos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(ghostObj.gameObject, _simulationScene);

        ghostObj.Init(velocity, true);

        _line.positionCount = _maxPhysicsFrameIterations;

        for (var i = 0; i < _maxPhysicsFrameIterations; i++) {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            _line.SetPosition(i, ghostObj.transform.position);
        }

        Destroy(ghostObj.gameObject);
    }
}