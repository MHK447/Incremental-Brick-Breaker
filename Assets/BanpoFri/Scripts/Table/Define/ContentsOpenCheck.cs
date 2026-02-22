using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class ContentsOpenCheckData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _stage_idx;
		public int stage_idx
		{
			get { return _stage_idx;}
			set { _stage_idx = value;}
		}
		[SerializeField]
		private int _failed_count;
		public int failed_count
		{
			get { return _failed_count;}
			set { _failed_count = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}

    }

    [System.Serializable]
    public class ContentsOpenCheck : Table<ContentsOpenCheckData, int>
    {
    }
}

