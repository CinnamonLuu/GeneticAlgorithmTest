using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSimulationManager : MonoBehaviour
{
    [SerializeField] List<PopulationController> populationControllers;

    private void Start()
    {
        if (SimulationController.Instance && SimulationController.Instance.visualSimulation)
        {
            if (SimulationController.Instance.type == SimulationController.SimulationType.IndividualAlgorithms)
            {
                populationControllers[0].SetInitialVariables(SimulationController.Instance.NumAgents, SimulationController.Instance.NumMovements, SimulationController.Instance.typeOfDistance);
            }
            else
            {
                foreach (PopulationController pc in populationControllers)
                {
                    pc.SetInitialVariables(SimulationController.Instance.NumAgents, SimulationController.Instance.NumMovements);
                }
            }
        }
    }

    public void StartSimulation()
    {
        foreach (PopulationController pc in populationControllers)
        {
            pc.InitPopulation(SimulationController.Instance.stepLengthMultiplier);
        }
    }
}
