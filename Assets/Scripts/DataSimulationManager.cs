using UnityEngine;

[System.Serializable]
public class DataSimulationManager : MonoBehaviour
{
    [SerializeField] private MapSerializer mapSerializer;
    [SerializeField] private GPUIntersectionChecker intersectionChecker;

    private void Start()
    {
        if (!mapSerializer)
        {
            mapSerializer = GetComponent<MapSerializer>();
        }
        mapSerializer.Init();

        intersectionChecker = new GPUIntersectionChecker();
        intersectionChecker.Init(SimulationController.Instance.NumAgents, SimulationController.Instance.stepLengthMultiplier, mapSerializer.ObastacleMapLines.ToArray(), mapSerializer.spawnPosition.position);
    }
}
