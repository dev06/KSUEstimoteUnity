using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using UnityEngine.UI;

namespace OMobile.EstimoteUnity
{
    public class RoomBeaconManager : MonoBehaviour {

        EstimoteUnity estimoteUnity;                    //ESTIMOTE UNITY
        List<EstimoteUnityBeacon> estimoteBeacons;      //list of the beacons found by Estimote SDK
        CustomBeacon[] customBeacons;                   //list of our "beacons" in the scene

        //Estimote Beacons that are showing the data
        EstimoteUnityBeacon closestBeacon;
        EstimoteUnityBeacon selectedBeacon;

        //UI's used to display data
        Transform ui_selected;
        Transform ui_closest;

        // Use this for initialization
        void Start() {
            estimoteUnity = FindObjectOfType<EstimoteUnity>();
            //add event to delegate so when estimoteUnity find beacons, it calls "HandleDidRangeBeacons"
            estimoteUnity.OnDidRangeBeacons += HandleDidRangeBeacons;

            //keep track of the beacons in the room
            customBeacons = FindObjectsOfType<CustomBeacon>();

            //ui's to show beacon data
            ui_selected = GameObject.Find("UI_Selected").transform;
            ui_closest = GameObject.Find("UI_Closest").transform;

            Invoke("StartScanning", 1);
        }

        void Update()
        {
            UpdateUI();
        }

        //start the estimote api
        void StartScanning()
        {
            estimoteUnity.StartScanning();
        }

        //event delegate called whenever the beacon information is received by the phone
        void HandleDidRangeBeacons(List<EstimoteUnityBeacon> beacons)
        {
            estimoteBeacons = beacons;

            //iterate through all of custombeacons and compare to the detected estimote beacons
            for (int i = 0; i < customBeacons.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < estimoteBeacons.Count; j++)
                {
                    //if the custom beacon is detected by estimote
                    if (customBeacons[i].major == estimoteBeacons[j].Major && customBeacons[i].minor == estimoteBeacons[j].Minor)
                    {
                        found = true;
                        customBeacons[i].SetDistance(estimoteBeacons[j].BeaconRange, (float)estimoteBeacons[j].RSSI);
                        break;      //move to next custombeacon
                    }
                }
                //if the beacon is not detected by estimote
                if (!found)
                {
                    customBeacons[i].SetDistance(EstimoteUnityBeaconRange.UNKNOWN, 0);
                }
            }
            FindClosestBeacon();






            if (beacons.Count > 3)
            {
                //sort beacons by accuracy
                List<EstimoteUnityBeacon> sortedbeacons = SortBeacons(beacons);

                //use trilateration on the first 3 beacons (if enough)
                Vector2 pos = Trilateration(sortedbeacons[0], sortedbeacons[1], sortedbeacons[2]);
            }
        }

        List<EstimoteUnityBeacon> SortBeacons(List<EstimoteUnityBeacon> unsortedBeacons)
        {
            if (unsortedBeacons.Count < 2)
                return unsortedBeacons;
            List<EstimoteUnityBeacon> sortedBeacons = new List<EstimoteUnityBeacon>();
            while (unsortedBeacons.Count > 1)
            {
                double minAcc = unsortedBeacons[0].Accuracy;
                int minAccIndex = 0;
                for (int i = 1; i < unsortedBeacons.Count; i++)
                {
                    if (unsortedBeacons[i].Accuracy < minAcc)
                    {
                        minAcc = unsortedBeacons[i].Accuracy;
                        minAccIndex = i;
                    }
                }
                sortedBeacons.Add(unsortedBeacons[minAccIndex]);
                unsortedBeacons.RemoveAt(minAccIndex);
            }
            sortedBeacons.Add(unsortedBeacons[0]);
            return sortedBeacons;
        }

