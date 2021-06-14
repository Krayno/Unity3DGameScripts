using System.Diagnostics;
using System.Threading;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using System.Collections;
using System.Timers;

public class AccurateClock : MonoBehaviour
{
    public EventTimer Tick;

    private UnityClient Client; //The client that connects to the server.

    private System.Timers.Timer StartTickAt = null;

    private void Awake()
    {
        Tick = new EventTimer(true, ClientGlobals.TickRate);

        Client = transform.parent.GetComponent<UnityClient>();
    }

    private void OnDisable()
    {
        Tick.SetEnabled(false);
    }

    private void OnEnable()
    {
        Client.MessageReceived += OnMessageReceived;
    }

    //Start the tick based off the RTT of the message. Observed to be 20-30 ms ahead of the server.
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        StartTickAt = new System.Timers.Timer(ClientGlobals.WorldServer.Client.RoundTripTime.LatestRtt);
        StartTickAt.Elapsed += OnTimedEvent;
        StartTickAt.Enabled = true;
        Client.MessageReceived -= OnMessageReceived;
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        Tick.SetEnabled(true);
        StartTickAt.Dispose();
    }

    private void Update()
    {
        EventTimerManager.UpdateTimers(Time.deltaTime);
    }
}

//Smart things below that I did not write.

public delegate void EventTimerTickedEventHandler(object sender, EventTimerTickedArgs e);
public class EventTimerTickedArgs
{
    public readonly EventTimer Timer;
    public readonly float DeltaTime;

    public EventTimerTickedArgs(EventTimer timer, float deltaTime)
    {
        Timer = timer;
        DeltaTime = deltaTime;
    }
}
public class EventTimer
{
    private bool _enabled;
    private float _interval, _time;

    public event EventTimerTickedEventHandler Ticked;
    protected virtual void OnTimerTicked(EventTimerTickedArgs e)
    {
        Ticked?.Invoke(this, e);
    }
    public void OnTimerTickedTrigger(EventTimerTickedArgs e)
    {
        OnTimerTicked(e);
    }

    public EventTimer()
    {
        _interval = 1;
        _time = 0;
        _enabled = false;

        EventTimerManager.AddTimer(this);
    }

    public EventTimer(float interval)
    {
        _interval = interval;
        _time = 0;
        _enabled = false;

        EventTimerManager.AddTimer(this);
    }

    public EventTimer(bool state, float interval)
    {
        _interval = interval;
        _time = 0;
        _enabled = state;

        EventTimerManager.AddTimer(this);
    }

    public bool IsEnabled() { return _enabled; }
    public float GetInterval() { return _interval; }
    public float GetTime() { return _time; }

    public void SetEnabled(bool state) { _enabled = state; }
    public void SetInterval(float interval) { _interval = interval; }
    public void SetTime(float time) { _time = time; }
    public void IncrementTime(float time) { _time += time; }
    public void DecrementTime(float time) { _time -= time; }
}
public static class EventTimerManager
{
    private static List<EventTimer> _eventTimers = new List<EventTimer>();

    public static void AddTimer(EventTimer timer)
    {
        if (!_eventTimers.Contains(timer))
            _eventTimers.Add(timer);
    }

    public static void RemoveTimer(EventTimer timer)
    {
        if (_eventTimers.Contains(timer))
            _eventTimers.Remove(timer);
    }

    public static void UpdateTimers(float DeltaTime)
    {
        _incrementTimers(DeltaTime);
    }

    private static void _incrementTimers(float time)
    {
        foreach (EventTimer timer in _eventTimers.Where(t => t.IsEnabled()).ToList())
        {
            timer.IncrementTime(time);

            if (timer.GetTime() >= timer.GetInterval())
            {
                timer.DecrementTime(timer.GetInterval());
                timer.OnTimerTickedTrigger(new EventTimerTickedArgs(timer, time));
            }
        }
    }
}
