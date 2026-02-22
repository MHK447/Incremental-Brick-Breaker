using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using BanpoFri;
using UniRx;

public class TileWeaponComponent : MonoBehaviour
{
    public Image WeaponImg;


    [SerializeField]
    private Image WeaponCoolTimeImg;

    [SerializeField]
    private GameObject UpgradeonObj;

    [SerializeField]
    private Button InfoBtn;

    [SerializeField]
    private GameObject AdRoot;

    [HideInInspector]
    public Vector3 OffsetPos;

    [HideInInspector]
    public int EquipIdx = 0;

    [HideInInspector]
    public bool IsEquip = false;

    [HideInInspector]
    public int Grade = 0;


    [HideInInspector]
    public TileComponent EquipTargetTileComponent = null;

    [HideInInspector]
    public PlayerBlock LinkedPlayerBlock = null;

    [HideInInspector]
    public bool IsHolding = false; // 홀딩 중인지 추적



    private CompositeDisposable disposables = new CompositeDisposable();


    private float TileCoolTime = 0f;

    private float CurCoolTime = 0f;

    private int BuffValue = 0;

    private bool IsCoolTimeActive = false;

    [HideInInspector]
    public bool IsAD = false;

    private bool IsCoolDownVisualActive = true;

    // 트윈 애니메이션 추적
    private Tweener currentScaleTween = null;


    void Awake()
    {
        InfoBtn.onClick.AddListener(OnClickInfo);
    }

    void OnDestroy()
    {
        // 트윈 정리
        if (currentScaleTween != null && currentScaleTween.IsActive())
        {
            currentScaleTween.Kill();
        }
        transform.DOKill();
    }

    public void Set(int weaponidx, int grade, bool isad = false)
    {
        EquipIdx = weaponidx;
        Grade = grade;
        IsAD = isad;

        var td = Tables.Instance.GetTable<EquipInfo>().GetData(weaponidx);

        if (td != null)
        {
            IsCoolDownVisualActive = td.idx == 106 ? false : true;

            BaseBuffValueSet();

#if BANPOFRI_LOG
            ProjectUtility.SetActiveCheck(AdRoot, false);
#else
            ProjectUtility.SetActiveCheck(AdRoot, isad);
#endif

            WeaponImg.sprite = WeaponCoolTimeImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_CommonWeapon, $"Common_Weapon_Type_{weaponidx}_{grade}");

            // 스프라이트가 로드되고 레이아웃이 업데이트된 후에 SetNativeSize 호출
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(SetNativeSizeAfterLayoutUpdate());
            }
            else
            {
                // 비활성화된 상태에서는 즉시 호출 (활성화될 때 정상 작동)
                WeaponImg.SetNativeSize();
                WeaponCoolTimeImg.SetNativeSize();
            }

            WeaponImg.raycastTarget = true;

            WeaponCoolTimeImg.fillAmount = 0f;

            // 클릭 영역을 넓게 설정 (약간 투명한 부분도 클릭 가능)

            if (WeaponImg != null)
            {
                // WeaponImg.alphaHitTestMinimumThreshold = td.tilecheck_type == 6 ? 0.0001f : 0f;
                WeaponImg.raycastTarget = true;
            }
            // 쿨타임 이미지는 클릭을 막지 않도록 설정
            WeaponCoolTimeImg.raycastTarget = false;


            // weapon_offset이 null이거나 배열 길이가 부족하면 기본값 사용
            if (td.weapon_offset != null && td.weapon_offset.Count >= 2)
            {
                OffsetPos = new Vector3(td.weapon_offset[0], td.weapon_offset[1], 0f);
            }
            else
            {
                OffsetPos = Vector3.zero;
            }

            // 쿨타임 설정
            TileCoolTime = td.cooltime * 0.01f;

            // 게임 시작 시 쿨타임 시작

            disposables.Clear();

            GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Subscribe(x =>
            {
                if (x)
                {
                    IsCoolTimeActive = false;
                    WeaponCoolTimeImg.fillAmount = 0f;
                }
                else
                {
                    StartCoolTime();
                }
            }).AddTo(disposables);
        }
    }

    // 무기 사용 시 쿨타임 시작
    public void StartCoolTime()
    {
        if (TileCoolTime > 0f)
        {
            CurCoolTime = TileCoolTime;
            IsCoolTimeActive = true;
            WeaponCoolTimeImg.fillAmount = 1f;
        }
    }

    // 쿨타임이 활성화되어 있는지 확인
    public bool IsOnCoolTime()
    {
        return IsCoolTimeActive;
    }

    public void AdCheck(bool isad)
    {
        IsAD = isad;
        ProjectUtility.SetActiveCheck(AdRoot, isad);
    }

    void Update()
    {
        if (IsCoolTimeActive && IsCoolDownVisualActive)
        {
            CurCoolTime -= Time.deltaTime;

            if (CurCoolTime <= 0f)
            {
                ResetCoolTime();
                PassiveBuffCheck(EquipIdx);
            }
            else
            {
                WeaponCoolTimeImg.fillAmount = CurCoolTime / TileCoolTime;
            }
        }
    }

    public void ResetCoolTime()
    {
        WeaponCoolTimeImg.fillAmount = 0f;

        CurCoolTime = TileCoolTime;

        // 이전 트윈 정리
        if (currentScaleTween != null && currentScaleTween.IsActive())
        {
            currentScaleTween.Kill();
        }
        transform.DOKill();

        // 생성 시 스케일 연출
        currentScaleTween = transform.DOScaleY(1, 0.2f)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                currentScaleTween = transform.DOScaleX(1.2f, 0.1f)
                    .SetEase(Ease.OutCubic)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        currentScaleTween = null;
                    });
            });
    }

    // 레이아웃 업데이트 후 SetNativeSize 호출
    private IEnumerator SetNativeSizeAfterLayoutUpdate()
    {
        // 스프라이트 로드를 위한 한 프레임 대기
        yield return null;

        // Canvas 업데이트를 위한 한 프레임 대기
        yield return new WaitForEndOfFrame();

        if (WeaponImg != null && WeaponImg.sprite != null)
        {
            WeaponImg.SetNativeSize();
        }

        if (WeaponCoolTimeImg != null && WeaponCoolTimeImg.sprite != null)
        {
            WeaponCoolTimeImg.SetNativeSize();
        }
    }


    public void EquipTile(TileComponent tile)
    {
        var td = Tables.Instance.GetTable<EquipInfo>().GetData(EquipIdx);

        Vector3 targetvec = Vector3.zero;

        if (td != null)
        {
            IsEquip = true;
            EquipTargetTileComponent = tile;

            //WeaponImg.raycastTarget = false;

            // 장착 시 TileSystem의 리스트에 데이터 추가 (중복 허용)
            GameRoot.Instance.TileSystem.EquipItemList.Add(td);

            var equipveclist = GameRoot.Instance.TileSystem.TileTypeList[td.tilecheck_type - 1];

            var tileGroup = GetComponentInParent<TileWeaponGroup>();
            if (tileGroup != null && equipveclist.Count > 0)
            {
                // equipveclist의 Vector2 좌표들의 평균을 계산
                Vector2 avgVec = Vector2.zero;

                for (int i = 1; i < equipveclist.Count; i++)
                {
                    avgVec += equipveclist[i];
                }

                avgVec /= equipveclist.Count;

                // 기준 타일 위치 가져오기
                Vector3 baseTilePos = tile.transform.position;

                // 타일 간격 계산 (기준 타일과 (1,0) 타일의 거리로 x간격, (0,-1) 타일의 거리로 y간격 계산)
                Vector2 tileSpacing = Vector2.zero;



                foreach (var equipVec in equipveclist)
                {
                    var downTile = tileGroup.GetTileComponent(tile.TileOrderVec + equipVec);
                    tileSpacing.y += downTile.transform.localPosition.y;
                    tileSpacing.x += downTile.transform.localPosition.x;
                }

                // 최종 위치 = 기준 타일 위치 + 오프셋
                targetvec = new Vector2(tileSpacing.x / equipveclist.Count, tileSpacing.y / equipveclist.Count);


            }
        }

        this.transform.localPosition = targetvec;
    }

    public void UnequipTile()
    {
        var td = Tables.Instance.GetTable<EquipInfo>().GetData(EquipIdx);

        if (td != null)
        {
            // 장착 해제 시 TileSystem의 리스트에서 데이터 제거 (첫 번째 일치 항목만 제거)
            GameRoot.Instance.TileSystem.EquipItemList.Remove(td);
        }

        IsEquip = false;
        EquipTargetTileComponent = null;
    }


    public void OnClickInfo()
    {
        // 홀딩 중이면 버튼 클릭 무시
        if (IsHolding || GameRoot.Instance. UserData.Stageidx.Value <= 2) return;

        GameRoot.Instance.UISystem.OpenUI<PopupMergeWeaponInfo>(popup => popup.Set(this));
    }


    public void MergeOnCheck()
    {
        // TileWeaponGroup에서 같은 Grade와 EquipIdx를 가진 무기가 있는지 확인
        var tileWeaponGroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup;

        if (tileWeaponGroup != null)
        {
            // 현재 무기와 병합 가능한 무기들 찾기 (자기 자신 제외)
            var mergeableWeapons = tileWeaponGroup.GetTileWeaponComponentList
                .Find(x => x.Grade == this.Grade
                    && x.EquipIdx == this.EquipIdx
                    && x != this
                    && x.gameObject.activeSelf
                    && !x.IsAD
                    && x.Grade < 4);

            // 병합 가능한 무기가 있으면 UpgradeonObj 활성화, 없으면 비활성화
            ProjectUtility.SetActiveCheck(UpgradeonObj.gameObject, mergeableWeapons != null);
        }
        else
        {
            // TileWeaponGroup을 찾지 못한 경우 비활성화
            ProjectUtility.SetActiveCheck(UpgradeonObj.gameObject, false);
        }
    }



    public void PassiveBuffCheck(int equipidx)
    {
        switch (equipidx)
        {
            case 107:
                {
                    var cardShieldValue = GameRoot.Instance.CardSystem.GetCardValue(107, UnitStatusType.Shield);
                    GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Value += BuffValue + (int)cardShieldValue;

                    SoundPlayer.Instance.PlaySound("effect_item_shield");
                }
                break;
            case 108:
                {
                    SpriteThrowEffectParameters coinparameters = new()
                    {
                        sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"),
                        scale = 0.7f,
                        duration = 1.2f,
                    };
                    Vector3 weaponUIPos = this.transform.position;

                    GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(weaponUIPos, (x) =>
                      {
                          var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;


                          x.ShowUIPos(weaponUIPos, target, () =>
                                           {
                                               var cardhealvalue = GameRoot.Instance.CardSystem.GetCardValue(108, UnitStatusType.SILVERCOIN);

                                               target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                                               GameRoot.Instance.UserData.Ingamesilvercoin.Value += BuffValue + (int)cardhealvalue;
                                           }, coinparameters);


                          x.SetAutoRemove(true, 2f);
                      });
                }
                break;
            case 109:
                {
                    var cardhealvalue = GameRoot.Instance.CardSystem.GetCardValue(109, UnitStatusType.HpHeal);

                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerBlockGroup.HpHeal(BuffValue + (int)cardhealvalue);


                    SoundPlayer.Instance.PlaySound("effect_castle_heal");
                }
                break;
        }

    }



    public void BaseBuffValueSet()
    {
        var td = Tables.Instance.GetTable<EquipInfo>().GetData(EquipIdx);

        if (td != null)
        {
            BuffValue = td.base_value_1 * Grade;
        }
    }
}