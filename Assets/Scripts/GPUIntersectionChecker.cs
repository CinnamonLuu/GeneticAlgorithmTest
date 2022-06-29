using System.Collections.Generic;
using UnityEngine;

public class GPUIntersectionChecker
{
    private int _numAgents;
    private int _numMovemets;
    private int _numObstacles;
    private TypeOfDistance typeOfDistance;

    /*-------------------------INTERSECTION----------------------------- */
    private Line[] _obstacleBounds;
    private Line[] _agentsPathLines;
    private int[] _agentCrashed;
    private bool[] _agentCrashedBool;

    private int[] _firstCollisionIndex;
    private Vector2[] _lastAgentPositions;
    /*-------------------------OBSTACLES----------------------------- */
    private Obstacle[] _obstacleArray;
    private int[] _numObstaclesIntersectedWith;


    /*-------------------------FITNESS----------------------------- */
    private float[] _obstacleMultiplier;
    private float[] _fitness;


    /*-------------------------DISTANCES----------------------------- */
    private Vector2 _targetPoint;

    private float[] _distances;


    public Line[] AgentsPathLines { get => _agentsPathLines; set => _agentsPathLines = value; }
    public Line[] ObstacleBounds { get => _obstacleBounds; set => _obstacleBounds = value; }
    public Obstacle[] ObstacleArray { get => _obstacleArray; set => _obstacleArray = value; }


    private static readonly int
        _obstacleBoundsArrayID = Shader.PropertyToID("_obstacleBounds"),
        _agentPathArrayID = Shader.PropertyToID("_path"),
        _agentCrashedArrayID = Shader.PropertyToID("_intersects"),
        _firstCollisionIndexID = Shader.PropertyToID("_firstCollisionIndex"),
        _lastAgentPositionsID = Shader.PropertyToID("_lastAgentPosition"),

        _obstaclesID = Shader.PropertyToID("_obstacles"),
        _numObstaclesIntersectedWithID = Shader.PropertyToID("_numObstaclesIntersectedWith"),

        _obstacleMultiplierID = Shader.PropertyToID("_obstacleMultiplier"),
        _fitnessID = Shader.PropertyToID("_fitness"),

        _distancesID = Shader.PropertyToID("_distances");

    private ComputeShader computeShader;

