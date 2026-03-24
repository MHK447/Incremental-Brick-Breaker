using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Weapon_Blade : Weapon_Base
{
    [SerializeField]
    private SpriteRenderer BladeImg;

    [SerializeField]
    private SpriteRenderer GearImg;


    [SerializeField]
    private ColliderAction Col;

    [SerializeField]
    private float _bladeRotateSpeed = 360f;

    private HashSet<EnemyUnitBase> _enemiesInContact = new HashSet<EnemyUnitBase>();
    private Coroutine _damageRoutine;

    public override void Set(WeaponData weaponData)
    {
        base.Set(weaponData);

        BladeImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame, $"InGame_Blade_Parts_7_1");
        GearImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame, $"InGame_Gear_{weaponData.WeaponIdx}");

        Col.TriggerEnterAction = OnTriggerEnter2D;
        Col.TriggerExitAction = OnTriggerExit2D;
    }

    void Update()
    {
        if (BladeImg != null)
            BladeImg.transform.Rotate(0f, 0f, -_bladeRotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<EnemyUnitBase>();
        if (enemy == null) return;

        _enemiesInContact.Add(enemy);
        if (_damageRoutine == null)
            _damageRoutine = StartCoroutine(ApplyDamageEveryInterval());
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<EnemyUnitBase>();
        if (enemy == null) return;

        _enemiesInContact.Remove(enemy);
        if (_enemiesInContact.Count == 0 && _damageRoutine != null)
        {
            StopCoroutine(_damageRoutine);
            _damageRoutine = null;
        }
    }

    IEnumerator ApplyDamageEveryInterval()
    {
        var wait = new WaitForSeconds(0.3f);
        while (true)
        {
            yield return wait;
            if (WeaponData == null) continue;
            int damage = WeaponData.WeaponDamage;
            foreach (var enemy in _enemiesInContact.ToList())
            {
                if (enemy == null || enemy.IsDead)
                    _enemiesInContact.Remove(enemy);
                else
                    enemy.Damage(damage);
            }
        }
    }
}

