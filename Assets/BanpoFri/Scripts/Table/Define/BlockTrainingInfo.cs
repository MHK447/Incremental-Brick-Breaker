using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

namespace BanpoFri
{
    [System.Serializable]
    public class BlockTrainingInfoData
    {
        [SerializeField]
		private int _upgrade_order;
		public int upgrade_order
		{
			get { return _upgrade_order;}
			set { _upgrade_order = value;}
		}
		[SerializeField]
		private int _level;
		public int level
		{
			get { return _level;}
			set { _level = value;}
		}
		[SerializeField]
		private int _training_type;
		public int training_type
		{
			get { return _training_type;}
			set { _training_type = value;}
		}
		[SerializeField]
		private int _value;
		public int value
		{
			get { return _value;}
			set { _value = value;}
		}
		[SerializeField]
		private int _cost;
		public int cost
		{
			get { return _cost;}
			set { _cost = value;}
		}
		[SerializeField]
		private string _upgrade_name;
		public string upgrade_name
		{
			get { return _upgrade_name;}
			set { _upgrade_name = value;}
		}
		[SerializeField]
		private string _upgrade_desc;
		public string upgrade_desc
		{
			get { return _upgrade_desc;}
			set { _upgrade_desc = value;}
		}
		[SerializeField]
		private string _upgrade_icon;
		public string upgrade_icon
		{
			get { return _upgrade_icon;}
			set { _upgrade_icon = value;}
		}

    }

    [System.Serializable]
    public class BlockTrainingInfo : Table<BlockTrainingInfoData, int>
    {
    }
}

