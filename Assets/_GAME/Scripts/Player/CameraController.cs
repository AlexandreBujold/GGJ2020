using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.youtube.com/watch?v=EhNzQyGDnHk
public class CameraController : MonoBehaviour
{
    [Header("Camera View Configuration")]
    public float transitionTime;
    public bool transitionView = true;
    [Tooltip("Match the controllable rotation to the targetView's rotation to avoid jittering when swapping.")]
    public bool matchRotation = true;

    [Space]
    [Header("FPS Control Configuration")]
    public bool firstPersonControl = true;
    [Range(0, 200)]
    public float rotateSpeed = 150f;

    [Space]
    [Header("References")]
    public Transform cameraTransform;
    [Space]
    public Transform targetView;
    public Transform pastTargetView;
    public Transform trackTransform;
    private Vector2 rotationValue;
    public ParticleSystem transitionEffect;

    [HideInInspector]
    public Coroutine transitioning;

    private void Start()
    {
        if (targetView != null)
        {
            transform.position = targetView.position;
            transform.eulerAngles = targetView.eulerAngles;
            trackTransform = targetView;
        }
    }

    void Update()
    {
        if (trackTransform != null && transitioning == null)
        {
            transform.position = trackTransform.position;
            targetView.parent.rotation = Quaternion.Euler(targetView.parent.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetView.parent.rotation.eulerAngles.z);
        }

        if (firstPersonControl && transitioning == null)
        {
            if (cameraTransform != null)
            {
                //FPS Input
                Vector2 rotationInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                //Calculate Values (all scaled to Time.deltaTime already)
                Vector3 rotVal = Vector3.zero;
                rotVal = CalculateRotation(rotationInput);

                //Apply Values
                transform.localRotation = Quaternion.Euler(0, rotVal.y, 0f); //Rotation Y
                cameraTransform.localRotation = Quaternion.Euler(rotVal.x, cameraTransform.rotation.eulerAngles.y, cameraTransform.rotation.eulerAngles.z); //Rotation X
            }
        }
    }

    public Vector3 CalculateRotation(Vector2 controlInput)
    {
        //First Person Control
        Cursor.lockState = CursorLockMode.Locked;
        controlInput *= rotateSpeed * Time.deltaTime;
        Vector3 previousRotationValue = rotationValue;
        rotationValue.x -= controlInput.y;
        rotationValue.x = Mathf.Clamp(rotationValue.x, -90f, 90f);
        rotationValue.y += controlInput.x;

        return rotationValue;
    }

    public void TransitionCameraView(Transform newTargetView)
    {
        pastTargetView = targetView;
        targetView = newTargetView;
        trackTransform = targetView;
        if (transitionView)
        {
            transitioning = StartCoroutine(TransitionView(newTargetView));
        }
        else
        {
            if (matchRotation)
            {
                rotationValue.y = targetView.rotation.eulerAngles.y;
            }
            else
            {
                //Sets targetView's rotation to match Camera's before transitioning, resulting in a smoother transition
                targetView.eulerAngles = transform.localRotation.eulerAngles;
            }

            targetView.eulerAngles = transform.localRotation.eulerAngles;
            transform.position = targetView.position;
            transform.eulerAngles = newTargetView.eulerAngles;
        }
    }

    public IEnumerator TransitionView(Transform newTargetView)
    {
        Vector3 startPos = transform.position;
        Vector3 startAngle = transform.rotation.eulerAngles;

        if (transitionEffect != null)
        {
            transitionEffect.Play();
        }

        if (matchRotation)
        {
            rotationValue.y = newTargetView.rotation.eulerAngles.y;
        }
        else
        {
            //Sets newTargetView's rotation to match Camera's before transitioning, resulting in a smoother transition
            newTargetView.eulerAngles = transform.localRotation.eulerAngles;
        }

        for (float t = 0; t <= 1; t += 1 / (transitionTime / Time.deltaTime))
        {
            //Lerp Position
            transform.position = Vector3.Lerp(startPos, newTargetView.position, t);

            //Lerp Angle
            Vector3 currentAngle = new Vector3(
            Mathf.LerpAngle(startAngle.x, newTargetView.transform.rotation.eulerAngles.x, t),
            Mathf.LerpAngle(startAngle.y, newTargetView.transform.rotation.eulerAngles.y, t),
            Mathf.LerpAngle(startAngle.z, newTargetView.transform.rotation.eulerAngles.z, t));
            transform.eulerAngles = currentAngle;

            yield return new WaitForEndOfFrame();
        }

        if (transitionEffect != null)
        {
            transitionEffect.Stop();
        }

        transform.position = newTargetView.position;
        transform.eulerAngles = newTargetView.eulerAngles;
        transitioning = null;
    }
}
