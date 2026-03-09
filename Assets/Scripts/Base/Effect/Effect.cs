using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class Effect : MonoBehaviour
{
    [SerializeField]
    protected List<ParticleSystem> particles = new List<ParticleSystem>();

    private Transform FollowTrans = null;

    private bool autoRemove = false;
    private float deltaTime = 0f;
    private float targetTime = 1f;

    public virtual void Play(Vector3 worldPos, Transform followTrans)
    {
        this.transform.position = worldPos;
        foreach(var pt in particles)
        {
            pt.Clear();
            pt.Play();
            //pt.Simulate(0f, false, true);
        }
        //var tweens = this.transform.GetComponentsInChildren<DoTweenScriptComponent>();
        //foreach (var tw in tweens)
        //    tw.Restart();

        //if(tweens.Length > 0)
        //{
        //    tweens.Last().AddCallback(()=> {
        //        OnForceCollect();
        //        deltaTime = 0f;
        //    });
        //}

        FollowTrans = followTrans;
    }

    public virtual void Play()
    {
        foreach(var pt in particles)
        {
            pt.Play();
        }
    }

    public virtual void Stop()
    {
        foreach(var pt in particles)
        {            
            pt.Stop();
        }
    }

    public void SetAutoRemove(bool value, float time)
    {
        autoRemove = value;
        targetTime = time;
    }

    /// <summary>풀 재사용 시 자동 제거 상태 초기화 (재사용 직후 바로 꺼지는 현상 방지)</summary>
    public void ResetAutoRemoveState()
    {
        autoRemove = false;
        deltaTime = 0f;
    }

    private void Update()
    {
        if(FollowTrans != null)
        {
            this.transform.position = FollowTrans.position;
        }
        if(autoRemove)
        {
            if(deltaTime >= targetTime)
            {
                OnForceCollect();
                deltaTime = 0f;
                return;
            }
            deltaTime += Time.deltaTime;
        }
    }
    
    public void OnForceCollect()
    {
        GameRoot.Instance.EffectSystem.ReturnMultiEffect(this);
    }
}
