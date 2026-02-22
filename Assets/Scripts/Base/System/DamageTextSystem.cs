using System;
using Cysharp.Threading.Tasks;
using DamageNumbersPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DamageTextSystem
{
    private DmgNumberEffect dmg;
    private GameObject dmgAsset;

    public async void Create()
    {
        try
        {
            dmgAsset = await Addressables.LoadAssetAsync<GameObject>("DmgNumberEffect").ToUniTask();
            dmg = dmgAsset.GetComponent<DmgNumberEffect>();
            dmg.Number.PrewarmPool();
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    public void Dispose()
    {
        if (dmgAsset != null)
        {
            Addressables.Release(dmgAsset);
            dmgAsset = null;
            dmg = null;
        }
    }
    public void ShowDamage(double damage, Vector3 pos, Color color, bool iscritical = false)
    {
        double rounded = Math.Ceiling(damage * 10) / 10;
        int intPart = (int)rounded;
        double decimalPart = rounded - intPart;
        if (rounded > 10) decimalPart = 0.0;

        string formattedName;
        if (decimalPart == 0.0)
        {
            formattedName = intPart.ToString();
        }
        else
        {
            formattedName = rounded.ToString("0.#");
        }
        DmgNumberEffect instance = dmg.Number.Spawn(pos, formattedName, color).GetComponent<DmgNumberEffect>();

        instance.CriticalIcon.gameObject.SetActive(iscritical);
        instance.CriticalIcon.DORewind();
        instance.CriticalIcon.color = Color.white;
        instance.CriticalIcon.DOFade(0,0.3f)
            .SetEase(Ease.Linear)
            .SetDelay(0.1f);
    }
}


