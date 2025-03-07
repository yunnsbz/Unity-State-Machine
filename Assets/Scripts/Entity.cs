using System;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected int Health { get; set; }
    public int MaxHealth { get; protected set; } = 100;
    public bool IsLootable { get; private set; } = false;

    // events:
    public event Action<int> OnHealthChange;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        
    }

    public virtual void TakeDamage(Entity AttackOwner, int DamageAmount)
    {
        Health -= DamageAmount;

        if (Health <= 0) Health = 0;

        // bunun on death'dan önce çaðýrýlmasý gerekir
        OnHealthChange?.Invoke(Health);

        if (Health == 0) OnDeath(AttackOwner);

        //Debug.Log(gameObject.name + " took damage: " + DamageAmount);

    }

    public virtual void OnDeath(Entity AttackOwner)
    {
        Debug.Log(gameObject.name + " died");
        gameObject.SetActive(false);
    }

    public void TakeHealthPoints(int healthIncreaseAmount)
    {
        Health += healthIncreaseAmount;

        // tavan deðeri geçmemelisin:
        if (Health > MaxHealth) Health = MaxHealth;

        OnHealthChange?.Invoke(Health);
    }
}
