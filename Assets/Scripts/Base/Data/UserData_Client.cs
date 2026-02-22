using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using BanpoFri;

public enum DataState
{
    None,
    Main,
    Event,
    Travel,
}
public partial class UserDataSystem
{
    public bool Bgm = true;
    public bool Effect = true;
    public bool Vib = true;
    public bool SlowGraphic = false;
    public long UUID { get; private set; } = 0;

    public Language Language = Language.en;
    public IReactiveProperty<int> Garnet { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> UpgradeStone { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Cash { get; private set; } = new ReactiveProperty<int>(0);
    public int ABTestValue { get; set; } = -1;

    public IReactiveCollection<string> BuyInappIds { get; private set; } = new ReactiveCollection<string>();
    public Dictionary<string, int> RecordCount { get; private set; } = new Dictionary<string, int>();
    public Dictionary<string, int> RecordValue { get; private set; } = new Dictionary<string, int>();
    public System.DateTime FirstStartTime = default(System.DateTime);

    public IReactiveCollection<string> GameNotifications { get; private set; } = new ReactiveCollection<string>();
    public IReactiveCollection<int> EquipCostumes { get; set; } = new ReactiveCollection<int>();

    public Queue<int> RandomSeeds = new Queue<int>();
    public int GameSpeedCount { get; set; }
    public IReactiveProperty<int> ADSpeedUpTimeProperty { get; set; } = new ReactiveProperty<int>(0);


    public bool AutoFelling { get; set; } = false;
    public bool SubscribeOrder { get; set; } = true;
    public IUserDataMode CurMode { get; private set; }
    public UserDataMain mainData { get; private set; } = new UserDataMain();


    private UserDataEvent eventData = new UserDataEvent();

    //public ReactiveCollection<SellerItemData> SellerItemData { get; set; } = new ReactiveCollection<SellerItemData>();
    public System.DateTime SellerTime { get; set; }

    public DataState DataState { get; private set; } = DataState.None;
    public bool IsMainState { get { return DataState == DataState.Main; } }
    public int WatchCount = 0;
    public int WatchInterval = 0;
    public BigInteger HeartBeatCount = 1;
    //public int StarRewardOrder = 1;

    public GameType LastMode = GameType.Main;

    public IReactiveProperty<BigInteger> HUDMoney = new ReactiveProperty<BigInteger>(0);
    public IReactiveProperty<int> HUDGarnet = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> HUDCash = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> HUDAdsTicket = new ReactiveProperty<int>(0);
    public IReactiveProperty<double> HUDMaterial = new ReactiveProperty<double>(0);
    public IReactiveProperty<double> HUDArtifactStone = new ReactiveProperty<double>(0);

    // @변수 자동 등록 위치
    public List<int> Equipcarddatas = new List<int>();
    public DateTime Resetdaytime { get; set; } = new DateTime();
    public DateTime Attendancetime { get; set; } = new DateTime();
    public IReactiveProperty<int> Ingamesilvercoin { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Newtrainingdatabuyorder { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Material { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Waveidx { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Vipticket { get; set; } = new ReactiveProperty<int>(0);
    public float Incomestartupgrade { get; set; } = 0.0f;
    public float Incomemultivalue { get; set; } = 0.0f;
    public int Highscorevalue { get; set; } = 0;
    public IReactiveProperty<int> Stageidx { get; set; } = new ReactiveProperty<int>(1);
    public IReactiveProperty<bool> Fishingautoproperty { get; set; } = new ReactiveProperty<bool>(false);
    public DateTime Dayinitialtime { get; set; }
    public int Energycreatefood { get; set; } = 0;
    public IReactiveProperty<int> Starcoinvalue { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Starvalue { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Energycoin { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Stageenergycount { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Nextstagecount { get; private set; } = new ReactiveProperty<int>(0);
    public List<int> Pobtest = new List<int>();
    public List<int> Ordertest = new List<int>();
    public int Testmoney { get; set; } = 0;
    public List<string> Tutorial = new List<string>();
    public IReactiveProperty<System.Numerics.BigInteger> Money { get; private set; } = new ReactiveProperty<System.Numerics.BigInteger>(0);
    public int Abtestvalue { get; set; } = 0;
    public long Uuid { get; set; } = 0;
    public long Gamestarttime { get; set; } = 0;
    public long Lastlogintime { get; set; } = 0;
    public string Buyinappids { get; set; } = "";
    public InGameResumeData IngameResumeData { get { return Ingameresumedata; } }

    private void ResetInGameResumeData()
    {
        
        Ingameresumedata.Isvalid = false;
        Ingameresumedata.Stageidx = 0;
        Ingameresumedata.Waveidx = 0;
        Ingameresumedata.Silvercoin = 0;
        Ingameresumedata.Hp = 0;
        Ingameresumedata.Starthp = 0;
        Ingameresumedata.Shield = 0;
        Ingameresumedata.Killcount = 0;
        Ingameresumedata.Ingameexp = 0;
        Ingameresumedata.Upgradecount = 1;
        Ingameresumedata.Ingamemoney = 0;
        Ingameresumedata.Rerollcount = 0;
        Ingameresumedata.Tiles.Clear();
        Ingameresumedata.Unlockedtiles.Clear();
        Ingameresumedata.Upgradeindices.Clear();
    }

    public bool HasInGameResumeData()
    {
        return Ingameresumedata.Isvalid && Ingameresumedata.Stageidx > 0 && Ingameresumedata.Waveidx > 0 && Ingameresumedata.Tiles.Count > 0;
    }

    public void ClearInGameResumeData(bool saveNow = true)
    {
        ResetInGameResumeData();
        if (saveNow)
        {
            Save(true);
        }
    }
    private InGameResumeData cachedResumeData = null;

    public bool StartBattleWithInGameResumeData()
    {
        if (!HasInGameResumeData())
        {
            return false;
        }

        cachedResumeData = new InGameResumeData();
        cachedResumeData.CopyFrom(Ingameresumedata);

        Stageidx.Value = cachedResumeData.Stageidx;

        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>()?.Stage;
        if (stage == null)
        {
            cachedResumeData = null;
            return false;
        }

        stage.StartBattle();

        Ingameresumedata.CopyFrom(cachedResumeData);
        cachedResumeData = null;

        return true;
    }


    void SetLoadDatas()
    {
        /* 아래 @주석 위치를 찾아서 함수가 자동 추가됩니다 ConnectReadOnlyDatas 함수에서 SetLoadDatas를 호출해주세요 */
        // @자동 로드 데이터 함수들
        LoadData_StarterPackageData();
        LoadData_ItemData();
        LoadData_HeroGroupData();
        LoadData_CardData();
        LoadData_PlayerData();
        
        LoadData_TrainingGroupData();
        LoadData_UnitGroupData();
        LoadData_InGameResumeData();
        LoadData_RecordCount();
        LoadData_OptionData();
    }
    void ConnectReadOnlyDatas()
    {
        GameNotifications.Clear();
        BuyInappIds.Clear();

        Tutorial.Clear();
        for (int i = 0; i < flatBufferUserData.TutorialLength; i++)
        {
            var item = flatBufferUserData.Tutorial(i);
            Tutorial.Add(item);
        }



        Money.Value = HUDMoney.Value = BigInteger.Parse(flatBufferUserData.Money);



        FirstStartTime = new System.DateTime(flatBufferUserData.Gamestarttime);

        BuyInappIds.Clear();
        if (!string.IsNullOrEmpty(flatBufferUserData.Buyinappids))
        {
            var splitArr = flatBufferUserData.Buyinappids.Split(';');
            foreach (var split in splitArr)
            {
                BuyInappIds.Add(split);
            }
        }


        if (flatBufferUserData.Uuid != 0)
            UUID = flatBufferUserData.Uuid;


        RecordCount.Clear();
        for (int i = 0; i < flatBufferUserData.RecordcountLength; ++i)
        {
            var data = flatBufferUserData.Recordcount(i);

            RecordCount.Add(data.Value.Idx, data.Value.Count);
        }


        RecordValue.Clear();
        for (int i = 0; i < flatBufferUserData.RecordvalueLength; ++i)
        {
            var data = flatBufferUserData.Recordvalue(i);
            RecordValue.Add(data.Value.Idx, data.Value.Count);
        }



        mainData.LastLoginTime = new System.DateTime(flatBufferUserData.Lastlogintime);

        HUDGarnet.Value = Garnet.Value = flatBufferUserData.Cash;

        // @로드 함수 호출
        SetLoadDatas();


        Resetdaytime = new DateTime(flatBufferUserData.Resetdaytime);
        Attendancetime = new DateTime(flatBufferUserData.Attendancetime);
        Ingamesilvercoin.Value = flatBufferUserData.Ingamesilvercoin;
        Newtrainingdatabuyorder.Value = flatBufferUserData.Newtrainingdatabuyorder;
        Material.Value = flatBufferUserData.Material;
        Waveidx.Value = flatBufferUserData.Waveidx;
        Vipticket.Value = flatBufferUserData.Vipticket;
        Waveidx.Value = flatBufferUserData.Waveidx;
        Money.Value = BigInteger.Parse(flatBufferUserData.Money);
        Stageidx.Value = flatBufferUserData.Stageidx;
        Cash.Value = flatBufferUserData.Cash;



        ChangeDataMode(LastMode == GameType.Event ? DataState.Event : DataState.Main);

        Language = (Language)System.Enum.Parse(typeof(Language), flatBufferUserData.Optiondata.Value.Language);
        Bgm = flatBufferUserData.Optiondata.Value.Bgm;
        Effect = flatBufferUserData.Optiondata.Value.Effect;
        SlowGraphic = flatBufferUserData.Optiondata.Value.Slowgraphic;
        Vib = flatBufferUserData.Optiondata.Value.Vibration;
        AutoFelling = flatBufferUserData.Optiondata.Value.Autofelling;
        SubscribeOrder = flatBufferUserData.Optiondata.Value.Subscribeorder;


        // var curStage = flatBufferUserData.stage;
        // mainData.StageData.SetStageIdx(curStage);

        //Debug.Log("stagedata:" + curStage);
    }

    public void ChangeDataMode(DataState state)
    {
        if (state == DataState)
            return;

        BpLog.Log($"ChangeDataMode:{state.ToString()}");
        switch (state)
        {
            case DataState.Main:
                CurMode = mainData;
                break;
            case DataState.Event:
                CurMode = eventData;
                break;
        }

        DataState = state;
    }


    public UserDataEvent CurEventData { get { return eventData; } }

    private void SnycCollectionToDB<T, U>(IList<T> db, IEnumerable<U> collector) where T : class
    {
        db.Clear();
        foreach (var iter in collector)
        {
            db.Add(iter as T);
        }
    }

    private void SnycCollectionToClient<T, U>(IList<T> db, IEnumerable<U> collector)
    where T : class, IReadOnlyData
    where U : class, IReadOnlyData
    {
        db.Clear();
        foreach (var iter in collector)
        {
            db.Add(iter.Clone() as T);
        }
    }

    public void SetRecordValue(Config.RecordKeys key, int value, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordValueText(key, objs);
        if (RecordValue.ContainsKey(strKey))
        {
            RecordValue[strKey] = value;
        }
        else
        {
            RecordValue.Add(strKey, value);
        }

        Save();
    }

    //public int GetABUser()
    //{
    //    if (GetRecordValue(Config.RecordKeys.AttTestUser) < 0)
    //        return 1;
    //    else
    //        return GetRecordValue(Config.RecordKeys.AttTestUser);
    //}

    public int GetRecordValue(Config.RecordKeys key, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordValueText(key, objs);
        if (RecordValue.ContainsKey(strKey))
        {
            return RecordValue[strKey];
        }
        else
        {
            return -1;
        }
    }

    public void ResetContainKeyRecordCount(Config.RecordCountKeys idx)
    {
        foreach (var key in RecordCount.Keys)
        {
            if (key.Contains(idx.ToString()))
            {
                RecordCount[key] = 0;
            }
        }
    }

    public void ResetRecordCount(Config.RecordCountKeys idx, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);
        if (RecordCount.ContainsKey(strKey))
            RecordCount[strKey] = 0;
    }


    public void ResetRecordCount(Config.RecordCountKeys idx, int resetvalue = 0, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);
        if (RecordCount.ContainsKey(strKey))
            RecordCount[strKey] = resetvalue;
    }



    public void AddRecordCount(Config.RecordCountKeys idx, int count, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);
        if (RecordCount.ContainsKey(strKey))
            RecordCount[strKey] += count;
        else
            RecordCount.Add(strKey, count);
    }

    public int GetRecordCount(Config.RecordCountKeys idx, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);

        if (RecordCount.ContainsKey(strKey))
        {
            return RecordCount[strKey];
        }
        else
        {
            return 0;
        }
    }

    public void SyncHUDCurrency(int currencyID = -1)
    {
        if (currencyID > 0)
        {
            HUDMoney.Value = Money.Value;
            HUDCash.Value = Cash.Value;
            HUDMaterial.Value = Material.Value;
        }
    }

    public void SetReward(int rewardType, int rewardIdx, System.Numerics.BigInteger rewardCnt, bool hudRefresh = true)
    {
        switch ((Config.RewardType)rewardType)
        {
            case Config.RewardType.Currency:
                {
                    switch (rewardIdx)
                    {

                        case (int)Config.CurrencyID.Cash:
                            {
                                GameRoot.Instance.UserData.Cash.Value += (int)rewardCnt;
                            }
                            break;
                        case (int)Config.CurrencyID.Money:
                            {
                                Money.Value += rewardCnt;
                            }
                            break;
                        case (int)Config.CurrencyID.Material:
                            {
                                Material.Value += (int)rewardCnt;
                            }
                            break;
                        case (int)Config.CurrencyID.SilverCoin:
                            {
                                Ingamesilvercoin.Value += (int)rewardCnt;
                            }
                            break;
                    }
                    GameRoot.Instance.UserData.Save();
                }
                break;
            case Config.RewardType.Item:
                {
                    var finddata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, rewardIdx);

                    if (finddata != null)
                    {
                        finddata.Itemcnt.Value += (int)rewardCnt;
                    }
                }
                break;
            case Config.RewardType.Card:
                {
                    GameRoot.Instance.CardSystem.AddCard(rewardIdx, (int)rewardCnt);
                }
                break;
            case Config.RewardType.HeroEquipment:
                {
                    GameRoot.Instance.UserData.Herogroudata.AddHeroItem(rewardIdx, (int)rewardCnt);
                }
                break;
            case Config.RewardType.RandHeroItem:
                {
                    var tdlist = Tables.Instance.GetTable<HeroItemInfo>().DataList;

                    var randtd = UnityEngine.Random.Range(0, tdlist.Count);

                    var findtd = tdlist[randtd];

                    GameRoot.Instance.UserData.Herogroudata.AddHeroItem(findtd.item_idx, rewardIdx);
                }
                break;
        }

        if (hudRefresh)
        {
            SyncHUDCurrency(rewardIdx);
        }
    }

    public void RemoveEventData()
    {

        //userEventData = new EventData();
        eventData = new UserDataEvent();
        Save();
    }


}
