using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class WayChoicesSelectInfoData
    {
        [SerializeField]
		private int _order;
		public int order
		{
			get { return _order;}
			set { _order = value;}
		}
		[SerializeField]
		private int _stage;
		public int stage
		{
			get { return _stage;}
			set { _stage = value;}
		}
		[SerializeField]
		private int _challenge_count;
		public int challenge_count
		{
			get { return _challenge_count;}
			set { _challenge_count = value;}
		}
		[SerializeField]
		private int _choice_count;
		public int choice_count
		{
			get { return _choice_count;}
			set { _choice_count = value;}
		}
		[SerializeField]
		private List<int> _choices_idx;
		public List<int> choices_idx
		{
			get { return _choices_idx;}
			set { _choices_idx = value;}
		}
		[SerializeField]
		private int _recommend_order;
		public int recommend_order
		{
			get { return _recommend_order;}
			set { _recommend_order = value;}
		}
		[SerializeField]
		private List<int> _unit_idx;
		public List<int> unit_idx
		{
			get { return _unit_idx;}
			set { _unit_idx = value;}
		}

    }

    [System.Serializable]
    public class WayChoicesSelectInfo : Table<WayChoicesSelectInfoData, int>
    {
    }
}

