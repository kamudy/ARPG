using UnityEngine;

public class PlayerRegen : MonoBehaviour
{
    [Header("HP Regen")]
    public bool regenHP = true;
    public float hpPerSecond = 1.5f;

    [Header("Mana Regen")]
    public bool regenMana = true;
    public float manaPerSecond = 2.0f;

    private PlayerHealth health;
    private PlayerMana mana;

    private float hpAcc;
    private float manaAcc;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        mana = GetComponent<PlayerMana>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (regenHP && health != null && health.currentHealth > 0)
        {
            hpAcc += hpPerSecond * dt;
            int heal = Mathf.FloorToInt(hpAcc);
            if (heal > 0)
            {
                hpAcc -= heal;
                bool healed = health.Heal(heal);
                if (healed)
                    Debug.Log($"[PlayerRegen] HP Regeneration: +{heal} (accumulated: {hpAcc})");
            }
        }

        if (regenMana && mana != null)
        {
            manaAcc += manaPerSecond * dt;
            int regen = Mathf.FloorToInt(manaAcc);
            if (regen > 0)
            {
                manaAcc -= regen;
                mana.Regen(regen);
            }
        }
    }
}
