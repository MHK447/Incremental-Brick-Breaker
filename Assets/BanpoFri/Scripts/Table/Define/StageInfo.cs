using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class StageInfoData
    {
        [SerializeField]
		private int _stage_idx;
		public int stage_idx
		{
			get { return _stage_idx;}
			set { _stage_idx = value;}
		}
		[SerializeField]
		private int _enemy_hp_increase;
		public int enemy_hp_increase
		{
			get { return _enemy_hp_increase;}
			set { _enemy_hp_increase = value;}
		}
		[SerializeField]
		private int _enemy_atk_increase;
		public int enemy_atk_increase
		{
			get { return _enemy_atk_increase;}
			set { _enemy_atk_increase = value;}
		}
		[SerializeField]
		private int _stage_reward_type;
		public int stage_reward_type
		{
			get { return _stage_reward_type;}
			set { _stage_reward_type = value;}
		}
		[SerializeField]
		private int _stage_reward_idx;
		public int stage_reward_idx
		{
			get { return _stage_reward_idx;}
			set { _stage_reward_idx = value;}
		}
		[SerializeField]
		private int _stage_reward_value;
		public int stage_reward_value
		{
			get { return _stage_reward_value;}
			set { _stage_reward_value = value;}
		}
		[SerializeField]
		private int _stage_ad_check;
		public int stage_ad_check
		{
			get { return _stage_ad_check;}
			set { _stage_ad_check = value;}
		}
		[SerializeField]
		private List<int> _map_enemy_unit;
		public List<int> map_enemy_unit
		{
			get { return _map_enemy_unit;}
			set { _map_enemy_unit = value;}
		}
		[SerializeField]
		private List<int> _spawn_tile_order;
		public List<int> spawn_tile_order
		{
			get { return _spawn_tile_order;}
			set { _spawn_tile_order = value;}
		}
		[SerializeField]
		private int _stage_clear_gold_value;
		public int stage_clear_gold_value
		{
			get { return _stage_clear_gold_value;}
			set { _stage_clear_gold_value = value;}
		}
		[SerializeField]
		private string _map_img;
		public string map_img
		{
			get { return _map_img;}
			set { _map_img = value;}
		}
		[SerializeField]
		private int _map_unit_ypos;
		public int map_unit_ypos
		{
			get { return _map_unit_ypos;}
			set { _map_unit_ypos = value;}
		}
		[SerializeField]
		private int _ingame_map_idx;
		public int ingame_map_idx
		{
			get { return _ingame_map_idx;}
			set { _ingame_map_idx = value;}
		}
		[SerializeField]
		private int _diffiyculty;
		public int diffiyculty
		{
			get { return _diffiyculty;}
			set { _diffiyculty = value;}
		}

    }

    [System.Serializable]
    public class StageInfo : Table<StageInfoData, int>
    {
    }
}

