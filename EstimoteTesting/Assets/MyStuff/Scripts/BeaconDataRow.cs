using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BeaconDataRow : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetData(string name, int major, int minor, int curRSSI, float avgRSSI, float distance, DateTime lastSeen)
    {
        if (name == null)
            name = "UNKNOWN BEACON";
        transform.Find("Identification").GetComponent<Text>().text = "" + name + "\nMajor: " + major + "\nMinor: " + minor;
        transform.Find("RSSI Data").GetComponent<Text>().text = "Current: " + curRSSI + "\nAverage: " + avgRSSI;
        transform.Find("Other Data").GetComponent<Text>().text = "Distance: " + distance + "\nLast Seen: " + lastSeen;
    }
}
