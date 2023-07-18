using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    [SerializeField]
    private Spider spider;

    [SerializeField]
    private List<Transform> transformRandoms;

    private void Spawn(int number)
    {
        int ran = Random.RandomRange(0, transformRandoms.Count);
        while(number > 0)
        {
            AllPoolContainer.Instance.Spawn(spider, transformRandoms[ran].position, Quaternion.identity);
            number--;
        }
    }
}
