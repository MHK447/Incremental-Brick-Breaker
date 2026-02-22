using System.Collections;
using System;
using System.Numerics;
using System.Collections.Generic;
using UniRx;

public interface IUserDataMode
{
	DateTime LastLoginTime { get; set; }
	DateTime CurPlayDateTime { get; set; }
	IReactiveProperty<BigInteger> EnergyMoney { get; set; }
	IReactiveProperty<int> GachaCoin { get; set; }

	public IReactiveCollection<NoticeData> NoticeCollections { get; set; }

	public IReactiveProperty<int> BoostTime { get; set; }


}

public class UserDataMain : IUserDataMode
{
	public DateTime LastLoginTime { get; set; } = default(DateTime);
	public DateTime CurPlayDateTime { get; set; } = new DateTime(1, 1, 1);
	public IReactiveProperty<BigInteger> EnergyMoney { get; set; } = new ReactiveProperty<BigInteger>(0);
	public IReactiveProperty<int> GachaCoin { get; set; } = new ReactiveProperty<int>(0);
	public IReactiveProperty<int> BoostTime { get; set; } = new ReactiveProperty<int>();


	public IReactiveCollection<NoticeData> NoticeCollections { get; set; } = new ReactiveCollection<NoticeData>();
}



public class UserDataEvent : UserDataMain
{
}