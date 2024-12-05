using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform planeToFollow;

    
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 2, -6);  
    public float followSpeed = 5f;  
    public float rotationSpeed = 3f;  

    
    [Header("Dynamic Adjustments")]
    public float lookAheadFactor = 0.1f;  
    public float dynamicPitchFactor = 0.3f;  
    public float dynamicRollFactor = 0.5f;

    void FixedUpdate()
    {
        FollowPlane();
        RotateToMatchPlane();
    }

    
    private void FollowPlane()
    {
        Vector3 targetPosition = planeToFollow.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
    
    private void RotateToMatchPlane()
    {
        Vector3 lookAheadTarget = planeToFollow.position + planeToFollow.forward * lookAheadFactor;
        
        float planePitch = planeToFollow.eulerAngles.x;
        if (planePitch > 180) planePitch -= 360;  

        float planeRoll = planeToFollow.eulerAngles.z;
        if (planeRoll > 180) planeRoll -= 360;  
        
        Quaternion targetRotation = Quaternion.LookRotation(lookAheadTarget - transform.position);
        targetRotation *= Quaternion.Euler(planePitch * dynamicPitchFactor, 0, planeRoll * dynamicRollFactor);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