        Vector2 Trilateration (EstimoteUnityBeacon b1, EstimoteUnityBeacon b2, EstimoteUnityBeacon b3)
        {
            Vector2 position = new Vector2();

            //get the custom beacons to get data from Unity
            CustomBeacon c1 = FindCustomBeacon(b1);
            CustomBeacon c2 = FindCustomBeacon(b2);
            CustomBeacon c3 = FindCustomBeacon(b3);

            //get estimated distances from each estimoteBeacon
            float d1 = GetBeaconDistance(b1.RSSI);
            float d2 = GetBeaconDistance(b2.RSSI);
            float d3 = GetBeaconDistance(b3.RSSI);

            //temp variables set 1
            float A = (c1.transform.position.x * c1.transform.position.x) + (c1.transform.position.y * c1.transform.position.y) - (d1 * d1);
            float B = (c2.transform.position.x * c2.transform.position.x) + (c2.transform.position.y * c2.transform.position.y) - (d2 * d2);
            float C = (c3.transform.position.x * c3.transform.position.x) + (c3.transform.position.y * c3.transform.position.y) - (d3 * d3);

            //temp variable set 2
            float x32 = c3.transform.position.x - c2.transform.position.x;
            float x13 = c1.transform.position.x - c3.transform.position.x;
            float x21 = c2.transform.position.x - c1.transform.position.x;
            float y32 = c3.transform.position.y - c2.transform.position.y;
            float y13 = c1.transform.position.y - c3.transform.position.y;
            float y21 = c2.transform.position.y - c1.transform.position.y;

            //use temp variabales in trilateration equation to get position
            position.x = (A * y32) + (B * y13) + (C * y21);
            position.x /= (2 * ((c1.transform.position.x * y32) + (c2.transform.position.x * y13) + (c3.transform.position.x * y21)));
            position.y = (A * x32) + (B * x13) + (C * x21);
            position.y /= (2 * ((c1.transform.position.y * y32) + (c2.transform.position.y * y13) + (c3.transform.position.y * y21)));
            return position;
        }

        //return true if found
        public bool SelectBeacon(CustomBeacon _beacon)
        {
            DeselectBeacon();
            for (int i = 0; i < estimoteBeacons.Count; i++)
            {
                if (estimoteBeacons[i].Major == _beacon.major && estimoteBeacons[i].Minor == _beacon.minor)
                {
                    selectedBeacon = estimoteBeacons[i];
                    _beacon.SelectBeacon();
                    return true;
                }
            }
            return false;
            //selectedBeacon = null;
        }

        public void DeselectBeacon()
        {
            if (selectedBeacon != null)
            {
                for (int i = 0; i < customBeacons.Length; i++)
                {
                    if (selectedBeacon.Major == customBeacons[i].major && selectedBeacon.Minor == customBeacons[i].minor)
                    {
                        customBeacons[i].DeselectBeacon();
                    }
                }
            }
        }

        void FindClosestBeacon()
        {
            EstimoteUnityBeacon clo;        //closest estimote beacon
            double minVal;                  //min value
            if (estimoteBeacons != null && estimoteBeacons.Count > 0)
            {
                clo = estimoteBeacons[0];
                minVal = clo.RSSI;
                for (int i = 1; i < estimoteBeacons.Count; i++)
                {
                    if (estimoteBeacons[i].RSSI > minVal)
                    {
                        clo = estimoteBeacons[i];
                        minVal = clo.RSSI;
                    }
                }
                closestBeacon = clo;
                return;
            }
            else
            {
                closestBeacon = null;
            }

        }

