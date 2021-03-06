using UnityEngine;

[System.Serializable]
public class DataSimulationManager : MonoBehaviour
{
    [SerializeField] private MapSerializer mapSerializer;
    [SerializeField] private GPUIntersectionChecker intersectionChecker;
    [SerializeField] private LineRenderer lineRenderer;

    private void Start()
    {
        if (!mapSerializer)
        {
            mapSerializer = GetComponent<MapSerializer>();
        }
        mapSerializer.Init();

        intersectionChecker = new GPUIntersectionChecker();
        if (SimulationController.Instance)
        {

            intersectionChecker.Init(SimulationController.Instance.NumAgents,
                                        SimulationController.Instance.NumMovements,
                                        SimulationController.Instance.stepLengthMultiplier,
                                        mapSerializer.spawnPosition.position,
                                        mapSerializer.targetPosition.position,
                                        mapSerializer.ObastacleMapLines.ToArray(),
                                        mapSerializer.Obstacles.ToArray(),
                                        SimulationController.Instance.typeOfDistance);
        }
    }

    public void StartSimulation()
    {

        intersectionChecker.CheckIntersectionCPU();
        intersectionChecker.ResetVariables();
        intersectionChecker.CheckIntersectionGPU();
    }

    public void RepresentBestSimulation()
    {
        intersectionChecker.CheckIntersectionCPU();
        intersectionChecker.ResetVariables();
        intersectionChecker.CheckIntersectionGPU();
        //PopulationController populationController = FindObjectOfType<PopulationController>();
        lineRenderer.positionCount = SimulationController.Instance.NumMovements;
        lineRenderer.SetPositions(intersectionChecker.GetBestSimulation());
    }
}
