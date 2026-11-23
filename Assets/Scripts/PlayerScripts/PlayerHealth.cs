using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    // Player Health Points
    [SerializeField] private int Health = 100;
    // Damage taken from a weaker enemy
    [SerializeField] private int weakDamage = 10;
    // Damage taken from a normal enemy
    [SerializeField] private int normalDamage = 25;
    // Damage taken from a strong enemy
    [SerializeField] private int heavyDamge = 40;
    
    // Health recovery for a small amount of health
    [SerializeField] private int smallHealing = 25;
    //Health recovery for a normal amount of health 
    [SerializeField] private int nomralHealing = 50;
    // Health recovery for a large amount of health
    [SerializeField] private int largeHealing = 75;

    // Audio for Damage
    [SerializeField] private AudioClip[] hurtSounds;
    // Audio for Critical Damage
    [SerializeField] private AudioClip criticalDamage;
    // Audio for healing from a small item
    [SerializeField] private AudioClip smallHeal;
    // Audio for healing from a large item
    [SerializeField] private AudioClip bigHeal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Function used for when the player takes damage
    void TakeDamage()
    {
        
    }

    // Function used for when the player heals themselves
    void HealHealth()
    {
        
    }
}
