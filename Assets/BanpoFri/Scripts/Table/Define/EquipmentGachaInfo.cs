using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class EquipmentGachaInfoData
    {
        [SerializeField]
		private int _level;
		public int level
		{
			get { return _level;}
			set { _level = value;}
		}
		[SerializeField]
		private List<int> _gacha_ratio;
		public List<int> gacha_ratio
		{
			get { return _gacha_ratio;}
			set { _gacha_ratio = value;}
		}
		[SerializeField]
		private int _rand_level_min;
		public int rand_level_min
		{
			get { return _rand_level_min;}
			set { _rand_level_min = value;}
		}
		[SerializeField]
		private int _rand_level_max;
		public int rand_level_max
		{
			get { return _rand_level_max;}
			set { _rand_level_max = value;}
		}

    }

    [System.Serializable]
    public class EquipmentGachaInfo : Table<EquipmentGachaInfoData, int>
    {
    }
}

