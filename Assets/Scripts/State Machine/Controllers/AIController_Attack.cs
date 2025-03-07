using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Attack : MonoBehaviour
{

    public List<Gun> Weapons;

    public Transform[] BulletSpawnPoints;

    private AIController controller;

    private void Awake()
    {
        controller = GetComponent<AIController>();
    }

    private void Start()
    {
        SetWeapons();
    }

    private void SetWeapons()
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (BulletSpawnPoints.Length > i)
                Weapons[i].SetBulletSpawnPoint(controller.EnemyEntity, BulletSpawnPoints[i]);
            else Debug.LogWarning("not enough bullet spawn point for enemy weaponary");
        }
    }

    public virtual void StartAttackingTarget(Transform target)
    {
        foreach (var weapon in Weapons)
        {
            weapon.FireWeapon();
        }
    }
    public virtual void StopAttacking()
    {
        foreach (var weapon in Weapons)
        {
            weapon.StopFiringWeapon();
        }
    }
}
