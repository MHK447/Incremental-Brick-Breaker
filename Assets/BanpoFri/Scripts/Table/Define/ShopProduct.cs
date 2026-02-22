using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class ShopProductData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _order;
		public int order
		{
			get { return _order;}
			set { _order = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private string _desc;
		public string desc
		{
			get { return _desc;}
			set { _desc = value;}
		}
		[SerializeField]
		private string _product_id;
		public string product_id
		{
			get { return _product_id;}
			set { _product_id = value;}
		}
		[SerializeField]
		private int _package_buff;
		public int package_buff
		{
			get { return _package_buff;}
			set { _package_buff = value;}
		}
		[SerializeField]
		private int _buff_value;
		public int buff_value
		{
			get { return _buff_value;}
			set { _buff_value = value;}
		}
		[SerializeField]
		private List<int> _reward_type;
		public List<int> reward_type
		{
			get { return _reward_type;}
			set { _reward_type = value;}
		}
		[SerializeField]
		private List<int> _reward_idx;
		public List<int> reward_idx
		{
			get { return _reward_idx;}
			set { _reward_idx = value;}
		}
		[SerializeField]
		private List<int> _reward_value;
		public List<int> reward_value
		{
			get { return _reward_value;}
			set { _reward_value = value;}
		}
		[SerializeField]
		private int _sale;
		public int sale
		{
			get { return _sale;}
			set { _sale = value;}
		}
		[SerializeField]
		private int _value;
		public int value
		{
			get { return _value;}
			set { _value = value;}
		}
		[SerializeField]
		private int _double_sale;
		public int double_sale
		{
			get { return _double_sale;}
			set { _double_sale = value;}
		}
		[SerializeField]
		private int _contents_open_idx;
		public int contents_open_idx
		{
			get { return _contents_open_idx;}
			set { _contents_open_idx = value;}
		}
		[SerializeField]
		private int _stage_end;
		public int stage_end
		{
			get { return _stage_end;}
			set { _stage_end = value;}
		}
		[SerializeField]
		private int _package_type;
		public int package_type
		{
			get { return _package_type;}
			set { _package_type = value;}
		}
		[SerializeField]
		private int _char_idx;
		public int char_idx
		{
			get { return _char_idx;}
			set { _char_idx = value;}
		}
		[SerializeField]
		private int _offer_type;
		public int offer_type
		{
			get { return _offer_type;}
			set { _offer_type = value;}
		}
		[SerializeField]
		private int _offer_duration;
		public int offer_duration
		{
			get { return _offer_duration;}
			set { _offer_duration = value;}
		}

    }

    [System.Serializable]
    public class ShopProduct : Table<ShopProductData, int>
    {
    }
}

