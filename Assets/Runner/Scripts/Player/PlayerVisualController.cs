using System;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    private enum PlayerAnimationTriggers
    {
        Idle,
        Run,
        Land
    }

    private enum PlayerAnimationBooleans
    {
        IsJumping,
        IsFalling,
        IsStunned,
        IsGliding
    }

    private Animator[] _animators;
    private void Awake()
    {
        _animators = GetComponentsInChildren<Animator>();
    }

    public void Run()
    {
        SetTrigger(PlayerAnimationTriggers.Run.ToString());
    }    
    
    public void Land()
    {
        SetTrigger(PlayerAnimationTriggers.Land.ToString());
    }

    public void SetIsJumping(bool value)
    {
        SetBool(PlayerAnimationBooleans.IsJumping, value);
    }

    public void SetIsFalling(bool value)
    {
        SetBool(PlayerAnimationBooleans.IsFalling, value);
    }
    
    public void SetIsGliding(bool value)
    {
        SetBool(PlayerAnimationBooleans.IsGliding, value);
    }

    public void Idle()
    {
        SetTrigger(PlayerAnimationTriggers.Idle.ToString());
    }

    public void SetIsStunned(bool value)
    {
        SetBool(PlayerAnimationBooleans.IsStunned, value);
    }

    private void SetTrigger(string triggerName)
    {
        foreach (Animator animator in _animators)
        {
            animator.SetTrigger(triggerName);
        }
    }
    
    private void SetBool(PlayerAnimationBooleans booleanName, bool value)
    {
        foreach (Animator animator in _animators)
        {
            foreach (var boolean in (PlayerAnimationBooleans[])Enum.GetValues(typeof(PlayerAnimationBooleans)))
            {
                animator.SetBool(boolean.ToString(), booleanName == boolean && value);   
            }
        }
    }
}