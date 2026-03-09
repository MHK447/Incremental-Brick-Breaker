using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[EffectPath("Effect/WeaponFallEffect", false, false)]
public class WeaponFallEffect : Effect
{
    private int Damage = 0;

    public void Init(int damage)
    {
        Damage = damage;
    }

    public void WeaponFall()
    {
        SetAutoRemove(true , 0f);
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.GetWeaponFallControler.FallAddBullet(
            GameRoot.Instance.UserData.InGamePlayerData.FallWeaponIdxProperty.Value, 
        new Vector3(this.transform.position.x , this.transform.position.y + 5f, this.transform.position.z));
    }


}