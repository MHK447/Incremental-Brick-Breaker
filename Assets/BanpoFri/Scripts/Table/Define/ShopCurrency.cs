using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class ShopCurrencyData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private string _product_id;
		public string product_id
		{
			get { return _product_id;}
			set { _product_id = value;}
		}
		[SerializeField]
		private int _purchase_type;
		public int purchase_type
		{
			get { return _purchase_type;}
			set { _purchase_type = value;}
		}
		[SerializeField]
		private int _reward_type;
		public int reward_type
		{
			get { return _reward_type;}
			set { _reward_type = value;}
		}
		[SerializeField]
		private int _reward_idx;
		public int reward_idx
		{
			get { return _reward_idx;}
			set { _reward_idx = value;}
		}
		[SerializeField]
		private int _reward_value;
		public int reward_value
		{
			get { return _reward_value;}
			set { _reward_value = value;}
		}
		[SerializeField]
		private int _price;
		public int price
		{
			get { return _price;}
			set { _price = value;}
		}
		[SerializeField]
		private int _additional;
		public int additional
		{
			get { return _additional;}
			set { _additional = value;}
		}
		[SerializeField]
		private string _icon;
		public string icon
		{
			get { return _icon;}
			set { _icon = value;}
		}

    }

    [System.Serializable]
    public class ShopCurrency : Table<ShopCurrencyData, int>
    {
    }
}

