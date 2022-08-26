using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class ARHandler : MonoBehaviour
{

    public static ARHandler Instance;

    public GameObject ARSessionOrigin;
    public GameObject ARSession;
	public Camera ARCamera;
	public GameObject picturecapturePanel;


	private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void AREnable()
    {
		//Disable all necessary panels
		picturecapturePanel.SetActive(true);
		ARSession.SetActive(true);
		ARSessionOrigin.SetActive(true);
		StartCoroutine(LoadPinsData());
	}

    public void ARDisable()
    {
		//Disable all unnecessary panels
		picturecapturePanel.SetActive(false);
		ARSession.SetActive(false);
		ARSessionOrigin.SetActive(false);
	}

	private IEnumerator LoadPinsData()
	{
		yield return new WaitUntil(() => Authenticator.jwtToken != null);
		Pin pin = new Pin();
		
		if(!ARSession.activeInHierarchy)
		{
			PinInstantiate.Instance.RemoveArPins();
		}

		//Getting information of the closes pins related to (Current position + Range radius)
		using (UnityWebRequest www = UnityWebRequest.Get("https://api.stg.*******/api/search/pins/location_distance=" + PinInstantiate.Instance.spawnRange + "km__" + GPS.latitude + "__" + GPS.longitude + "&page_size=" + PinsData.pageSize))
		{
			www.method = "Get";
			www.SetRequestHeader("Authorization", "Bearer " + Authenticator.jwtToken);
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log(www.error);
				Debug.LogError("Username or password are incorrect");
			}
			else
			{
				JSONNode root = JSONNode.Parse(www.downloadHandler.text);
				JSONArray nodes = root["results"].AsArray;

				foreach (JSONNode node in nodes)
				{
					pin.id = node["id"];
					pin.name = node["name"];
					pin.description = node["description"];
					pin.image = node["image"];
					pin.type = node["type_id"];
					pin.difficulty = node["difficulty_id"];
					pin.rarity = node["rarity_id"];
					pin.creator = node["creator_username"];
					JSONObject location = node["location"].AsObject;
					pin.lat = location["lat"];
					pin.lon = location["lon"];

					//Spawning pin filled with all the provided information
					PinInstantiate.Instance.SpawnPinAR(pin);
				}
			}
		}
	}
}
