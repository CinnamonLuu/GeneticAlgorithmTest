using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticPathFinderManhattan : GeneticPathFinder
{
    protected override float CalculateDistance()
    {
        return Mathf.Abs(transform.position.x - target.x) + Mathf.Abs(transform.position.y - target.y);
    }
}
