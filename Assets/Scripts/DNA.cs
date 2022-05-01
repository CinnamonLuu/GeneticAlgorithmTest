using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNA
{
    public List<Vector2> genes = new List<Vector2>();

    public DNA(int genomeLenght = 50)
    {
        for (int i = 0; i < genomeLenght; i++)
        {
            genes.Add(new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)));
        }
    }

    public DNA(DNA parent, DNA partner, float mutationRate = 0.01f, float mutationWeight = 0.5f)
    {
        for (int i = 0; i < parent.genes.Count; i++)
        {
            float mutationChance = Random.Range(0.0f, 1.0f);
            if (mutationChance <= mutationRate)
            {
                Vector2 randomMovement = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                Vector2 mutatedMovement = (parent.genes[i] * mutationWeight + randomMovement * (1 - mutationWeight)) / 2;
                genes.Add(mutatedMovement.normalized);
            }
            else
            {
                int chance = Random.Range(0, 2);
                if (chance == 0)
                {
                    genes.Add(parent.genes[i]);
                }
                else
                {
                    genes.Add(partner.genes[i]);
                }
            }
        }
    }
}
