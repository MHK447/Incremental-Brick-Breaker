using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class HeroInfoData
    {
        [SerializeField]
		private int _hero_idx;
		public int hero_idx
		{
			get { return _hero_idx;}
			set { _hero_idx = value;}
		}
		[SerializeField]
		private int _base_atk_dmg;
		public int base_atk_dmg
		{
			get { return _base_atk_dmg;}
			set { _base_atk_dmg = value;}
		}
		[SerializeField]
		private int _base_hp;
		public int base_hp
		{
			get { return _base_hp;}
			set { _base_hp = value;}
		}
		[SerializeField]
		private float _base_atk_speed;
		public float base_atk_speed
		{
			get { return _base_atk_speed;}
			set { _base_atk_speed = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}
		[SerializeField]
		private string _prefab;
		public string prefab
		{
			get { return _prefab;}
			set { _prefab = value;}
		}

    }

    [System.Serializable]
    public class HeroInfo : Table<HeroInfoData, int>
    {
    }
}

