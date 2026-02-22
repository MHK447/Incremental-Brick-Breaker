using System.Collections.Generic;
using System.Linq;
using BanpoFri;
using UnityEngine;

public enum InGameUpgradeCategory
{
    AddBlock = 1,
    AddSkill = 2,
    Other = 3,
    UpgradeBlock = 4,
}

public class InGameUpgradeSystem
{
    public int UpgradeCount = 0;

    private List<InGameUpgrade> AllUpgrades = new();

    public List<InGameUpgrade> ChoiceUpgrades = new();
    public List<UpgradeTier> ChoiceUpgradeTiers = new();

    public bool IsLuckyChoiceContext = false;

    private Dictionary<int, int> UpgradeLevelDic = new();


    public List<PlayerUpgradeStateModifier> InGameUpgradeList = new();


    public void Init()
    {
        AllUpgrades = GetAllUpgrades();
        Reset();
    }

    public void Reset()
    {
        InGameUpgradeList.Clear();
        UpgradeLevelDic.Clear();
        ChoiceUpgradeTiers.Clear();
        ChoiceUpgrades.Clear();
        UpgradeCount = 0;
        ClearUnitLevels();
    }

    //업그레이드 시젼할때 부름
    public List<InGameUpgrade> GetUpgrades(UpgradeTier minimumTier = UpgradeTier.Rare)
    {
        //티어설정
        UpgradeTier tierToApply;


        tierToApply = SelectTierByWeight(minimumTier);

        var selectData = GetSelectInfoData();
        if (selectData == null)
        {
            return NaturalChoices(AllUpgrades, tierToApply);
        }
        else
        {
            return ForceChoices(AllUpgrades, selectData, UpgradeTier.Rare);
        }
    }

    public WayChoicesSelectInfoData GetSelectInfoData()
    {

        WayChoicesSelectInfoData data = Tables.Instance.GetTable<WayChoicesSelectInfo>().DataList.FirstOrDefault(x =>
        {
            return x.stage == GameRoot.Instance.UserData.Stageidx.Value
            && x.challenge_count == GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageFailedCount, GameRoot.Instance.UserData.Stageidx.Value)
            && x.choice_count == UpgradeCount;
        });

