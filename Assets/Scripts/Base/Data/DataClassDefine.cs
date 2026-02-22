using System;
using System.Numerics;
using UniRx;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using System.Linq;
using BanpoFri.Data;

public interface IReadOnlyData : ICloneable
{
	void Create();
}
public interface IClientData { }



public class NoticeData
{
	public int NotiIdx = 0;
	public Transform Target;

	public NoticeData(int notiidx, Transform target)
	{
		NotiIdx = notiidx;
		Target = target;
	}

}









