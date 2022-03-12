using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticPathFinder : MonoBehaviour
{
    [SerializeField]
    float creatureSpeed;

    public float pathMultiplier;
    int pathIndex = 0;

    DNA dna;
    bool hasFinished = false;
    bool hasBeenInitialized = false;
    Vector2 target;
    Vector2 nextPoint;

    public void InitCreature(DNA newDna, Vector2 target)
    {
        dna = newDna;
        this.target = target;
        nextPoint = transform.position;
    }

    private void Update()
    {
        if (hasBeenInitialized)
        {
            if((Vector2)transform.position == nextPoint)
            {
                nextPoint = ((Vector2)transform.position + dna.genes[pathIndex]);
                pathIndex++;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, nextPoint, creatureSpeed * Time.deltaTime);
            }
        }
    }
}
