using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSerializer: MonoBehaviour
{
    public List<Line> ObastacleMapLines = new List<Line>();
    public List<Vector2> points = new List<Vector2>();

    public Transform spawnPosition;
    public Transform targetPosition;

    //public LayerMask obstacleLayer = 1 << 6;

    public void Init()
    {
        SerializeMapObstacles();
    }

    private void SerializeMapObstacles()
    {
        GameObject[] obstaclesInScene = GameObject.FindGameObjectsWithTag("Obstacles");
        foreach (GameObject item in obstaclesInScene)
        {
            BoxCollider2D sprite = item.GetComponent<BoxCollider2D>();
            Vector2[] rectVertices = GetBoxPoints2D(sprite);
            for (int i = 0; i < 4; i++)
            {
                if (i == 3)
                    ObastacleMapLines.Add(new Line(rectVertices[i], rectVertices[0]));
                else
                    ObastacleMapLines.Add(new Line(rectVertices[i], rectVertices[i + 1]));


                points.Add(rectVertices[i]);
            }
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
        var parentMTRmtx = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);

        points[0] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(-size.x, size.y)));
        points[1] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(-size.x, -size.y)));
        points[2] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(size.x, -size.y)));
        points[3] = transform.TransformPoint(mtx.MultiplyPoint3x4(new Vector2(size.x, size.y)));

        //Bounds b = box.bounds;
        //points[0] = new Vector2(b.max.x, b.max.y); // or do = b.max
        //points[1] = new Vector2(b.max.x, b.min.y);
        //points[2] = new Vector2(b.min.x, b.min.y); // = b.min
        //points[3] = new Vector2(b.min.x, b.max.y);
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
