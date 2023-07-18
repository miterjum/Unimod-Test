using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : AllPool
{
    [SerializeField]
    private Animation anim;

    [SerializeField]
    private Transform posWin;

    private State currentState = State.None;

    public void SetState(State state)
    {
        currentState = state;
        anim.Play(state.ToString());
    }

    public enum State
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }
}
