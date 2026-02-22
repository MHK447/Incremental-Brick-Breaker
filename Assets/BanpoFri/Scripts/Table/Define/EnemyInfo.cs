using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class EnemyInfoData
    {
        [SerializeField]
		private int _enemy_idx;
		public int enemy_idx
		{
			get { return _enemy_idx;}
			set { _enemy_idx = value;}
		}
		[SerializeField]
		private int _type;
		public int type
		{
			get { return _type;}
			set { _type = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private int _move_speed;
		public int move_speed
		{
			get { return _move_speed;}
			set { _move_speed = value;}
		}
		[SerializeField]
		private int _attack_speed;
		public int attack_speed
		{
			get { return _attack_speed;}
			set { _attack_speed = value;}
		}
		[SerializeField]
		private int _atk_range_factor;
		public int atk_range_factor
		{
			get { return _atk_range_factor;}
			set { _atk_range_factor = value;}
		}
		[SerializeField]
		private string _prefab;
		public string prefab
		{
			get { return _prefab;}
			set { _prefab = value;}
		}
		[SerializeField]
		private int _boss_unit;
		public int boss_unit
		{
			get { return _boss_unit;}
			set { _boss_unit = value;}
		}

    }

    [System.Serializable]
    public class EnemyInfo : Table<EnemyInfoData, int>
    {
    }
}

