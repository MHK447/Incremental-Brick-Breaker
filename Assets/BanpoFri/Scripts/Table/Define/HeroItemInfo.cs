using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class HeroItemInfoData
    {
        [SerializeField]
		private int _item_idx;
		public int item_idx
		{
			get { return _item_idx;}
			set { _item_idx = value;}
		}
		[SerializeField]
		private int _set_group_type;
		public int set_group_type
		{
			get { return _set_group_type;}
			set { _set_group_type = value;}
		}
		[SerializeField]
		private int _item_equip_type;
		public int item_equip_type
		{
			get { return _item_equip_type;}
			set { _item_equip_type = value;}
		}
		[SerializeField]
		private List<int> _item_ability_type;
		public List<int> item_ability_type
		{
			get { return _item_ability_type;}
			set { _item_ability_type = value;}
		}
		[SerializeField]
		private List<int> _item_ability_value;
		public List<int> item_ability_value
		{
			get { return _item_ability_value;}
			set { _item_ability_value = value;}
		}
		[SerializeField]
		private List<int> _levelup_increase_value;
		public List<int> levelup_increase_value
		{
			get { return _levelup_increase_value;}
			set { _levelup_increase_value = value;}
		}
		[SerializeField]
		private int _item_set_type;
		public int item_set_type
		{
			get { return _item_set_type;}
			set { _item_set_type = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private string _item_img;
		public string item_img
		{
			get { return _item_img;}
			set { _item_img = value;}
		}
		[SerializeField]
		private List<int> _item_grade_ability_type;
		public List<int> item_grade_ability_type
		{
			get { return _item_grade_ability_type;}
			set { _item_grade_ability_type = value;}
		}
		[SerializeField]
		private List<int> _item_grade_ability_value;
		public List<int> item_grade_ability_value
		{
			get { return _item_grade_ability_value;}
			set { _item_grade_ability_value = value;}
		}

    }

    [System.Serializable]
    public class HeroItemInfo : Table<HeroItemInfoData, int>
    {
    }
}

