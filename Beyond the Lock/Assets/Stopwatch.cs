using UnityEngine;
using TMPro;
public class Stopwatch : MonoBehaviour
{
    private float stopwatchVal;
    public TextMeshProUGUI timerText;
    public bool str;

    // Start is called before the first frame update
    void Start()
    {
        stopwatchVal = 0;
        str = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (str)
        {
            stopwatchVal += Time.deltaTime;
        }
        timerText.text = stopwatchVal.ToString("F2");
        
    }

    public void StartStopwatch()
    {
        str = true;
    }

    public void StopStopwatch()
    {
        str = false;
    }
}
