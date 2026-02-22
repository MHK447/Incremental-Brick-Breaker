using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using BanpoFri;
using Unity.VisualScripting;
public class GameNotificationSystem
{
    public enum NotificationType
    {
        Once,
        Passive
    }

    public enum NotificationCategory
    {
        Training,
        CardUpgrade,
        CardBook,
        HeroUpgradeCheck,
        ShopAdCash,
    }

    public class NotificationData
    {
        public NotificationType type;
        public NotificationCategory category;
        public int targetIdx = -1;
        public int targetSubIdx = -1;
        public IReactiveProperty<bool> on = new ReactiveProperty<bool>(false);
        public IReactiveProperty<int> onCount = new ReactiveProperty<int>(0);
    }

    private ReactiveDictionary<NotificationCategory, List<NotificationData>> notifications = new ReactiveDictionary<NotificationCategory, List<NotificationData>>();
    private CompositeDisposable disposables = new CompositeDisposable();

    public ReactiveDictionary<NotificationCategory, List<NotificationData>> GetNotifications { get { return notifications; } }


    private bool IsPassive(NotificationCategory category)
    {
        return false;
    }

    public NotificationData GetData(NotificationCategory _key, int _targetIdx, int _targetSubIdx)
    {
        if (notifications.ContainsKey(_key))
        {
            if (notifications[_key].Count > 0)
            {
                return notifications[_key].Find(x => x.targetIdx == _targetIdx && x.targetSubIdx == _targetSubIdx);
            }
        }

        return null;
    }


    private int BuyOneCardPrice = 0;
    private int BuyTenCardPrice = 0;

    public void Create()
    {
        notifications.Clear();
        disposables.Clear();

        foreach (NotificationCategory e in System.Enum.GetValues(typeof(NotificationCategory)))
        {
            var managerNotiList = new List<NotificationData>();
            var managerNoti = new NotificationData()
            {
                type = IsPassive(e) ? NotificationType.Passive : NotificationType.Once,
                category = e,
                targetIdx = -1,
                targetSubIdx = -1
            };
            managerNotiList.Add(managerNoti);
            notifications.Add(e, managerNotiList);
        }

        GameRoot.Instance.UserData.Money.Subscribe(x =>
        {
            UpdateNotification(NotificationCategory.Training);
            UpdateNotification(NotificationCategory.HeroUpgradeCheck);
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Material.Subscribe(x =>
       {
           UpdateNotification(NotificationCategory.CardUpgrade);
       }).AddTo(disposables);

        foreach (var herogroupdata in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
        {
            herogroupdata.Level.Subscribe(x =>
            {
                UpdateNotification(NotificationCategory.HeroUpgradeCheck);
            }).AddTo(disposables);
        }


        UpdateNotification(NotificationCategory.ShopAdCash);


    }

    public void UpdateNotification(NotificationCategory category, int subIdx = -1)
    {
        switch (category)
        {
            case NotificationCategory.Training:
                {
                    bool ison = false;

                    var data = GetData(category, -1, -1);
                    if (data != null)
                    {
                        var NextBuyOrder = GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value + 1;


                        var td = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(NextBuyOrder);

                        var level = GameRoot.Instance.UserData.Stageidx.Value;

                        if (td != null)
                        {
                            ison = GameRoot.Instance.UserData.Money.Value >= td.cost && level >= td.level;
                        }


                        data.on.Value = ison;
                    }
                }
                break;
            case NotificationCategory.CardUpgrade:
                {
                    bool ison = false;

                    var data = GetData(category, -1, -1);
                    if (data != null)
                    {
                        bool isupgradelevel = false;

                        foreach (var carddata in GameRoot.Instance.UserData.Carddatas)
                        {
                            var cardupgradleveltd = Tables.Instance.GetTable<CardUpgradeLevel>().GetData(carddata.Cardlevel.Value);

                            if (cardupgradleveltd == null) return;

                            isupgradelevel = carddata.Cardcount.Value >= cardupgradleveltd.need_card;

                            if (ison) break;

                        }

                        ison = GameRoot.Instance.UserData.Material.Value >= 20 || isupgradelevel;

                        data.on.Value = ison;
                    }
                }
                break;
            case NotificationCategory.CardBook:
                {
                    bool ison = false;

                    var data = GetData(category, -1, -1);
                    if (data != null)
                    {

                        var tdlist = Tables.Instance.GetTable<EquipInfo>().DataList.FindAll(x => x.item_type != 3).ToList();

                        foreach (var td in tdlist)
                        {
                            for (int i = 0; i < 4; ++i)
                            {
                                var equipcheck = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.EQUIPWEAPONCOUNT, $"{td.idx}_{i + 1}");

                                var rewardbookweaponcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.REWARDBOOKWEAPONCOUNT, $"{td.idx}_{i + 1}");

                                if (equipcheck > 0 && rewardbookweaponcount == 0)
                                {
                                    ison = true;
                                    break;
                                }
                            }
                        }


                        data.on.Value = ison;
                    }
                }
                break;
            case NotificationCategory.HeroUpgradeCheck:
                {
                    bool ison = false;

                    var data = GetData(category, -1, -1);
                    if (data != null)
                    {
                        foreach (var equipheroitem in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
                        {
                            if (equipheroitem.Heroitemdata == null) continue;

                            var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(equipheroitem.Heroitemdata.Heroitemidx);

                            if (td == null) continue;

                            var finditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, td.item_equip_type);

                            var allfinditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, 0);

                            var UpgradeGoldCost = GameRoot.Instance.HeroSystem.equip_item_level_gold_cost * equipheroitem.Level.Value;
                            var UpgradeMaterialCount = GameRoot.Instance.HeroSystem.equip_item_level_count * equipheroitem.Level.Value;

                            var curupgradecountvalue = allfinditemdata.Itemcnt.Value + finditemdata.Itemcnt.Value;

                            if (UpgradeGoldCost <= GameRoot.Instance.UserData.Money.Value && UpgradeMaterialCount <= curupgradecountvalue)
                            {
                                ison = true;
                                break;
                            }
                        }

                        // merge available with 3+ same idx/grade
                        var heroItems = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas;
                        if (heroItems != null)
                        {
                            var canMerge = heroItems
                                .GroupBy(item => new { item.Heroitemidx, item.Grade })
                                .Any(group => group.Count() >= 3);

                            if (canMerge)
                            {
                                ison = true;
                            }
                        }

                        data.on.Value = ison;
                    }
                }
                break;
            case NotificationCategory.ShopAdCash:
                {

                    var data = GetData(category, -1, -1);
                    if (data != null)
                    {

                        var freecashcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.FreeCashCount);

                        data.on.Value = freecashcount == 0;
                    }
                }
                break;
        }
    }

    public void ChangeOnceNotification(NotificationCategory category, bool value, int targetIdx = -1)
    {
        if (notifications.ContainsKey(category))
        {
            var find = notifications[category].Find(x => x.targetIdx == targetIdx);
            if (find != null)
            {
                find.on.Value = value;
            }
        }
    }

    public void UpdateOneSeconds()
    {
    }

}
