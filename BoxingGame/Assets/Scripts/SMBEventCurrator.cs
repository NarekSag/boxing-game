using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SMBEventCurrator : MonoBehaviour
{
    [SerializeField]
    private bool debug = false;
    [SerializeField]
    private UnityEvent<string> eventString = new UnityEvent<string>();
    public UnityEvent<string> EventString { get => eventString; }

    private void Awake()
    {
        eventString.AddListener(OnSMBEvent);
    }

    private void OnSMBEvent(string eventName)
    {
        if(debug)
        {
            Debug.Log(eventName);
        }
    }
}
