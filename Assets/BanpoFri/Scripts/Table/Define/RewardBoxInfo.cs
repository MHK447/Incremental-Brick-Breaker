using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class RewardBoxInfoData
    {
        [SerializeField]
		private int _box_idx;
		public int box_idx
		{
			get { return _box_idx;}
			set { _box_idx = value;}
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
		private List<int> _reward_value_min;
		public List<int> reward_value_min
		{
			get { return _reward_value_min;}
			set { _reward_value_min = value;}
		}
		[SerializeField]
		private List<int> _reward_value_max;
		public List<int> reward_value_max
		{
			get { return _reward_value_max;}
			set { _reward_value_max = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private int _time;
		public int time
		{
			get { return _time;}
			set { _time = value;}
		}

    }

    [System.Serializable]
    public class RewardBoxInfo : Table<RewardBoxInfoData, int>
    {
    }
}

