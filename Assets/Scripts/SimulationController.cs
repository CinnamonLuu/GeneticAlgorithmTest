using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    private static SimulationController instance;
    public static SimulationController Instance => instance;

    public SimulationManager simulationManager;

    public bool visualSimulation = true;

    /// 
    public int NumAgents;
    public int NumMovements;
    /// 



    public PopulationController manhattanPopulationController;
    public PopulationController euclideanPopulationController;
    public PopulationController chebychevPopulationController;

    public MapSerializer mapSerializer;
    public GPUIntersectionChecker intersectionChecker;


    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (visualSimulation)
        {
            InitializeCPUSimulation();
        }
        else
        {
            InitializeGPUSimulation();
            //simulation with gpu

        }
    }

    private void InitializeCPUSimulation()
    {
        //TODO: Instantiate population Controllers
        manhattanPopulationController.InitPopulation();
        euclideanPopulationController.InitPopulation();
        chebychevPopulationController.InitPopulation();
    }
    private void InitializeGPUSimulation()
    {
        mapSerializer = FindObjectOfType<MapSerializer>();
        mapSerializer.Init();

        intersectionChecker = new GPUIntersectionChecker();

        List<Line> tmpLines = new List<Line>();
        for (int i = 0; i < NumAgents; i++)
        {
            tmpLines.AddRange(new GPUDna(mapSerializer.spawnPosition.position, NumMovements).lines);
        }
        intersectionChecker.Init(NumAgents, mapSerializer.ObastacleMapLines.ToArray() ,tmpLines.ToArray());
    }


}
