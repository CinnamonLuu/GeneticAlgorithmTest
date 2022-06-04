using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSimulationManager : MonoBehaviour
{
    [SerializeField] List<PopulationController> populationControllers;

    public void StartSimulation()
    {
        foreach (PopulationController pc in populationControllers)
        {
            pc.InitPopulation();
        }
    }
}
