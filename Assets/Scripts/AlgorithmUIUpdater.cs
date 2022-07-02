using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlgorithmUIUpdater : MonoBehaviour
{
    [SerializeField]
    private Text iterationNumber;
    [SerializeField]
    private Text arrivedNumber;
    [SerializeField]
    private Text noArrivedNumber;
    [SerializeField]
    private Text ratioNumber;
    [SerializeField]
    private Text firstArrivedIteration;

    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button representSimulationButton;

    [SerializeField] private VisualSimulationManager visualSimulationManager;
    [SerializeField] private DataSimulationManager dataSimulationManager;

    public string IterationNumber
    {
        set
        {
            iterationNumber.text = value;
        }
    }
    public string ArrivedNumber
    {
        set
        {
            arrivedNumber.text = value;
        }
    }
    public string NoArrivedNumber
    {
        set
        {
            noArrivedNumber.text = value;
        }
    }
    public string RatioNumber
    {
        set
        {
            ratioNumber.text = value;
        }
    }
    public string FirstArrivedIteration
    {
        set
        {
            firstArrivedIteration.text = value;
        }
    }

    private void Awake()
    {
        visualSimulationManager = FindObjectOfType<VisualSimulationManager>();
        dataSimulationManager = FindObjectOfType<DataSimulationManager>();
        startButton.onClick.AddListener(StartSimulation);
        if (SimulationController.Instance)
        {
            SimulationController.Instance.DataSimulationFinished += EnableRepresentationButton;
        }
    }

    private void StartSimulation()
    {
        if (SimulationController.Instance.visualSimulation)
        {
            visualSimulationManager.StartSimulation();
        }
        else
        {
            dataSimulationManager.StartSimulation();
        }
        startButton.gameObject.SetActive(false);
    }
    private void EnableRepresentationButton()
    {
        representSimulationButton.gameObject.SetActive(true);
    }
}