    public void Init(int numAgents, int numMovements, float stepPathMultiplier, Vector2 spawnPoint, Vector2 targetPoint, Line[] obstacleBounds, Obstacle[] obstacleArray, TypeOfDistance type)
    {

        _numAgents = numAgents;
        _numMovemets = numMovements;
        _numObstacles = obstacleArray.Length;
        typeOfDistance = type;

        #region Initialize intersection Variables
        _obstacleBounds = obstacleBounds;
        List<Line> agentsPathLines = new List<Line>();
        for (int i = 0; i < numAgents; i++)
        {
            agentsPathLines.AddRange(new DNA_DataSimulation(stepPathMultiplier, spawnPoint, SimulationController.Instance.NumMovements).lines);
        }
        _agentsPathLines = agentsPathLines.ToArray();
        _agentCrashed = new int[_numAgents];

        _firstCollisionIndex = new int[_numAgents];
        for (int i = 0; i < _firstCollisionIndex.Length; i++)
        {
            _firstCollisionIndex[i] = _numMovemets - 1;
        }

        _lastAgentPositions = new Vector2[_numAgents];
        #endregion
        #region Initialize obstacle variables
        _obstacleArray = obstacleArray;
        _numObstaclesIntersectedWith = new int[_numAgents];
        #endregion

        #region Initialize firness variables
        _obstacleMultiplier = new float[_numAgents];
        _fitness = new float[_numAgents];
        #endregion

        #region Initialize distances variables
        _targetPoint = targetPoint;
        _distances = new float[_numAgents];
        #endregion

        computeShader = Resources.Load<ComputeShader>("ComputeShaders/LineSegmentIntersection");
        //CheckIntersectionGPU();
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
        int lineIntersectionShaderIndex = computeShader.FindKernel("LineIntersection");
        int calculateObstacleIntersections = computeShader.FindKernel("CalculateObstacleIntersections");
        int calculateDistEuclideanShaderIndex = computeShader.FindKernel("CalculateDistanceManhattan");
        int calculateDistManhattanShaderIndex = computeShader.FindKernel("CalculateDistanceEuclidean");
        int calculateDistChevyshevShaderIndex = computeShader.FindKernel("CalculateDistanceChevyshev");
        int calculatFitnessShaderIndex = computeShader.FindKernel("CalculateFitness");

        int vec2Size = sizeof(float) * 2;
        int lineSize = vec2Size * 2;
        int obstacleSize = vec2Size * 4;

        /*-------------------------INTERSECTION----------------------------- */
        computeShader.SetInt("numMovements", SimulationController.Instance.NumMovements);

        //every Line conforming every Obstacle
        ComputeBuffer bufferObstacleLines = new ComputeBuffer(_obstacleBounds.Length, lineSize);
        bufferObstacleLines.SetData(ObstacleBounds);
        computeShader.SetBuffer(lineIntersectionShaderIndex, _obstacleBoundsArrayID, bufferObstacleLines);

        //path of lines of every agent
        ComputeBuffer bufferPathLines = new ComputeBuffer(_agentsPathLines.Length, lineSize);
        bufferPathLines.SetData(AgentsPathLines);
        computeShader.SetBuffer(lineIntersectionShaderIndex, _agentPathArrayID, bufferPathLines);

        //has agent crashed?
        ComputeBuffer bufferAgentsCrashed = new ComputeBuffer(_numAgents, sizeof(int));
        bufferAgentsCrashed.SetData(_agentCrashed);
        computeShader.SetBuffer(lineIntersectionShaderIndex, _agentCrashedArrayID, bufferAgentsCrashed);

        //first crash in agent movement
        ComputeBuffer bufferFirstCollinsionIndex = new ComputeBuffer(_numAgents, sizeof(int));
        bufferFirstCollinsionIndex.SetData(_firstCollisionIndex);
        computeShader.SetBuffer(lineIntersectionShaderIndex, _firstCollisionIndexID, bufferFirstCollinsionIndex);

        //last valid agent position
        ComputeBuffer bufferLastAgentPositions = new ComputeBuffer(_numAgents, vec2Size);
        bufferLastAgentPositions.SetData(_lastAgentPositions);
        computeShader.SetBuffer(lineIntersectionShaderIndex, _lastAgentPositionsID, bufferLastAgentPositions);


        computeShader.Dispatch(lineIntersectionShaderIndex, Mathf.CeilToInt(_obstacleBounds.Length / 8),
                                           Mathf.CeilToInt(SimulationController.Instance.NumMovements / 8),
                                           Mathf.CeilToInt(SimulationController.Instance.NumAgents / 8));

        /*-------------------------OBSTACLES----------------------------- */
        ComputeBuffer bufferObstacles = new ComputeBuffer(_obstacleBounds.Length, obstacleSize);
        bufferObstacles.SetData(_obstacleArray);
        computeShader.SetBuffer(calculateObstacleIntersections, _obstaclesID, bufferObstacles);

        ComputeBuffer bufferNumObstaclesIntersectedWith = new ComputeBuffer(_numAgents, sizeof(int));
        bufferNumObstaclesIntersectedWith.SetData(_numObstaclesIntersectedWith);
        computeShader.SetBuffer(calculateObstacleIntersections, _numObstaclesIntersectedWithID, bufferNumObstaclesIntersectedWith);


        computeShader.SetFloats("_targetPoint", new float[] { _targetPoint.x, _targetPoint.y });

        computeShader.Dispatch(calculateObstacleIntersections, Mathf.CeilToInt(_numAgents / 8), 1, 1);


        /*-------------------------DISTANCES----------------------------- */
        ComputeBuffer bufferDistances = new ComputeBuffer(_numAgents, sizeof(float));
        bufferDistances.SetData(_distances);
        computeShader.SetBuffer(calculateDistEuclideanShaderIndex, _distancesID, bufferDistances);
        computeShader.SetBuffer(calculateDistEuclideanShaderIndex, _lastAgentPositionsID, bufferLastAgentPositions);

        computeShader.Dispatch(calculateDistEuclideanShaderIndex,
                                 Mathf.CeilToInt(_obstacleBounds.Length / 8),
                                 1,
                                 1);


        /*-------------------------FITNESS----------------------------- */

        ComputeBuffer bufferObstacleMultiplier = new ComputeBuffer(_numAgents, sizeof(float));
        bufferObstacleMultiplier.SetData(_obstacleMultiplier);

        ComputeBuffer bufferFitness = new ComputeBuffer(_numAgents, sizeof(float));
        bufferFitness.SetData(_fitness);

        computeShader.SetBuffer(calculatFitnessShaderIndex, _obstacleMultiplierID, bufferObstacleMultiplier);
        computeShader.SetBuffer(calculatFitnessShaderIndex, _numObstaclesIntersectedWithID, bufferNumObstaclesIntersectedWith);

        computeShader.SetBuffer(calculatFitnessShaderIndex, _fitnessID, bufferFitness);
        computeShader.SetBuffer(calculatFitnessShaderIndex, _distancesID, bufferDistances);
        computeShader.SetBuffer(calculatFitnessShaderIndex, _agentCrashedArrayID, bufferAgentsCrashed);

        computeShader.Dispatch(calculatFitnessShaderIndex,
                                 Mathf.CeilToInt(_obstacleBounds.Length / 8),
                                 1,
                                 1);

        /*--------------------------------------------------------------- */

        bufferAgentsCrashed.GetData(_agentCrashed);
        bufferAgentsCrashed.GetData(_agentCrashed);
        bufferFirstCollinsionIndex.GetData(_firstCollisionIndex);
        bufferLastAgentPositions.GetData(_lastAgentPositions);
        bufferNumObstaclesIntersectedWith.GetData(_numObstaclesIntersectedWith);
        bufferDistances.GetData(_distances);

        bufferFitness.GetData(_fitness);

        bufferObstacleLines.Dispose();
        bufferPathLines.Dispose();
        bufferAgentsCrashed.Dispose();
        bufferFirstCollinsionIndex.Dispose();
        bufferLastAgentPositions.Dispose();
        bufferObstacles.Dispose();
        bufferNumObstaclesIntersectedWith.Dispose();
        bufferObstacleMultiplier.Dispose();
        bufferFitness.Dispose();
        bufferDistances.Dispose();

        for (int i = 0; i < _numAgents; i++)
        {
            Debug.Log($"{i}: {_fitness[i]}");
        }
        if (SimulationController.Instance)
        {
            SimulationController.Instance.DataSimulationFinished?.Invoke();
        }
        //TODO: introduce data in data base
    }

    public Vector3[] GetBestSimulation()
    {
        int arrayIndex = GetBestFitnesssIndex();
        Vector3[] positions = new Vector3[_numMovemets];
        for (int i = 0; i < _numMovemets; i++)
        {
            positions[i] = _agentsPathLines[i + (_numMovemets * arrayIndex)].PointA;
        }
        return positions;
    }

    public int GetBestFitnesssIndex()
    {
        float maxFitness = float.MinValue;
        int index = 0;
        for (int i = 0; i < _fitness.Length; i++)
        {
            if (_fitness[i] > maxFitness)
            {
                maxFitness = _fitness[i];
                index = i;
            }
        }
        return index;
    }
}