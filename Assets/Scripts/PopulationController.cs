using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationController : MonoBehaviour
{
    List<GeneticPathFinder> population = new List<GeneticPathFinder>();
    public GameObject creaturePrefab;
    public int populationSize = 100;
    public int genomeLenght;
    public float cutoff = 0.3f;
    [Range(0f, 1f)] public float mutationRate;
    public Transform spawnPoint;
    public Transform end;

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
            GeneticPathFinder geneticPathFinder = go.GetComponent<GeneticPathFinder>();
            geneticPathFinder.InitCreature(new DNA(genomeLenght), end.position);
            population.Add(geneticPathFinder);
        }
    }

    void NextGeneration()
    {
        int survivorCut = Mathf.RoundToInt(populationSize * cutoff);
        List<GeneticPathFinder> survivors = new List<GeneticPathFinder>();

        for (int i = 0; i < survivorCut; i++)
        {
            survivors.Add(GetFittest());
        }
        for (int i = 0; i < population.Count; i++)
        {
            Destroy(population[i].gameObject);
        }
        population.Clear();

        for (int i = 0; i < survivorKeep; i++)
        {
            GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
            GeneticPathFinder geneticPathFinder = go.GetComponent<GeneticPathFinder>();
            geneticPathFinder.InitCreature(survivors[i].dna, end.position);
            population.Add(geneticPathFinder);
        }

        while (population.Count < populationSize)
        {
            for (int i = 0; i < survivorCut; i++)
            {
                GameObject go = Instantiate(creaturePrefab, spawnPoint.position, Quaternion.identity);
                GeneticPathFinder geneticPathFinder = go.GetComponent<GeneticPathFinder>();
                geneticPathFinder.InitCreature(new DNA(survivors[i].dna, survivors[Random.Range(0, 10)].dna, mutationRate), end.position);
                population.Add(geneticPathFinder);
                if (population.Count >= populationSize)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < survivors.Count; i++)
        {
            Destroy(survivors[i].gameObject);
        }
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
