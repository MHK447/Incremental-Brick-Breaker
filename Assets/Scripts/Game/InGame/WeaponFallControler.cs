using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class WeaponFallControler : MonoBehaviour
{
    private int StartFallCount = 1;

    private float FallDelTime = 0f;

    private float BaseFallCoolTime = 2f;


    protected List<FallWeaponBase> ActiveFallBullets = new List<FallWeaponBase>();


    public void Init()
    {
        FallDelTime = 0f;

        GameRoot.Instance.UserData.InGamePlayerData.InitIncreaMentalBonuses();

        var playerData = GameRoot.Instance.UserData.InGamePlayerData;
        StartFallCount = Mathf.Max(1, playerData.WeaponFallStartCount);

        playerData.WeaponFallCountProperty.Value = StartFallCount;
        playerData.WeaponFallCooldownProgressProperty.Value = 1f;
    }

    public void CreateFallEffect(Vector3 worldPosition, int weaponidx)
    {
        if (GameRoot.Instance.UserData.InGamePlayerData.WeaponFallCountProperty.Value < 1)
            return;

        GameRoot.Instance.UserData.InGamePlayerData.WeaponFallCountProperty.Value -= 1;

        GameRoot.Instance.EffectSystem.MultiPlay<WeaponFallEffect>(worldPosition, (effect) =>
        {
            ProjectUtility.SetActiveCheck(effect.gameObject, true);
            effect.Init(weaponidx);
        });
    }

    private float GetCurrentFallCoolTime()
    {
        float speedMult = GameRoot.Instance.UserData.InGamePlayerData.FallWeaponSpeedMultiplier;
        return speedMult > 0f ? BaseFallCoolTime / speedMult : BaseFallCoolTime;
    }

    void Update()
    {
        var inGameData = GameRoot.Instance.UserData.InGamePlayerData;
        float fallCoolTime = GetCurrentFallCoolTime();
        if (inGameData.WeaponFallCountProperty.Value < inGameData.WeaponFallStartCount)
        {
            FallDelTime += Time.deltaTime;
            inGameData.WeaponFallCooldownProgressProperty.Value = Mathf.Clamp01(FallDelTime / fallCoolTime);
            if (FallDelTime >= fallCoolTime)
            {
                inGameData.WeaponFallCountProperty.Value += 1;
                FallDelTime = 0f;
            }
        }
        else
        {
            inGameData.WeaponFallCooldownProgressProperty.Value = 1f;
        }

        if (Input.GetMouseButtonUp(0)
            && !GameRoot.Instance.IsPointerOverUIObject(Input.mousePosition)
            && GameRoot.Instance.UserData.InGamePlayerData.WeaponFallCountProperty.Value >= 1)
        {
            var inGame = GameRoot.Instance.InGameSystem.CurInGame;
            if (inGame != null)
            {
                var cam = inGame.GetMainCam?.cam;
                if (cam != null)
                {
                    var screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z));
                    var worldPos = cam.ScreenToWorldPoint(screenPos);
                    CreateFallEffect(worldPos, GameRoot.Instance.UserData.InGamePlayerData.FallWeaponIdxProperty.Value);
                }
            }
        }
    }

    public void FallAddBullet(int weaponidx, Vector3 pos)
    {
        if (GameRoot.Instance.IncreaMentalSystem.IsBombUpgraded())
        {
            weaponidx = InGamePlayerData.FallWeaponIdx_Bomb;
        }

        var td = Tables.Instance.GetTable<WeaponInfo>().GetData(weaponidx);

        if (td == null)
        {
            weaponidx = InGamePlayerData.FallWeaponIdx_Default;
            td = Tables.Instance.GetTable<WeaponInfo>().GetData(weaponidx);
        }

        if (td != null)
        {
            var finddata = ActiveFallBullets.FirstOrDefault(x => x.GetWeaponData.WeaponIdx == weaponidx && !x.gameObject.activeSelf);

            if (finddata == null)
            {

                Addressables.InstantiateAsync(td.prefab).Completed += (handle) =>
                {
                    var weaponData = new WeaponData();
                    weaponData.WeaponIdx = weaponidx;
                    weaponData.WeaponDamage = td.base_dmg;
                    weaponData.WeaponCoolTime = td.attack_cooltime / 100f;
                    weaponData.WeaponRange = td.attack_range / 100f;

                    var bullet = handle.Result.GetComponent<FallWeaponBase>();

                    bullet.Set(weaponData, pos, OnBulletHit);

                    ActiveFallBullets.Add(bullet);
                };
            }
            else
            {
                finddata.Set(finddata.GetWeaponData, pos, OnBulletHit);
                finddata.transform.position = pos;

                ProjectUtility.SetActiveCheck(finddata.gameObject, true);
            }
        }
    }

    public void OnBulletHit(FallWeaponBase bullet)
    {
        ProjectUtility.SetActiveCheck(bullet.gameObject, false);
    }

    private IEnumerator DisableAfterTrail(FallWeaponBase bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bullet != null && bullet.gameObject != null)
            ProjectUtility.SetActiveCheck(bullet.gameObject, false);
    }

}

