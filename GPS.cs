using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfinityCode.OnlineMapsExamples;

public class GPS : MonoBehaviour
{
    public GameObject player;
    public OnlineMapsMarker3D playerMarker;
    
    public static float latitude;
    public static float longitude;

    public void LockToPlayer()
    {
        // Zoom in to player position

        OnlineMaps.instance.SmoothZoom(longitude, latitude, 20f);	
    }

    void Start()
    {
        StartCoroutine(StartGPS());
    }
    IEnumerator StartGPS()
    {
        // Check if the user has location service enabled.

        Input.location.Start();
        if (!Input.location.isEnabledByUser)
        {
            //Debug.Log("Location permission is not allowed");
            yield return new WaitForSecondsRealtime(1f);
            StartCoroutine(StartGPS());
            yield break;
        }

        // Starts the location service.

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Timeout: " + maxWait);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            StartCoroutine(StartGPS());
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield return new WaitForSecondsRealtime(1f);
            StartCoroutine(StartGPS());
            yield break;
        }
        else
        {
            yield return new WaitUntil (() => Input.location.status == LocationServiceStatus.Running);
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;

            playerMarker = OnlineMapsMarker3DManager.CreateItem(longitude, latitude, player);
            playerMarker.checkMapBoundaries = false;
            
            LockToPlayer();

            //Check for the position each second
            InvokeRepeating("OnLocationChanged", 0f, 1f);
        }
    }

    public UnityEngine.UI.Text latText;
    public UnityEngine.UI.Text longiText;
    
    // On location has changed
    private void OnLocationChanged()
    {
        if(Input.location.status == LocationServiceStatus.Running)
        {
            //Changing player position related to GPS position

            playerMarker.latitude = latitude = Input.location.lastData.latitude;
            playerMarker.longitude = longitude = Input.location.lastData.longitude;

          
            OnlineMaps.instance.Redraw();
        }
        else
        {
            Input.location.Stop();
        }
    }
}

