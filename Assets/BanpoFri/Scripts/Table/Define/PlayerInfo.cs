using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class PlayerInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _base_atk_speed;
		public int base_atk_speed
		{
			get { return _base_atk_speed;}
			set { _base_atk_speed = value;}
		}
		[SerializeField]
		private int _base_dmg;
		public int base_dmg
		{
			get { return _base_dmg;}
			set { _base_dmg = value;}
		}
		[SerializeField]
		private int _base_hp;
		public int base_hp
		{
			get { return _base_hp;}
			set { _base_hp = value;}
		}
		[SerializeField]
		private int _projection_type;
		public int projection_type
		{
			get { return _projection_type;}
			set { _projection_type = value;}
		}
		[SerializeField]
		private int _attack_range;
		public int attack_range
		{
			get { return _attack_range;}
			set { _attack_range = value;}
		}

    }

    [System.Serializable]
    public class PlayerInfo : Table<PlayerInfoData, int>
    {
    }
}

