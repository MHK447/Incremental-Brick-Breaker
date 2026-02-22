using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;
using BanpoFri;


public enum Language
{
    en,
    ko,
    es,
    ja,
    ptbr,
    th,
    tw,
    vi,
    bg,
    cn,
    cs,
    da,
    nl,
    et,
    fi,
    fr,
    de,
    el,
    hu,
    id,
    it,
    lv,
    lt,
    no,
    pl,
    pt,
    ro,
    ru,
    sk,
    sl,
    sv,
    tr,
    ua

}


[System.Serializable]
public class FontDefine
{
    public Language country;
    public Font font;
}


[System.Serializable]
public class Config : BanpoFri.SingletonScriptableObject<Config>, BanpoFri.ILoader
{
    public enum FloatingUIDepth
    {
        HpProgress,
    }
    public enum LandCondination
    {
        Great,
        Basic,
        Sad,
    }

    public enum InGameUpgradeIdx
    {
        ATTACK,
        ATTACKSPEED,
        ATTACKRANGE,
        ATTACKREGEN,
        HP,
        HPREGEN,
        CRITICALPERCENT,
        CRITICALMULTIPLE,
    }

    public enum LABUpgradeIdx
    {
        ATTACK,
        ATTACKREGEN,
        ATTACKRANGE,
        HP,
        HPREGEN,
        CRITICALPERCENT,
        CRITICALDAMAGE,
    }

    public enum RecordKeys
    {
        StagePlayTime,
        EventStagePlayTime,
        Init,
        FirstDayPlayTime,
        FirstDayLogTime,
        M_Rev_05,
        ABTest,
        ShopDailyPurchaseCnt,
        TryTowerClear,
        UseADTicketCnt,

        AdCycleCount,

    }

    public enum ItemInfoType
    {
        AllEquipmentItem = 0,
        HelmatItem = 1,
        Aromor = 2,
        Shoose = 3,
        Ring = 4,
    }

    public enum WeaponType
    {
        Base = 1,
        TrackEnemy,
    }

    public enum CurrencyID
    {
        Money = 1,
        Cash = 2,
        Material = 3,

        SilverCoin = 101,
    }


    public enum InGameUpgradeChoice
    {
        // 기본 효과
        ElectricShock = 1,     // 고압 충격기 (적 사망 시 일정 확률로 폭발)
        SnailSlime = 2,     // 민달팽이 점액 (이동속도 50% 감소)
        GoblinBag = 3,     // 황금 고블린 가방 (피격 시 은화 강탈)
        SilverBar = 4,     // 은괴 가방 (즉시 50원 지급)

        // 속성 / 탄환 계열
        LightningStatue = 101,   // 번개의 석상 (확률 번개 낙하)
        FreezeBullet = 102,   // 동결탄 (확률 빙결)
        PoisonBullet = 103,   // 독 탄 (확률 중독)
        IronHornSpear = 104,   // 무쇠 뿔창 (관통 횟수 증가)
        SpeedBullet = 105,   // 스피드 총알 (투사체/공속 증가)
        KnockBackGun = 106,   // 넉백 산탄총
        RubberBullet = 107,   // 고무탄 (튕김)
        DoubleShot = 108,   // 더블샷건 (확률 2회 발사)
        BlockKnockBack = 109,   // 블럭 넉백 (확률 넉백)
        HpFull = 110,   // 체력 완전 회복 (체력 100% 회복)
        IncreaseHpMax = 111,   // 체력 최대치 증가 (체력 최대치 증가)

        // 특수 패시브
        BerserkerDoll = 1001,  // 광전사의 인형 (체력 일정 이하 공격력 상승)
        SlimeClone = 1002   // 복제 슬라임 (소환 시 확률로 2마리)
    }

    public enum UnitType
    {
        Enemy,
        Player,

    }

    public enum RewardType
    {
        Currency = 1,
        Card = 2,
        Item = 3,
        HeroEquipment = 4,
        RandHeroItem = 5,
    }

    public enum RecordCountKeys
    {
        FirstEcpm,
        Init,
        StartStage,
        Navi_Start,
        FreeGemCount,
        AdGemCount,
        AdCycleCount,
        TutorialStageCount,
        FirstSwayAdd,
        AdWatchCount,
        BuyInAppCountTotal,
        StageFailedCount,
        TrainingLevelUp,
        ReviewPopup,

        TrainingAdWatchCount,
        StageReward,

        NewEnemyBlockSpawnerReward,

        NewChapter,

        FREEREVIVALCOUNT,

        EQUIPWEAPONCOUNT,

        REWARDBOOKWEAPONCOUNT,
        AttendanceCount,
        AdAttendanceCount,
        StageLifeTime,

        AdsEquipCount,

        TryStageClear,

        ShowInterAdCount,
        StarterPackage,

        FreeCashCount,
    }

    public enum BoxTypeIdx
    {
        Rare,
        Epic,
        Legend,
    }



    public enum WeaponUnit
    {
        Knife = 1,
        Spear = 2,
        FireBallMagic = 3,
        BigSword = 4,
        Sickle = 5,
        Bow = 6,
        Shiled = 7,
        Shuriken = 8,
        LightSaber = 9,
        Katana = 10, // 구연 안함 아직 검기 
        Lifle = 11,
        Hammer = 12,
        Chainsaw = 13,

    }



