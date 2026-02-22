using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class StageRewardInfoData
    {
        [SerializeField]
		private int _reward_idx;
		public int reward_idx
		{
			get { return _reward_idx;}
			set { _reward_idx = value;}
		}
		[SerializeField]
		private int _coin_base_cost;
		public int coin_base_cost
		{
			get { return _coin_base_cost;}
			set { _coin_base_cost = value;}
		}
		[SerializeField]
		private int _coin_increase;
		public int coin_increase
		{
			get { return _coin_increase;}
			set { _coin_increase = value;}
		}

    }

    [System.Serializable]
    public class StageRewardInfo : Table<StageRewardInfoData, int>
    {
    }
}

