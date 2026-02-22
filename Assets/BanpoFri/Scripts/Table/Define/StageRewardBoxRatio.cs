using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

namespace BanpoFri
{
    [System.Serializable]
    public class StageRewardBoxRatioData
    {
        [SerializeField]
		private int _reward_idx;
		public int reward_idx
		{
			get { return _reward_idx;}
			set { _reward_idx = value;}
		}
		[SerializeField]
		private int _ratio;
		public int ratio
		{
			get { return _ratio;}
			set { _ratio = value;}
		}

    }

    [System.Serializable]
    public class StageRewardBoxRatio : Table<StageRewardBoxRatioData, int>
    {
    }
}

