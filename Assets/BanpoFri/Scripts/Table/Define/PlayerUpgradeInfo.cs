using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class PlayerUpgradeInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _increase_attack;
		public int increase_attack
		{
			get { return _increase_attack;}
			set { _increase_attack = value;}
		}
		[SerializeField]
		private int _increase_hp;
		public int increase_hp
		{
			get { return _increase_hp;}
			set { _increase_hp = value;}
		}
		[SerializeField]
		private int _increase_rot;
		public int increase_rot
		{
			get { return _increase_rot;}
			set { _increase_rot = value;}
		}
		[SerializeField]
		private int _base_material_cost;
		public int base_material_cost
		{
			get { return _base_material_cost;}
			set { _base_material_cost = value;}
		}
		[SerializeField]
		private int _base_material_increase_cost;
		public int base_material_increase_cost
		{
			get { return _base_material_increase_cost;}
			set { _base_material_increase_cost = value;}
		}
		[SerializeField]
		private int _base_cash_cost;
		public int base_cash_cost
		{
			get { return _base_cash_cost;}
			set { _base_cash_cost = value;}
		}
		[SerializeField]
		private int _base_cash_increase_cost;
		public int base_cash_increase_cost
		{
			get { return _base_cash_increase_cost;}
			set { _base_cash_increase_cost = value;}
		}
		[SerializeField]
		private List<int> _unlock_skill_type;
		public List<int> unlock_skill_type
		{
			get { return _unlock_skill_type;}
			set { _unlock_skill_type = value;}
		}
		[SerializeField]
		private List<int> _unlock_skill_value;
		public List<int> unlock_skill_value
		{
			get { return _unlock_skill_value;}
			set { _unlock_skill_value = value;}
		}
		[SerializeField]
		private List<int> _unlock_skill_level;
		public List<int> unlock_skill_level
		{
			get { return _unlock_skill_level;}
			set { _unlock_skill_level = value;}
		}

    }

    [System.Serializable]
    public class PlayerUpgradeInfo : Table<PlayerUpgradeInfoData, int>
    {
    }
}

