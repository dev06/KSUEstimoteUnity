using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OMobile.EstimoteUnity;
public class Trilateration : MonoBehaviour {


	EstimoteUnity estimoteUnity;
	public Text discoveredBeacons, distance;
	public GameObject beaconLabel;
	private Transform parent;
	private List<EstimoteUnityBeacon> beacons;


	Vector3 b1_1 = new Vector3(0, 0, 0);
	Vector3 b1_4 = new Vector3(10, 0, 0);
	Vector3 b1_6 = new Vector3(10, 0, -10);
	private double A;
	private double B;
	private double C;


	private double d1;
	private double d2;
	private double d3;

	private double c1_x;
	private double c1_y;
	private double c2_x;
	private double c2_y;
	private double c3_x;
	private double c3_y;

	private double x;
	private double y;

	private double b1_1_dist;
	private double b1_4_dist;
	private double b1_6_dist;


	void OnEnable()
	{
		if (estimoteUnity == null)
		{
			estimoteUnity = FindObjectOfType<EstimoteUnity>();
		}

		estimoteUnity.OnDidRangeBeacons += OnDidRangeBeacons;
	}
	void OnDisable()
	{

		estimoteUnity.OnDidRangeBeacons -= OnDidRangeBeacons;
	}
	void Start () {

//		parent = GameObject.FindWithTag("UI/ScrollContent").transform;
		Invoke("StartScanning", 1);

	}

	void StartScanning()
	{
		estimoteUnity.StartScanning();
	}

	// Update is called once per frame
	void Update ()
	{

		AssignDistance();
		c1_x = b1_1.x;
		c1_y = b1_1.z;

		c2_x = b1_4.x;
		c2_y = b1_4.z;

		c3_x = b1_6.x;
		c3_y = b1_6.z;

		A = (c1_x * c1_x) + (c1_y * c1_y) - (b1_1_dist * b1_1_dist);
		B = (c2_x * c2_x) + (c2_y * c2_y) - (b1_4_dist * b1_4_dist);
		C = (c3_x * c3_x) + (c3_y * c3_y) - (b1_6_dist * b1_6_dist);
		double X_P1 = A * (c3_y - c2_y);
		double X_P2 = B * (c1_y - c3_y);
		double X_P3 = C * (c2_y - c1_y);

		double X_P4 = c1_x * (c3_y - c2_y);
		double X_P5 = c2_x * (c1_y - c3_y);
		double X_P6 = c3_x * (c2_y - c1_y);

		double X_TOP = X_P1 + X_P2 + X_P3;
		double X_BOT = 2 * (X_P4 + X_P5 + X_P6);

		x = X_TOP / X_BOT;

		double Y_P1 = A * (c3_x - c2_x);
		double Y_P2 = B * (c1_x - c3_x);
		double Y_P3 = C * (c2_x - c1_x);

		double Y_P4 = c1_y * (c3_x - c2_x);
		double Y_P5 = c2_y * (c1_x - c3_x);
		double Y_P6 = c3_y * (c2_x - c1_x);

		double Y_TOP = Y_P1 + Y_P2 + Y_P3;
		double Y_BOT = 2 * (Y_P4 + Y_P5 + Y_P6);

		y = Y_TOP / Y_BOT;

		distance.text = x.ToString("F2") + " " + y.ToString("F2") + "\n" + "B1-1 Distance: " + b1_1_dist + "\n" + " B1-4 Distance: " + " " + b1_4_dist
		                + "\n" + " B1-6 Distance: " + b1_6_dist;



	}

	public void AssignDistance()
	{
		for (int i = 0; i < beacons.Count; i++)
		{
			string name = GetBeaconName(beacons[i].Major, beacons[i].Minor);
			switch (name)
			{
				case "B1-1": b1_1_dist = Mathf.Round((float)(beacons[i].Accuracy * 100f)) / 100f; break;
				case "B1-4": b1_4_dist = Mathf.Round((float)(beacons[i].Accuracy * 100f)) / 100f; break;
				case "B1-6": b1_6_dist = Mathf.Round((float)(beacons[i].Accuracy * 100f)) / 100f; break;
			}
		}
	}

	int count = 0;
	public void OnDidRangeBeacons(List<EstimoteUnityBeacon> beacons)
	{
		count++;
		this.beacons =  beacons;
		discoveredBeacons.text = "";

		for (int i = 0; i < beacons.Count; i++)
		{
			if (beacons[i].Accuracy < 15)
			{
				discoveredBeacons.text += GetBeaconName(beacons[i].Major, beacons[i].Minor) + " " + beacons[i].Major + " " + beacons[i].Minor + " " + beacons[i].RSSI +   " " + beacons[i].Accuracy + "\n";
			}
		}
		// PopulateLabels(beacons);
	}

	public string GetBeaconName(int major, int minor)
	{
		if (major == 18017 && minor == 29391) return "B1-1";
		if (major == 63382 && minor == 48144) return "B1-2";
		if (major == 28991 && minor == 52996) return "B1-3";
		if (major == 49679 && minor == 13454) return "B1-4";
		if (major == 63382 && minor == 6058) return "B1-5";
		if (major == 4474 && minor == 42339) return "B1-6";

		return "Unnamed";



	}


	// private void PopulateLabels(List<EstimoteUnityBeacon> beacons)
	// {
	// 	if (parent == null)
	// 	{
	// 		parent = GameObject.FindWithTag("UI/ScrollContent").transform;
	// 	}


	// 	if (parent.childCount > 0)
	// 	{
	// 		foreach (GameObject go in parent)
	// 		{
	// 			Destroy(go);
	// 		}
	// 	}

	// 	for (int i = 0; i < beacons.Count; i++)
	// 	{
	// 		GameObject beaconLabelClone = Instantiate(beaconLabel) as GameObject;
	// 		beaconLabelClone.transform.SetParent(parent);
	// 		RectTransform rt = beaconLabelClone.GetComponent<RectTransform>();
	// 		rt.anchoredPosition = new Vector3(0, -i * rt.sizeDelta.y * 1.05f, 0);
	// 		rt.localScale = new Vector3(1, 1, 1);

	// 		beaconLabelClone.transform.GetChild(0).GetComponent<Text>().text = beacons[i].RSSI + " " + beacons[i].Major + " " + beacons[i].Minor + " " + rt.offsetMin + " " + rt.offsetMax;
	// 	}
	// }
}
