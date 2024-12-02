using UnityEngine;
using System.Collections.Generic;

public class RoomCamGroup : MonoBehaviour
{
    public List<GameObject> roomCameras = new List<GameObject>();
    private bool activeRoom = true;

    void Update()
    {
        if (roomCameras.Count == 0)
        {
            return;
        }

        if (activeRoom)
        {
            foreach (var roomCamera in roomCameras)
            {
                Transform rotator = roomCamera.transform.Find("rotator");
                if (rotator == null)
                {
                    Debug.LogError("Rotator child not found in cameraInstance.");
                    continue;
                }

                Transform spotLightTransform = rotator.Find("Spot Light");
                if (spotLightTransform != null)
                {
                    SpotlightDetection spotlightDetection = spotLightTransform.GetComponent<SpotlightDetection>();
                    if (spotlightDetection != null && spotlightDetection.enemiesSpawned)
                    {
                        activeRoom = false;
                        DisableOtherCameras(roomCamera);
                        break;
                    }
                }
            }
        }
    }

    private void DisableOtherCameras(GameObject exceptCamera)
    {
        Debug.Log("Disabling all cameras except the triggered one");

        foreach (var roomCamera in roomCameras)
        {
            if (roomCamera == exceptCamera)
            {
                continue; // Skip the camera that triggered the disable
            }

            Transform rotator = roomCamera.transform.Find("rotator");
            if (rotator == null)
            {
                Debug.LogError("Rotator child not found in cameraInstance.");
                continue;
            }

            Transform spotLightTransform = rotator.Find("Spot Light");
            if (spotLightTransform != null)
            {
                SpotlightDetection spotlightDetection = spotLightTransform.GetComponent<SpotlightDetection>();
                if (spotlightDetection != null)
                {
                    spotlightDetection.enabled = false; // Disable the spotlight detection
                    Debug.Log($"Disabled spotlight detection for camera: {roomCamera.name}");
                }
            }
        }
    }
}