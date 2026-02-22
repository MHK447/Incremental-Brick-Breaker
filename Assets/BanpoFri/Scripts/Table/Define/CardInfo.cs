using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class CardInfoData
    {
        [SerializeField]
		private int _card_idx;
		public int card_idx
		{
			get { return _card_idx;}
			set { _card_idx = value;}
		}
		[SerializeField]
		private int _card_type;
		public int card_type
		{
			get { return _card_type;}
			set { _card_type = value;}
		}
		[SerializeField]
		private string _card_name;
		public string card_name
		{
			get { return _card_name;}
			set { _card_name = value;}
		}
		[SerializeField]
		private string _card_desc;
		public string card_desc
		{
			get { return _card_desc;}
			set { _card_desc = value;}
		}
		[SerializeField]
		private string _icon;
		public string icon
		{
			get { return _icon;}
			set { _icon = value;}
		}
		[SerializeField]
		private List<int> _card_upgrade_type;
		public List<int> card_upgrade_type
		{
			get { return _card_upgrade_type;}
			set { _card_upgrade_type = value;}
		}
		[SerializeField]
		private List<int> _card_upgrade_increase;
		public List<int> card_upgrade_increase
		{
			get { return _card_upgrade_increase;}
			set { _card_upgrade_increase = value;}
		}
		[SerializeField]
		private int _unlock_type;
		public int unlock_type
		{
			get { return _unlock_type;}
			set { _unlock_type = value;}
		}
		[SerializeField]
		private int _unlock_stage;
		public int unlock_stage
		{
			get { return _unlock_stage;}
			set { _unlock_stage = value;}
		}
		[SerializeField]
		private List<int> _card_ability_type;
		public List<int> card_ability_type
		{
			get { return _card_ability_type;}
			set { _card_ability_type = value;}
		}
		[SerializeField]
		private List<int> _card_ability_level;
		public List<int> card_ability_level
		{
			get { return _card_ability_level;}
			set { _card_ability_level = value;}
		}
		[SerializeField]
		private List<int> _card_ability_value;
		public List<int> card_ability_value
		{
			get { return _card_ability_value;}
			set { _card_ability_value = value;}
		}

    }

    [System.Serializable]
    public class CardInfo : Table<CardInfoData, int>
    {
    }
}

