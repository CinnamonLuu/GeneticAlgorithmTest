using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    private static SimulationController instance;

    public SimulationManager simulationManager;

    public bool visualSimulation = true;

    public PopulationController manhattanPopulationController;
    public PopulationController euclideanPopulationController;
    public PopulationController chebychevPopulationController;

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
            InitializeSimulation();
        }
        else
        {
            //simulation with gpu
        }
    }

    private void InitializeSimulation()
    {
        manhattanPopulationController.InitPopulation();
        euclideanPopulationController.InitPopulation();
        chebychevPopulationController.InitPopulation();
    }
}
