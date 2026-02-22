using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using System.Numerics;

public class EnemyBlockSpawner : MonoBehaviour
{
    public Transform UnitSpawner;

    [SerializeField]
    private SpriteRenderer EnemyBlockImg;

    [SerializeField]
    private List<Transform> SpawnerPointList = new List<Transform>();

    public List<Transform> GetUnitSpawnerPointList { get { return SpawnerPointList; } }

    [SerializeField]
    private EnemyUnit_Magic_Spawn EnemyUnit_Magic_Spawn;

    public int StartHp = 0;
    public int CurHp = 0;

    private EnemySpawnerHpProgress HpProgress;

    public bool IsDead { get { return CurHp <= 0; } }

    public bool IsSpawn = false;

    private int WaveAddSilverCoin = 0;
    private int RemainingCoin = 0;
    private float previousHpRatio = 1.0f;

    private WaveInfoData WaveInfoTd = null;

    private EnemyUnitGroup EnemyUnitGroup = null;

    private Coroutine enemyAllDieSpawnCoroutine = null;

    private bool IsDevil = false;

    public void Set(int enemyblockidx, bool isdevil = false)
    {
        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        IsDevil = isdevil;

        WaveInfoTd = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

        if (WaveInfoTd != null)
        {
            EnemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;

            StartHp = CurHp = WaveInfoTd.block_spawn_hp;

            IsSpawn = true;

            WaveAddSilverCoin = WaveInfoTd.add_silver_coin;
            RemainingCoin = WaveInfoTd.add_silver_coin;
            previousHpRatio = 1.0f;

            var offset = isdevil ? new UnityEngine.Vector3(0, 5.3f, 0) : new UnityEngine.Vector3(0, 2.5f, 0);

            if (HpProgress == null)
            {
                GameRoot.Instance.UISystem.LoadFloatingUI<EnemySpawnerHpProgress>(hpprogress =>
                  {
                      ProjectUtility.SetActiveCheck(hpprogress.gameObject, true);
                      HpProgress = hpprogress;
                      hpprogress.Init(UnitSpawner);
                      hpprogress.SetHpText(CurHp, StartHp);
                      hpprogress.SetOffset(offset);
                  });
            }
            else
            {
                ProjectUtility.SetActiveCheck(HpProgress.gameObject, true);
                HpProgress.SetHpText(CurHp, StartHp);
                HpProgress.SetOffset(offset);
            }

            var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

            ProjectUtility.SetActiveCheck(EnemyUnit_Magic_Spawn.gameObject, isdevil);

            if (isdevil)
            {
                var wavetd = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

                if (wavetd != null)
                {
                    EnemyUnit_Magic_Spawn.Set(22, wavetd.unit_dmg.Last(), wavetd.unit_hp.Last(), wavetd.add_exp_value);
                }
            }


            EnemyBlockImg.sprite = isdevil ? AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, $"InGameDevilTower_01") : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, $"InGameEnemySpawn_0{stagetd.ingame_map_idx}");

            EnemyBlockImg.transform.localScale = UnityEngine.Vector3.one;

