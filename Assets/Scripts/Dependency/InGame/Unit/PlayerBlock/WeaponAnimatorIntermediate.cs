using System;
using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

//애니매이션 이벤트 구독 쉽게 빼주는 컴포넌트
public class WeaponAnimatorIntermediate : MonoBehaviour
{

    public Action ShootActionEvent;
    public void ShootAction()
    {
        ShootActionEvent?.Invoke();
    }
}
