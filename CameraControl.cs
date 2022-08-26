using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl
{
    float cameraSpeed = 1f;
    public void Move(Transform transform, Transform camera)
    {
        float time = cameraSpeed * .05f;
        Vector3[] vector3s = new Vector3[4];
        vector3s[0] = vector3s[1] = camera.transform.position;
        vector3s[1] = transform.transform.position;
        vector3s[3].x = Mathf.SmoothDamp(vector3s[0].x, vector3s[1].x, ref vector3s[2].x, time);
        vector3s[3].y = Mathf.SmoothDamp(vector3s[0].y, vector3s[1].y, ref vector3s[2].y, time);
        vector3s[3].z = Mathf.SmoothDamp(vector3s[0].z, vector3s[1].z, ref vector3s[2].z, time);
        camera.transform.position = vector3s[3];
    }
    
    public void Lock(Transform transform, Transform camera)
    {
        var rotation = Quaternion.LookRotation(transform.position - camera.transform.position);
        camera.rotation = Quaternion.Slerp(camera.rotation, rotation, Time.deltaTime * 3);
    }
}

