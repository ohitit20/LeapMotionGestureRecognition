using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using System;

public class MapZoomer : MonoBehaviour
{
    public float zoomSpeed = 10e-60f; // Adjust as needed
    private LeapProvider leapProvider;
    private List<float> distances = new List<float>(); // To store distances over frames
    private int frameCount = 60; // Number of frames to consider for zooming
    private Vector3 initialScale;
    private HandRayCaster handRayCaster;
    private GameObject objectToZoom;
    private Vector3 initialObjectScale;
    private Dictionary<GameObject, Vector3> initialScales = new Dictionary<GameObject, Vector3>();


    private void Start()
    {
        // Populate the dictionary with all game objects and their initial scales
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            initialScales[obj] = obj.transform.localScale;
        }
    }

    private void Update()
    {
        leapProvider = FindObjectOfType<LeapProvider>();
        if(leapProvider == null)
        {
            Debug.Log("Leap Provider not found!");
        }
        handRayCaster = FindObjectOfType<HandRayCaster>();
        if(handRayCaster == null)
        {
            Debug.Log("Hand Ray Caster not found!");
        }
        objectToZoom = handRayCaster.GetSelectionCompletedObject();
        if(objectToZoom == null)
        {
            Debug.Log("Object to zoom not found!");
        }
        Frame frame = leapProvider.CurrentFrame;
        if (frame.Hands.Count == 1 && frame.Hands[0].IsRight && objectToZoom != null)
        {   
            initialObjectScale = initialScales[objectToZoom];
            Zoom(frame.Hands[0], objectToZoom);
            
        }
    }
    void Zoom(Hand hand, GameObject objectToZoom)
    {
        Debug.Log("Zoomun içindeyim!");
        Finger thumb = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];
        Finger index = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];

        float currentDistance = Vector3.Distance(thumb.TipPosition, index.TipPosition);
        //Debug.Log(currentDistance);
        distances.Add(currentDistance);

        // Ensure we only keep the last 'frameCount' distances
        while (distances.Count > frameCount)
        {
            distances.RemoveAt(0);
        }

        if (distances.Count == frameCount)
        {
            bool zoomValidator = true;
            float fingerTipDistanceInOneSecond = distances[distances.Count - 1] - distances[0];
            // Check if each distance is greater than the one before it
            if ((Math.Abs(fingerTipDistanceInOneSecond) < 0.04))
            {
                zoomValidator = false;
            }
            
            
            if (zoomValidator)
            {
                int greaterCount = 0;
                int lesserCount = 0;

                for (int i = 1; i < distances.Count; i++)
                {
                    float difference = distances[i] - distances[i-1];
                    
                    if (difference > 0)
                        greaterCount++;
                    else if (difference < 0)
                        lesserCount++;
                }

                float targetScaleX = transform.localScale.x + fingerTipDistanceInOneSecond*zoomSpeed;
                float maxScaleX = 0.2f;
                float minScaleX = 0.05f;

                targetScaleX = Mathf.Clamp(targetScaleX, minScaleX, maxScaleX);
                float scaleFactor = targetScaleX / initialObjectScale.x;
                Vector3 targetScale = initialObjectScale * scaleFactor;

                if (greaterCount >= 40)
                {
                    // Zoom In
                    StartCoroutine(SmoothZoom(transform.localScale, targetScale, 1f / 60f));

                }
                else if (lesserCount >= 40)
                {
                    // Zoom Out
                    StartCoroutine(SmoothZoom(transform.localScale, targetScale, 1f / 60f));

                }
            }

        }
    }

    System.Collections.IEnumerator SmoothZoom(Vector3 startScale, Vector3 targetScale, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }
        transform.localScale = targetScale;  // Ensure you end up at the exact target scale
    }

    

}