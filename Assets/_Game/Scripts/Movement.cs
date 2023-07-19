using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private Spider spider;

    [SerializeField]
    private float Speed = 1.0f;

    public Queue<Vector2> mWayPoints = new Queue<Vector2>();

    // Start is called before the first frame update
    public void StartMove(float speed)
    {
        Speed = speed;
        StartCoroutine(Coroutine_MoveTo());
    }

    public void AddWayPoint(Vector2 pt)
    {
        mWayPoints.Enqueue(pt);
    }

    public void SetDestination(Vector2 destination)
    {
        AddWayPoint(destination);
    }

    public IEnumerator Coroutine_MoveTo()
    {
        while (true)
        {
            while (mWayPoints.Count > 0)
            {
                yield return StartCoroutine(Coroutine_MoveToPoint(mWayPoints.Dequeue(), Speed));
            }
            spider.IsDone = true;
            yield return null;
        }
    }

    // coroutine to move smoothly
    private IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }

    IEnumerator Coroutine_MoveToPoint(Vector2 p, float speed)
    {
        Vector3 endP = new Vector3(p.x, p.y, transform.position.z);
        float duration = (transform.position - endP).magnitude / speed;
        ChangeState(p);
        yield return StartCoroutine(Coroutine_MoveOverSeconds(transform.gameObject, endP, duration));
    }

    void ChangeState(Vector2 p)
    {
        var pos = new Vector2(transform.position.x, transform.position.y);
        if (Mathf.Abs(p.x - pos.x) > Mathf.Abs(p.y - pos.y))
        {
            if(p.x - pos.x < 0)
            {
                spider.SetState(State.Left);
            }
            else
            {
                spider.SetState(State.Right);
            }
        }
        else
        {
            if (p.y - pos.y < 0)
            {
                spider.SetState(State.Down);
            }
            else
            {
                spider.SetState(State.Up);
            }
        }
    }
}