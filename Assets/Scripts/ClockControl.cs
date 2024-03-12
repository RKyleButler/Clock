using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

public class ClockControl : MonoBehaviour
{
    public Transform SecondHand;
    public Transform MinuteHand;
    public Transform HourHand;

    public GameObject ClockFace;
    public GameObject HourMarks;
    public GameObject MinuteMarks;

    public bool Analog = true;

    //  UnityEvent tracking
    public static UnityEvent OnSecond;
    public static UnityEvent OnMinute;
    public static FloatParamEvent OnHour;
    public static UnityEvent QuarterHour;
    public static UnityEvent HalfHour;

    private float previousSecond;
    private bool SecHasHitZero = false;
    private bool MinHasHitZero = false;

    void Start()
    {
        //  Build Clock Face
        for (int i = 0; i < 12; i++)
        {
            float minuteAngel = i * 30f;
            Instantiate(HourMarks, Vector3.zero, Quaternion.Euler(0f, minuteAngel, 0f), ClockFace.transform);
            for (int j = 1; j <= 4; j++)
            {
                Instantiate(MinuteMarks, Vector3.zero, Quaternion.Euler(0f, minuteAngel + (j*(360/60)), 0f), ClockFace.transform);
            }
        }

        //  Set up events
        if (OnSecond == null)
            OnSecond = new UnityEvent();
        previousSecond = System.DateTime.Now.Second;

        if (OnMinute == null)
            OnMinute = new UnityEvent();

        if (OnHour == null)
            OnHour = new FloatParamEvent();

        if (QuarterHour == null)
            QuarterHour = new UnityEvent();

        if (HalfHour == null)
            HalfHour = new UnityEvent();


        //  Add local listeners
        OnSecond.AddListener(SecondEvent);
        OnMinute.AddListener(MinuteEvent);
        OnHour.AddListener(HourEvent);
        QuarterHour.AddListener(QuarterEvent);
        HalfHour.AddListener(HalfHourEvent);
    }

    private void OnDisable()
    {
        OnSecond.RemoveListener(SecondEvent);
        OnMinute.RemoveListener(MinuteEvent);
        OnHour.RemoveListener(HourEvent);
        QuarterHour.RemoveListener(QuarterEvent);
        HalfHour.RemoveListener(HalfHourEvent);
    }

    void FixedUpdate()
    {
        float seconds = System.DateTime.Now.Second;
        float minutes = System.DateTime.Now.Minute;
        float hours = System.DateTime.Now.Hour;

        if (Analog)
            Analog_SetTime((int)hours, (int)minutes, (int)seconds);
        else
            Digital_SetTime(hours, minutes, seconds);
    }

    private void Analog_SetTime(int hours, int minutes, int seconds)
    {
        HourHand.rotation = Quaternion.Euler(0, hours * 360 / 12, 0);
        MinuteHand.rotation = Quaternion.Euler(0, minutes * 360 / 60, 0);
        SecondHand.rotation = Quaternion.Euler(0, seconds * 360 / 60, 0);

        CheckTimingEvent(hours, minutes, seconds);
    }

    private void Digital_SetTime(float hours, float minutes, float seconds)
    {
        float milliSeconds = System.DateTime.Now.TimeOfDay.Milliseconds;
        seconds += milliSeconds/1000;
        minutes += seconds/60;
        hours += minutes/60;

        HourHand.rotation = Quaternion.Euler(0, hours * 360 / 12, 0);
        MinuteHand.rotation = Quaternion.Euler(0, minutes * 360 / 60, 0);
        SecondHand.rotation = Quaternion.Euler(0, seconds * 360 / 60, 0);

        CheckTimingEvent((int)hours, (int)minutes, (int)seconds);
    }

    private void CheckTimingEvent(int hours, int minutes, int seconds)
    {
        //  Check each second
        if (seconds > previousSecond)
        {
            SecHasHitZero = false;
            OnSecond.Invoke();
            previousSecond = seconds;
        }
        //  seconds reaching zero is a new minute
        else if(seconds == 0 && !SecHasHitZero)
        {
            OnSecond.Invoke();
            previousSecond = 0;
            SecHasHitZero = true;
            Debug.Log($"Minute - {minutes}!");

            //  every time seconds is zero that is the top of the minute.
            OnMinute.Invoke();
            MinHasHitZero = false;
            if(minutes == 0 && !MinHasHitZero)
            {
                //  every time minutes is zero that is the top of the hour.
                OnHour.Invoke(hours);
                MinHasHitZero = true;
                return;
            }

            if (minutes == 15)
            {
                Debug.Log($"That's a quarter after {hours}!");
                QuarterHour.Invoke();
                return;
            }
            if (minutes == 30)
            {
                Debug.Log($"That's half passed {hours}!");
                HalfHour.Invoke();
                return;
            }
            if (minutes == 45)
            {
                if (hours != 23)
                {
                    Debug.Log($"That's a quarter till {hours + 1}!");
                    QuarterHour.Invoke();
                }
                else
                {
                    Debug.Log($"That's a quarter till {0}!");
                    QuarterHour.Invoke();
                }
                QuarterHour.Invoke();
                return;
            }
        }
    }

    void SecondEvent()
    {
        //Debug.Log("That's a Second!");
    }

    void MinuteEvent()
    {
        //Debug.Log("That's a Minute!");
    }

    void HourEvent(float hours)
    {
        Debug.Log($"That's the top of the {hours} hour!");
    }
    void QuarterEvent()
    {
        Debug.Log("Quarter hour mark!");
    }
    void HalfHourEvent()
    {
        Debug.Log("Half hour mark!");
    }
}

public class FloatParamEvent : UnityEvent<float> { }