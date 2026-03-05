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




        UpdateNotification(NotificationCategory.ShopAdCash);


    }

    public void UpdateNotification(NotificationCategory category, int subIdx = -1)
    {
        switch (category)
        {
            case NotificationCategory.Training:
                {
              
                }
                break;
            case NotificationCategory.CardUpgrade:
                {
                 
                    
                }
                break;
            case NotificationCategory.CardBook:
                {
                  
                    
                }
                break;
            case NotificationCategory.HeroUpgradeCheck:
                {
                    bool ison = false;

                    var data = GetData(category, -1, -1);
                    if (data != null)
                    {
                      
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
