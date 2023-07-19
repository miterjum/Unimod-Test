using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public List<DataPath> allPaths;
    
    [SerializeField]
    private Spider spider;

    [SerializeField]
    private List<Transform> transformRandoms;

    public override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        allPaths = Grid2D.Instance.allPaths;
    }
    public void Spawn(int number)
    {
        while(number > 0)
        {
            int ran = Random.Range(0, allPaths.Count);
            float speed = Random.Range(4.5f, 5f);
            var s = AllPoolContainer.Instance.Spawn(spider, allPaths[ran].startPos, Quaternion.identity);
            (s as Spider).OnInit(allPaths[ran], speed);
            number--;
        }
    }
}