        void UpdateUI()
        {
            EstimoteUnityBeacon curBeacon;
            Transform curUI;

            //Update closest UI
            curBeacon = closestBeacon;
            curUI = ui_closest;

            if (curBeacon != null)
            {
                curUI.Find("Title").GetComponent<Text>().text = "CLOSEST BEACON";
                curUI.Find("UUID").GetComponent<Text>().text = "UUID: " + curBeacon.UUID;
                curUI.Find("Major").GetComponent<Text>().text = "MAJOR: " + curBeacon.Major;
                curUI.Find("Minor").GetComponent<Text>().text = "MINOR: " + curBeacon.Minor;
                curUI.Find("Accuracy").GetComponent<Text>().text = "ACCURACY: " + curBeacon.Accuracy;
                curUI.Find("BeaconRange").GetComponent<Text>().text = "BEACON RANGE: " + curBeacon.BeaconRange;
                curUI.Find("LastSeen").GetComponent<Text>().text = "LAST SEEN: " + curBeacon.LastSeen;
                curUI.Find("RSSI").GetComponent<Text>().text = "RSSI: " + curBeacon.RSSI;
            }
            else
            {
                curUI.Find("Title").GetComponent<Text>().text = "NO BEACONS FOUND";
                curUI.Find("UUID").GetComponent<Text>().text = "UUID: ";
                curUI.Find("Major").GetComponent<Text>().text = "MAJOR: ";
                curUI.Find("Minor").GetComponent<Text>().text = "MINOR: ";
                curUI.Find("Accuracy").GetComponent<Text>().text = "ACCURACY: ";
                curUI.Find("BeaconRange").GetComponent<Text>().text = "BEACON RANGE: ";
                curUI.Find("LastSeen").GetComponent<Text>().text = "LAST SEEN: ";
                curUI.Find("RSSI").GetComponent<Text>().text = "RSSI: ";
            }

            //Update selected UI
            curBeacon = selectedBeacon;
            curUI = ui_selected;

            if (curBeacon != null)
            {
                curUI.Find("Title").GetComponent<Text>().text = "SELECTED BEACON";
                curUI.Find("UUID").GetComponent<Text>().text = "UUID: " + curBeacon.UUID;
                curUI.Find("Major").GetComponent<Text>().text = "MAJOR: " + curBeacon.Major;
                curUI.Find("Minor").GetComponent<Text>().text = "MINOR: " + curBeacon.Minor;
                curUI.Find("Accuracy").GetComponent<Text>().text = "ACCURACY: " + curBeacon.Accuracy;
                curUI.Find("BeaconRange").GetComponent<Text>().text = "BEACON RANGE: " + curBeacon.BeaconRange;
                curUI.Find("LastSeen").GetComponent<Text>().text = "LAST SEEN: " + curBeacon.LastSeen;
                curUI.Find("RSSI").GetComponent<Text>().text = "RSSI: " + curBeacon.RSSI;
            }
            else
            {
                curUI.Find("Title").GetComponent<Text>().text = "SELECT BEACON";
                curUI.Find("UUID").GetComponent<Text>().text = "UUID: ";
                curUI.Find("Major").GetComponent<Text>().text = "MAJOR: ";
                curUI.Find("Minor").GetComponent<Text>().text = "MINOR: ";
                curUI.Find("Accuracy").GetComponent<Text>().text = "ACCURACY: ";
                curUI.Find("BeaconRange").GetComponent<Text>().text = "BEACON RANGE: ";
                curUI.Find("LastSeen").GetComponent<Text>().text = "LAST SEEN: ";
                curUI.Find("RSSI").GetComponent<Text>().text = "RSSI: ";
            }
        }

        //Find the Unite representation of each beacon if it exists
        CustomBeacon FindCustomBeacon(EstimoteUnityBeacon estimoteBeacon)
        {
            for (int i = 0; i < customBeacons.Length; i++)
            {
                if (customBeacons[i].major == estimoteBeacon.Major && customBeacons[i].minor == estimoteBeacon.Minor)
                {
                    return customBeacons[i];
                }
            }
            //none of the custom beacons have the estimote beacon major/minor
            Debug.LogError("Found an estimote beacon that didnt have corresponding CustomBeacon... " + estimoteBeacon.Major + ": " + estimoteBeacon.Minor);
            return null;
        }

        //use the RSSI -> distance equation we made using test data
        float GetBeaconDistance(int beaconRSSI)
        {
            return 1;
        }
    }
}
