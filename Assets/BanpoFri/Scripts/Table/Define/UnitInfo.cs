using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class UnitInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _base_atk_speed;
		public int base_atk_speed
		{
			get { return _base_atk_speed;}
			set { _base_atk_speed = value;}
		}
		[SerializeField]
		private int _base_move_speed;
		public int base_move_speed
		{
			get { return _base_move_speed;}
			set { _base_move_speed = value;}
		}
		[SerializeField]
		private int _base_hp;
		public int base_hp
		{
			get { return _base_hp;}
			set { _base_hp = value;}
		}
		[SerializeField]
		private int _base_dmg;
		public int base_dmg
		{
			get { return _base_dmg;}
			set { _base_dmg = value;}
		}
		[SerializeField]
		private int _projection_type;
		public int projection_type
		{
			get { return _projection_type;}
			set { _projection_type = value;}
		}
		[SerializeField]
		private int _attack_range;
		public int attack_range
		{
			get { return _attack_range;}
			set { _attack_range = value;}
		}
		[SerializeField]
		private string _prefab;
		public string prefab
		{
			get { return _prefab;}
			set { _prefab = value;}
		}
		[SerializeField]
		private string _unit_img;
		public string unit_img
		{
			get { return _unit_img;}
			set { _unit_img = value;}
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

    }

    [System.Serializable]
    public class UnitInfo : Table<UnitInfoData, int>
    {
    }
}

