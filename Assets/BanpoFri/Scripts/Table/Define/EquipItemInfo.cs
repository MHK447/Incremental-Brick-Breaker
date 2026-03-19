using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class EquipItemInfoData
    {
        [SerializeField]
		private int _item_equip_type;
		public int item_equip_type
		{
			get { return _item_equip_type;}
			set { _item_equip_type = value;}
		}
		[SerializeField]
		private int _item_idx;
		public int item_idx
		{
			get { return _item_idx;}
			set { _item_idx = value;}
		}
		[SerializeField]
		private int _item_ability_type;
		public int item_ability_type
		{
			get { return _item_ability_type;}
			set { _item_ability_type = value;}
		}
		[SerializeField]
		private int _ability_value;
		public int ability_value
		{
			get { return _ability_value;}
			set { _ability_value = value;}
		}
		[SerializeField]
		private string _item_desc;
		public string item_desc
		{
			get { return _item_desc;}
			set { _item_desc = value;}
		}
		[SerializeField]
		private string _item_name;
		public string item_name
		{
			get { return _item_name;}
			set { _item_name = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}

    }

    [System.Serializable]
    public class EquipItemInfo : Table<EquipItemInfoData, KeyValuePair<int,int>>
    {
    }
}

