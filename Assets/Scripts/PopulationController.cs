using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TypeOfDistance
{
    Manhattan,
    Euclidean,
    Chebyshev
}

public class PopulationController : MonoBehaviour
{
    List<GeneticPathFinder> population = new List<GeneticPathFinder>();
    public GameObject creaturePrefab;
    public int populationSize = 100;
    public int genomeLenght;
    public TypeOfDistance type;
    public float cutoff = 0.3f;
    [Range(0f, 1f)] public float mutationRate;
    public Transform spawnPoint;
    public Transform end;

    private int iterationCounter = 0;
    private int arrived = 0;
    private int crashed = 0;
    private int noArrived = 100;
    private int firstArrivedIteration = 0;
    public AlgorithmUIUpdater uiUpdater;

    public int survivorKeep = 5;

    public int Arrived
    {
        set
        {
            arrived = value;
            uiUpdater.ArrivedNumber = arrived.ToString();
        }
        get
        {
            return arrived;
        }
    }
    public int NoArrived
    {
        set
        {
            noArrived = value;
            uiUpdater.NoArrivedNumber = noArrived.ToString();
        }
        get
        {
            return noArrived;
        }
    }
    private string Ratio => (int)(((float)arrived / (float)populationSize) * 100) + "%";


    private void Start()
    {
        InitPopulation();
    }

    private void Update()
    {
        if (!HasActive())
        {
            SimulationDatabase.AddIteration(type, iterationCounter, populationSize, arrived, crashed);

            NextGeneration();
        }
    }

    void InitPopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            GeneticPathFinder geneticPathFinder = GenerateAgent();
            population.Add(geneticPathFinder);
        }
    }

    private GeneticPathFinder GenerateAgent()
    {
        GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
        GeneticPathFinder geneticPathFinder;
        switch (type)
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
        geneticPathFinder.InitCreature(new DNA(genomeLenght), end.position, spawnPoint.position);
        return geneticPathFinder;
    }

    void NextGeneration()
    {
        int survivorCut = Mathf.RoundToInt(populationSize * cutoff);
        List<GeneticPathFinder> survivors = new List<GeneticPathFinder>(population);
        uiUpdater.RatioNumber = Ratio;

        if(arrived > survivorKeep)
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
                population[i].InitCreature(survivors[i].dna, end.position, spawnPoint.position);
            }
            else
            {
                population[i].InitCreature(new DNA(survivors[i % survivorCut].dna, survivors[Random.Range(0, survivorCut)].dna, mutationRate), end.position, spawnPoint.position);
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
        uiUpdater.IterationNumber = iterationCounter.ToString();
    }

    private void IncreseArrived()
    {
        Arrived++;
        NoArrived--;
        if (firstArrivedIteration == 0)
        {
            firstArrivedIteration = iterationCounter;
            uiUpdater.FirstArrivedIteration = iterationCounter.ToString();
        }
    }
    private void IncreseCrashed()
    {
        crashed++;
        uiUpdater.NoArrivedNumber = noArrived.ToString();
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
}