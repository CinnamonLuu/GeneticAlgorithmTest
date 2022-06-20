using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSerializer : MonoBehaviour
{
    public List<Obstacle> Obstacles = new List<Obstacle>();
    public List<Line> ObastacleMapLines = new List<Line>();
    public List<Vector2> points = new List<Vector2>();
    private GameObject[] obstaclesInScene;

    public Transform spawnPosition;
    public Transform targetPosition;

    public void Init()
    {
        SerializeMapObstacles();
    }

    private void SerializeMapObstacles()
    {
        obstaclesInScene = GameObject.FindGameObjectsWithTag("Obstacles");
        BoxCollider2D sprite;
        Vector2[] rectVertices;
        foreach (GameObject item in obstaclesInScene)
        {
            sprite = item.GetComponent<BoxCollider2D>();
            rectVertices = GetBoxPoints2D(sprite);

            Obstacle obstacle = new Obstacle();
            obstacle.ObstacleLines = new Line[4];
            for (int i = 0; i < 4; i++)
            {
                Line line;
                if (i == 3)
                {
                    line = new Line(rectVertices[i], rectVertices[0]);
                }
                else
                {
                    line = new Line(rectVertices[i], rectVertices[i + 1]);
                }
                ObastacleMapLines.Add(line);
                obstacle.ObstacleLines[i] = line;
                points.Add(rectVertices[i]);
            }
            Obstacles.Add(obstacle);
        }
        spawnPosition = GameObject.FindGameObjectWithTag("Spawn").transform;
        targetPosition = GameObject.FindGameObjectWithTag("Target").transform;
    }

    private void OnDrawGizmos()
    {
        foreach (Vector2 vector in points)
        {
            Gizmos.DrawSphere(vector, .1f);
        }
        foreach (Line line in ObastacleMapLines)
        {
            Debug.DrawLine(line.PointA, line.PointB, Color.red, 100.0f);
        }
    }

    public Vector2[] GetBoxPoints2D(BoxCollider2D box)
    {
        Vector2[] points = new Vector2[4];
        var size = box.size * 0.5f;

        var mtx = Matrix4x4.TRS(box.bounds.center, box.transform.localRotation, box.transform.localScale);

        points[0] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(-size.x, size.y)));
        points[1] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(-size.x, -size.y)));
        points[2] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(size.x, -size.y)));
        points[3] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(size.x, size.y)));

        return points;
    }
}

[Serializable]
public struct Line
{
    public Vector2 PointA;
    public Vector2 PointB;

    public Line(Vector2 a, Vector2 b)
    {
        this.PointA = a;
        this.PointB = b;
    }

    public float Distance => Vector2.Distance(PointA, PointB);
}

public struct Obstacle
{
    public Line[] ObstacleLines;

    public Obstacle(Line[] obstacleLines)
    {
        this.ObstacleLines = obstacleLines;
    }
}