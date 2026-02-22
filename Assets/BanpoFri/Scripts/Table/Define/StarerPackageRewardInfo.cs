using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

namespace BanpoFri
{
    [System.Serializable]
    public class StarerPackageRewardInfoData
    {
        [SerializeField]
		private int _order;
		public int order
		{
			get { return _order;}
			set { _order = value;}
		}
		[SerializeField]
		private List<int> _reward_type;
		public List<int> reward_type
		{
			get { return _reward_type;}
			set { _reward_type = value;}
		}
		[SerializeField]
		private List<int> _reward_idx;
		public List<int> reward_idx
		{
			get { return _reward_idx;}
			set { _reward_idx = value;}
		}
		[SerializeField]
		private List<int> _reward_value;
		public List<int> reward_value
		{
			get { return _reward_value;}
			set { _reward_value = value;}
		}

    }

    [System.Serializable]
    public class StarerPackageRewardInfo : Table<StarerPackageRewardInfoData, int>
    {
    }
}

