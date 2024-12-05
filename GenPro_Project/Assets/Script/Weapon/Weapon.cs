using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] internal float bulletLifeSpan;
    
    private float lastTimeFired;
    private bool isEven;
    
    internal void Start()
    {
        Reload();
    }

    internal void Update()
    {
        if(GetInput.Instance.GetIsFiring()) Shoot();
    }

    internal void Reload()
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
            
            Bullet bullet = Instantiate(bulletGo, transform.position, Quaternion.identity);
            bullet.transform.position =
                isEven ? gunBarrels[0].transform.position : gunBarrels[1].transform.position;
                
            bullet.InitBullet(bulletSpeed, isEven ? gunBarrels[0].transform.forward : gunBarrels[1].transform.forward,20, bulletLifeSpan);
        }
    }
}
