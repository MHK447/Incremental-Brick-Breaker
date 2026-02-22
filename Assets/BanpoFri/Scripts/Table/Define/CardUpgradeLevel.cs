using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

namespace BanpoFri
{
    [System.Serializable]
    public class CardUpgradeLevelData
    {
        [SerializeField]
		private int _level;
		public int level
		{
			get { return _level;}
			set { _level = value;}
		}
		[SerializeField]
		private int _need_card;
		public int need_card
		{
			get { return _need_card;}
			set { _need_card = value;}
		}

    }

    [System.Serializable]
    public class CardUpgradeLevel : Table<CardUpgradeLevelData, int>
    {
    }
}

