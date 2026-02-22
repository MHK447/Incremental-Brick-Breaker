using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class HeroItemSetData
    {
        [SerializeField]
		private int _set_idx;
		public int set_idx
		{
			get { return _set_idx;}
			set { _set_idx = value;}
		}
		[SerializeField]
		private int _set_ability_type;
		public int set_ability_type
		{
			get { return _set_ability_type;}
			set { _set_ability_type = value;}
		}
		[SerializeField]
		private int _set_ability_value;
		public int set_ability_value
		{
			get { return _set_ability_value;}
			set { _set_ability_value = value;}
		}

    }

    [System.Serializable]
    public class HeroItemSet : Table<HeroItemSetData, int>
    {
    }
}

