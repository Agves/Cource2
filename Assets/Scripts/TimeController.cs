using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    public static event Action<bool> OnTimeLimitEnd = delegate { };
    [SerializeField] private Text timeDisplay;
    private float timeRange = 300;          
    private float timeInterval = 1f;          
    private float timeElapsed;                 
    public float[] timeLapse = new float[2];
    public static TimeController Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        timeElapsed = timeInterval;
    }

    private void Update()
    {
        if (!Board.GameOver)
        {
            float time;

            timeLapse[DicesController.turn] += Time.deltaTime;
            time = timeLapse[DicesController.turn];

            if ((timeElapsed += Time.deltaTime) >= timeInterval)
            {
                timeElapsed = 0;
                UpdateDisplay(time);
            }
        }
    }

    private void UpdateDisplay(float time)
    {
        if (timeDisplay.text == "0:00")             // player has timed out
        {
            OnTimeLimitEnd(DicesController.turn != 0);
            Destroy(this);
            return;
        }

        time = timeRange + 1 - time;

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timeDisplay.text = minutes.ToString() + ":" + seconds.ToString("00");
    }
}
