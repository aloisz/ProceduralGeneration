using UnityEngine;

public class RotatingSun : MonoBehaviour
{
    [SerializeField] private Vector3 rotatingVector = Vector3.up; 
    [SerializeField]private float speed = 10;
    [SerializeField]private float acceleratedSpeed = 10;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Rotate(rotatingVector, Time.deltaTime * acceleratedSpeed);
        }
        else
        {
            transform.Rotate(rotatingVector, Time.deltaTime * speed);
        }
    }
}
