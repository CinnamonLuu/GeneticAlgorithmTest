using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DNA
{
    public List<Vector2> genes = new List<Vector2>();

    public DNA()
    {

    }
    public DNA(float stepPathMultiplier, int genomeLenght = 50)
    {
        for (int i = 0; i < genomeLenght; i++)
        {
            Vector2 randomMovement = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            genes.Add(randomMovement.normalized * stepPathMultiplier);
        }
    }

    public DNA(float stepPathMultiplier, DNA parent, DNA partner, float mutationChance = 0.01f, float mutationWeight = 0.5f)
    {
        for (int i = 0; i < parent.genes.Count; i++)
        {
            float randomChance = Random.Range(0.0f, 1.0f);
            if (randomChance <= mutationChance)
            {
                Vector2 randomMovement = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                Vector2 mutatedMovement = (parent.genes[i] * mutationWeight + randomMovement * (1 - mutationWeight)) / 2;
                genes.Add(mutatedMovement.normalized * stepPathMultiplier);
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