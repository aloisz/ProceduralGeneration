using System.Collections;
using System.Collections.Generic;
using Airplane;
using NaughtyAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] internal Transform[] gunBarrels = new Transform[2];
    
    [Header("Gun properties")] 
    [SerializeField] private Bullet bulletGo;
    [SerializeField] internal int numberOfBullets;
    [SerializeField] [ReadOnly] internal int remainingBullets;
    [SerializeField] internal float fireRate;
    [SerializeField] internal float bulletSpeed;
    [SerializeField] internal float bulletSpeedPlaneFactor = 3f;
    [SerializeField] internal float bulletLifeSpan;
    
    private float lastTimeFired;
    private bool isEven;

    protected PlaneController plane;
    
    internal void Start()
    {
        plane = GetComponent<PlaneController>();
        Reload();
    }

    internal void Update()
    {
        if(GetInput.Instance.GetIsFiring()) Shoot();
    }

    public void Reload()
    {
        remainingBullets = numberOfBullets;
    }

    internal void Shoot()
    {
        if(remainingBullets <= 0) return;

        if (Time.time - lastTimeFired > 1 / fireRate)
        {
            lastTimeFired = Time.time;
            isEven = !isEven;
            
            Quaternion planeDir = Quaternion.LookRotation(plane.transform.forward * 1500 + plane.transform.position);
            
            Bullet bullet = Instantiate(bulletGo, transform.position, planeDir);
            bullet.transform.position =
                isEven ? gunBarrels[0].transform.position : gunBarrels[1].transform.position;
            
            bullet.InitBullet(bulletSpeed + (plane.actualSpeed * bulletSpeedPlaneFactor), 
                isEven ? gunBarrels[0].transform.forward : gunBarrels[1].transform.forward,20, bulletLifeSpan);

            remainingBullets--;
        }
    }
}
