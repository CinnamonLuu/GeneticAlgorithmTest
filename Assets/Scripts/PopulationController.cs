using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public int survivorKeep = 5;

    private void Start()
    {
        InitPopulation();
    }

    private void Update()
    {
        if (!HasActive())
        {
            SimulationDatabase.AddIteration(type,iterationCounter, populationSize, arrived, crashed);

            NextGeneration();
        }
    }

    void InitPopulation()
    {
        for (int i = 0; i < populationSize; i++)
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
            geneticPathFinder.InitCreature(new DNA(genomeLenght), end.position);
            population.Add(geneticPathFinder);
        }
    }

    void NextGeneration()
    {
        int survivorCut = Mathf.RoundToInt(populationSize * cutoff);
        List<GeneticPathFinder> survivors = new List<GeneticPathFinder>();
        uiUpdater.RatioNumber = Ratio;
        Arrived = 0;
        crashed = 0;
        NoArrived = populationSize;

        population = population.OrderByDescending(o => o.fitness).ToList();

        for (int i = 0; i < survivorCut; i++)
        {
            survivors.Add(population[0]);
            population.Remove(population[0]);
        }

        ClearPopulation(population);

        //THE BEST AGENTS OF THE POPULATION KEEP THE SAME DNA
        for (int i = 0; i < survivorKeep; i++)
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
            geneticPathFinder.InitCreature(survivors[i].dna, end.position);
            population.Add(geneticPathFinder);
        }

        //THE REST ARE MUTATIONS OF THESE BEST AGENTS
        for (int i = 0; i < survivorCut; i++)
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
            geneticPathFinder.InitCreature(new DNA(survivors[i].dna, survivors[UnityEngine.Random.Range(0, 10)].dna, mutationRate), end.position);
            population.Add(geneticPathFinder);
            if (population.Count >= populationSize)
            {
                break;

            }


            ClearPopulation(survivors);

            IncrementIterationCounter();
        }
    }

    private void ClearPopulation(List<GeneticPathFinder> genticPathFinders)
    {
        for (int i = 0; i < genticPathFinders.Count; i++)

            genticPathFinders.Clear();
    }

    public void IncrementIterationCounter()
    {
        iterationCounter++;
        uiUpdater.IterationNumber = iterationCounter.ToString();
    }

    private void IncreseArrived()
    {
        arrived++;
        noArrived--;
        uiUpdater.ArrivedNumber = arrived.ToString();
        uiUpdater.NoArrivedNumber = noArrived.ToString();
        if (firstArrivedIteration == 0)
        {
            firstArrivedIteration = iterationCounter;
            uiUpdater.FirstArrivedIteration = iterationCounter.ToString();
        }
    }
    private void IncreseCrashed()
    {
        crashed++;
        noArrived--;
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