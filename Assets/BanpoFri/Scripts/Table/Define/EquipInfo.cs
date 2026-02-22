using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class EquipInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _base_dmg;
		public int base_dmg
		{
			get { return _base_dmg;}
			set { _base_dmg = value;}
		}
		[SerializeField]
		private int _base_value_1;
		public int base_value_1
		{
			get { return _base_value_1;}
			set { _base_value_1 = value;}
		}
		[SerializeField]
		private int _first_ad_type;
		public int first_ad_type
		{
			get { return _first_ad_type;}
			set { _first_ad_type = value;}
		}
		[SerializeField]
		private int _block_check_type;
		public int block_check_type
		{
			get { return _block_check_type;}
			set { _block_check_type = value;}
		}
		[SerializeField]
		private int _equip_purpose;
		public int equip_purpose
		{
			get { return _equip_purpose;}
			set { _equip_purpose = value;}
		}
		[SerializeField]
		private int _item_type;
		public int item_type
		{
			get { return _item_type;}
			set { _item_type = value;}
		}
		[SerializeField]
		private int _projection_type;
		public int projection_type
		{
			get { return _projection_type;}
			set { _projection_type = value;}
		}
		[SerializeField]
		private int _cooltime;
		public int cooltime
		{
			get { return _cooltime;}
			set { _cooltime = value;}
		}
		[SerializeField]
		private int _attack_range;
		public int attack_range
		{
			get { return _attack_range;}
			set { _attack_range = value;}
		}
		[SerializeField]
		private string _bullet_image;
		public string bullet_image
		{
			get { return _bullet_image;}
			set { _bullet_image = value;}
		}
		[SerializeField]
		private int _effect_type;
		public int effect_type
		{
			get { return _effect_type;}
			set { _effect_type = value;}
		}
		[SerializeField]
		private int _noneequip_ypos;
		public int noneequip_ypos
		{
			get { return _noneequip_ypos;}
			set { _noneequip_ypos = value;}
		}
		[SerializeField]
		private List<int> _weapon_offset;
		public List<int> weapon_offset
		{
			get { return _weapon_offset;}
			set { _weapon_offset = value;}
		}
		[SerializeField]
		private int _tilecheck_type;
		public int tilecheck_type
		{
			get { return _tilecheck_type;}
			set { _tilecheck_type = value;}
		}

    }

    [System.Serializable]
    public class EquipInfo : Table<EquipInfoData, int>
    {
    }
}

