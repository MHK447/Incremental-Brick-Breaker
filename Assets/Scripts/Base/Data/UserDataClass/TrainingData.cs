using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{



}


public class TrainingData
{
    public IReactiveProperty<bool> Isupgradeproperty { get; set; } = new ReactiveProperty<bool>(false);

    public IReactiveProperty<bool> Specialisupgradeproperty { get; set; } = new ReactiveProperty<bool>(false);

    public int Traininglevel { get; set; } = 1;
    public int Trainingorder { get; set; } = 1;

}
