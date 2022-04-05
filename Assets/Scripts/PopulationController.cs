using System;
using System.Collections;
using System.Collections.Generic;
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
    private int firstArrivedIteration=0;
    public AlgorithmUIUpdater uiUpdater;

    private string Ratio => (int)(((float)arrived / (float)populationSize) * 100)+"%";

    public int survivorKeep = 5;

    private void Start()
    {
        InitPopulation();
    }

    private void Update()
    {
        if (!HasActive())
        {
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
        //for (int i = 0; i < packsToSpawn.Length; i++)
        //{
        //    for (int j = 0; j < packsToSpawn[i].numberOfAgents; j++)
        //    {
        //        GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
        //        LineRenderer lr = go.GetComponent<LineRenderer>();
        //        if (packsToSpawn[i].type == TypeOfDistance.Manhattan)
        //        {
        //            lr.startColor = Color.red;
        //            lr.endColor = Color.red;
        //        }
        //        else
        //        {
        //            lr.startColor = Color.blue;
        //            lr.endColor = Color.blue;
        //        }
        //        GeneticPathFinder geneticPathFinder = go.GetComponent<GeneticPathFinder>();
        //        geneticPathFinder.type = packsToSpawn[i].type;
        //        geneticPathFinder.InitCreature(new DNA(genomeLenght), end.position);
        //        population.Add(geneticPathFinder);
        //    }
        //}
    }

    void NextGeneration()
    {
        int survivorCut = Mathf.RoundToInt(populationSize * cutoff);
        List<GeneticPathFinder> survivors = new List<GeneticPathFinder>();
        uiUpdater.RatioNumber = Ratio;
        arrived = 0;
        crashed = 0;
        noArrived = populationSize;
        uiUpdater.ArrivedNumber = arrived.ToString();
        uiUpdater.NoArrivedNumber = noArrived.ToString();

        for (int i = 0; i < survivorCut; i++)
        {
            survivors.Add(GetFittest());
        }
        for (int i = 0; i < population.Count; i++)
        {
            population[i].finished -= IncreseArrived;
            population[i].crashed -= IncreseCrashed;
            Destroy(population[i].gameObject);
        }
        population.Clear();

        for (int i = 0; i < survivorKeep; i++)
        {
            GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
            //LineRenderer lr = go.GetComponent<LineRenderer>();
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
            //if (geneticPathFinder.type == TypeOfDistance.Manhattan)
            //{
            //    lr.startColor = Color.red;
            //    lr.endColor = Color.red;
            //}
            //else
            //{
            //    lr.startColor = Color.blue;
            //    lr.endColor = Color.blue;
            //}
            geneticPathFinder.finished += IncreseArrived;
            geneticPathFinder.crashed += IncreseCrashed;
            geneticPathFinder.InitCreature(survivors[i].dna, end.position);
            population.Add(geneticPathFinder);
        }

        while (population.Count < populationSize)
        {
            for (int i = 0; i < survivorCut; i++)
            {
                GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
                //LineRenderer lr = go.GetComponent<LineRenderer>();
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
                //if (geneticPathFinder.type == TypeOfDistance.Manhattan)
                //{
                //    lr.startColor=Color.red;
                //    lr.endColor=Color.red;
                //}
                //else
                //{
                //    lr.startColor = Color.blue;
                //    lr.endColor = Color.blue;
                //}
                geneticPathFinder.finished += IncreseArrived;
                geneticPathFinder.crashed += IncreseCrashed;
                geneticPathFinder.InitCreature(new DNA(survivors[i].dna, survivors[UnityEngine.Random.Range(0, 10)].dna, mutationRate), end.position);
                population.Add(geneticPathFinder);
                if (population.Count >= populationSize)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < survivors.Count; i++)
        {
            survivors[i].finished -= IncreseArrived;
            survivors[i].crashed -= IncreseCrashed;
            Destroy(survivors[i].gameObject);
        }

        IncrementCounter();
    }

    public void IncrementCounter()
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

    GeneticPathFinder GetFittest()
    {
        float maxFitness = float.MinValue;
        int index = 0;
        for (int i = 0; i < population.Count; i++)
        {
            if (population[i].fitness > maxFitness)
            {
                maxFitness = population[i].fitness;
                index = i;
            }
        }
        GeneticPathFinder fittest = population[index];
        population.Remove(fittest);
        return fittest;
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
