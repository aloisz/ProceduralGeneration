using System;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class RotatingPlanet : MonoBehaviour
{
    private Quaternion baseRotation;
    [SerializeField] private bool notRandomizeRotation;
    [ShowIf("notRandomizeRotation")][SerializeField] private Vector3 rotatingVector = Vector3.up;
    [ShowIf("notRandomizeRotation")][SerializeField]private float speed = 10;

    private void Start()
    {
        baseRotation = transform.rotation;
        if(notRandomizeRotation) return;
        rotatingVector = Random.onUnitSphere;
        speed = Random.Range(2, 15);
    }

    private void Update()
    {
        transform.Rotate(rotatingVector, Time.deltaTime * speed);
    }

    public void ResetRotation()
    {
        if (!notRandomizeRotation)
        {
            rotatingVector = Random.onUnitSphere;
            speed = Random.Range(2, 15);
        }
        
        transform.rotation = baseRotation;
    }
}
