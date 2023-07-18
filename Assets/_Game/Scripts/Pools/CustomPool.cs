using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities.Common
{
    [System.Serializable]
    public class CustomPool<T> : MonoBehaviour where T : Component
    {
        #region Members

        public Action<T> onSpawn;
        [SerializeField] protected T mPrefab;
        [SerializeField] protected Transform mParent;
        [SerializeField] protected string mName;
        [SerializeField] protected bool mPushToLastSibling;
        [SerializeField] protected bool mAutoRelocate;
        [SerializeField] protected int mLimitNumber;

        private List<T> mActiveList = new List<T>();
        private List<T> mInactiveList = new List<T>();

        public T Prefab { get { return mPrefab; } }
        public Transform Parent { get { return mParent; } }
        public string Name { get { return mName; } }
        public List<T> ActiveList { get { return mActiveList; } }
        public List<T> InactiveList { get { return mInactiveList; } }
        public bool pushToLastSibling { get { return pushToLastSibling; } set { pushToLastSibling = value; } }
        public int limitNumber { get { return mLimitNumber; } set { mLimitNumber = value; } }
        public Transform parent { get { return mParent; } }

        #endregion

        //====================================

        #region Public


        protected void Awake()
        {
            if (string.IsNullOrEmpty(mName))
                mName = mPrefab.name;

            if (mParent == null)
            {
                GameObject temp = new GameObject();
                temp.name = string.Format("Pool_{0}", mName);
                mParent = temp.transform;
            }

            mActiveList = new List<T>();
            mInactiveList = new List<T>();
            mInactiveList.Prepare(mPrefab, this.mParent, this.mLimitNumber);
            //if (true)
            //{
            //    mInactiveList.Add(mPrefab);
            //    mPrefab.SetParent(mParent);
            //    mPrefab.transform.SetAsLastSibling();
            //    mPrefab.SetActive(false);
            //}
        }

        public CustomPool(T pPrefab, int pInitialCount, Transform pParent, bool pBuildinPrefab, string pName = "", bool pAutoRelocate = true)
        {
            mPrefab = pPrefab;
            mParent = pParent;
            mName = pName;
            mAutoRelocate = pAutoRelocate;

            if (string.IsNullOrEmpty(mName))
                mName = mPrefab.name;

            if (mParent == null)
            {
                GameObject temp = new GameObject();
                temp.name = string.Format("Pool_{0}", mName);
                mParent = temp.transform;
            }

            mActiveList = new List<T>();
            mInactiveList = new List<T>();
            mInactiveList.Prepare(mPrefab, pParent, pInitialCount, pPrefab.name);
            if (pBuildinPrefab)
            {
                mInactiveList.Add(mPrefab);
                pPrefab.SetParent(pParent);
                pPrefab.transform.SetAsLastSibling();
                pPrefab.SetActive(false);
            }
        }

        public CustomPool(GameObject pPrefab, int pInitialCount, Transform pParent, bool pBuildinPrefab, string pName = "", bool pAutoRelocate = true)
        {
#if UNITY_2019_2_OR_NEWER
            pPrefab.TryGetComponent(out T component);
            mPrefab = component;
#else
            prefab = pPrefab.GetComponent<T>();
#endif
            mParent = pParent;
            mName = pName;
            mAutoRelocate = pAutoRelocate;

            if (string.IsNullOrEmpty(mName))
                mName = mPrefab.name;

            if (mParent == null)
            {
                GameObject temp = new GameObject();
                temp.name = string.Format("Pool_{0}", mName);
                mParent = temp.transform;
            }

            mActiveList = new List<T>();
            mInactiveList = new List<T>();
            mInactiveList.Prepare(mPrefab, pParent, pInitialCount, pPrefab.name);
            if (pBuildinPrefab)
            {
                mInactiveList.Add(mPrefab);
                pPrefab.transform.SetParent(pParent);
                pPrefab.transform.SetAsLastSibling();
                pPrefab.SetActive(false);
            }
        }

        public void Prepare(int pInitialCount)
        {
            int numberNeeded = pInitialCount - mInactiveList.Count;
            if (numberNeeded > 0)
            {
                var list = new List<T>();
                list.Prepare(mPrefab, mParent, pInitialCount, mPrefab.name);
                mInactiveList.AddRange(list);
            }
        }

        public T Spawn()
        {
            return Spawn(Vector3.zero, false);
        }

        public T Spawn(Transform pPoint)
        {
            return Spawn(pPoint.position, true);
        }

        public T Spawn(Vector3 position, bool pIsWorldPosition)
        {
            if (mLimitNumber > 0 && mActiveList.Count == mLimitNumber)
            {
                if (!mAutoRelocate)
                {
                    mLimitNumber++;
                    mInactiveList.CreatePoolElement(mPrefab, parent, name);
                } else
                {
                    var activeItem = mActiveList[0];
                    mInactiveList.Add(activeItem);
                    mActiveList.Remove(activeItem);
                }
            }

            int count = mInactiveList.Count;
            if (mAutoRelocate && count == 0)
                RelocateInactive();

            if (count > 0)
            {
                var item = mInactiveList[0];
                if (pIsWorldPosition)
                    item.transform.position = position;
                else
                    item.transform.localPosition = position;
                Active(item, true);

                if (onSpawn != null)
                    onSpawn(item);

                if (mPushToLastSibling)
                    item.transform.SetAsLastSibling();
                //Debug.Log("Game Object instance id = " + item.gameObject.GetInstanceID());
                return item;
            }

            //T newItem = Object.Instantiate(mPrefab, mParent);
            //newItem.name = mName;
            //mInactiveList.Add(newItem);

            return Spawn(position, pIsWorldPosition);
        }

        public T Spawn(Vector3 position, bool pIsWorldPosition, ref bool pReused)
        {
            if (mLimitNumber > 0 && mActiveList.Count == mLimitNumber)
            {
                var activeItem = mActiveList[0];
                mInactiveList.Add(activeItem);
                mActiveList.Remove(activeItem);
            }

            int count = mInactiveList.Count;
            if (mAutoRelocate && count == 0)
                RelocateInactive();

            if (count > 0)
            {
                var item = mInactiveList[0];
                if (pIsWorldPosition)
                    item.transform.position = position;
                else
                    item.transform.localPosition = position;
                Active(item, true);

                if (onSpawn != null)
                    onSpawn(item);

                if (mPushToLastSibling)
                    item.transform.SetAsLastSibling();
                return item;
            }

            T newItem = Object.Instantiate(mPrefab, mParent);
            newItem.name = mName;
            mInactiveList.Add(newItem);
            pReused = false;

            return Spawn(position, pIsWorldPosition, ref pReused);
        }

        public void AddOutsiders(List<T> pInSceneObjs)
        {
            for (int i = pInSceneObjs.Count - 1; i >= 0; i--)
                AddOutsider(pInSceneObjs[i]);
        }

        public void AddOutsider(T pInSceneObj)
        {
            if (mInactiveList == null)
                mInactiveList = new List<T>();
            if (mActiveList == null)
                mActiveList = new List<T>();

            if (mInactiveList.Contains(pInSceneObj)
                || mActiveList.Contains(pInSceneObj))
                return;

            if (pInSceneObj.gameObject.activeSelf)
                mActiveList.Add(pInSceneObj);
            else
                mInactiveList.Add(pInSceneObj);
            pInSceneObj.transform.SetParent(mParent);
        }

        public void Release(T pObj)
        {
            //pObj.transform.SetParent(this.mParent);
            for (int i = 0; i < mActiveList.Count; i++)
            {
                if (ReferenceEquals(mActiveList[i], pObj))
                {
                    Active(mActiveList[i], false);
                    return;
                }
            }
        }

        public void Release(GameObject pObj)
        {
            for (int i = 0; i < mActiveList.Count; i++)
            {
                if (mActiveList[i].gameObject.GetInstanceID() == pObj.GetInstanceID())
                {
                    Active(mActiveList[i], false);
                    return;
                }
            }
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < mActiveList.Count; i++)
            {
                var item = mActiveList[i];
                mInactiveList.Add(item);
                item.SetActive(false);
            }
            mActiveList.Clear();
        }

        public void DestroyAll()
        {
            while (mActiveList.Count > 0)
            {
                Object.Destroy(mActiveList[0]);
                mActiveList.RemoveAt(0);
            }
            while (mInactiveList.Count > 0)
            {
                Object.Destroy(mInactiveList[0]);
                mInactiveList.RemoveAt(0);
            }
        }

        public void Destroy(T pItem)
        {
            mActiveList.Remove(pItem);
            mInactiveList.Remove(pItem);
            Object.Destroy(pItem);
        }

        public T FindFromActive(T t)
        {
            for (int i = 0; i < mActiveList.Count; i++)
            {
                var item = mActiveList[i];
                if (item == t)
                    return item;
            }
            return null;
        }

        public T FindComponent(GameObject pObj)
        {
            for (int i = 0; i < mActiveList.Count; i++)
            {
                if (mActiveList[i].gameObject == pObj)
                {
                    var temp = mActiveList[i];
                    return temp;
                }
            }
            for (int i = 0; i < mInactiveList.Count; i++)
            {
                if (mInactiveList[i].gameObject == pObj)
                {
                    var temp = mInactiveList[i];
                    return temp;
                }
            }
            return null;
        }

        public T GetFromActive(int pIndex)
        {
            if (pIndex < 0 || pIndex >= mActiveList.Count)
                return null;
            return mActiveList[pIndex];
        }

        public T FindFromInactive(T t)
        {
            for (int i = 0; i < mInactiveList.Count; i++)
            {
                var item = mInactiveList[i];
                if (item == t)
                    return item;
            }
            return null;
        }

        public void RelocateInactive()
        {
            for (int i = mActiveList.Count - 1; i >= 0; i--)
                if (!mActiveList[i].gameObject.activeSelf)
                    Active(mActiveList[i], false);
        }

        public void SetParent(Transform pParent)
        {
            mParent = pParent;
        }

        public void SetName(string pName)
        {
            mName = pName;
        }

        #endregion

        //========================================

        #region Private

        private void Active(T pItem, bool pValue)
        {
            if (pValue)
            {
                mActiveList.Add(pItem);
                mInactiveList.Remove(pItem);
            }
            else
            {
                mInactiveList.Add(pItem);
                mActiveList.Remove(pItem);
                pItem.transform.parent = this.transform;
            }
            pItem.SetActive(pValue);
        }

        #endregion

        //=========================================

//#if UNITY_EDITOR
//        public void DrawOnEditor()
//        {
//            if (UnityEditor.EditorApplication.isPlaying)
//            {
//                EditorHelper.BoxVertical(() =>
//                {
//                    if (mActiveList != null)
//                        EditorHelper.ListReadonlyObjects(mActiveList, mName + "ActiveList", false);
//                    if (mInactiveList != null)
//                        EditorHelper.ListReadonlyObjects(mInactiveList, mName + "InactiveList", false);
//                    if (EditorHelper.Button("Relocate"))
//                        RelocateInactive();
//                }, Color.white, true);
//            }
//        }
//#endif
    }
}