            EnemyAllDieSpawn();
        }
    }



    public virtual void Damage(int damage)
    {
        // 데미지 전 HP 비율 계산
        float currentHpBeforeDamage = CurHp;
        float hpRatioBefore = currentHpBeforeDamage / StartHp;

        if (damage < 0) damage = 1;

        CurHp -= damage;
        SoundPlayer.Instance.PlaySound("effect_player_damage");
        ProjectUtility.Vibrate();

        // 데미지 후 HP 비율 계산
        float currentHpAfterDamage = Mathf.Max(0, CurHp);
        float hpRatioAfter = currentHpAfterDamage / StartHp;

        // HP 비율 차이만큼 코인 지급
        float hpRatioDiff = hpRatioBefore - hpRatioAfter;
        if (hpRatioDiff > 0 && RemainingCoin > 0)
        {
            int coinToGive = Mathf.RoundToInt(WaveAddSilverCoin * hpRatioDiff);
            coinToGive = Mathf.Min(coinToGive, RemainingCoin); // 남은 코인을 초과하지 않도록

            if (coinToGive > 0)
            {
                RemainingCoin -= coinToGive;
                DamageAddCoin(coinToGive);
            }
        }

        HpProgress.SetHpText(CurHp, StartHp);

        DamageColorEffect();

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, new UnityEngine.Vector3(transform.position.x, transform.position.y + 1.5f, 0), Color.white);

        if (CurHp <= 0 && IsSpawn)
        {
            ProjectUtility.SetActiveCheck(EnemyUnit_Magic_Spawn.gameObject , false);

            // 죽을 때 코루틴 정리
            StopEnemyAllDieSpawn();

            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.WaveEnd();

            IsSpawn = false;

            // 죽을 때 남은 코인 모두 지급
            if (RemainingCoin > 0)
            {
                DamageAddCoin(RemainingCoin);
                RemainingCoin = 0;
            }

            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StopWave();

            EnemyBlockImg.transform.DOScale(0, 0.5f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true)
            .OnComplete(() =>
            {
                if(!IsDevil)
                {
                    GameRoot.Instance.WaitTimeAndCallback(1.5f, () =>
                    {
                        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.EndStageReset();
                    });
                }
            });


            var wavecount = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == GameRoot.Instance.UserData.Stageidx.Value).Count;
            bool isLastWave = GameRoot.Instance.UserData.Waveidx.Value >= wavecount;

            if ((GameRoot.Instance.UserData.Waveidx.Value % 4 == 0 && GameRoot.Instance.UserData.Stageidx.Value > 2 && !isLastWave) || IsDevil)
            {
                GetStageReward(IsDevil);
            }



            ProjectUtility.SetActiveCheck(HpProgress.gameObject, false);
        }

        // 다음 데미지 계산을 위해 현재 HP 비율 저장
        previousHpRatio = hpRatioAfter;
    }

    public void GetStageReward(bool isdevil)
    {
        IsSpawn = false;

        // 화면 중앙 위치 구하기
        var cam = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().GetMainCam.cam;
        var screenCenter = cam.ScreenToWorldPoint(new UnityEngine.Vector3(Screen.width / 2f, Screen.height / 2f + Screen.height * 0.15f, cam.nearClipPlane + 10f));
        var targetWorldPos = new UnityEngine.Vector3(screenCenter.x, screenCenter.y, transform.position.z);

        // 화면 중앙에 임시 Transform 생성 (보상이 도착할 위치)
        GameObject tempTargetObj = new GameObject("RewardTarget");
        tempTargetObj.transform.SetParent(GameRoot.Instance.UISystem.WorldCanvas.transform);
        tempTargetObj.transform.position = ProjectUtility.worldToUISpace(GameRoot.Instance.UISystem.WorldCanvas, targetWorldPos);

        // 보상 스프라이트 파라미터 설정
        SpriteThrowEffectParameters rewardParameters = new SpriteThrowEffectParameters
        {
            sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"), // 기본 보상 스프라이트
            scale = 1.0f,
            duration = 3f,
            delay = 0f,
            fixRotation = false,
            rotation = 0f,
            scaleCurve = null
        };

        GameRoot.Instance.EffectSystem.MultiPlay<RewardThrowEffect>(transform.position, x =>
        {
            x.ShowWorldPos(isdevil, transform.position, tempTargetObj.transform, () =>
            {
                Destroy(tempTargetObj);
                
                if(isdevil)
                {
                    GameRoot.Instance.WaitTimeAndCallback(1.5f, () =>
                    {
                        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.EndStageReset();
                    });
                }
            }, rewardParameters);
        });
    }

    private bool IsDamageDirect = false;

    public virtual void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;


            EnemyBlockImg.EnableHitEffect();

            // Scale X 흔들림 효과 (맞는 듯한 효과)
            EnemyBlockImg.transform.DOPunchScale(new UnityEngine.Vector3(0.15f, 0, 0), 0.2f, 1, 0.5f).SetUpdate(true);



            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀

                    EnemyBlockImg.DisableHitEffect();

                    IsDamageDirect = false;
                }
            });
        }
    }

    public void CalcDamageAddCoin(int damage)
    {
    }


    private int currentSpawnIndex = 0;

    public Transform GetUnitSpawnerPoint()
    {
        if (SpawnerPointList == null || SpawnerPointList.Count == 0)
        {
            return UnitSpawner;
        }

        // 0~4 인덱스를 순차적으로 순환
        int index = currentSpawnIndex % SpawnerPointList.Count;
        currentSpawnIndex++;

        // currentSpawnIndex가 너무 커지는 것을 방지
        if (currentSpawnIndex >= SpawnerPointList.Count * 1000)
        {
            currentSpawnIndex = currentSpawnIndex % SpawnerPointList.Count;
        }

        return SpawnerPointList[index];
    }


    public void DamageAddCoin(int coin)
    {
        SpriteThrowEffectParameters coinparameters = new()
        {
            sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"),
            scale = 0.7f,
            duration = 1.2f,
        };
        GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(transform.position, (x) =>
          {
              var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;

              x.ShowWorldPos(transform.position, target, () =>
                               {
                                   GameRoot.Instance.UserData.Ingamesilvercoin.Value += coin;
                                   target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                               }, coinparameters);


              x.SetAutoRemove(true, 2f);
          });
    }


    public void EnemyAllDieSpawn()
    {
        if (GameRoot.Instance.UserData.Stageidx.Value < 3) return;

        // 이미 실행 중이면 중지
        if (enemyAllDieSpawnCoroutine != null)
        {
            if (GameRoot.Instance != null)
            {
                GameRoot.Instance.StopCoroutine(enemyAllDieSpawnCoroutine);
            }
            enemyAllDieSpawnCoroutine = null;
        }

        // 코루틴 시작
        if (GameRoot.Instance != null)
        {
            enemyAllDieSpawnCoroutine = GameRoot.Instance.StartCoroutine(EnemyAllDieSpawnCoroutine());
        }
    }

    public void StopEnemyAllDieSpawn()
    {
        if (enemyAllDieSpawnCoroutine != null)
        {
            if (GameRoot.Instance != null)
            {
                GameRoot.Instance.StopCoroutine(enemyAllDieSpawnCoroutine);
            }
            enemyAllDieSpawnCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        // 객체가 파괴될 때 코루틴 정리
        StopEnemyAllDieSpawn();
    }

    public void ClearData()
    {
        ProjectUtility.SetActiveCheck(HpProgress.gameObject, false);
    }

    private IEnumerator EnemyAllDieSpawnCoroutine()
    {
        // EnemyUnitGroup이 없으면 가져오기
        if (EnemyUnitGroup == null)
        {
            if (GameRoot.Instance == null || GameRoot.Instance.InGameSystem == null)
            {
                yield break;
            }
            EnemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;
        }

        // WaveInfoTd가 없으면 가져오기
        if (WaveInfoTd == null)
        {
            if (GameRoot.Instance == null || GameRoot.Instance.UserData == null)
            {
                yield break;
            }
            var waveidx = GameRoot.Instance.UserData.Waveidx.Value;
            var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
            WaveInfoTd = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));
        }

        // WaveInfoTd나 EnemyUnitGroup이 없으면 종료
        if (WaveInfoTd == null || EnemyUnitGroup == null || WaveInfoTd.unit_idx == null || WaveInfoTd.unit_idx.Count == 0)
        {
            yield break;
        }


        while (true)
        {
            // 객체가 파괴되었거나 필수 참조가 null이면 코루틴 종료
            if (this == null || GameRoot.Instance == null || EnemyUnitGroup == null || WaveInfoTd == null)
            {
                yield break;
            }

            // 게임이 시작되고, 웨이브 휴식 중이 아니며, EnemyBlockSpawner가 살아있을 때만 스폰 로직 실행
            if (GameRoot.Instance.UserData != null &&
                GameRoot.Instance.UserData.Playerdata != null &&
                GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value &&
                !GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value &&
                !IsDead && IsSpawn)
            {
                // 모든 적 유닛이 죽었는지 확인
                if (EnemyUnitGroup.ActiveUnits != null && EnemyUnitGroup.ActiveUnits.Count == 0)
                {
                    // 적 유닛 스폰
                    if (WaveInfoTd.unit_idx != null && WaveInfoTd.unit_idx.Count > 0 &&
                        WaveInfoTd.unit_dmg != null && WaveInfoTd.unit_dmg.Count > 0 &&
                        WaveInfoTd.unit_hp != null && WaveInfoTd.unit_hp.Count > 0)
                    {
                        EnemyUnitGroup.AddUnit(WaveInfoTd.unit_idx.Last(), 0, WaveInfoTd.unit_dmg.Last(), WaveInfoTd.unit_hp.Last());
                    }
                }
            }

            // 항상 5초 대기 (조건이 만족되지 않아도 대기 시간을 유지)
            yield return new WaitForSeconds(5f);
        }
    }
}

