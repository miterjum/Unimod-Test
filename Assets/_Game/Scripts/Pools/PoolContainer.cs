using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

namespace Utilities.Common
{
    [System.Serializable]
    public class PoolsContainer<T,V> : MonoBehaviourSingletonPersistent<V> where T : Component
        where V: PoolsContainer<T,V>
    {
        
        public Dictionary<int, CustomPool<T>> poolDict = new Dictionary<int, CustomPool<T>>();
        public Transform container;
        private int mInitialNumber;

        //public static V instance;

        private void Awake()
        {
            base.Awake();
            CollectPool();
        }
        //private void Awake()
        //{
        //    // TODO: DucNV. change to singleton later.
        //    instance = this as V;
        //    CollectPool();
        //}



        // Run in inspector.
        [Button("Collect Pool"), GUIColor(0, 1, 0)]
        public void CollectPool()
        {
            if (this.poolDict != null)
            {
                this.poolDict?.Clear();
            }
            else
            {
                this.poolDict = new Dictionary<int, CustomPool<T>>();
            }
            var allPool = GetComponentsInChildren<CustomPool<T>>();
            for (int i = 0; i < allPool.Length; i++)
            {
                this.poolDict.Add(allPool[i].Prefab.gameObject.GetInstanceID(), allPool[i]);
            }
        }


        public PoolsContainer(Transform pContainer)
        {
            container = pContainer;
            //poolDict = new Dictionary<int, CustomPool<T>>();
        }

        public PoolsContainer(string pContainerName, int pInitialNumber = 1, Transform pParent = null)
        {
            var container = new GameObject(pContainerName);
            container.transform.SetParent(pParent);
            container.transform.localPosition = Vector3.zero;
            container.transform.rotation = Quaternion.identity;
            this.container = container.transform;
            //poolDict = new Dictionary<int, CustomPool<T>>();
            mInitialNumber = pInitialNumber;
        }

        public CustomPool<T> Get(T pPrefab)
        {
            //Debug.Log("GameObject id = " + pPrefab.gameObject.GetInstanceID());
            if (poolDict.ContainsKey(pPrefab.gameObject.GetInstanceID())) {
                //Debug.Log(pPrefab.name + " is ...IN pools");
                return poolDict[pPrefab.gameObject.GetInstanceID()];
            } else {
                //Debug.Log(pPrefab.name + " Object is OUT>> pools");
                var pool = new CustomPool<T>(pPrefab, mInitialNumber, container.transform, !pPrefab.gameObject.IsPrefab());
                poolDict.Add(pPrefab.GetInstanceID(), pool);
                return pool;
            }
        }

        public CustomPool<T> Get(int instanceId)
        {
            if (poolDict.ContainsKey(instanceId))
            {
                return poolDict[instanceId];
            }
            else
            {
                Debug.LogWarning("Couldn't find pool");                
                return null;
            }
        }

        public T Spawn(int instanceId, Vector3 position)
        {
            var pool = Get(instanceId);
            var obj = pool.Spawn(position, true);
            return obj;
        }

        public T Spawn(int instanceId)
        {
            var pool = Get(instanceId);
            var obj = pool.Spawn();
            return obj;
        }


        public T Spawn(T prefab, Vector3 position)
        {
            var pool = Get(prefab);
            var obj = pool.Spawn(position, true);
            return obj;
        }

        public T Spawn(int instanceId, Vector3 position, Quaternion rotation)
        {
            var t = Spawn(instanceId, position);
            t.gameObject.transform.rotation = rotation;
            return t;
        }



        public T Spawn(T prefab, Vector3 position, Quaternion rotation) {
            var t = Spawn(prefab, position);
            t.gameObject.transform.rotation = rotation;
            return t;
        }

        public T Spawn(int instanceId, Vector3 position, Quaternion rotation, GameObject parent)
        {
            var t = Spawn(instanceId, position, rotation);
            t.gameObject.transform.SetParent(parent.transform);
            return t;
        }

        public T Spawn(T prefab, Vector3 position, Quaternion rotation, GameObject parent)
        {
            var t=Spawn(prefab, position, rotation);
            t.gameObject.transform.SetParent(parent.transform);
            return t;
        }
        public T Spawn(T prefab, Vector3 position, Quaternion rotation, GameObject parent, Vector3 localPosition)
        {
            var t = Spawn(prefab, position, rotation);
            t.gameObject.transform.SetParent(parent.transform);
            t.gameObject.transform.localPosition = localPosition;
            return t;
        }

        public T Spawn(T prefab, Transform transform)
        {
            var pool = Get(prefab);
            var obj = pool.Spawn(transform);
            return obj;
        }

        public CustomPool<T> Add(T pPrefab)
        {
            if (!poolDict.ContainsKey(pPrefab.GetInstanceID()))
            {
                var pool = new CustomPool<T>(pPrefab, mInitialNumber, container.transform, !pPrefab.gameObject.IsPrefab());
                poolDict.Add(pPrefab.GetInstanceID(), pool);
            }
            else
                Debug.Log($"Pool Prefab {pPrefab.name} has already existed!");
            return poolDict[pPrefab.GetInstanceID()];
        }

        public void Add(CustomPool<T> pPool)
        {
            if (!poolDict.ContainsKey(pPool.Prefab.GetInstanceID()))
                poolDict.Add(pPool.Prefab.GetInstanceID(), pPool);
            else
            {
                var pool = poolDict[pPool.Prefab.GetInstanceID()];
                //Merge two pool
                foreach (var obj in pPool.ActiveList)
                    if (!pool.ActiveList.Contains(obj))
                        pool.ActiveList.Add(obj);
                foreach (var obj in pPool.InactiveList)
                    if (!pool.InactiveList.Contains(obj))
                        pool.InactiveList.Add(obj);
            }
        }

        public List<T> GetActiveList()
        {
            var list = new List<T>();
            foreach (var pool in poolDict)
            {
                list.AddRange(pool.Value.ActiveList);
            }
            return list;
        }

        public void Release(T pObj)
        {
            foreach (var pool in poolDict)
            {
                pool.Value.Release(pObj);
            }
        }

        public void Release(T pPrefab, T pObj)
        {
            if (poolDict.ContainsKey(pPrefab.GetInstanceID()))
            {
                poolDict[pPrefab.GetInstanceID()].Release(pObj);
            }
        }

        public void ReleaseAll()
        {
            foreach (var pool in poolDict)
            {
                pool.Value.ReleaseAll();
            }
        }

        public void Release(GameObject pObj)
        {
            foreach (var pool in poolDict)
            {
                pool.Value.Release(pObj);
            }
        }

        public T FindComponent(GameObject pObj)
        {
            foreach (var pool in poolDict)
            {
                var component = pool.Value.FindComponent(pObj);
                if (component != null)
                    return component;
            }
            return null;
        }
    }
}
