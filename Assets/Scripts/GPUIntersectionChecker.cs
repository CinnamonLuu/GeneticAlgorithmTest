using System.Collections.Generic;
using UnityEngine;

public class GPUIntersectionChecker
{
    public int[] _agentCrashed;
    public bool[] _agentCrashedBool;
    private Line[] _obstacleBounds;
    private Line[] _agentsPathLines;
    public Line[] AgentsPathLines { get => _agentsPathLines; set => _agentsPathLines = value; }
    public Line[] ObstacleBounds { get => _obstacleBounds; set => _obstacleBounds = value; }

    private static readonly int
        obstacleBoundsArrayID = Shader.PropertyToID("_obstacleBounds"),
        agentPathArrayID = Shader.PropertyToID("_path"),
        agentCrashedArrayID = Shader.PropertyToID("_intersects");

    private ComputeShader computeShader;

    public void Init(int numAgents, float stepPathMultiplier, Line[] obstacleBounds, Vector2 spawnPoint)
    {
        _agentCrashed = new int[numAgents];
        _obstacleBounds = obstacleBounds;
        List<Line> agentsPathLines = new List<Line>();
        for (int i = 0; i < numAgents; i++)
        {
            agentsPathLines.AddRange(new DNA_DataSimulation(stepPathMultiplier, spawnPoint, SimulationController.Instance.NumMovements).lines);
        }
        _agentsPathLines = agentsPathLines.ToArray();

        computeShader = Resources.Load<ComputeShader>("ComputeShaders/LineSegmentIntersection");
        CheckIntersectionGPU();
        //CheckIntersectionCPU();
    }

    private void CheckIntersectionCPU()
    {
        for (int i = 0; i < _agentsPathLines.Length; i++)
        {
            for (int j = 0; j < _obstacleBounds.Length; j++)
            {
                _agentCrashedBool[i / SimulationController.Instance.NumMovements] |= AreLineSegmentsIntersectingDotProduct(_obstacleBounds[j], _agentsPathLines[i]);
            }
        }

        for (int i = 0; i < _agentCrashedBool.Length; i++)
        {
            Debug.Log($"{i}: {_agentCrashedBool[i]}");
        }

        bool IsPointsOnDifferentSides(Vector2 obsA, Vector2 obsB, Vector2 pathA, Vector2 pathB)
        {
            bool isOnDifferentSides = false;

            //The direction of the line
            Vector2 lineDir = obsB - obsA;

            //The normal to a line is just flipping x and y and making y negative
            Vector2 lineNormal = new Vector2(-lineDir.y, lineDir.x);

            //Now we need to take the dot product between the normal and the points on the other line
            float dot1 = Vector2.Dot(lineNormal, pathA - obsA);
            float dot2 = Vector2.Dot(lineNormal, pathB - obsA);

            //If you multiply them and get a negative value then p3 and p4 are on different sides of the line
            if (dot1 * dot2 < 0)
            {
                isOnDifferentSides = true;
            }

            return isOnDifferentSides;
        }

        bool AreLineSegmentsIntersectingDotProduct(Line obstacleLine, Line pathLine)
        {
            bool isIntersecting = false;

            if (IsPointsOnDifferentSides(obstacleLine.PointA, obstacleLine.PointB, pathLine.PointA, pathLine.PointB)
                && IsPointsOnDifferentSides(pathLine.PointA, pathLine.PointB, obstacleLine.PointA, obstacleLine.PointB))
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }
    }

    public void CheckIntersectionGPU()
    {
        int vec2Size = sizeof(float) * 2;
        int totalSize = vec2Size * 2;
        ComputeBuffer bufferObstacleLines = new ComputeBuffer(_obstacleBounds.Length, totalSize);
        ComputeBuffer bufferPathLines = new ComputeBuffer(_agentsPathLines.Length, totalSize);
        ComputeBuffer bufferAgentsCrashed = new ComputeBuffer(_agentCrashed.Length, sizeof(int));

        int kernelIndex = computeShader.FindKernel("LineIntersection");

        bufferObstacleLines.SetData(ObstacleBounds);
        computeShader.SetBuffer(0, obstacleBoundsArrayID, bufferObstacleLines);

        bufferPathLines.SetData(AgentsPathLines);
        computeShader.SetBuffer(0, agentPathArrayID, bufferPathLines);

        bufferAgentsCrashed.SetData(_agentCrashed);
        computeShader.SetBuffer(0, agentCrashedArrayID, bufferAgentsCrashed);

        computeShader.SetInt("numMovements", SimulationController.Instance.NumMovements);

        computeShader.Dispatch(kernelIndex, Mathf.CeilToInt(_obstacleBounds.Length / 8), Mathf.CeilToInt(_agentsPathLines.Length / 8), 1);

        bufferAgentsCrashed.GetData(_agentCrashed);

        bufferObstacleLines.Dispose();
        bufferPathLines.Dispose();
        bufferAgentsCrashed.Dispose();

        for (int i = 0; i < _agentCrashed.Length; i++)
        {
            Debug.Log($"{i}: {_agentCrashed[i]}");
        }

        //TODO: introduce data in data base
    }
}