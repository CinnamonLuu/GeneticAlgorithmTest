using System.Collections;
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


}
