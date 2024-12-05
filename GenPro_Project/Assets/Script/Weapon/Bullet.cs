using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;
    [SerializeField] private float damage;
    [SerializeField] private float lifeSpan;
    [SerializeField] private GameObject particleSys;
    private Rigidbody _rb;
    
    public void InitBullet (float speed, Vector3 direction, float damage, float lifeSpan)
    {
        this.speed = speed;
        this.direction = direction;
        this.lifeSpan = lifeSpan;
        this.damage = damage;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;
        Destroy(gameObject, lifeSpan);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.TryGetComponent(typeof(IDamage), out var damage))
        {
            ((IDamage)damage)?.Damage(this.damage);
            
        }

        Instantiate(particleSys, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = direction * (speed * Time.deltaTime);
    }
}