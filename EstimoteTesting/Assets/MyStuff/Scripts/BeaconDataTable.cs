using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OMobile.EstimoteUnity;

public class BeaconDataTable : MonoBehaviour {
    EstimoteDataRecorder data;
    bool scanning;

	// Use this for initialization
	void Start () {
        data = EstimoteDataRecorder.INSTANCE;
    }
	
	// Update is called once per frame
	void Update () {
        
        List<BeaconData> beacondata = data.GetData();
        if (beacondata == null)
            return;
        int i;
        for(i = 0; i < beacondata.Count; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
            transform.GetChild(i).GetComponent<RectTransform>().localPosition = new Vector3(0, (-220*i) + 860, 0);
            transform.GetChild(i).GetComponent<BeaconDataRow>().SetData(beacondata[i].name,beacondata[i].major, beacondata[i].minor, beacondata[i].CUR_RSSI, beacondata[i].AVG_RSSI, beacondata[i].DISTANCE, beacondata[i].LAST_SEEN);
        }
        for(int j = i; j < transform.childCount; j++)
        {
            transform.GetChild(j).gameObject.SetActive(false);
        }
    }
}


