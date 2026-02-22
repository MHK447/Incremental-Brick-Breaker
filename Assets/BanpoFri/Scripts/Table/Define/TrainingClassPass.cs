using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class TrainingClassPassData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _group;
		public int group
		{
			get { return _group;}
			set { _group = value;}
		}
		[SerializeField]
		private int _upgrade_count;
		public int upgrade_count
		{
			get { return _upgrade_count;}
			set { _upgrade_count = value;}
		}
		[SerializeField]
		private int _reward_type;
		public int reward_type
		{
			get { return _reward_type;}
			set { _reward_type = value;}
		}
		[SerializeField]
		private int _reward_index;
		public int reward_index
		{
			get { return _reward_index;}
			set { _reward_index = value;}
		}
		[SerializeField]
		private byte[] _reward_value;
		public System.Numerics.BigInteger reward_value
		{
			get { return new System.Numerics.BigInteger(_reward_value);}
			set { _reward_value = value.ToByteArray();}
		}

    }

    [System.Serializable]
    public class TrainingClassPass : Table<TrainingClassPassData, int>
    {
    }
}

