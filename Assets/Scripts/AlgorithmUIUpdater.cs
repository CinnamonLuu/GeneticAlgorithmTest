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
}
