using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

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

    }

    [System.Serializable]
    public class EquipmentGachaInfo : Table<EquipmentGachaInfoData, int>
    {
    }
}

