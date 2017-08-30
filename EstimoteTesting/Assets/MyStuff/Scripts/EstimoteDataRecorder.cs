using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OMobile.EstimoteUnity;
using System;

public class EstimoteDataRecorder : MonoBehaviour {
    static EstimoteDataRecorder instance;       //the instance of this object
    public List<KnownBeaconValue> knownvalues;
    List<BeaconData> estimoteBeaconData;        //list to record beacondata for each beacon
    EstimoteUnity estimoteUnity;                //ESTIMOTE UNITY
    BeaconData nullBeaconData;                  //represents if beacondata is null (major = -1)

    private void OnEnable()
    {
        instance = this;
    }

    public static EstimoteDataRecorder INSTANCE
    {
        get
        {
            if(instance == null)
            {
                GameObject go = Instantiate(Resources.Load("EstimoteDataRecorder") as GameObject);
                instance = go.GetComponent<EstimoteDataRecorder>();
            }
            return instance;
        }
    }

	// Use this for initialization
	void Start () {
        estimoteBeaconData = new List<BeaconData>();
        estimoteUnity = FindObjectOfType<EstimoteUnity>();
        if(estimoteUnity == null)
        {
            GameObject go = Instantiate(Resources.Load("EstimoteUnity") as GameObject);
            estimoteUnity = go.GetComponent<EstimoteUnity>();
        }
        nullBeaconData = new BeaconData(-1, -1, null);
        estimoteUnity.OnDidRangeBeacons += RecordBeaconData;
        //start scanning after a second
        Invoke("StartScanning", 1);
	}

    void StartScanning()
    {
        estimoteUnity.StartScanning();
    }

    void RecordBeaconData(List<EstimoteUnityBeacon> beacons)
    {
        for(int i = 0; i < beacons.Count; i ++)
        {
            BeaconData thisBeaconData = nullBeaconData;
            for(int j = 0; j < estimoteBeaconData.Count; j++)
            {
                if(beacons[i].Major == estimoteBeaconData[j].major && beacons[i].Minor == estimoteBeaconData[j].minor)
                {
                    estimoteBeaconData[j].AddData(beacons[i].RSSI, beacons[i].LastSeen);
                    return;
                }
            }
            if (thisBeaconData.major == nullBeaconData.major)      //make new beacondata if not found
            {
                thisBeaconData = new BeaconData(beacons[i].Major, beacons[i].Minor, GetName(beacons[i].Major, beacons[i].Minor));
            }
            estimoteBeaconData.Add(thisBeaconData);
            estimoteBeaconData[estimoteBeaconData.Count-1].AddData(beacons[i].RSSI, beacons[i].LastSeen);
        }
    }

    //return name of the beacon if it is found in the local known values, null if not
    string GetName(int major, int minor)
    {
        foreach (KnownBeaconValue val in knownvalues)
        {
            if (val.major == major && val.minor == minor)
            {
                return val.name;
            }
        }
        return null;
    }

    public List<BeaconData> GetData()
    {
        return estimoteBeaconData;
    }

    public float GetDistance(int maj, int min)
    {
        for(int i = 0; i < estimoteBeaconData.Count; i++)
        {
            if (estimoteBeaconData[i].major == maj && estimoteBeaconData[i].minor == min)
                return estimoteBeaconData[i].DISTANCE;      //if found, return distance
        }
        return -1;      //if not found
    }
}

[System.Serializable]
public struct BeaconData
{
    List<BeaconRSSIValue> myRSSIs;      //RSSI values over time
    public string name;
    public int major;                   //major identifier
    public int minor;                   //minor identifier
    static float recordTime = 5;        //record data for 5 seconds, anything older, remove

    public BeaconData(int _major, int _minor, string _name)
    {
        myRSSIs = new List<BeaconRSSIValue>();
        major = _major;
        minor = _minor;
        if (_name == null)
            name = "UNKNOWN";
        else
            name = _name;
    }

    public void AddData(int RSSI, DateTime curTime)
    {
        myRSSIs.Add(new BeaconRSSIValue(RSSI, curTime));
    }

    public int CUR_RSSI
    {
        get
        {
            CleanRSSIs();
            if (myRSSIs.Count < 1)
                return -1;
            return myRSSIs[myRSSIs.Count-1].rssi;
        }
    }

    public float AVG_RSSI               //return the avg_rssi of the rssivalues
    {
        get
        {
            CleanRSSIs();           //get rid of old rssivalues
            if (myRSSIs.Count < 1)
                return -1;
            int sum = 0;
            int i = 0;
            for (i = 0; i < myRSSIs.Count; i++)
            {
                sum += myRSSIs[i].rssi;
            }
            if (i == 0)
                return sum;
            return (float)sum / (float)i;
        }
    }

    public float DISTANCE
    {
        get
        {
            float avg = AVG_RSSI;   //get avgrssi
            if (avg == -1)
                return -1;         //return -1 if avg returns -1 (no rssivalues)

            //formula from comment in...https://forums.estimote.com/t/determine-accurate-distance-of-signal/2858/4
            //TX power represents RSSI at 1 m...
            float distance = -1;
            float RSSIAtOneMeter = -60.5f;
            distance = Mathf.Pow(10, (RSSIAtOneMeter - avg) / 20);

            return Mathf.Abs(distance);
        }
    }

    public DateTime LAST_SEEN
    {
        get
        {
            CleanRSSIs();
            if (myRSSIs.Count < 1)
                return DateTime.MinValue;
            return myRSSIs[myRSSIs.Count - 1].timeRecorded;
        }
    }

    void CleanRSSIs()           //get rid of BeaconRSSIValues that are too old, according to recordTime
    {
        if (myRSSIs.Count < 1)
            return;
        for(int i = myRSSIs.Count-1; i >= 0; i--)
        {
            //if time recorded plus recordTime < now, remove from list
            if(myRSSIs[i].timeRecorded.AddSeconds(recordTime) - DateTime.Now < TimeSpan.Zero)
            {
                myRSSIs.RemoveAt(i);
            }
        }
    }
}


[System.Serializable]
public struct BeaconRSSIValue
{
    public readonly int rssi;
    public readonly DateTime timeRecorded;

    public BeaconRSSIValue(int _rssi, DateTime _time)
    {
        rssi = _rssi;
        timeRecorded = _time;
    }
}

[System.Serializable]
public struct KnownBeaconValue
{
    public string name;
    public int major;
    public int minor;
}
