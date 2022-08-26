using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using System;
using SimpleJSON;
using TMPro;
public class ProfileLoader : MonoBehaviour
{
    public GameObject profile;
	public Image photoProfile;
	public Sprite defaultPhoto;
	public TextMeshProUGUI textProfile;
	public Image maskImage;
	public static string profileName;

	void OnEnable()
	{
		photoProfile.sprite = defaultPhoto;
		StartCoroutine(LoadProfile());
	}

	void OnDisable()
	{
		profileName = null;
		photoProfile.sprite = defaultPhoto;
	}
	public void PickImage(int maxSize)
	{
		NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
		{
			//Native gallery, to pick image for the Profile
			if (path != null)
			{
				// Create Texture from selected image
				Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
				if (texture == null)
				{
					Debug.Log("Couldn't load texture from " + path);
					return;
				}
                else
                {
					byte[] imageData = File.ReadAllBytes(path);
					texture.name = Path.GetFileNameWithoutExtension(path);
					StartCoroutine(Settings.Instance.PostProfilePhoto(texture, imageData));
					Sprite sprite;
        			sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));

					photoProfile.sprite = sprite;

					//Resizing image to fit the scales
					if (texture.width > texture.height)
					{
						float currentCoef = ((float)texture.height / (float)texture.width);

						photoProfile.rectTransform.sizeDelta = new Vector2(maskImage.rectTransform.sizeDelta.x / currentCoef, maskImage.rectTransform.sizeDelta.y);
					}
					else
					{
						float currentCoef = ((float)texture.height / (float)texture.width);

						photoProfile.rectTransform.sizeDelta = new Vector2(maskImage.rectTransform.sizeDelta.x, maskImage.rectTransform.sizeDelta.y * currentCoef);
					}
				}
			}
		});

		Debug.Log("Permission result: " + permission);
	}

	public IEnumerator LoadProfile()
    {
        yield return new WaitUntil(() => Authenticator.jwtToken != null);

		string photoURL = "";

		UserPhoto jsonString = new UserPhoto();
        WWWForm form = new WWWForm();

        using (UnityWebRequest www = UnityWebRequest.Get("https://api.stg.********/api/accounts/users/me/"))
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
				//Getting profile Image URL and Username from Backend
				JSONNode root = JSONNode.Parse(www.downloadHandler.text);
				photoURL = root["profile_picture"];
				profileName = root["username"];
				Authenticator.usernameID = profileName;

				textProfile.text = profileName;
				//Setting profile Image we got
				StartCoroutine(GetTexture(photoURL));
			}
        }
    }

	IEnumerator GetTexture(string url)
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
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
			yield return new WaitUntil(() => profile.activeInHierarchy == true);
			photoProfile.sprite = sprite;

			if (texture.width > texture.height)
			{
				float currentCoef = ((float)texture.height / (float)texture.width);

				photoProfile.rectTransform.sizeDelta = new Vector2(maskImage.rectTransform.sizeDelta.x / currentCoef, maskImage.rectTransform.sizeDelta.y);
			}
			else
			{
				float currentCoef = ((float)texture.height / (float)texture.width);

				photoProfile.rectTransform.sizeDelta = new Vector2(maskImage.rectTransform.sizeDelta.x, maskImage.rectTransform.sizeDelta.y * currentCoef);
			}
		}
	}
	
	[System.Serializable]
	public class UserPhoto
	{
	    public string profile_photo;
	}
}
