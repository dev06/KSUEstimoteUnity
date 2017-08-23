using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OMobile.EstimoteUnity;

public class CustomBeacon : MonoBehaviour {
    [Header("Identification")]
    public int major;
    public int minor;
    [Header("Display")]
    public Material mat_red;
    public Material mat_orange;
    public Material mat_yellow;
    public Material mat_green;
    public Material mat_blue;

    RoomBeaconManager beaconManager;

    float distance;

    Transform rangeModel;
    TextMesh rangeText;
	// Use this for initialization
	void Start () {
        rangeModel = transform.Find("Range");
        rangeText = transform.Find("Range_Text").GetComponent<TextMesh>();
        beaconManager = FindObjectOfType<RoomBeaconManager>();

        //default state with nothing selected, no beacons found
        rangeModel.GetComponent<Renderer>().material = mat_red;
        DeselectBeacon();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetDistance(EstimoteUnityBeaconRange proximity, float dist)
    {
        distance = dist;
        switch(proximity)
        {
            case EstimoteUnityBeaconRange.IMMEDIATE:
                rangeModel.GetComponent<Renderer>().material = mat_green;
                rangeText.text = "IMMEDIATE: " + dist;
                break;
            case EstimoteUnityBeaconRange.NEAR:
                rangeModel.GetComponent<Renderer>().material = mat_yellow;
                rangeText.text = "NEAR: " + dist;
                break;
            case EstimoteUnityBeaconRange.FAR:
                rangeModel.GetComponent<Renderer>().material = mat_orange;
                rangeText.text = "FAR: " + dist;
                break;
            case EstimoteUnityBeaconRange.UNKNOWN:
                rangeModel.GetComponent<Renderer>().material = mat_red;
                rangeText.text = "UNKNOWN";
                break;
            default:
                break;
        }
    }

    public void SetDistance2(float rssi, float Power)
    {


        if (rssi == 0 || Power == 0)
        {
            distance = -1;
            rangeModel.localScale = Vector3.one * 0;
            rangeText.text = "" + distance;
        }

        else
        {
            float ratio2 = Power - rssi;
            float ratio2_linear = Mathf.Pow(10, ratio2 / 10);
            float y = 0;
            var r = Mathf.Sqrt(ratio2_linear);

            float ratio = rssi / Power;
            if (ratio < 1.0)
            {
                y = Mathf.Pow(ratio, 10);
            }
            else
            {
                y = (0.89976f) * Mathf.Pow(ratio, 7.7095f) + 0.111f;
            }
            distance = y * 10;
            rangeModel.localScale = Vector3.one * distance;
            rangeText.text = "" + distance;
        }

    }

    public void SelectBeacon()
    {
        transform.Find("SelectedIndicator").gameObject.SetActive(true);
    }

    public void DeselectBeacon()
    {
        transform.Find("SelectedIndicator").gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (beaconManager == null)
            beaconManager = FindObjectOfType<RoomBeaconManager>();
        beaconManager.SelectBeacon(this);
    }
}
