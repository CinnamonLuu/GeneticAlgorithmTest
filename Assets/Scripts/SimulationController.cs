using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SimulationMap 
{ 
    DiagonalObstacles, 
    DiagonalObstacles1, 
    StraightObstacles,
    StraightObstacles1
}

public class SimulationController : MonoBehaviour
{
    /*-------------------------SINGLETON----------------------------- */
    private static SimulationController instance;
    public static SimulationController Instance => instance;
    /*--------------------------------------------------------------- */

    /*---------------------------CONSTS------------------------------ */
    public const int visualSimulationSceneIndex = 1;
    public const int dataSimulationSceneIndex = 2;
    /*--------------------------------------------------------------- */

    /*---------------------CONFIGURABLE INFO------------------------- */
    public bool visualSimulation = true;
    public int NumAgents;
    public int NumMovements;
    public SimulationMap map;
    /*--------------------------------------------------------------- */

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
        }
        FindObjectOfType<Camera>().enabled = false;
    }

    private void InitializeCPUSimulation()
    {
        SceneManager.LoadScene(visualSimulationSceneIndex, LoadSceneMode.Additive);
    }
    private void InitializeGPUSimulation()
    {
        SceneManager.LoadScene(dataSimulationSceneIndex, LoadSceneMode.Additive);
    }
}
