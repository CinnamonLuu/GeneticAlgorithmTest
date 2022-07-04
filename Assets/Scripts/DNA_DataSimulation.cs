using System.Collections.Generic;
using UnityEngine;

public class DNA_DataSimulation : DNA
{
    public List<Line> lines = new List<Line>();

    public DNA_DataSimulation()
    {

    }

    public DNA_DataSimulation(float stepPathMultiplier, Vector2 spawnPosition, int genomeLenght = 50) : base(stepPathMultiplier,genomeLenght)
    {
        Vector2 lastCoordinate = spawnPosition;
        for (int i = 0; i < genomeLenght; i++)
        {
            lines.Add(new Line(lastCoordinate, lastCoordinate + genes[i]));
            lastCoordinate += genes[i];
        }
    }

    public DNA_DataSimulation(float stepPathMultiplier, Vector2 spawnPosition, DNA parent, DNA partner, float mutationChance = 0.01f, float mutationWeight = 0.5f) :
        base(stepPathMultiplier, parent, partner, mutationChance, mutationWeight)
    {
        Vector2 lastCoordinate = spawnPosition;
        for (int i = 0; i < parent.genes.Count; i++)
        {
            lines.Add(new Line(lastCoordinate, lastCoordinate + genes[i]));
            lastCoordinate += genes[i];
        }
    }

    public static DNA_DataSimulation DNA_DataSimulationDebug(float stepPathMultiplier, Vector2 spawnPosition, int genomeLenght = 50)
    {

        List<Line> lines = new List<Line>();
        float pos = spawnPosition.y;
        for (int i = 0; i < genomeLenght; i++)
        {
            lines.Add(new Line(new Vector2(0, pos), new Vector2(0, ++pos)));
        }
        DNA_DataSimulation aux = new DNA_DataSimulation();
        aux.lines = lines;
        return aux;
    }
}