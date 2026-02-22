using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using TMPro;


public class TileWeaponGroup : MonoBehaviour
{

    private List<TileWeaponComponent> TileWeaponComponentList = new List<TileWeaponComponent>();

    public List<TileWeaponComponent> GetTileWeaponComponentList { get { return TileWeaponComponentList; } }

    private List<TileAddComponent> TileAddComponentList = new List<TileAddComponent>();

    public List<TileAddComponent> GetTileAddComponentList { get { return TileAddComponentList; } }

    // 각 무기의 흔들림 시퀀스를 추적
    private Dictionary<TileWeaponComponent, Sequence> shakeSequences = new Dictionary<TileWeaponComponent, Sequence>();

    [SerializeField]
    public List<TileComponent> TileComponentList = new List<TileComponent>();

    [SerializeField]
    private Transform TileWeaponRootTr;


    public bool IsWeaponHolding { get { return TileWeaponComponentList.Find(x => x.IsHolding) != null; } }


    [SerializeField]
    private GameObject TileWeaponPrefab;

    [SerializeField]
    private GameObject TileAddPrefab;

    [SerializeField]
    private RectTransform RootTr;

    private float BannerRestYPos = 0f;
    private float BannerBattleYPos = 0f;


    private float QueueWeaponYPos = -210;

    private float QueueTileYPos = -210f;


    private CompositeDisposable disposables = new CompositeDisposable();

    private int CurSlotCount = 10;


