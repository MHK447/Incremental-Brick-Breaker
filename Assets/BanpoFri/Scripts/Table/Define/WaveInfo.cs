using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

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
		private int _block_spawn_check;
		public int block_spawn_check
		{
			get { return _block_spawn_check;}
			set { _block_spawn_check = value;}
		}
		[SerializeField]
		private List<int> _unit_appear_time;
		public List<int> unit_appear_time
		{
			get { return _unit_appear_time;}
			set { _unit_appear_time = value;}
		}
		[SerializeField]
		private int _unit_dead_coin;
		public int unit_dead_coin
		{
			get { return _unit_dead_coin;}
			set { _unit_dead_coin = value;}
		}

    }

    [System.Serializable]
    public class WaveInfo : Table<WaveInfoData, KeyValuePair<int,int>>
    {
    }
}