    [System.Serializable]
    public class ColorDefine
    {
        public string key_string;
        public Color color;
    }

    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _textColorDefines = new List<ColorDefine>();
    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _eventTextColorDefines = new List<ColorDefine>();
    private Dictionary<string, Color> _textColorDefinesDic = new Dictionary<string, Color>();

    [SerializeField]
    private Font mainfont;
    [SerializeField]
    private List<FontDefine> _fontDefines = new List<FontDefine>();



    public List<ColorDefine> TextColorDefines
    {
        get
        {
            return _textColorDefines;
        }
    }
    public List<ColorDefine> EventTextColorDefines
    {
        get
        {
            return _eventTextColorDefines;
        }
    }

    public List<Material> TextFontMaterialList = new List<Material>();


    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _imageColorDefines = new List<ColorDefine>();
    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _eventImgaeColorDefines = new List<ColorDefine>();
    private Dictionary<string, Color> _imageColorDefinesDic = new Dictionary<string, Color>();
    public List<ColorDefine> ImageColorDefines
    {
        get
        {
            return _imageColorDefines;
        }
    }
    public List<ColorDefine> EventImageColorDefines
    {
        get
        {
            return _eventImgaeColorDefines;
        }
    }

    public Material SkeletonGraphicMat;
    public Material DisableSpriteMat;
    public Material EnableSpriteMat;
    public Material ImgAddtiveMat;
    public Material SolidMat;

    public Material DefaultMat;

    public List<Material> TextMaterialList = new List<Material>();




    public Color GetTextColor(string key)
    {
        if (_textColorDefinesDic.ContainsKey(key))
            return _textColorDefinesDic[key];

        return Color.white;
    }

    public Color GetBoxGradeTextColor(int grade)
    {
        switch (grade)
        {
            case 2:
                return GetTextColor("Blue");
            case 3:
                return GetTextColor("Purple");
            case 4:
                return GetTextColor("Yellow");
        }
        return Color.white;
    }


    public Color GetImageColor(string key)
    {
        if (_imageColorDefinesDic.ContainsKey(key))
            return _imageColorDefinesDic[key];

        return Color.white;
    }


    public Color GetUnitGradeColor(int grade)
    {
        switch (grade)
        {
            case 1:
                return GetImageColor("Unit_Grade_1");
            case 2:
                return GetImageColor("Unit_Grade_2");
            case 3:
                return GetImageColor("Unit_Grade_3");
        }

        return Color.white;
    }

    public void UpdateFallbackOrder(Language CurLangauge)
    {
        if (CurLangauge != Language.ja &&
            CurLangauge != Language.tw) return;

        // foreach (var name in mainfont.fontNames)
        //     Debug.Log("before :" + name);

        var list = mainfont.fontNames.ToList();

        var font = _fontDefines.Where(x => x.country == CurLangauge).FirstOrDefault();
        if (font != null)
        {
            list.Remove(font.font.fontNames[0]);
            list.Insert(1, font.font.fontNames[0]);

            var new_array = new string[list.Count];

            for (int i = 0; i < mainfont.fontNames.Length; i++)
            {
                new_array[i] = list[i];
            }

            mainfont.fontNames = new_array;

            new_array = null;

            // foreach (var name in mainfont.fontNames)
            //     Debug.Log("after :" + name);

        }
    }



    public void Load()
    {
        _textColorDefinesDic.Clear();
        foreach (var cd in _textColorDefines)
        {
            _textColorDefinesDic.Add(cd.key_string, cd.color);
        }
        foreach (var cd in _eventTextColorDefines)
        {
            _textColorDefinesDic.Add(cd.key_string, cd.color);
        }
        _imageColorDefinesDic.Clear();
        foreach (var cd in _imageColorDefines)
        {
            _imageColorDefinesDic.Add(cd.key_string, cd.color);
        }
        foreach (var cd in _eventImgaeColorDefines)
        {
            _imageColorDefinesDic.Add(cd.key_string, cd.color);
        }
    }

    public Sprite GetRewardImage(int rewardtype, int rewardidx)
    {
        switch (rewardtype)
        {
            case (int)RewardType.Currency:
                {
                    switch (rewardidx)
                    {
                        case (int)CurrencyID.Money:
                            return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Gold");
                        case (int)CurrencyID.Cash:
                            return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Cash");
                        case (int)CurrencyID.Material:
                            return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Material");
                        case (int)CurrencyID.SilverCoin:
                            return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Silver");
                    }
                }
                break;
            case (int)RewardType.Card:
                {
                    return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Card_Icon_{rewardidx}");
                }
            case (int)RewardType.Item:
                {
                    switch (rewardidx)
                    {
                        case (int)ItemInfoType.AllEquipmentItem:
                            {
                                return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, $"Equip_Upgrade_Item_{rewardidx}");
                            }
                    }

                    break;
                }
            case (int)RewardType.HeroEquipment:
                {
                    var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(rewardidx);

                    return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, td.item_img);
                }
            case (int)RewardType.RandHeroItem:
                {
                    return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_UI_Chest_{rewardidx}");
                }
        }
        return null;
    }
}