    public void Init()
    {
        CurSlotCount = 10;

        foreach (var tilecomponent in TileComponentList)
        {
            tilecomponent.Init();
        }


        SortQueueTileWeapon();

        SetBannerPos();

        RootTr.anchoredPosition = new Vector3(0, BannerRestYPos, 0);

        disposables.Clear();


        GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.SkipLatestValueOnSubscribe().Subscribe(x =>
        {
            if (x)
            {
                RootTr.DOAnchorPos(new Vector2(0, BannerRestYPos), 0.3f).SetEase(Ease.OutQuad);
            }
            else
            {
                RootTr.DOAnchorPos(new Vector2(0, BannerBattleYPos), 0.3f).SetEase(Ease.OutQuad);
            }
        }).AddTo(disposables);

        SetStartBattleWeapon();

        if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TileWeaponAd))
        {
            AddPlusTile(1001, false, true);
        }
    }

    private void SetBannerPos()
    {
        BannerBattleYPos = GameRoot.Instance.ShopSystem.NoInterstitialAds.Value ? -300f : -141;
        BannerRestYPos = GameRoot.Instance.ShopSystem.NoInterstitialAds.Value ? 0f : 125f;

    }

    public void SetStartBattleWeapon()
    {
        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;


        var td = Tables.Instance.GetTable<WeaponSelectChoice>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));
        if (td != null)
        {
            for (int i = 0; i < td.select_equip_idx.Count; i++)
            {
                AddTileWeapon(td.select_equip_idx[i], td.select_equi_grade[i]);
            }
        }
        else
        {
            StartRandSelectWeapon(2, true);
            RandAdCheck();
        }

        // 세트 버프: 1웨이브 시 FirstStartRandWeapon / FirstStartRandTile 적용
        if (waveidx == 1)
        {
            if (GameRoot.Instance.HeroSystem.GetSetBuffValue(HeroItemSetType.FirstStartRandWeapon) > 0)
                StartRandSelectWeapon(1, true);
            if (GameRoot.Instance.HeroSystem.GetSetBuffValue(HeroItemSetType.FirstStartRandTile) > 0)
            {
                var randTileIdx = Random.Range(1001, 1004);
                AddPlusTile(randTileIdx, true);
            }
        }
    }

    public TileComponent GetTileComponent(Vector2 tileordervec, bool isunlocktile = true)
    {
        return isunlocktile ? TileComponentList.FirstOrDefault(x => x.TileOrderVec == tileordervec && x.IsUnLock) :
         TileComponentList.FirstOrDefault(x => x.TileOrderVec == tileordervec);
    }


    public void RandAdCheck()
    {
        if (!GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TileWeaponAd)) return;


        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        if (stageidx > 6)
        {

            var tdlist = Tables.Instance.GetTable<EquipInfo>().DataList.ToList().FindAll(x => x.equip_purpose == 1);

            var randvalue = Random.Range(0, tdlist.Count);

            var randidx = tdlist[randvalue].idx;

            var randgrade = Random.Range(2, 3);

            AddTileWeapon(randidx, randgrade, true);
        }
        else
        {

            var tdlist = Tables.Instance.GetTable<EquipInfo>().DataList.ToList().FindAll(x => x.first_ad_type == 1);

            var randvalue = Random.Range(0, tdlist.Count);

            var randidx = tdlist[randvalue].idx;


            var randgrade = Random.Range(2, 3);

            AddTileWeapon(randidx, randgrade, true);

        }

    }



    public void AddPlusTile(int equipidx, bool islucky = false, bool isad = false)
    {
        TileAddComponent targettile = null;

        var findtile = TileAddComponentList.Find(x => x.gameObject.activeSelf == false);

        if (findtile != null)
        {
            findtile.Set(equipidx, isad);
            ProjectUtility.SetActiveCheck(findtile.gameObject, true);
            findtile.transform.SetParent(TileWeaponRootTr);
            findtile.transform.localScale = Vector3.one;

            targettile = findtile;
        }
        else
        {
            var tileaddcomponent = Instantiate(TileAddPrefab, transform).GetComponent<TileAddComponent>();

            TileAddComponentList.Add(tileaddcomponent);

            tileaddcomponent.Set(equipidx, isad);

            tileaddcomponent.transform.SetParent(TileWeaponRootTr);
            tileaddcomponent.transform.localScale = Vector3.one;
            ProjectUtility.SetActiveCheck(tileaddcomponent.gameObject, true);

            targettile = tileaddcomponent;
        }

        // 새로운 타일이 추가되었으므로 대기열의 위치를 재정렬
        SortQueueTileWeapon();


        if (islucky)
        {
            GameRoot.Instance.WaitRealTimeAndCallback(0.1f, () =>
            {
                GameRoot.Instance.EffectSystem.MultiPlay<LuckyEffect>(targettile.transform.position, (x) =>
                      {
                          x.Init();
                          x.SetAutoRemove(true, 2f);
                      });
            });
        }
    }


    public void AddTileWeapon(int weaponidx, int grade, bool isad = false)
    {

        var findtile = TileWeaponComponentList.Find(x => x.gameObject.activeSelf == false);

        if (findtile != null)
        {
            findtile.Set(weaponidx, grade, isad);
            findtile.transform.SetParent(TileWeaponRootTr, false);

            // 이전 트윈 정리
            findtile.transform.DOKill();
            findtile.transform.localScale = Vector3.zero;
            ProjectUtility.SetActiveCheck(findtile.gameObject, true);

            // SetNativeSize가 완료된 후 스케일 트윈 실행 (레이아웃 업데이트를 위해 더 긴 대기)
            GameRoot.Instance.WaitRealTimeAndCallback(0.15f, () =>
            {
                // 트윈 시작 전 다시 한번 정리 (혹시 다른 곳에서 트윈이 실행되었을 경우)
                findtile.transform.DOKill();
                findtile.transform.localScale = Vector3.zero;

                findtile.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .OnKill(() =>
                    {
                        if (findtile != null && findtile.gameObject.activeSelf)
                        {
                            findtile.transform.localScale = Vector3.one;
                        }
                    })
                    .OnComplete(() =>
                    {
                        findtile.transform.localScale = Vector3.one;
                    });

                if (grade > 1 && !isad)
                {
                    GameRoot.Instance.EffectSystem.MultiPlay<LuckyEffect>(findtile.transform.position, (x) =>
                    {
                        x.Init();
                        x.SetAutoRemove(true, 2f);
                    });
                }
            });
        }
        else
        {
            var tileweaponcomponent = Instantiate(TileWeaponPrefab, TileWeaponRootTr).GetComponent<TileWeaponComponent>();

            TileWeaponComponentList.Add(tileweaponcomponent);

            tileweaponcomponent.Set(weaponidx, grade, isad);

            // 이전 트윈 정리
            tileweaponcomponent.transform.DOKill();
            ProjectUtility.SetActiveCheck(tileweaponcomponent.gameObject, true);

            // SetNativeSize가 완료된 후 스케일 트윈 실행 (레이아웃 업데이트를 위해 더 긴 대기)
            GameRoot.Instance.WaitRealTimeAndCallback(0.3f, () =>
            {
                // 트윈 시작 전 다시 한번 정리 (혹시 다른 곳에서 트윈이 실행되었을 경우)
                tileweaponcomponent.transform.DOKill();
                tileweaponcomponent.transform.localScale = Vector3.zero;

                tileweaponcomponent.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .OnKill(() =>
                    {
                        if (tileweaponcomponent != null && tileweaponcomponent.gameObject.activeSelf)
                        {
                            tileweaponcomponent.transform.localScale = Vector3.one;
                        }
                    })
                    .OnComplete(() =>
                    {
                        tileweaponcomponent.transform.localScale = Vector3.one;
                    });

                if (grade > 1 && !isad)
                {
                    GameRoot.Instance.EffectSystem.MultiPlay<LuckyEffect>(tileweaponcomponent.transform.position, (x) =>
                    {
                        x.Init();
                        x.SetAutoRemove(true, 2f);
                    });
                }
            }
            );
        }

        // 새로운 무기가 추가되었으므로 대기열 무기들의 위치를 재정렬
        SortQueueTileWeapon();

        // 새로운 무기가 추가되었으므로 모든 활성화된 무기들의 MergeOnCheck 호출
        RefreshAllWeaponsMergeCheck();
    }

    public void AddTileWeaponImmediate(int weaponidx, int grade, bool isad = false)
    {
        var findtile = TileWeaponComponentList.Find(x => x.gameObject.activeSelf == false);

        if (findtile != null)
        {
            findtile.Set(weaponidx, grade, isad);
            findtile.transform.SetParent(TileWeaponRootTr, false);
            findtile.transform.DOKill();
            findtile.transform.localScale = Vector3.one;
            ProjectUtility.SetActiveCheck(findtile.gameObject, true);
        }
        else
        {
            var tileweaponcomponent = Instantiate(TileWeaponPrefab, TileWeaponRootTr).GetComponent<TileWeaponComponent>();
            TileWeaponComponentList.Add(tileweaponcomponent);
            tileweaponcomponent.Set(weaponidx, grade, isad);
            tileweaponcomponent.transform.DOKill();
            tileweaponcomponent.transform.localScale = Vector3.one;
            ProjectUtility.SetActiveCheck(tileweaponcomponent.gameObject, true);
        }
    }

    public void RefreshAllWeaponsMergeCheck()
    {
        foreach (var weapon in TileWeaponComponentList)
        {
            if (weapon != null && weapon.gameObject.activeSelf)
            {
                weapon.MergeOnCheck();
            }
        }
    }


    public void SellNoneEquipWeapon()
    {
        // 장착되지 않은 무기들을 찾기
        var noneEquipWeapons = TileWeaponComponentList.FindAll(x =>
            x.IsEquip == false && x.gameObject.activeSelf == true);

        if (noneEquipWeapons == null || noneEquipWeapons.Count == 0)
        {
            return;
        }

        SpriteThrowEffectParameters coinparameters = new()
        {
            sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"),
            scale = 1.5f,
            duration = 1.2f,
        };

        var findall = TileAddComponentList.FindAll(x => x.IsAd);

        foreach (var add in findall)
        {
            ProjectUtility.SetActiveCheck(add.gameObject, false);
        }


        foreach (var weapon in noneEquipWeapons)
        {
            // 무기의 실제 UI 위치를 가져옴
            Vector3 weaponUIPos = new Vector3(0, -300f, 0) + weapon.transform.position;

            GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(weaponUIPos, (x) =>
        {
            var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;

            x.ShowUIPos(weaponUIPos, target, () =>
                             {
                                 GameRoot.Instance.UserData.Ingamesilvercoin.Value += weapon.Grade;
                                 target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                             }, coinparameters);


            x.SetAutoRemove(true, 2f);
        });
        }
        // 판매된 무기들을 비활성화
        foreach (var weapon in noneEquipWeapons)
        {
            ProjectUtility.SetActiveCheck(weapon.gameObject, false);
        }

        // 무기 위치 재정렬
        SortQueueTileWeapon();
    }


    public void DragEnd()
    {
        foreach (var tilecomponent in TileComponentList)
        {
            if (!tilecomponent.IsUnLock) continue;
            if (tilecomponent.IsTempUnlocked) continue;

            tilecomponent.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
        }
    }

    public void WeaponDragStart(TileWeaponComponent targetweapon)
    {
        SoundPlayer.Instance.PlaySound("sfx_get_equip_weapon");
        // 이 weapon을 가지고 있는 모든 TileComponent들의 IsEquip을 false로 설정
        foreach (var tileComponent in TileComponentList)
        {
            if (tileComponent.TargetTileWeaponComponent == targetweapon)
            {
                tileComponent.TargetTileWeaponComponent = null;
            }
        }
    }


    public int GetRandTileIdx()
    {
        var carddatas = GameRoot.Instance.UserData.Carddatas.FindAll(x=> x.Isequip.Value).ToList();

        var randvalue = Random.Range(0, carddatas.Count);


        return carddatas[randvalue].Cardidx;
    }

    public void StartRanSelectTile(int selecttileidx = -1)
    {
        var noneequiptilelist = TileWeaponComponentList.FindAll(x => x.IsEquip == false);

        var tdlist = Tables.Instance.GetTable<EquipInfo>().DataList.FindAll(x => x.item_type == 3);

        var randvalue = Random.Range(0, tdlist.Count);

        var tileId = selecttileidx != -1 ? selecttileidx : tdlist[randvalue].idx;

        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddPlusTile(tileId);
    }

    public void StartRandSelectBag()
    {
        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        if (waveidx >= 4)
        {
            int stageidx = GameRoot.Instance.UserData.Stageidx.Value;
            var curstagefailedcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageFailedCount, stageidx);
            int gradeuppercent = curstagefailedcount * 5;
            if (gradeuppercent > 20)
            {
                gradeuppercent = 20;
            }

            if (ProjectUtility.IsPercentSuccess(15 + gradeuppercent))
            {
                var randvalue = Random.Range(1002, 1005);
                AddPlusTile(randvalue, true);


            }
        }
    }

    public void StartRandSelectBagAd()
    {

        if (ProjectUtility.IsPercentSuccess(30) && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TileWeaponAd))
        {
            AddPlusTile(1001, false, true);
        }
    }


    public void StartRandSelectWeapon(int count, bool isinit = false)
    {
        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        int stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var curstagefailedcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageFailedCount, stageidx);

        int gradeuppercent = curstagefailedcount * 5;

        if (gradeuppercent > 20)
        {
            gradeuppercent = 20;
        }

        int grade = waveidx < 5 ? 1 : !ProjectUtility.IsPercentSuccess(20 + gradeuppercent) ? 1 : 2;

        var noneequiptilelist = TileWeaponComponentList.FindAll(x => x.IsEquip == false);

        foreach (var tileweaponcomponent in noneequiptilelist)
        {
            ProjectUtility.SetActiveCheck(tileweaponcomponent.gameObject, false);
        }

        // isinit이 true일 때 equip_purpose가 1인 것만 필터링
        List<int> availableCardIndices = null;
        if (isinit)
        {
            var equipPurpose1List = Tables.Instance.GetTable<EquipInfo>().DataList
                .Where(x => x.equip_purpose == 1)
                .Select(x => x.idx)
                .ToList();

            availableCardIndices = GameRoot.Instance.UserData.Carddatas
                .Where(card => equipPurpose1List.Contains(card.Cardidx) && card.Isequip.Value)
                .Select(card => card.Cardidx)
                .ToList();
        }

        // 이미 선택된 인덱스를 추적하는 HashSet
        HashSet<int> selectedIndices = new HashSet<int>();
        int maxAttempts = 100; // 무한 루프 방지를 위한 최대 시도 횟수

        for (int i = 0; i < count; i++)
        {
            int randidx = -1;
            int attempts = 0;

            // 중복되지 않는 인덱스를 찾을 때까지 반복
            while (attempts < maxAttempts)
            {
                if (isinit && availableCardIndices != null && availableCardIndices.Count > 0)
                {
                    // isinit이 true일 때는 equip_purpose가 1인 것만 선택
                    var randvalue = Random.Range(0, availableCardIndices.Count);
                    randidx = availableCardIndices[randvalue];
                }
                else
                {
                    randidx = GetRandTileIdx();
                }

                // 이미 선택되지 않은 인덱스라면 추가
                if (!selectedIndices.Contains(randidx))
                {
                    selectedIndices.Add(randidx);
                    break;
                }

                attempts++;
            }

            // 최대 시도 횟수를 초과한 경우 스킵
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"StartRandSelectWeapon: 중복되지 않는 무기를 찾을 수 없습니다. (시도 횟수 초과)");
                continue;
            }

            var td = Tables.Instance.GetTable<EquipInfo>().GetData(randidx);

            if (td != null)
            {
                if (td.item_type == 3)
                {
                    AddPlusTile(randidx);
                }
                else
                {
                    if (td.tilecheck_type == 9)
                    {
                        randidx = GetRandTileIdx();
                        AddTileWeapon(randidx, grade);
                    }

                    AddTileWeapon(randidx, grade);
                }
            }
        }
    }


    public void CheckMergeEquipTile(TileWeaponComponent targetweapon)
    {
        // 같은 Grade와 EquipIdx를 가진 무기들을 찾기 (활성화되어 있고, targetweapon은 제외)
        var findallcomponets = TileWeaponComponentList.FindAll(x =>
            x.gameObject.activeSelf
            && x.Grade == targetweapon.Grade
            && x.EquipIdx == targetweapon.EquipIdx
            && x != targetweapon);

        // 머지 가능한 무기들을 좌우로 흔들기
        foreach (var component in findallcomponets)
        {
            // 기존 시퀀스가 있으면 중지하고 제거
            if (shakeSequences.ContainsKey(component))
            {
                shakeSequences[component].Kill();
                shakeSequences.Remove(component);
            }

            // 기존 애니메이션 중지
            component.transform.DOKill();

            // 원래 로테이션 저장
            Vector3 originalRotation = component.transform.localEulerAngles;

            // 좌우로 회전하며 흔드는 애니메이션 (z축 회전)
            Sequence shakeSequence = DOTween.Sequence();
            shakeSequence.Append(component.transform.DOLocalRotate(new Vector3(0, 0, 15f), 0.1f))
                .Append(component.transform.DOLocalRotate(new Vector3(0, 0, -15f), 0.1f))
                .Append(component.transform.DOLocalRotate(new Vector3(0, 0, 10f), 0.1f))
                .Append(component.transform.DOLocalRotate(new Vector3(0, 0, -10f), 0.1f))
                .Append(component.transform.DOLocalRotate(originalRotation, 0.1f))
                .SetEase(Ease.OutQuad)
                .SetLoops(-1, LoopType.Restart) // 무한 반복
                .OnKill(() =>
                {
                    // 시퀀스가 Kill될 때 Dictionary에서 제거
                    if (shakeSequences.ContainsKey(component))
                    {
                        shakeSequences.Remove(component);
                    }
                });

            // 시퀀스를 Dictionary에 저장
            shakeSequences[component] = shakeSequence;
        }
    }

    public void StopAllShakeAnimations()
    {
        // Dictionary를 순회하면서 수정하지 않도록 ToList()로 복사본 생성
        var sequenceList = shakeSequences.ToList();

        // 저장된 모든 시퀀스를 중지
        foreach (var kvp in sequenceList)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Kill();
            }
        }
        shakeSequences.Clear();

        // 모든 무기의 흔들림 애니메이션 중지 및 로테이션 초기화
        foreach (var component in TileWeaponComponentList)
        {
            if (component != null && component.gameObject.activeSelf)
            {
                component.transform.DOKill();
                // 로테이션을 원래대로 복구
                component.transform.localEulerAngles = Vector3.zero;
            }
        }
    }



    public void SortQueueTileWeapon(TileWeaponComponent excludeWeapon = null)
    {
        var weaponList = TileWeaponComponentList.FindAll(x => x.IsEquip == false && x.gameObject.activeSelf == true && x != excludeWeapon);
        var addList = TileAddComponentList.FindAll(x => x.gameObject.activeSelf == true);

        // 두 리스트를 Transform과 타입 정보, IsAD 정보로 합침
        List<(Transform transform, bool isAddComponent, bool isAD)> findlist = new List<(Transform, bool, bool)>();
        foreach (var weapon in weaponList)
        {
            findlist.Add((weapon.transform, false, weapon.IsAD));
        }
        foreach (var add in addList)
        {
            findlist.Add((add.transform, true, add.IsAd));
        }

        // IsAD가 false인 것들을 먼저, true인 것들을 나중에 정렬 (맨 오른쪽으로)
        findlist = findlist.OrderBy(x => x.isAD).ToList();

        int count = findlist.Count;

        for (int i = 0; i < count; i++)
        {
            var item = findlist[i];
            item.transform.SetAsLastSibling();

            // x값 계산: 중앙(0)을 기준으로 좌우로 배치
            float xPos;
            if (count == 1)
            {
                xPos = 0f; // 1개일 때는 중앙에 배치
            }
            else
            {
                // 2개 이상일 때는 중앙을 기준으로 좌우로 배치
                float spacing = 150f; // 아이템 간 선호 간격
                float maxWidth = 580f; // -290 ~ 290의 최대 범위

                // 필요한 전체 너비가 최대 범위를 넘으면 간격 조절
                float requiredWidth = (count - 1) * spacing;
                if (requiredWidth > maxWidth)
                {
                    spacing = maxWidth / (count - 1);
                }

                // 중앙을 기준으로 배치
                float totalWidth = (count - 1) * spacing;
                float startX = -totalWidth / 2f;
                xPos = startX + i * spacing;
            }

            // 현재 위치에서 x값만 변경 (타입에 따라 다른 Y 위치 적용)
            Vector3 currentPos = item.transform.localPosition;
            float yPos = item.isAddComponent ? QueueTileYPos : QueueWeaponYPos;
            item.transform.localPosition = new Vector3(xPos, yPos, currentPos.z);
        }
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    public void ClearData()
    {
        // 모든 흔들림 애니메이션 중지
        StopAllShakeAnimations();

        // TileComponent에 장착된 무기들 초기화
        foreach (var tilecomponent in TileComponentList)
        {
            if (tilecomponent != null)
            {
                // 장착된 무기가 있다면 IsEquip을 false로 설정
                if (tilecomponent.TargetTileWeaponComponent != null)
                {
                    tilecomponent.TargetTileWeaponComponent.IsEquip = false;
                    tilecomponent.TargetTileWeaponComponent = null;
                }
            }
        }

        // TileWeaponComponent 비활성화
        foreach (var tileweaponcomponent in TileWeaponComponentList)
        {
            if (tileweaponcomponent != null)
            {
                tileweaponcomponent.IsEquip = false;
                ProjectUtility.SetActiveCheck(tileweaponcomponent.gameObject, false);
            }
        }

        // TileAddComponent 비활성화
        foreach (var tileaddcomponent in TileAddComponentList)
        {
            if (tileaddcomponent != null)
            {
                ProjectUtility.SetActiveCheck(tileaddcomponent.gameObject, false);
            }
        }

        // 슬롯 카운트 초기화
        CurSlotCount = 10;
    }

    public TileComponent GetTileComponent(int order)
    {
        return TileComponentList.FirstOrDefault(x => x.GetTileOrder == order);
    }


}

