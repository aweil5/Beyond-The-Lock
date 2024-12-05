using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    public TextMeshProUGUI text;
    private float startTime;
    private bool isRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        startTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            startTime += Time.deltaTime;
           
        }
        text.text = startTime.ToString("F2");

        
    }
    public void StartStopwatch()
    {
        isRunning = true;
    }
    public void StopStopwatch()
    {
        isRunning = false;
    }
    
}
