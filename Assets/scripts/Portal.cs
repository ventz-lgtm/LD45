﻿using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Portal : MonoBehaviour
{
    public float distance = 50f;
    public bool inverted = false;
    public Portal destination;
    public GameObject player;
    public Camera portalCamera;
    public Material renderTarget;

    private PlayerController playerController;
    private GameObject playerObject;
    private MeshRenderer mr;
    [HideInInspector]
    public bool playerInside = false;

    private bool wasInsideFromFront = false;
    private bool teleporting = false;
    private float lastTeleport = 0f;

    void Start()
    {
        if(portalCamera.targetTexture != null)
        {
            portalCamera.targetTexture.Release();
        }

        if(portalCamera != null)
        {
            portalCamera.targetTexture = (RenderTexture)renderTarget.mainTexture ?? new RenderTexture(Screen.width, Screen.height, 24);
            renderTarget.mainTexture = portalCamera.targetTexture;
        }

        mr = this.GetComponentInChildren<MeshRenderer>(true);

        if(player == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            
            if(playerController != null)
            {
                playerObject = playerController.gameObject;

                var camera = playerController.gameObject.GetComponentInChildren<Camera>(true);
                if(camera != null)
                {
                    player = camera.gameObject;
                }
                
            }
        }
    }

    private void Update()
    {
        this.UpdatePortal();
        
        if (Time.time - lastTeleport > 0.5f && playerInside && wasInsideFromFront)
        {
            var dot = this.GetPlayerDot();

            if (inverted ? dot >= 0f : dot <= 0f)
            {
                lastTeleport = Time.time;

                destination.NotifyTeleporting();

                var rotDiff = Quaternion.Angle(transform.rotation, destination.transform.rotation);

                if (inverted)
                {
                    rotDiff += 180f;
                }

                playerObject.transform.Rotate(Vector3.up, rotDiff);
                playerController.cameraYaw += rotDiff;

                var localPosition = transform.InverseTransformPoint(playerObject.transform.position);

                if (!inverted)
                {
                    localPosition.x = -localPosition.x;
                    localPosition.y = -localPosition.y;
                }

                playerObject.transform.position = destination.transform.TransformPoint(localPosition);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (teleporting || destination == null || playerInside || !this.IsInRoom())
        {
            return;
        }

        var hitObject = other.gameObject;

        var controller = hitObject.GetComponent<PlayerController>();

        if (controller != null)
        {
            playerInside = true;

            var dot = this.GetPlayerDot();

            if (inverted ? dot < 0f : dot > 0f)
            {
                wasInsideFromFront = true;
            }            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var hitObject = other.gameObject;

        if (hitObject.GetComponent<PlayerController>() != null)
        {
            playerInside = false;
            wasInsideFromFront = false;
            teleporting = false;
        }
    }

    public void NotifyTeleporting()
    {
        teleporting = true;
    }

    public void UpdatePortal()
    {
        if (destination == null)
        {
            if (portalCamera.gameObject.activeSelf)
            {
                portalCamera.gameObject.SetActive(false);
            }

            if (mr.enabled)
            {
                mr.enabled = false;
            }

            return;
        }

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

        if (!inverted)
        {
            direction = -direction;
            direction.y = -direction.y;
        }

        return Quaternion.LookRotation(direction, Vector3.up);
    }

    private Vector3 GetCameraPosition()
    {
        var relativePosition = destination.transform.InverseTransformPoint(player.transform.position);
        relativePosition.x = -relativePosition.x;
        relativePosition.y = -relativePosition.y;

        return this.transform.TransformPoint(relativePosition);
    }

    private float GetPlayerDot()
    {
        var origin = transform.position + transform.up * 1.2f;

        var toPlayer = Vector3.Normalize(playerObject.transform.position - origin);
        return Vector3.Dot(transform.up, toPlayer);
    }
}
