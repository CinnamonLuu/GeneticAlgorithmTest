using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class GPUIntersectionChecker
{
    private int _numAgents;
    private int _numMovements;
    private int _numObstacles;
    private TypeOfDistance typeOfDistance;

    [SerializeField] private Vector2[] u;

    /*-------------------------INTERSECTION----------------------------- */
    [SerializeField]private Line[] _obstacleBounds;
    [SerializeField]private Line[] _agentsPathLines;
    [SerializeField]private int[] _agentCrashed;
    [SerializeField]private bool[] _agentCrashedBool;

    [SerializeField]private int[] _firstCollisionIndex;
    [SerializeField]private Vector2[] _lastAgentPositions;
    /*-------------------------OBSTACLES----------------------------- */
    [SerializeField]private Obstacle[] _obstacleArray;
    [SerializeField]private int[] _numObstaclesIntersectedWith;


    /*-------------------------FITNESS----------------------------- */
    [SerializeField]private float[] _obstacleMultiplier;
    [SerializeField]private float[] _fitness;


    /*-------------------------DISTANCES----------------------------- */
    private Vector2 _spawnPoint;
    private Vector2 _targetPoint;

    [SerializeField] private float[] _distances;


    public Line[] AgentsPathLines { get => _agentsPathLines; set => _agentsPathLines = value; }
    public Line[] ObstacleBounds { get => _obstacleBounds; set => _obstacleBounds = value; }
    public Obstacle[] ObstacleArray { get => _obstacleArray; set => _obstacleArray = value; }


    private static readonly int
        _obstacleBoundsArrayID = Shader.PropertyToID("_obstacleBounds"),
        _agentPathArrayID = Shader.PropertyToID("_path"),
        _agentCrashedArrayID = Shader.PropertyToID("_intersects"),
        _firstCollisionIndexID = Shader.PropertyToID("_firstCollisionIndex"),
        _lastAgentPositionsID = Shader.PropertyToID("_lastAgentPositions"),

        _obstaclesID = Shader.PropertyToID("_obstacles"),
        _numObstaclesIntersectedWithID = Shader.PropertyToID("_numObstaclesIntersectedWith"),

        _obstacleMultiplierID = Shader.PropertyToID("_obstacleMultiplier"),
        _fitnessID = Shader.PropertyToID("_fitness"),

        uID = Shader.PropertyToID("u"),

        _distancesID = Shader.PropertyToID("_distances");



    private ComputeShader computeShader;

    public void Init(int numAgents, int numMovements, float stepPathMultiplier, Vector2 spawnPoint, Vector2 targetPoint, Line[] obstacleBounds, Obstacle[] obstacleArray, TypeOfDistance type)
    {

        _numAgents = numAgents;
        _numMovements = numMovements;
        _numObstacles = obstacleArray.Length;
        typeOfDistance = type;

        #region Initialize intersection Variables
        _obstacleBounds = obstacleBounds;
        List<Line> agentsPathLines = new List<Line>();
        for (int i = 0; i < numAgents; i++)
        {
            agentsPathLines.AddRange(DNA_DataSimulation.DNA_DataSimulationDebug(stepPathMultiplier, spawnPoint, SimulationController.Instance.NumMovements).lines);
        }
        _agentsPathLines = agentsPathLines.ToArray();
        _agentCrashed = new int[_numAgents];

        _firstCollisionIndex = new int[_numAgents];
        for (int i = 0; i < _firstCollisionIndex.Length; i++)
        {
            _firstCollisionIndex[i] = _numMovements - 1;
        }
        _spawnPoint = spawnPoint;
        _lastAgentPositions = new Vector2[_numAgents];
        for (int i = 0; i < _lastAgentPositions.Length; i++)
        {
            _lastAgentPositions[i] = _agentsPathLines[((i + 1) * _numMovements) - 1 ].PointB;
        }
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

        _agentCrashedBool = new bool[numAgents];

        u = new Vector2[_numMovements * ObstacleBounds.Length];
        computeShader = Resources.Load<ComputeShader>("ComputeShaders/LineSegmentIntersection");
        //CheckIntersectionGPU();
        //CheckIntersectionCPU();
    }

    public void ResetVariables()
    {
        _firstCollisionIndex = new int[_numAgents];
        for (int i = 0; i < _firstCollisionIndex.Length; i++)
        {
            _firstCollisionIndex[i] = _numMovements - 1;
        }

        _lastAgentPositions = new Vector2[_numAgents];
        for (int i = 0; i < _lastAgentPositions.Length; i++)
        {
            _lastAgentPositions[i] = _agentsPathLines[((i + 1) * _numMovements) - 1].PointB;
        }

        _numObstaclesIntersectedWith = new int[_numAgents];

        _obstacleMultiplier = new float[_numAgents];
        _fitness = new float[_numAgents];

        _distances = new float[_numAgents];
        _agentCrashedBool = new bool[_numAgents];
        _agentCrashed = new int[_numAgents];

        u = new Vector2[_numMovements * ObstacleBounds.Length];
    }

    public void CheckIntersectionCPU()
    {
        for (int i = 0; i < _obstacleBounds.Length; i++)
        {
            for (int j = 0; j < _numMovements; j++)
            {
                for (int k = 0; k < _numAgents; k++)
                {
                    LineIntersection(new Vector3(i, j, k));
                }
            }
        }

        for (int i = 0; i < _numAgents; i++)
        {
            //CalculateObstacleIntersections(i);
            switch (SimulationController.Instance.typeOfDistance)
            {
                case TypeOfDistance.Manhattan:
                    _distances[i] = Mathf.Abs(_lastAgentPositions[i].x - _targetPoint.x)
                        * Mathf.Abs(_lastAgentPositions[i].y - _targetPoint.y);
                    break;
                case TypeOfDistance.Euclidean:
                    _distances[i] = Vector2.Distance(_lastAgentPositions[i], _targetPoint);
                    break;
                case TypeOfDistance.Chebyshev:
                    _distances[i] = Mathf.Max(_lastAgentPositions[i].x - _targetPoint.x,
                            _lastAgentPositions[i].y - _targetPoint.y);

                    break;
            }

            _obstacleMultiplier[i] = 1 - (0.1f * _numObstaclesIntersectedWith[i]);

            _fitness[i] = 60 / _distances[i]
                            * (_agentCrashedBool[i] ? 0.65f : 1f)
                            * _obstacleMultiplier[i];

            Debug.Log($"CPU: Fitness <color=red>{i}</color>: {_fitness[i]} ");
        }


        void LineIntersection(Vector3 id)
        {

            int currentAgentMovementIndex = (int)id.z * _numMovements + (int)id.y;

            bool intersects = AreLinesIntersecting(
                _obstacleBounds[(int)id.x].PointA,
                _obstacleBounds[(int)id.x].PointB,
                _agentsPathLines[currentAgentMovementIndex].PointA,
                _agentsPathLines[currentAgentMovementIndex].PointB);

            _agentCrashedBool[(int)id.z] |= intersects;
            _agentCrashed[(int)id.z] = _agentCrashedBool[(int)id.z] ? 1 : 0;

            bool improves = (_firstCollisionIndex[(int)id.z] > (int)id.y);

            _firstCollisionIndex[(int)id.z] = intersects && improves ? (int)id.y : _firstCollisionIndex[(int)id.z];

            _lastAgentPositions[(int)id.z] = intersects && improves
                                            ? CalculateIntersectionPosition(_obstacleBounds[(int)id.x].PointA, _obstacleBounds[(int)id.x].PointB,
                                                                            _agentsPathLines[currentAgentMovementIndex].PointA, _agentsPathLines[currentAgentMovementIndex].PointB)
                                            : _lastAgentPositions[(int)id.z];
        }

        bool AreLinesIntersecting(Vector2 obsA, Vector2 obsB, Vector2 pathA, Vector2 pathB)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = 0.00001f;

            bool isIntersecting = false;

            float a = (pathB.y - pathA.y),
            b = (obsB.x - obsA.x),
            c = (pathB.x - pathA.x),
            d = (obsB.y - obsA.y);

            float e = a * b,
            f = c * d,
            g = e - f;
            float denominator2 = g;


            float denominator = (pathB.y - pathA.y) * (obsB.x - obsA.x) 
                               - (pathB.x - pathA.x) * (obsB.y - obsA.y) ;

            //Make sure the denominator is > 0, if so the lines are parallel
            if (denominator != 0)
            {
                float u_a = ((pathB.x - pathA.x) * (obsA.y - pathA.y) - (pathB.y - pathA.y) * (obsA.x - pathA.x)) / denominator;
                float u_b = ((obsB.x - obsA.x) * (obsA.y - pathA.y) - (obsB.y - obsA.y) * (obsA.x - pathA.x)) / denominator;



                //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                {
                    Debug.Log($"CPU: u_a:{u_a}, u_b:{u_b}, Denominator{denominator}" +
                        $"Crash in : {obsA},{obsB}x{pathA},{pathB}");
                    isIntersecting = true;
                }
            }
            return isIntersecting;
        }

        Vector2 CalculateIntersectionPosition(Vector2 obsA, Vector2 obsB, Vector2 pathA, Vector2 pathB)
        {

            float denominator = (pathB.y - pathA.y) * (obsB.x - obsA.x)
                                - (pathB.x - pathA.x) * (obsB.y - obsA.y);

            float u_a = ((pathB.x - pathA.x) * (obsA.y - pathA.y) - (pathB.y - pathA.y) * (obsA.x - pathA.x)) / denominator;
            float u_b = ((obsB.x - obsA.x) * (obsA.y - pathA.y) - (obsB.y - obsA.y) * (obsA.x - pathA.x)) / denominator;

            return new Vector2(obsA.x + u_a * (obsB.x - obsA.x),
                                obsA.y + u_a * (obsB.y - obsA.y));
        }

        void CalculateObstacleIntersections(int id)
        {

            Line line = new Line();
            line.PointA = _lastAgentPositions[id];
            line.PointB = _targetPoint;

            _numObstaclesIntersectedWith[id] = CalculateNumOfObstaclesIntersectedWith(line);
        }

        int CalculateNumOfObstaclesIntersectedWith(Line lastPosToTarget)
        {
            int numObstaclesIntersected = 0;

            //Each obstacle always have 4 lines
            for (int i = 0; i < _numObstacles; i++)
            {
                if (AreLinesIntersecting(_obstacleArray[i].lineA.PointA,
                                            _obstacleArray[i].lineA.PointB,
                                            lastPosToTarget.PointA,
                                            lastPosToTarget.PointB))
                {
                    numObstaclesIntersected++;
                }
                else if (AreLinesIntersecting(_obstacleArray[i].lineB.PointA,
                                                _obstacleArray[i].lineB.PointB,
                                                lastPosToTarget.PointA,
                                                lastPosToTarget.PointB))
                {
                    numObstaclesIntersected++;
                }
                else if (AreLinesIntersecting(_obstacleArray[i].lineC.PointA,
                                                _obstacleArray[i].lineC.PointB,
                                                lastPosToTarget.PointA,
                                                lastPosToTarget.PointB))
                {
                    numObstaclesIntersected++;
                }
                else if (AreLinesIntersecting(_obstacleArray[i].lineD.PointA,
                                                _obstacleArray[i].lineD.PointB,
                                                lastPosToTarget.PointA,
                                                lastPosToTarget.PointB))
                {
                    numObstaclesIntersected++;
                }
            }
            return numObstaclesIntersected;
        }
    }

    private void oldCheckIntersectionCPU()
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
        int calculateObstacleIntersectionsIndex = computeShader.FindKernel("CalculateObstacleIntersections");
        int calculateDistEuclideanShaderIndex = computeShader.FindKernel("CalculateDistanceManhattan");
        int calculateDistManhattanShaderIndex = computeShader.FindKernel("CalculateDistanceEuclidean");
        int calculateDistChevyshevShaderIndex = computeShader.FindKernel("CalculateDistanceChevyshev");
        int calculatFitnessShaderIndex = computeShader.FindKernel("CalculateFitness");

        int vec2Size = sizeof(float) * 2;
        int lineSize = vec2Size * 2;
        int obstacleSize = vec2Size * 4;

        /*-------------------------INTERSECTION----------------------------- */
        computeShader.SetInt("numMovements", SimulationController.Instance.NumMovements);
        computeShader.SetInt("numAgents", SimulationController.Instance.NumAgents);

        //every Line conforming every Obstacle
        ComputeBuffer bufferObstacleLines = new ComputeBuffer(_obstacleBounds.Length, lineSize);
        bufferObstacleLines.SetData(_obstacleBounds);
        computeShader.SetBuffer(lineIntersectionShaderIndex, _obstacleBoundsArrayID, bufferObstacleLines);

        //path of lines of every agent
        ComputeBuffer bufferPathLines = new ComputeBuffer(_agentsPathLines.Length, lineSize);
        bufferPathLines.SetData(_agentsPathLines);
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


        ComputeBuffer bufferU = new ComputeBuffer(_numMovements * ObstacleBounds.Length, vec2Size);
        bufferU.SetData(u);
        computeShader.SetBuffer(lineIntersectionShaderIndex, uID, bufferU);


        computeShader.Dispatch(lineIntersectionShaderIndex, Mathf.CeilToInt(_obstacleBounds.Length / 8.0f),
                                           Mathf.CeilToInt(_numMovements / 8.0f),
                                           Mathf.CeilToInt(_numAgents / 8.0f));

        bufferObstacleLines.GetData(_obstacleBounds);
        bufferPathLines.GetData(_agentsPathLines);
        bufferLastAgentPositions.GetData(_lastAgentPositions);
        bufferU.GetData(u);

        /*-------------------------OBSTACLES----------------------------- */
        ComputeBuffer bufferObstacles = new ComputeBuffer(_obstacleBounds.Length, obstacleSize);
        bufferObstacles.SetData(_obstacleArray);
        computeShader.SetBuffer(calculateObstacleIntersectionsIndex, _obstaclesID, bufferObstacles);

        ComputeBuffer bufferNumObstaclesIntersectedWith = new ComputeBuffer(_numAgents, sizeof(int));
        bufferNumObstaclesIntersectedWith.SetData(_numObstaclesIntersectedWith);
        computeShader.SetBuffer(calculateObstacleIntersectionsIndex, _numObstaclesIntersectedWithID, bufferNumObstaclesIntersectedWith);

        bufferLastAgentPositions.SetData(_lastAgentPositions);
        computeShader.SetBuffer(calculateObstacleIntersectionsIndex, _lastAgentPositionsID, bufferLastAgentPositions);

        computeShader.SetFloats("_targetPoint", new float[] { _targetPoint.x, _targetPoint.y });

        computeShader.Dispatch(calculateObstacleIntersectionsIndex, Mathf.CeilToInt(_numAgents / 8.0f), 1, 1);


        /*-------------------------DISTANCES----------------------------- */
        ComputeBuffer bufferDistances = new ComputeBuffer(_numAgents, sizeof(float));
        bufferDistances.SetData(_distances);
        switch (SimulationController.Instance.typeOfDistance)
        {
            case TypeOfDistance.Manhattan:
                computeShader.SetBuffer(calculateDistManhattanShaderIndex, _distancesID, bufferDistances);
                computeShader.SetBuffer(calculateDistManhattanShaderIndex, _lastAgentPositionsID, bufferLastAgentPositions);

                computeShader.Dispatch(calculateDistManhattanShaderIndex,
                         Mathf.CeilToInt(_obstacleBounds.Length / 8.0f),
                         1,
                         1);
                break;
            case TypeOfDistance.Euclidean:
                computeShader.SetBuffer(calculateDistEuclideanShaderIndex, _distancesID, bufferDistances);
                computeShader.SetBuffer(calculateDistEuclideanShaderIndex, _lastAgentPositionsID, bufferLastAgentPositions);

                computeShader.Dispatch(calculateDistEuclideanShaderIndex,
                         Mathf.CeilToInt(_obstacleBounds.Length / 8.0f),
                         1,
                         1);
                break;
            case TypeOfDistance.Chebyshev:
                computeShader.SetBuffer(calculateDistChevyshevShaderIndex, _distancesID, bufferDistances);
                computeShader.SetBuffer(calculateDistChevyshevShaderIndex, _lastAgentPositionsID, bufferLastAgentPositions);

                computeShader.Dispatch(calculateDistChevyshevShaderIndex,
                         Mathf.CeilToInt(_obstacleBounds.Length / 8.0f),
                         1,
                         1);
                break;
        }


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
                                 Mathf.CeilToInt(_obstacleBounds.Length / 8.0f),
                                 1,
                                 1);

        /*--------------------------------------------------------------- */

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


        bufferU.Dispose();

        for (int i = 0; i < _numAgents; i++)
        {
            Debug.Log($"GPU: Fitness <color=green>{i}</color>: {_fitness[i]} ");
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
        Vector3[] positions = new Vector3[_numMovements];
        for (int i = 0; i < _numMovements; i++)
        {
            positions[i] = _agentsPathLines[i + (_numMovements * arrayIndex)].PointA;
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