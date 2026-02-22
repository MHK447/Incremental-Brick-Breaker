using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class ChoicesGroupData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _choice_1;
		public int choice_1
		{
			get { return _choice_1;}
			set { _choice_1 = value;}
		}
		[SerializeField]
		private int _choice_2;
		public int choice_2
		{
			get { return _choice_2;}
			set { _choice_2 = value;}
		}
		[SerializeField]
		private int _choice_3;
		public int choice_3
		{
			get { return _choice_3;}
			set { _choice_3 = value;}
		}
		[SerializeField]
		private int _value;
		public int value
		{
			get { return _value;}
			set { _value = value;}
		}

    }

    [System.Serializable]
    public class ChoicesGroup : Table<ChoicesGroupData, int>
    {
    }
}

