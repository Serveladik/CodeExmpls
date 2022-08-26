using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System.Globalization;
using ARLocation;
using System.Globalization;
using VirtualTour.Utils.Extensions;
using System.Threading.Tasks;


public class PinInstantiate : MonoBehaviour
{
    
    [Serializable]
    public class PinLoc
    {
        public PinLoc(double _latitude, double _longitude)
        {
            this.latitude = _latitude;
            this.longitude = _longitude;
        }
        public double latitude;
        public double longitude;
    }

    public static PinInstantiate Instance;
    public float spawnRange = 1f;
    public float spawnRangeInAR = 0.2f;
    public GameObject pinPrefab, pinARPrefab;
    GameObject currentGO;
    //public int id = 0;
    Dictionary<int, PinLoc> pinLocation = new Dictionary<int, PinLoc>();
    Dictionary<int, PinLoc> ARInstances = new Dictionary<int, PinLoc>();

    public List<OnlineMapsMarker3D> playerMarker = new List<OnlineMapsMarker3D>();
    public List<GameObject> ARObjects = new List<GameObject>();
   

    public Transform placeToInstantiateObjects;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    PinLoc pin = new PinLoc(0,0);
    public void SpawnPin(int id, double latitude, double longitude)
    {
        //Identifying the start ID
        var tempId = id > 0 ? id - 1 : 1;

        tempId = id - 1;
        if (tempId < 0)
        {
            tempId = 0;
        }

        if (!pinLocation.ContainsKey(tempId) && Measure(latitude, longitude) <= PinsData.spawnDistance)
        {
            pin.latitude = latitude;
            pin.longitude = longitude;

            pinLocation.Add(tempId, pin);
            
            playerMarker.Add(OnlineMapsMarker3DManager.CreateItem(latitude, longitude, pinPrefab));
            playerMarker[playerMarker.Count - 1].instance.gameObject.GetComponent<LocationID>().pinID = id;
            playerMarker[playerMarker.Count - 1].checkMapBoundaries = true;
        }
        else if (pinLocation.ContainsKey(tempId) && Measure(latitude, longitude) > PinsData.spawnDistance)
        {
            if (playerMarker != null && playerMarker.Count > 0)
            {
                //Deleting pins when the player is out of range
                Destroy(playerMarker[tempId].instance.gameObject);
                playerMarker.Remove(playerMarker[tempId]);
            }
        }
    }

    PinLoc pinArea = new PinLoc(0,0);
    public OnlineMaps map;
    public void SpawnPinByArea(int id, double latitude, double longitude)
    {
        //Identifying the start ID
        var tempId = id > 0 ? id - 1 : 1;

        tempId = id - 1;
        if (tempId < 0)
        {
            tempId = 0;
        }
        //Checking for pin is already spawned(so in list) and is in our range
        if (!pinLocation.ContainsKey(tempId) && MeasureByArea(map.latitude, map.longitude, latitude, longitude) <= PinsData.spawnDistance)
        {
            pinArea.latitude = latitude;
            pinArea.longitude = longitude;
            
            pinLocation.Add(tempId, pinArea);
            
            playerMarker.Add(OnlineMapsMarker3DManager.CreateItem(latitude, longitude, pinPrefab));
            playerMarker[playerMarker.Count - 1].instance.gameObject.GetComponent<LocationID>().pinID = id;
            playerMarker[playerMarker.Count - 1].checkMapBoundaries = true;
        }
        //Checking for pin is already spawned(so in list) and isnt in our range so DELETE
        else if (pinLocation.ContainsKey(tempId) && MeasureByArea(map.latitude, map.longitude, latitude, longitude) > PinsData.spawnDistance)
        {
            if (playerMarker != null && playerMarker.Count > 0)
            {
                //Deleting pins when the player is out of range
                Destroy(playerMarker[tempId].instance.gameObject);
                playerMarker.Remove(playerMarker[tempId]);
            }
        }
    }

