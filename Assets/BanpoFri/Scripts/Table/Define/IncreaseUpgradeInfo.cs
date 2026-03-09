using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class IncreaseUpgradeInfoData
    {
        [SerializeField]
		private int _upgrade_idx;
		public int upgrade_idx
		{
			get { return _upgrade_idx;}
			set { _upgrade_idx = value;}
		}
		[SerializeField]
		private string _icon;
		public string icon
		{
			get { return _icon;}
			set { _icon = value;}
		}
		[SerializeField]
		private string _upgrade_name;
		public string upgrade_name
		{
			get { return _upgrade_name;}
			set { _upgrade_name = value;}
		}
		[SerializeField]
		private string _upgrade_desc;
		public string upgrade_desc
		{
			get { return _upgrade_desc;}
			set { _upgrade_desc = value;}
		}

    }

    [System.Serializable]
    public class IncreaseUpgradeInfo : Table<IncreaseUpgradeInfoData, int>
    {
    }
}

