using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class WeaponSelectChoiceData
    {
        [SerializeField]
		private int _stage;
		public int stage
		{
			get { return _stage;}
			set { _stage = value;}
		}
		[SerializeField]
		private int _wave;
		public int wave
		{
			get { return _wave;}
			set { _wave = value;}
		}
		[SerializeField]
		private List<int> _select_equi_grade;
		public List<int> select_equi_grade
		{
			get { return _select_equi_grade;}
			set { _select_equi_grade = value;}
		}
		[SerializeField]
		private List<int> _select_equip_idx;
		public List<int> select_equip_idx
		{
			get { return _select_equip_idx;}
			set { _select_equip_idx = value;}
		}

    }

    [System.Serializable]
    public class WeaponSelectChoice : Table<WeaponSelectChoiceData, KeyValuePair<int,int>>
    {
    }
}