    public void SpawnPinAR(Pin pin)
    {
        double lat = double.Parse(pin.lat, CultureInfo.InvariantCulture);
        double lon = double.Parse(pin.lon, CultureInfo.InvariantCulture);

        var tempId = pin.id > 0 ? pin.id - 1 : 1;
        tempId = pin.id - 1;
        if (tempId < 0)
        {
            tempId = 0;
        }
        //Checking for pin is already spawned(so in list) and is in our range
        if (!ARInstances.ContainsKey(tempId) && Measure(lat, lon) <= spawnRangeInAR)
        {
            PinLoc pinLoc = new PinLoc(lat, lon);
            ARInstances.Add(tempId, pinLoc);

            currentGO = Instantiate(pinARPrefab, Vector3.zero, Quaternion.identity) ;
            
            currentGO.GetComponent<PlaceAtLocation>().LocationOptions.LocationInput.Location.Latitude = lat;
            currentGO.GetComponent<PlaceAtLocation>().LocationOptions.LocationInput.Location.Longitude = lon;
            
            currentGO.GetComponent<Pin>().id = pin.id;
            currentGO.GetComponent<Pin>().name = pin.name;

            //Spawn pin in AR Mode
            StartCoroutine(GetTexture(currentGO, pin.image));

        }
        //Checking for pin is already spawned(so in list) and isnt in our range so DELETE
        else if (ARInstances.ContainsKey(tempId) && Measure(lat, lon) > spawnRangeInAR)
        {
            if (ARObjects != null && ARObjects.Count > 0)
            {
                Destroy(ARObjects[tempId].gameObject);
                ARObjects.Remove(ARObjects[tempId]);
            }
        }
    }
    public void RemoveArPins()
    {
        foreach(GameObject pin in ARObjects)
        {
            Destroy(pin.gameObject);
            ARObjects.Remove(pin); 
        }
    }

    //Measure formula for checking distance between player and coordinate in world (in kilometers)
    public float Measure(double latitude, double longitude)
    {
        var R = 6378.137; // Radius of earth in KM
        var dLat = latitude * Math.PI / 180 - GPS.latitude * Math.PI / 180;
        var dLon = longitude * Math.PI / 180 - GPS.longitude * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(GPS.latitude * Math.PI / 180) * Math.Cos(latitude * Math.PI / 180) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c;
        
        return Mathf.Round(Convert.ToSingle(d)); // km
    }

    //Measure formula for checking distance between world coordinate and coordinates (in kilometers)
    public float MeasureByArea(double mapLatitude, double mapLongitude, double latitude, double longitude)
    {
        var R = 6378.137; // Radius of earth in KM
        var dLat = latitude * Math.PI / 180 - mapLatitude * Math.PI / 180;
        var dLon = longitude * Math.PI / 180 - mapLongitude * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(mapLatitude * Math.PI / 180) * Math.Cos(latitude * Math.PI / 180) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c;

        return Mathf.Round(Convert.ToSingle(d)); // km
    }

    public ToggleController metricSystem;

    //Converting Measuring (Km \ meters)
    public string MeasureInString(double longitude, double latitude)
    {
        var R = 6378.137; // Radius of earth in KM
        var dLat = latitude * Math.PI / 180 - GPS.latitude * Math.PI / 180;
        var dLon = longitude * Math.PI / 180 - GPS.longitude * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(GPS.latitude * Math.PI / 180) * Math.Cos(latitude * Math.PI / 180) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c;
     
        if(metricSystem.isOn)
        {
            return Mathf.Round(Convert.ToSingle(d)).ToString() + "km"; // km
        }
        else
        {
            return Mathf.Round(Convert.ToSingle(d * 0.621371)).ToString() + "mi"; // miles
        }
    }

    //Checking the direction of player looking at
    public Vector2 Direction(double latitude, double longitude)
    {
        Vector2 endPoint = new Vector2((float)latitude, (float)longitude);

        
        Vector2 myPoint = new Vector2(GPS.latitude, GPS.longitude);
        Vector2 direction  = Vector2.MoveTowards(myPoint, endPoint, -1);

        Debug.LogError(direction);
        
        return direction;
    }

    //Getting info from backend to set the Sprite and Name for Pin in AR MODE
    IEnumerator GetTexture(GameObject currentGO, string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
           
           
            currentGO.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
            currentGO.GetComponentInChildren<BoxCollider>().size = sprite.bounds.size;
            currentGO.transform.localScale = new Vector3(3, 3, 3);
        }
    }
}
