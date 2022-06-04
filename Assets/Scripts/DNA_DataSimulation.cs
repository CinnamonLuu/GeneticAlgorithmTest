using System.Collections.Generic;
using UnityEngine;

public class DNA_DataSimulation : DNA
{
    public List<Line> lines = new List<Line>();

    public DNA_DataSimulation(Vector2 spawnPosition, int genomeLenght = 50) : base(genomeLenght)
    {
        Vector2 lastCoordinate = spawnPosition;
        for (int i = 0; i < genomeLenght; i++)
        {
            lines.Add(new Line(lastCoordinate, lastCoordinate + genes[i]));
            lastCoordinate += genes[i];
        }
    }

    public DNA_DataSimulation(Vector2 spawnPosition, DNA parent, DNA partner, float mutationChance = 0.01f, float mutationWeight = 0.5f) :
        base(parent, partner, mutationChance, mutationWeight)
    {
        Vector2 lastCoordinate = spawnPosition;
        for (int i = 0; i < parent.genes.Count; i++)
        {
            lines.Add(new Line(lastCoordinate, lastCoordinate + genes[i]));
            lastCoordinate += genes[i];
        }
    }
}