using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RotatingPlanet : MonoBehaviour
{
    private Quaternion baseRotation;
    private Vector3 rotatingVector = Vector3.up;
    private float speed = 10;

    private void Start()
    {
        baseRotation = transform.rotation;
        rotatingVector = Random.onUnitSphere;
        speed = Random.Range(2, 15);
    }

    private void Update()
    {
        transform.Rotate(rotatingVector, Time.deltaTime * speed);
    }

    public void ResetRotation()
    {
        rotatingVector = Random.onUnitSphere;
        speed = Random.Range(2, 15);
        transform.rotation = baseRotation;
    }
}
