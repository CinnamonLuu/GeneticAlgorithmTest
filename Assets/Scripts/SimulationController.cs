using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public const int VISUAL_SIMULATION_SCENE_INDEX = 1;
    public const int DATA_SIMULATION_SCENE_INDEX = 2;

    public const float STEP_LENGHT_MULTIPLAYER_HIGH = 1f;
    public const float STEP_LENGHT_MULTIPLAYER_LOW = 0.5f;
    /*--------------------------------------------------------------- */

    /*---------------------CONFIGURABLE INFO------------------------- */
    public bool visualSimulation = true;
    public int NumAgents;
    public int NumMovements;
    public SimulationMap map;
    public int[] compareAlgorithmsIndexes;
    public int[] mapScenesIndexes;

    [Range(0.2f,1.0f)]
    public float stepLengthMultiplier = 0.5f;

    public GameObject mainMenuPanel;
    public GameObject simulationTypePanel;
    public GameObject mapsPanel;
    /*--------------------------------------------------------------- */

    //TEMPORAL
    public List<Line> tmpLines = new List<Line>();

    public List<DNA_DataSimulation> temporalGPUValidator = new List<DNA_DataSimulation>();

    private GameObject scaneCamera;
    private GameObject eventSystem;

    private enum SimulationType { CompareAlgorithms, IndividualAlgorithms};
    private SimulationType type;

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
        mainMenuPanel.SetActive(true);
        simulationTypePanel.SetActive(false);
        mapsPanel.SetActive(false);
        scaneCamera = FindObjectOfType<Camera>().gameObject;
        eventSystem = FindObjectOfType<EventSystem>().gameObject;
        //if (visualSimulation)
        //{
        //    InitializeCPUSimulation();
        //}
        //else
        //{
        //    InitializeGPUSimulation();
        //}
        //FindObjectOfType<Camera>().enabled = false;
    }

    public void OpenMapsPanel()
    {
        mapsPanel.SetActive(true);
        simulationTypePanel.SetActive(false);
        mainMenuPanel.SetActive(false);
    }
    public void OpenMainMenuPanel()
    {
        mainMenuPanel.SetActive(true);
        simulationTypePanel.SetActive(false);
        mapsPanel.SetActive(false);
    }
    public void OpenSimulationTypePanel()
    {
        simulationTypePanel.SetActive(true); 
        mainMenuPanel.SetActive(false);
        mapsPanel.SetActive(false);
    }

    public void OUT_SetPathMultiplierValue(float value)
    {
        stepLengthMultiplier = value;
    }

    public void SetVisualSimulation()
    {
        visualSimulation = true;
    }
    public void SetDataSimulation()
    {
        visualSimulation = false;
    }

    public void SetCompareAlgorithms()
    {
        type = SimulationType.CompareAlgorithms;
    }

    public void SetIndividualAlgorithm()
    {
        type = SimulationType.IndividualAlgorithms;
    }


    public void InitializeSimulation(int sceneIndex)
    {
        mainMenuPanel.SetActive(false);
        mapsPanel.SetActive(false);

        scaneCamera.SetActive(false);
        eventSystem.SetActive(false);

        if (type == SimulationType.CompareAlgorithms)
        {
            SceneManager.LoadScene(compareAlgorithmsIndexes[sceneIndex], LoadSceneMode.Additive);
        }
        if (type == SimulationType.IndividualAlgorithms)
        {
            SceneManager.LoadScene(mapScenesIndexes[sceneIndex], LoadSceneMode.Additive);
        }
    }

    private void InitializeCPUSimulation(int sceneIndex)
    {
        if (type == SimulationType.CompareAlgorithms)
        {
            SceneManager.LoadScene(compareAlgorithmsIndexes[sceneIndex], LoadSceneMode.Additive);
        }
        if (type == SimulationType.IndividualAlgorithms)
        {
            SceneManager.LoadScene(mapScenesIndexes[sceneIndex], LoadSceneMode.Additive);
        }
    }

    private void InitializeGPUSimulation()
    {
        FindObjectOfType<Camera>().enabled = false;
        SceneManager.LoadScene(DATA_SIMULATION_SCENE_INDEX, LoadSceneMode.Additive);
        mainMenuPanel.SetActive(false);
        mapsPanel.SetActive(false);

        scaneCamera.SetActive(false);
        eventSystem.SetActive(false);

        //SceneManager.LoadScene(dataSimulationSceneIndex, LoadSceneMode.Additive);

        /*mapSerializer = FindObjectOfType<MapSerializer>();
        mapSerializer.Init();

        intersectionChecker = new GPUIntersectionChecker();

        List<Line> tmpLines2 = new List<Line>();

        Temporal
        manhattanPopulationController = new GameObject().AddComponent<PopulationController>();
        manhattanPopulationController.populationSize = NumAgents;
        manhattanPopulationController.genomeLenght = NumMovements;
        manhattanPopulationController.spawnPoint = GameObject.FindGameObjectWithTag("Spawn").transform;
        manhattanPopulationController.end = GameObject.FindGameObjectWithTag("Target").transform;

        for (int i = 0; i < NumAgents; i++)
        {
            temporalGPUValidator.Add(new GPUDna(mapSerializer.spawnPosition.position, NumMovements));
            tmpLines.AddRange(temporalGPUValidator[i].lines);
            tmpLines2.Add(new Line(new Vector2(0, i - 4), new Vector2(0, i - 3)));
        }
        intersectionChecker.Init(NumAgents, mapSerializer.ObastacleMapLines.ToArray(), tmpLines.ToArray());
        intersectionChecker.Init(NumAgents, mapSerializer.ObastacleMapLines.ToArray(), tmpLines2.ToArray());

        manhattanPopulationController.InitPopulation(true);*/
    }
}