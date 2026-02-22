using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class TrainingClassInfoData
    {
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
		private int _power_increase;
		public int power_increase
		{
			get { return _power_increase;}
			set { _power_increase = value;}
		}
		[SerializeField]
		private int _health_increase;
		public int health_increase
		{
			get { return _health_increase;}
			set { _health_increase = value;}
		}
		[SerializeField]
		private int _coin_increase;
		public int coin_increase
		{
			get { return _coin_increase;}
			set { _coin_increase = value;}
		}

    }

    [System.Serializable]
    public class TrainingClassInfo : Table<TrainingClassInfoData, int>
    {
    }
}

