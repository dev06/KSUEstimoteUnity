using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BeaconDataRow : MonoBehaviour {
    KalmanFilter filter;
    DateTime prevTime;
	// Use this for initialization
	void Start () {
        //each data row needs own filter to keep track of its own beacon data
        filter = new KalmanFilter(.01f, 3);
        prevTime = DateTime.MinValue;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetData(string name, int major, int minor, int curRSSI, float avgRSSI, float distance, DateTime lastSeen)
    {
        //only update when a new second... (fastest update would be once per second)
        if (lastSeen.Second == prevTime.Second)
            return;
        prevTime = lastSeen;
        if (name == null)
            name = "UNKNOWN BEACON";
        transform.Find("Identification").GetComponent<Text>().text = "" + name + "\nMajor: " + major + "\nMinor: " + minor;
        transform.Find("RSSI Data").GetComponent<Text>().text = "Current: " + curRSSI + "\nAverage: " + (int)avgRSSI;
        transform.Find("Other Data").GetComponent<Text>().text = "RawDist: " + distance + "\nFiltDist: " + filter.Filter(distance) +  "\nLast Seen: " + lastSeen;
    }
}
