using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TypeOfDistance
{
    Manhattan,
    Euclidean,
    Chebyshev
}

[System.Serializable]
public class PopulationController : MonoBehaviour
{
    private GameObject creaturePrefab;

    public Transform spawnPoint;
    public Transform targetPoint;

    public AlgorithmUIUpdater uiUpdater;

    [Header("Population Parameters")]
    [Tooltip("NumAgents")] public int populationSize = 100;
    [Tooltip("NumMovements")] public int genomeLenght;
    public float cutoff = 0.3f;
    [Tooltip("Elitism")] public int survivorKeep = 5;
    public TypeOfDistance distanceType;
    [Range(0f, 1f)] public float mutationChance = 0.5f;
    [Range(0f, 1f)]
    [Tooltip("Weight applied to the parent movement, at 0 the movement will be completely random")]
    public float parentMutationWeight = 0.5f;

    [Header("Simulation Config")]
    public bool usesPoissonBin;

    private int iterationCounter = 0;
    private int firstArrivedIteration = 0;
    private int arrived = 0;
    private int noArrived = 100;
    private int crashed = 0;

    private bool initialized;


    List<GeneticPathFinder> population = new List<GeneticPathFinder>();
    public List<Line> PopulationPathLines;


    public int Arrived
    {
        get => arrived;
        set
        {
            arrived = value;
            //uiUpdater.ArrivedNumber = arrived.ToString();
        }
    }
    public int NoArrived
    {
        get => noArrived;
        set
        {
            noArrived = value;
            //uiUpdater.NoArrivedNumber = noArrived.ToString();
        }

    }
    private string Ratio => (int)(((float)arrived / (float)populationSize) * 100) + "%";

    private void Start()
    {
        InitPopulation();
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (!HasActive())
        {
            SimulationDatabase.AddIteration(distanceType, iterationCounter, arrived / populationSize, arrived, crashed);

            NextGeneration();
        }
    }

    public void InitPopulation(bool initializeWithSimulationController = true, bool getGPUData = false)
    {
        creaturePrefab = Resources.Load<GameObject>("Creature");

        if (SimulationController.Instance)
        {
            if (SimulationController.Instance.visualSimulation)
            {
                GeneticPathFinder geneticPathFinder;
                for (int i = 0; i < SimulationController.Instance.NumAgents; i++)
                {
                    geneticPathFinder = GenerateAgent(getGPUData, i);
                    population.Add(geneticPathFinder);
                }
            }
            else
            {
                DNA dna;
                for (int i = 0; i < SimulationController.Instance.NumAgents; i++)
                {
                    dna = new DNA(SimulationController.Instance.NumMovements);
                    //SimulationController.Instance.
                }
            }
        }
        else
        {
            for (int i = 0; i < populationSize; i++)
            {
                GeneticPathFinder geneticPathFinder = GenerateAgent();
                population.Add(geneticPathFinder);
            }
        }
        initialized = true;
    }

    private GeneticPathFinder GenerateAgent(bool getGPUData = false, int index = -1)
    {
        GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
        GeneticPathFinder geneticPathFinder;
        switch (distanceType)
        {
            case TypeOfDistance.Manhattan:
                geneticPathFinder = go.AddComponent<GeneticPathFinderManhattan>();
                break;
            case TypeOfDistance.Euclidean:
                geneticPathFinder = go.AddComponent<GeneticPathFinderEuclidean>();
                break;
            case TypeOfDistance.Chebyshev:
                geneticPathFinder = go.AddComponent<GeneticPathFinderChebyshev>();
                break;
            default:
                geneticPathFinder = go.AddComponent<GeneticPathFinderManhattan>();
                break;
        }
        geneticPathFinder.finished += IncreseArrived;
        geneticPathFinder.crashed += IncreseCrashed;
        if (!getGPUData)
        {
            geneticPathFinder.InitCreature(new DNA(genomeLenght), targetPoint.position, spawnPoint.position);
        }
        else
        {
            geneticPathFinder.InitCreature(SimulationController.Instance.temporalGPUValidator[index], targetPoint.position, spawnPoint.position);
        }
        return geneticPathFinder;
    }

    void NextGeneration()
    {
        int survivorCut = Mathf.RoundToInt(populationSize * cutoff);
        List<GeneticPathFinder> survivors = new List<GeneticPathFinder>(population);
        // //uiUpdater.RatioNumber = Ratio;

        if (arrived > survivorKeep)
        {
            //Probability to increment the number of agents we keep with the same path
            //We don't always want to increment this number to keep the randomness
            survivorKeep = Random.Range(survivorKeep, arrived + 1);
        }

        survivors = survivors.OrderByDescending(o => o.fitness).ToList();

        //THE BEST AGENTS OF THE POPULATION KEEP THE SAME DNA
        for (int i = 0; i < populationSize; i++)
        {
            if (i < survivorKeep)
            {
                population[i].InitCreature(survivors[i].dna, targetPoint.position, spawnPoint.position);
            }
            else
            {
                population[i].InitCreature(new DNA(survivors[i % survivorCut].dna, survivors[Random.Range(0, survivorCut)].dna, mutationChance, parentMutationWeight), targetPoint.position, spawnPoint.position);
            }
        }
        ResetUIVariables();
        IncrementIterationCounter();
    }

    private void ResetUIVariables()
    {
        Arrived = 0;
        crashed = 0;
        NoArrived = populationSize;
    }

    public void IncrementIterationCounter()
    {
        iterationCounter++;
        //uiUpdater.IterationNumber = iterationCounter.ToString();
    }

    private void IncreseArrived()
    {
        Arrived++;
        NoArrived--;
        if (firstArrivedIteration == 0)
        {
            firstArrivedIteration = iterationCounter;
            //uiUpdater.FirstArrivedIteration = iterationCounter.ToString();
        }
    }
    private void IncreseCrashed()
    {
        crashed++;
        //uiUpdater.NoArrivedNumber = noArrived.ToString();
    }

    bool HasActive()
    {
        for (int i = 0; i < population.Count; i++)
        {
            if (!population[i].hasFinished)
            {
                return true;
            }
        }
        return false;
    }

    private void OnApplicationQuit()
    {
        SimulationDatabase.AddSimulation(distanceType, populationSize, survivorKeep, cutoff, mutationChance, parentMutationWeight, usesPoissonBin, 0);
    }
}