        return data;
    }

    private List<InGameUpgrade> ForceChoices(List<InGameUpgrade> allUpgrades, WayChoicesSelectInfoData data, UpgradeTier tier)
    {
        List<InGameUpgrade> selected = allUpgrades.Where(x =>
        {
            // null 체크
            if (x == null || x.UpgradeChoiceData == null) return false;

            //set tier
            bool upgradeReturnValue = x.SetTierAndCheckSpawn(tier);

            //if data choice contains x
            int index = data.choices_idx.IndexOf(x.UpgradeChoiceData.idx);
            if (index < 0) return false;

            //filter block index
            if (data.recommend_order != 0 && (index + 1 == data.recommend_order)) x.SetRecommend(true);

            return true;
        }).Take(3).OrderBy(x => data.choices_idx.IndexOf(x.UpgradeChoiceData.idx)).ToList();



        return selected;
    }

    public List<InGameUpgrade> NaturalChoices(List<InGameUpgrade> allUpgrades, UpgradeTier tier)
    {
        List<InGameUpgrade> upgrades = FilterUpgrades(allUpgrades, tier);
        List<InGameUpgrade> result = new();
        if (upgrades == null || upgrades.Count == 0) return null;



        //가중치로 랜덤픽
        SelectNormalUpgrades(upgrades, result);

        // 선택지가 3개 미만일 경우 기본 업그레이드로 채우기
        if (result.Count < 3)
        {
            // 기본 업그레이드 우선순위: 체력증가, 데미지증가, 회전증가
            List<int> defaultUpgradeIdsList = new List<int>();
            
            // null 체크를 포함하여 안전하게 idx 수집
            void AddUpgradeId(InGameUpgrade upgrade)
            {
                if (upgrade != null && upgrade.UpgradeChoiceData != null)
                {
                    defaultUpgradeIdsList.Add(upgrade.UpgradeChoiceData.idx);
                }
            }
            
            AddUpgradeId(new InGameUpgrade_ElectricShock());
            AddUpgradeId(new InGameUpgrade_SnailSlime());
            AddUpgradeId(new InGameUpgrade_GoblinBag());
            AddUpgradeId(new InGameUpgrade_SilverBar());
            AddUpgradeId(new InGameUpgrade_LightningStatue());
            AddUpgradeId(new InGameUpgrade_IronHornSpear());
            AddUpgradeId(new InGameUpgrade_SpeedBullet());
            AddUpgradeId(new InGameUpgrade_KnockBackGun());
            // InGameUpgrade_RubberBullet은 GetAllUpgrades()에서 주석 처리되어 있어서 제외
            AddUpgradeId(new InGameUpgrade_DoubleShot());
            AddUpgradeId(new InGameUpgrade_BerserkerDoll());
            AddUpgradeId(new InGameUpgrade_SlimeClone());
            AddUpgradeId(new InGameUpgrade_HpFull());
            AddUpgradeId(new InGameUpgrade_IncreaseHpMax());
            AddUpgradeId(new InGameUpgrade_BlockKnockBack());
            
            int[] defaultUpgradeIds = defaultUpgradeIdsList.ToArray();

            foreach (int upgradeId in defaultUpgradeIds)
            {
                if (result.Count >= 3) break;

                // 이미 선택지에 없는 기본 업그레이드만 추가
                if (!result.Any(x => x != null && x.UpgradeChoiceData != null && x.UpgradeChoiceData.idx == upgradeId))
                {
                    InGameUpgrade defaultUpgrade = allUpgrades.FirstOrDefault(x => x != null && x.UpgradeChoiceData != null && x.UpgradeChoiceData.idx == upgradeId);
                    if (defaultUpgrade != null)
                    {
                        // 레벨이 Max인 경우 제외
                        if (defaultUpgrade.CanSpawn())
                        {
                            defaultUpgrade.SetTierAndCheckSpawn(tier);
                            result.Add(defaultUpgrade);
                        }
                    }
                }
            }
        }

        if (result == null || result.Count == 0) return null;


        return result;
    }


    private void SelectNormalUpgrades(List<InGameUpgrade> upgrades, List<InGameUpgrade> outResult)
    {
        //첫번째 선택지 블럭만 뜨게
        if (UpgradeCount == 0)
        {
            //block
            InGameUpgrade tempUpgrade = upgrades.Where(x => x.UpgradeChoiceData.category == (int)InGameUpgradeCategory.AddBlock 
                && !outResult.Any(r => r != null && r.UpgradeChoiceData != null && r.UpgradeChoiceData.idx == x.UpgradeChoiceData.idx)).ToList().GetRandom();
            if (tempUpgrade != null)
            {
                outResult.Add(tempUpgrade);
                upgrades.Remove(tempUpgrade);
            }
        }

        //두번째 선택지 무조건 블럭/스킬 하나 뜸
        if (UpgradeCount == 1)
        {
            //block
            InGameUpgrade tempUpgrade = upgrades.Where(x => x.UpgradeChoiceData.category == (int)InGameUpgradeCategory.AddBlock 
                && !outResult.Any(r => r != null && r.UpgradeChoiceData != null && r.UpgradeChoiceData.idx == x.UpgradeChoiceData.idx)).ToList().GetRandom();
            if (tempUpgrade != null)
            {
                outResult.Add(tempUpgrade);
                upgrades.Remove(tempUpgrade);
            }

            //skill
            tempUpgrade = upgrades.Where(x => x.UpgradeChoiceData.category == (int)InGameUpgradeCategory.AddSkill 
                && !outResult.Any(r => r != null && r.UpgradeChoiceData != null && r.UpgradeChoiceData.idx == x.UpgradeChoiceData.idx)).ToList().GetRandom();
            if (tempUpgrade != null)
            {
                outResult.Add(tempUpgrade);
                upgrades.Remove(tempUpgrade);
            }
        }

        //웨이트 채집 (이미 선택된 idx 제외)
        List<int> weights = new();
        List<int> validIndices = new();
        for (int i = 0; i < upgrades.Count; i++)
        {
            // 이미 선택된 idx가 아닌 경우만 포함
            if (!outResult.Any(r => r != null && r.UpgradeChoiceData != null && r.UpgradeChoiceData.idx == upgrades[i].UpgradeChoiceData.idx))
            {
                weights.Add(upgrades[i].UpgradeChoiceData.weight);
                validIndices.Add(i);
            }
        }

        //나머지 선택지들은 가중치로 랜덤
        for (int i = outResult.Count; i < Mathf.Min(3, outResult.Count + validIndices.Count); i++)
        {
            if (weights.Count == 0) break;
            
            int randomWeightIndex = ProjectUtility.GetWeightedRandomGrade(weights) - 1;
            int actualIndex = validIndices[randomWeightIndex];
            InGameUpgrade item = upgrades[actualIndex];

            // 선택된 항목과 같은 idx를 가진 모든 항목 제거
            upgrades.RemoveAll(x => x != null && x.UpgradeChoiceData != null && x.UpgradeChoiceData.idx == item.UpgradeChoiceData.idx);
            
            // validIndices와 weights 재계산
            weights.Clear();
            validIndices.Clear();
            for (int j = 0; j < upgrades.Count; j++)
            {
                if (!outResult.Any(r => r != null && r.UpgradeChoiceData != null && r.UpgradeChoiceData.idx == upgrades[j].UpgradeChoiceData.idx))
                {
                    weights.Add(upgrades[j].UpgradeChoiceData.weight);
                    validIndices.Add(j);
                }
            }

            outResult.Add(item);
        }

        //첫번째 선택지 순서 랜덤으로 (블럭이 매번 첫번째 자리에만 뜨면 어색할까봐)
        if (outResult.Count > 0)
        {
            InGameUpgrade temp = outResult[0];
            outResult.RemoveAt(0);
            outResult.Insert(UnityEngine.Random.Range(0, outResult.Count), temp);
        }
    }

    private List<InGameUpgrade> GetAllUpgrades()
    {
        List<InGameUpgrade> all = new()
        {
            //모든 강화 항목 여기에 추가


            new InGameUpgrade_ElectricShock(),
            new InGameUpgrade_SnailSlime(),
            new InGameUpgrade_GoblinBag(),
            new InGameUpgrade_SilverBar(),
            new InGameUpgrade_LightningStatue(),
            new InGameUpgrade_IronHornSpear(),
            new InGameUpgrade_SpeedBullet(),
            new InGameUpgrade_KnockBackGun(),
            //new InGameUpgrade_RubberBullet(), // CanSpawn()이 항상 false를 반환하므로 제외
            new InGameUpgrade_DoubleShot(),
            new InGameUpgrade_BerserkerDoll(),
            new InGameUpgrade_SlimeClone(),
            new InGameUpgrade_FrezeeBullet(),
            new InGameUpgrade_PoisonBullet(),
            new InGameUpgrade_HpFull(),
            new InGameUpgrade_IncreaseHpMax(),
            new InGameUpgrade_BlockKnockBack(),
        };

        return all;
    }

    private List<InGameUpgrade> FilterUpgrades(List<InGameUpgrade> allUpgrades, UpgradeTier tier)
    {
        return allUpgrades.Where(x =>
        {
            // null 체크
            if (x == null || x.UpgradeChoiceData == null) return false;

            // AddBlock 카테고리는 무조건 tier 1 (Rare)로 설정
            UpgradeTier tierToApply = (x.UpgradeChoiceData.category == (int)InGameUpgradeCategory.AddBlock)
                ? UpgradeTier.Rare
                : tier;
            return x.SetTierAndCheckSpawn(tierToApply);
        }).ToList();
    }


    const int default_level = 0;

    public void IncreaseUpgradeLevel(int unitIndex)
    {
        if (UpgradeLevelDic.ContainsKey(unitIndex))
        {
            UpgradeLevelDic[unitIndex] = Mathf.Min(3, UpgradeLevelDic[unitIndex] + 1);
        }
        else
        {
            UpgradeLevelDic.Add(unitIndex, default_level + 1);
        }
        //GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.InvokeUnitRefresh();
    }

    public int GetUpgradeLevel(int unitIndex)
    {
        if (UpgradeLevelDic.ContainsKey(unitIndex)) return UpgradeLevelDic[unitIndex];
        else return default_level;
    }

    public void ClearUnitLevels()
    {
        UpgradeLevelDic.Clear();
    }


    public static int GetWeightedRandomGrade(List<int> weights)
    {
        int totalWeight = 0;

        // 가중치의 총합 계산
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        // 1부터 총합까지의 랜덤 값을 생성
        int randomValue = UnityEngine.Random.Range(1, totalWeight + 1);
        int accumulatedWeight = 0;

        // 랜덤 값을 가중치에 따라 분류
        for (int i = 0; i < weights.Count; i++)
        {
            accumulatedWeight += weights[i];
            if (randomValue <= accumulatedWeight)
            {
                return i + 1;
            }
        }

        // 기본값
        return 1;
    }


    private UpgradeTier SelectTierByWeight(UpgradeTier minimumTier = UpgradeTier.Rare)
    {
        int stageIndex = GameRoot.Instance.UserData.Stageidx.Value;
        int tryCount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageFailedCount, stageIndex);
        //티어별 웨이트 테이블값 사용

        List<int> tierWeights = new(){
                    Tables.Instance.GetTable<ChoicesGroup>().GetData((int)UpgradeTier.Rare).value,
                    Tables.Instance.GetTable<ChoicesGroup>().GetData((int)UpgradeTier.Epic).value,
                    Tables.Instance.GetTable<ChoicesGroup>().GetData((int)UpgradeTier.Legendary).value,
                };
        return (UpgradeTier)Mathf.Max((int)minimumTier, ProjectUtility.GetWeightedRandomGrade(tierWeights));

        //else return (UpgradeTier)Mathf.Max((int)minimumTier, wayChoiceData.choices[UpgradeCount]);
    }






    public void AddStatModifier(PlayerUpgradeStateModifier modifier)
    {
        if (InGameUpgradeList.Contains(modifier))
        {
            InGameUpgradeList.Remove(modifier);
        }

        InGameUpgradeList.Add(modifier);
        InvokeUnitRefresh();
    }

    public PlayerUpgradeStateModifier GetModifier(Config.InGameUpgradeChoice upgradeidx)
    {
        return InGameUpgradeList.Find(x => x.StatType == upgradeidx);
    }

    public event System.Action StateRefreshEvent;

    public void InvokeUnitRefresh()

    {
        StateRefreshEvent?.Invoke();
    }

    public void ReapplyUpgrades(List<int> upgradeIndices)
    {
        Reset();
        if (AllUpgrades == null || AllUpgrades.Count == 0)
        {
            AllUpgrades = GetAllUpgrades();
        }

        foreach (var idx in upgradeIndices)
        {
            var upgrade = AllUpgrades.FirstOrDefault(u => u.UpgradeChoiceData != null && u.UpgradeChoiceData.idx == idx);
            if (upgrade != null)
            {
                upgrade.CallApply();
            }
        }
    }

    public void ClearData()
    {
        InGameUpgradeList.Clear();
    }
}


