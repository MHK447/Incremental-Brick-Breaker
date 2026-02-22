using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class UnitUpgradeInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _level1;
		public int level1
		{
			get { return _level1;}
			set { _level1 = value;}
		}
		[SerializeField]
		private int _level2;
		public int level2
		{
			get { return _level2;}
			set { _level2 = value;}
		}
		[SerializeField]
		private int _level3;
		public int level3
		{
			get { return _level3;}
			set { _level3 = value;}
		}

    }

    [System.Serializable]
    public class UnitUpgradeInfo : Table<UnitUpgradeInfoData, int>
    {
    }
}

