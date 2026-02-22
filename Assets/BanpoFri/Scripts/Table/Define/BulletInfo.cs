using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class BulletInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _base_damage;
		public int base_damage
		{
			get { return _base_damage;}
			set { _base_damage = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}
		[SerializeField]
		private int _projection_speed;
		public int projection_speed
		{
			get { return _projection_speed;}
			set { _projection_speed = value;}
		}
		[SerializeField]
		private string _prefab;
		public string prefab
		{
			get { return _prefab;}
			set { _prefab = value;}
		}
		[SerializeField]
		private int _value_1;
		public int value_1
		{
			get { return _value_1;}
			set { _value_1 = value;}
		}
		[SerializeField]
		private int _value_2;
		public int value_2
		{
			get { return _value_2;}
			set { _value_2 = value;}
		}

    }

    [System.Serializable]
    public class BulletInfo : Table<BulletInfoData, int>
    {
    }
}

