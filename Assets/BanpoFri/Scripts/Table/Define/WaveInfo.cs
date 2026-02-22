using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class WaveInfoData
    {
        [SerializeField]
		private int _stage;
		public int stage
		{
			get { return _stage;}
			set { _stage = value;}
		}
		[SerializeField]
		private int _wave;
		public int wave
		{
			get { return _wave;}
			set { _wave = value;}
		}
		[SerializeField]
		private List<int> _unit_idx;
		public List<int> unit_idx
		{
			get { return _unit_idx;}
			set { _unit_idx = value;}
		}
		[SerializeField]
		private List<int> _unit_dmg;
		public List<int> unit_dmg
		{
			get { return _unit_dmg;}
			set { _unit_dmg = value;}
		}
		[SerializeField]
		private List<int> _unit_hp;
		public List<int> unit_hp
		{
			get { return _unit_hp;}
			set { _unit_hp = value;}
		}
		[SerializeField]
		private List<int> _unit_count;
		public List<int> unit_count
		{
			get { return _unit_count;}
			set { _unit_count = value;}
		}
		[SerializeField]
		private int _block_spawn_hp;
		public int block_spawn_hp
		{
			get { return _block_spawn_hp;}
			set { _block_spawn_hp = value;}
		}
		[SerializeField]
		private List<int> _unit_appear_time;
		public List<int> unit_appear_time
		{
			get { return _unit_appear_time;}
			set { _unit_appear_time = value;}
		}
		[SerializeField]
		private int _add_silver_coin;
		public int add_silver_coin
		{
			get { return _add_silver_coin;}
			set { _add_silver_coin = value;}
		}
		[SerializeField]
		private int _add_exp_value;
		public int add_exp_value
		{
			get { return _add_exp_value;}
			set { _add_exp_value = value;}
		}
		[SerializeField]
		private int _devil_check;
		public int devil_check
		{
			get { return _devil_check;}
			set { _devil_check = value;}
		}

    }

    [System.Serializable]
    public class WaveInfo : Table<WaveInfoData, KeyValuePair<int,int>>
    {
    }
}

