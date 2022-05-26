using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUIntersectionChecker
{

    public bool[] s_agentCrashed;

    private Line[] s_obstacleBounds;

    private Line[] s_agentsPathLines;

    public Line[] S_agentsPathLines { get => s_agentsPathLines; set => s_agentsPathLines = value; }
    public Line[] S_obstacleBounds { get => s_obstacleBounds; set => s_obstacleBounds = value; }

    private static readonly int
        obstacleBoundsArrayID = Shader.PropertyToID("_obstacleBounds"),
        agentPathArrayID = Shader.PropertyToID("_path"),
        agentCrashedArrayID = Shader.PropertyToID("_intersects");

    private ComputeShader computeShader;

    public void Init(int numAgents, Line[] obstacleBounds, Line[] agentsPathLines)
    {
        s_agentCrashed = new bool[numAgents];
        s_obstacleBounds = obstacleBounds;
        s_agentsPathLines = agentsPathLines;

        computeShader = Resources.Load<ComputeShader>("ComputeShaders/LineSegmentIntersection");
        CheckIntersectionGPU();
    }

    public void CheckIntersectionGPU()
    {
        int vec2Size = sizeof(float) * 2;
        int totalSize = vec2Size * 2;
        ComputeBuffer obstacleLines = new ComputeBuffer(s_obstacleBounds.Length, totalSize);
        ComputeBuffer pathLines = new ComputeBuffer(s_agentsPathLines.Length, totalSize);
        ComputeBuffer agentsCrashed = new ComputeBuffer(s_agentCrashed.Length, 4);

        //int kernelIndex = ComputeShader.FindKernel("LineIntersection");

        obstacleLines.SetData(S_obstacleBounds);
        computeShader.SetBuffer(0, obstacleBoundsArrayID, obstacleLines);

        pathLines.SetData(S_agentsPathLines);
        computeShader.SetBuffer(0, agentPathArrayID, pathLines);

        agentsCrashed.SetData(s_agentCrashed);
        computeShader.SetBuffer(0, agentCrashedArrayID, agentsCrashed);

        computeShader.SetInt("numMovements", SimulationController.Instance.NumMovements);

        computeShader.Dispatch(0, Mathf.CeilToInt(s_obstacleBounds.Length / 8), Mathf.CeilToInt(s_agentsPathLines.Length / 8), 1);

        agentsCrashed.GetData(s_agentCrashed);

        obstacleLines.Dispose();
        pathLines.Dispose();
        agentsCrashed.Dispose();

        for (int i = 0; i < s_agentCrashed.Length; i++)
        {
            Debug.Log($"{i}: {s_agentCrashed[i]}");
        }

    }
}
