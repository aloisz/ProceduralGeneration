using System;
using UnityEngine;

public class RotatingPlanet : MonoBehaviour
{
    private Quaternion baseRotation;
    [SerializeField] private Vector3 rotatingVector = Vector3.up;
    [SerializeField] private float speed = 10;


    private void Start()
    {
        baseRotation = transform.rotation;
    }

    private void Update()
    {
        transform.Rotate(rotatingVector, Time.deltaTime * speed);
    }

    public void ResetRotation()
    {
        transform.rotation = baseRotation;
    }
}
