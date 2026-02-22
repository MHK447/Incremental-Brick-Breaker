using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum AlimentType
{
    None,
    Poison,
    Fire,
    Freeze,
}


public class AlimentSystem
{
    public List<AlimentData> AlimentList = new List<AlimentData>();


    public void Clear()
    {
        AlimentList.Clear();
    }


    public void AddAliment(AlimentType type, float time, EnemyUnit target, double damage, float damagedelay = 1f, int maxStackCount = 1)
    {
        AlimentData data = null;

        var finddata = AlimentList.Find(x => x.Target == target && x.Type == type && x.Time > 0f);

        if (finddata == null)
        {
            switch (type)
            {
                case AlimentType.Freeze:
                    {
                        data = new Freeze_Aliment();
                        AlimentList.Add(data);
                        break;
                    }
                case AlimentType.Poison:
                    {
                        data = new Poison_Aliment();
                        AlimentList.Add(data);
                        break;
                    }

            }
            data.Set(type, time, target, damage, damagedelay, maxStackCount);
        }
        else
        {
            finddata.Time = time;
            finddata.Damage = damage;
            // 중첩 카운트 증가 (최대치까지만)
            if (finddata.StackCount < finddata.MaxStackCount)
            {
                finddata.StackCount++;
            }
        }
    }

    public void RemoveAliment(AlimentType type, EnemyUnit target)
    {
        var finddata = AlimentList.Find(x => x.Target == target && x.Type == type && x.Time > 0f);

        if (finddata != null)
        {
            AlimentList.Remove(finddata);
        }
    }


    public void Update()
    {
        if (AlimentList.Count == 0) return;
        if (!GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value) return;

        // 역순으로 순회하며 유효하지 않은 상태의 알리멘트를 즉시 정리
        for (int i = AlimentList.Count - 1; i >= 0; i--)
        {
            var data = AlimentList[i];

            if (data == null
                || data.Target == null
                || data.Target.IsDead
                || data.Target.gameObject == null
                || !data.Target.gameObject.activeInHierarchy)
            {
                data?.OnRemove();
                AlimentList.RemoveAt(i);
                continue;
            }

            data.Update();

            // 지속 시간이 모두 소진된 항목도 정리
            if (data.Time <= 0f)
            {
                data.OnRemove();
                AlimentList.RemoveAt(i);
            }
        }
    }
}
