﻿using UnityEngine;

public class Portal : MonoBehaviour
{
    public float distance = 50f;
    public Portal destination;
    public GameObject player;
    public Camera portalCamera;
    public Material renderTarget;

    private MeshRenderer mr;

    void Start()
    {
        if(portalCamera.targetTexture != null)
        {
            portalCamera.targetTexture.Release();
        }

        portalCamera.targetTexture = (RenderTexture)renderTarget.mainTexture ?? new RenderTexture(Screen.width, Screen.height, 24);
        renderTarget.mainTexture = portalCamera.targetTexture;

        mr = this.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (destination.IsInRoom())
        {
            if (!portalCamera.gameObject.activeSelf)
            {
                portalCamera.gameObject.SetActive(true);
            }

            this.PositionCamera();
        }
        else
        {
            if (portalCamera.gameObject.activeSelf)
            {
                portalCamera.gameObject.SetActive(false);
            }
        }

        if (this.IsInRoom())
        {
            if (!mr.enabled)
            {
                mr.enabled = true;
            }
        }
        else
        {
            if (mr.enabled)
            {
                mr.enabled = false;
            }
        }
    }

    public bool IsInRoom()
    {
        if(player == null)
        {
            return false;
        }

        var distanceFromPlayer = Vector3.Distance(player.transform.position, this.transform.position);

        return distanceFromPlayer <= distance;
    }

    private void PositionCamera()
    {
        portalCamera.transform.position = this.GetCameraPosition();
        portalCamera.transform.rotation = this.GetCameraAngle();

        Debug.DrawLine(portalCamera.transform.position, portalCamera.transform.position + portalCamera.transform.forward);
    }

    private Quaternion GetCameraAngle()
    {
        var angDiff = Quaternion.Angle(transform.rotation, destination.transform.rotation);
        var rotDiff = Quaternion.AngleAxis(angDiff, Vector3.up);
        var direction = rotDiff * player.transform.forward;
        return Quaternion.LookRotation(direction, Vector3.up);
    }
    private Vector3 GetCameraPosition()
    {
        var relativePosition = player.transform.position - destination.transform.position;

        return this.transform.position + relativePosition;
    }
}