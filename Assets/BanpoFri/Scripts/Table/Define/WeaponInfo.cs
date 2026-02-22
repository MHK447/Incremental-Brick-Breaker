using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class WeaponInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
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
		private string _bullet_image;
		public string bullet_image
		{
			get { return _bullet_image;}
			set { _bullet_image = value;}
		}
		[SerializeField]
		private string _prefab;
		public string prefab
		{
			get { return _prefab;}
			set { _prefab = value;}
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
    public class WeaponInfo : Table<WeaponInfoData, int>
    {
    }
}

