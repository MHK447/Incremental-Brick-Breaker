using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class InGameUpgradeChoiceData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private string _choice_name;
		public string choice_name
		{
			get { return _choice_name;}
			set { _choice_name = value;}
		}
		[SerializeField]
		private string _desc_1;
		public string desc_1
		{
			get { return _desc_1;}
			set { _desc_1 = value;}
		}
		[SerializeField]
		private string _front_img;
		public string front_img
		{
			get { return _front_img;}
			set { _front_img = value;}
		}
		[SerializeField]
		private string _back_img;
		public string back_img
		{
			get { return _back_img;}
			set { _back_img = value;}
		}
		[SerializeField]
		private int _category;
		public int category
		{
			get { return _category;}
			set { _category = value;}
		}
		[SerializeField]
		private int _weight;
		public int weight
		{
			get { return _weight;}
			set { _weight = value;}
		}
		[SerializeField]
		private int _upgrade_count;
		public int upgrade_count
		{
			get { return _upgrade_count;}
			set { _upgrade_count = value;}
		}
		[SerializeField]
		private List<int> _upgrade_value_1;
		public List<int> upgrade_value_1
		{
			get { return _upgrade_value_1;}
			set { _upgrade_value_1 = value;}
		}
		[SerializeField]
		private List<int> _upgrade_value_2;
		public List<int> upgrade_value_2
		{
			get { return _upgrade_value_2;}
			set { _upgrade_value_2 = value;}
		}

    }

    [System.Serializable]
    public class InGameUpgradeChoice : Table<InGameUpgradeChoiceData, int>
    {
    }
}

