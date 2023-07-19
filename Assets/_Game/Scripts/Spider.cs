using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : AllPool
{
    public List<Animator> animators;
    private bool isDone = false;
    public bool IsDone
    {
        get => isDone;
        set
        {
            isDone = value;
            if (isDone)
            {
                AllPoolContainer.Instance.Release(this);
            }
        }
    }
    private Animator anim;

    [SerializeField]
    private Movement move;

    private State currentState = State.None;

    public void OnInit(DataPath data, float speed)
    {
        int ran = Random.Range(0, animators.Count);
        foreach (Animator animator in animators)
        {
            animator.gameObject.SetActive(false);
        }
        anim = animators[ran];
        anim.gameObject.SetActive(true);
        SetState(State.Up);
        IsDone = false;
        move.mWayPoints = new Queue<Vector2>();
        foreach(var pos in data.path)
        {
            move.SetDestination(pos);
        }
        move.StartMove(speed);
    }

    public void SetState(State state)
    {
        currentState = state;
        anim.Play(state.ToString());
    }


}
public enum State
{
    None,
    Up,
    Down,
    Left,
    Right,
}
