using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneticPathFinder : MonoBehaviour
{
    //TODO Jose: Clean this class

    public DNA dna;
    public LayerMask obstacleLayer = 1 << 6;
    private LineRenderer lineRenderer;
    private List<Vector2> travelledPath = new List<Vector2>();

    private int pathIndex = 0;
    private float creatureSpeed = 20;
    private float rotationSpeed = 180;

    //this variable is useless unless implemented in gpu, probably should be removed
    public float pathMultiplier = .5f;

    private Vector2 nextPoint;

    protected Vector2 target;
    private Quaternion targetRotation;

    [Header("State")]
    private bool hasBeenInitialized = false;

    public bool hasFinished = false;
    private bool hasCrashed = false;

    public Action finished;
    public Action crashed;

    public void InitCreature(DNA newDna, Vector2 target, Vector2 beginPoint)
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        ResetAgent();

        transform.position = beginPoint;
        nextPoint = transform.position;

        dna = newDna;
        this.target = target;

        travelledPath.Add(nextPoint);
        hasBeenInitialized = true;
    }

    private void ResetAgent()
    {
        hasBeenInitialized = false;
        hasFinished = false;
        pathIndex = 0;
        travelledPath.Clear();
        lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        //TODO: Check if necessary
        //if (SimulationController.Instance && !SimulationController.Instance.visualSimulation) return;

        if (hasBeenInitialized && !hasFinished)
        {
            if (pathIndex == dna.genes.Count)
            {
                hasFinished = true;
            }
            if (Vector2.Distance(transform.position, target) < 0.5f)
            {
                hasFinished = true;
                finished?.Invoke();
            }
            if ((Vector2)transform.position == nextPoint)
            {
                nextPoint = (Vector2)transform.position + dna.genes[pathIndex] * pathMultiplier;
                travelledPath.Add(nextPoint);
                targetRotation = LookAt2D(nextPoint);
                pathIndex++;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, nextPoint, creatureSpeed * Time.deltaTime);
            }
            if (transform.rotation != targetRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            RenderLine();
        }
    }

    public void RenderLine()
    {
        List<Vector3> linePoints = new List<Vector3>();
        if (travelledPath.Count > 3)
        {
            for (int i = 0; i < travelledPath.Count - 1; i++)
            {
                linePoints.Add(travelledPath[i]);
            }
            linePoints.Add(transform.position);
        }
        else
        {
            linePoints.Add(travelledPath[0]);
            linePoints.Add(transform.position);
        }
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    public float fitness
    {
        get
        {
            float dist = CalculateDistance();
            //float dist = Vector2.Distance(transform.position, target);
            if (dist == 0)
            {
                dist = 0.00001f;
            }
            RaycastHit2D[] obstacles = Physics2D.RaycastAll(transform.position, target, obstacleLayer);
            float obstacleMultiplier = 1f - (0.1f * obstacles.Length);
            return 60 / dist * (hasCrashed ? 0.65f : 1f) * obstacleMultiplier;
            //float score = 60 / dist * (hasCrashed ? 0.65f : 1f) * obstacleMultiplier;
            //return (Mathf.Pow(2, score) - 1) / (2 - 1);
        }
    }

    public Quaternion LookAt2D(Vector2 target, float angleOffset = 90)
    {
        Vector2 fromTo = (target - (Vector2)transform.position).normalized;
        float zRotation = Mathf.Atan2(fromTo.y, fromTo.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0, 0, zRotation + angleOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            hasFinished = true;
            hasCrashed = true;
            crashed?.Invoke();
        }
    }

    protected virtual float CalculateDistance()
    {
        return 0;
    }
}