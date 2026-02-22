using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

namespace BanpoFri
{
    [System.Serializable]
    public class ItemInfoData
    {
        [SerializeField]
		private int _type;
		public int type
		{
			get { return _type;}
			set { _type = value;}
		}
		[SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}

    }

    [System.Serializable]
    public class ItemInfo : Table<ItemInfoData, KeyValuePair<int,int>>
    {
    }
}

