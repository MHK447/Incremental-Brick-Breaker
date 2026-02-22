using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerBlock_Pistal : PlayerBlock_Projectile
{

    [SerializeField]
    private Animator PistolAnim;

    public override void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        base.Set(blockidx, order, grade, parentGroup);

        WeaponImg.transform.localScale = Vector3.one;
    }

    protected override void UpdateWeaponScale()
    {
    }

    override protected void OnAttackStarted()
    {
    }

    protected override void FireBullet(Vector3 position, Vector3 direction)
    {
        if (IsDead) return;

        // IsWaveRestProperty가 true일 때 활성화된 bullet 모두 삭제
        if (GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value)
        {
            ClearAllActiveBullets();
            return;
        }

        // 발사 시점에 가장 가까운 적 찾기
        Transform targetTransform = GetTargetTransformForFire();
        if (targetTransform == null)
        {
            // 적이 없으면 현재 활성화된 모든 bullet 처리
            ClearAllActiveBullets();
            return;
        }

        Bullet instance = BulletPool.Get();
        if (instance == null) return;

        instance.Set(BulletInfo, barrelTransform, targetTransform, OnBulletHit);

        PistolAnim.Play("Shoot" , 0 , 0f);

        SoundPlayer.Instance.PlaySound("weapon_shoot");

        // 활성화된 bullet 리스트에 추가
        ActiveBullets.Add(instance);
    }
}


