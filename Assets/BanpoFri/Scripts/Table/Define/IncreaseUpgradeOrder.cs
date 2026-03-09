using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

namespace BanpoFri
{
    [System.Serializable]
    public class IncreaseUpgradeOrderData
    {
        [SerializeField]
		private int _order;
		public int order
		{
			get { return _order;}
			set { _order = value;}
		}
		[SerializeField]
		private List<int> _open_order;
		public List<int> open_order
		{
			get { return _open_order;}
			set { _open_order = value;}
		}
		[SerializeField]
		private int _increase_idx;
		public int increase_idx
		{
			get { return _increase_idx;}
			set { _increase_idx = value;}
		}
		[SerializeField]
		private int _increase_max_lv;
		public int increase_max_lv
		{
			get { return _increase_max_lv;}
			set { _increase_max_lv = value;}
		}
		[SerializeField]
		private int _cost;
		public int cost
		{
			get { return _cost;}
			set { _cost = value;}
		}

    }

    [System.Serializable]
    public class IncreaseUpgradeOrder : Table<IncreaseUpgradeOrderData, int>
    {
    }
}

