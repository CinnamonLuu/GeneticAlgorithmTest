using System.Collections.Generic;
using UnityEngine;

public class SimulationResult
{
    List<AgentSimulationData> simulationDatas;
}

public class AgentSimulationData
{
    int movementsDone;
    int distanceDone;
    int obstaclesInTheMiddle;
    int distanceToGoal;

    bool arrived;
    bool crashed;
}

[System.Serializable]
public class SimulationManager : MonoBehaviour
{
    public int numberOfSimulations;
    
    public void EvaluateSimulations()
    {
        //evaluate with the distance, same as doing before 
        //but now I want to add some parameters as the distance done already (the less the better)
        //and the number of movements
    }
}
