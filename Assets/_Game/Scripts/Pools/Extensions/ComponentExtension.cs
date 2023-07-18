
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities.Common
{
    public static class ComponentExtension
    {
        public static void SetActive(this Component target, bool value)
        {
            try
            {
                target.gameObject.SetActive(value);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        public static bool IsActive(this Component target)
        {
            try
            {
                return target.gameObject.activeSelf;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }

        public static List<T> SortByName<T>(this List<T> objects) where T : UnityEngine.Object
        {
            return objects = objects.OrderBy(m => m.name).ToList();
        }

        public static void SetParent(this Component target, Transform parent)
        {
            target.transform.SetParent(parent);
        }

        public static T FindComponentInChildren<T>(this GameObject objRoot) where T : Component
        {
            // if we don't find the component in this object 
            // recursively iterate children until we do
#if UNITY_2019_2_OR_NEWER
            objRoot.TryGetComponent(out T component);
#else
            T component = objRoot.GetComponent<T>();
#endif
            if (null == component)
            {
                // transform is what makes the hierarchy of GameObjects, so 
                // need to access it to iterate children
                Transform trnsRoot = objRoot.transform;
                int iNumChildren = trnsRoot.childCount;

                // could have used foreach(), but it causes GC churn
                for (int iChild = 0; iChild < iNumChildren; ++iChild)
                {
                    // recursive call to this function for each child
                    // break out of the loop and return as soon as we find 
                    // a component of the specified type
                    component = FindComponentInChildren<T>(trnsRoot.GetChild(iChild).gameObject);
                    if (null != component)
                    {
                        break;
                    }
                }
            }

            return component;
        }

        public static List<T> FindComponentsInChildren<T>(this GameObject objRoot) where T : Component
        {
            var list = new List<T>();
#if UNITY_2019_2_OR_NEWER
            objRoot.TryGetComponent(out T component);
#else
            T component = objRoot.GetComponent<T>();
#endif

            if (component != null)
                list.Add(component);

            foreach (Transform t in objRoot.transform)
            {
                var components = FindComponentsInChildren<T>(t.gameObject);
                if (components != null)
                    list.AddRange(components);
            }

            return list;
        }

        public static List<string> ToList(this string[] pArray)
        {
            var list = new List<string>();
            for (int i = 0; i < pArray.Length; i++)
                list.Add(pArray[i]);
            return list;
        }

        public static GameObject FindChildObject(this GameObject objRoot, string pName, bool pContain = false)
        {
            GameObject obj = null;
            bool found = !pContain ? (objRoot.name == pName) : (objRoot.name.Contains(pName));
            if (found)
            {
                obj = objRoot;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.Selection.activeObject = obj;
#endif
                return obj;
            }
            else
            {
                Transform trnsRoot = objRoot.transform;
                int iNumChildren = trnsRoot.childCount;
                for (int i = 0; i < iNumChildren; ++i)
                {
                    obj = trnsRoot.GetChild(i).gameObject.FindChildObject(pName);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }

            return null;
        }

        public static T Instantiate<T>(T original, Transform parent, string pName) where T : UnityEngine.Object
        {
            var obj = UnityEngine.Object.Instantiate(original, parent);
            obj.name = pName;
            return obj;
        }

        public static List<T> ToList<T>(this T[] pArray)
        {
            var list = new List<T>();
            foreach (var a in pArray)
                list.Add(a);
            return list;
        }

        public static T[] Add<T>(this T[] pArray, T pObj)
        {
            var newArray = new T[pArray.Length + 1];
            for (int i = 0; i < pArray.Length; i++)
                newArray[i] = pArray[i];
            newArray[pArray.Length] = pObj;
            return newArray;
        }

        public static T[] Remove<T>(this T[] pArray, T pObj)
        {
            int count = 0;
            for (int i = 0; i < pArray.Length; i++)
                if (EqualityComparer<T>.Default.Equals(pArray[i], pObj))
                    count++;
            int j = 0;
            var newArray = new T[pArray.Length - count];
            for (int i = 0; i < pArray.Length; i++)
                if (!EqualityComparer<T>.Default.Equals(pArray[i], pObj))
                {
                    newArray[j] = pArray[i];
                    j++;
                }
            return newArray;
        }

        public static List<T> RemoveNull<T>(this List<T> pList)
        {
            for (int i = pList.Count - 1; i >= 0; i--)
            {
                if (pList[i].ToString() == "null")
                    pList.RemoveAt(i);
            }
            return pList;
        }

        public static List<T> RemoveDupplicate<T>(this List<T> pList) where T : UnityEngine.Object
        {
            List<int> duplicate = new List<int>();
            for (int i = 0; i < pList.Count; i++)
            {
                int count = 0;
                for (int j = pList.Count - 1; j >= 0; j--)
                {
                    if (pList[j] == pList[i])
                    {
                        count++;
                        if (count > 1)
                            duplicate.Add(j);
                    }
                }
            }
            for (int j = pList.Count - 1; j >= 0; j--)
            {
                if (duplicate.Contains(j))
                    pList.Remove(pList[j]);
            }

            return pList;
        }

        public static void StopMove(this UnityEngine.AI.NavMeshAgent pAgent, bool pStop)
        {
            if (pAgent.gameObject.activeSelf || !pAgent.enabled || !pAgent.isOnNavMesh)
                return;
            pAgent.isStopped = pStop;
        }

        public static bool CompareTags(this Collider collider, params string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (collider.CompareTag(tags[i]))
                    return true;
            }
            return false;
        }

        public static bool CompareTags(this GameObject gameObject, params string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (gameObject.CompareTag(tags[i]))
                    return true;
            }
            return false;
        }

        #region Simple Pool

        public static T Obtain<T>(this List<T> pool, GameObject prefab, Transform parent, string name = null) where T : Component
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                {
                    pool[i].SetParent(parent);
                    pool[i].transform.localPosition = Vector3.zero;
                    return pool[i];
                }
            }

            GameObject temp = UnityEngine.Object.Instantiate(prefab, parent);
            temp.name = name == null ? prefab.name : name;
            temp.transform.localPosition = Vector3.zero;
#if UNITY_2019_2_OR_NEWER
            temp.TryGetComponent(out T t);
#else
            var t = temp.GetComponent<T>();
#endif
            pool.Add(t);

            return t;
        }

        public static T Obtain<T>(this List<T> pool, Transform parent, string name = null) where T : Component
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                {
                    pool[i].SetParent(parent);
                    pool[i].transform.localPosition = Vector3.zero;
                    return pool[i];
                }
            }

            GameObject temp = UnityEngine.Object.Instantiate(pool[0].gameObject, parent);
            temp.name = name == null ? string.Format("{0}_{1}", pool[0].name, (pool.Count() + 1)) : name;
            temp.transform.localPosition = Vector3.zero;
#if UNITY_2019_2_OR_NEWER
            temp.TryGetComponent(out T t);
#else
            var t = temp.GetComponent<T>();
#endif
            pool.Add(t);

            return t;
        }

        public static T Obtain<T>(this List<T> pool, Transform pParent, int max, string pName = null) where T : Component
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                {
                    var obj = pool[i];
                    pool.Remove(obj);
                    obj.transform.SetParent(pParent);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    pool.Add(obj);
                    return obj;
                }
            }

            if (max > 1 && max > pool.Count)
            {
                T temp = UnityEngine.Object.Instantiate(pool[0], pParent);
                pool.Add(temp);
                temp.transform.localPosition = Vector3.zero;
                temp.transform.localScale = Vector3.one;
                if (!string.IsNullOrEmpty(pName))
                    temp.name = pName;
                return temp;
            }
            else
            {
                var obj = pool[0];
                pool.Remove(obj);
                pool.Add(obj);
                return obj;
            }
        }

        public static void Free<T>(this List<T> pool) where T : Component
        {
            foreach (var t in pool)
                t.SetActive(false);
        }

        public static void Free<T>(this List<T> pool, Transform pParent) where T : Component
        {
            for (int i = 0; i < pool.Count; i++)
            {
                pool[i].SetParent(pParent);
                pool[i].SetActive(false);
            }
        }

        public static void Prepare<T>(this List<T> pool, GameObject prefab, Transform parent, int count) where T : Component
        {
            for (int i = 0; i < count; i++)
            {
                GameObject temp = UnityEngine.Object.Instantiate(prefab, parent);
                temp.SetActive(false);
#if UNITY_2019_2_OR_NEWER
                temp.TryGetComponent(out T t);
#else
                var t = temp.GetComponent<T>();
#endif
                pool.Add(t);
            }
        }

        public static T Obtain<T>(this List<T> pool, T prefab, Transform parent) where T : Component
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                    return pool[i];
            }

            T temp = UnityEngine.Object.Instantiate(prefab, parent);
            temp.name = prefab.name;
            pool.Add(temp);

            return temp;
        }

        public static void Prepare<T>(this List<T> pool, T prefab, Transform parent, int count, string name = "") where T : Component
        {
            for (int i = 0; i < count; i++)
            {
                pool.CreatePoolElement(prefab, parent, name);

                //var temp = UnityEngine.Object.Instantiate(prefab, parent);
                //temp.SetActive(false);
                //if (!string.IsNullOrEmpty(name))
                //    temp.name = name;
                //pool.Add(temp);
            }
        }


        public static void CreatePoolElement<T>(this List<T> pool, T prefab, Transform parent, string name = "") where T : Component
        {
            var temp = UnityEngine.Object.Instantiate(prefab, parent);
            temp.SetActive(false);
            if (!string.IsNullOrEmpty(name))
                temp.name = name;
            pool.Add(temp);
        }

        #endregion

        public static T Find<T>(this List<T> pList, string pName) where T : Component
        {
            for (int i = 0; i < pList.Count; i++)
            {
                if (pList[i].name == pName)
                    return pList[i];
            }
            return null;
        }

        public static T Find<T>(this T[] pList, string pName) where T : Component
        {
            for (int i = 0; i < pList.Length; i++)
            {
                if (pList[i].name == pName)
                    return pList[i];
            }
            return null;
        }

        public static bool IsPrefab(this GameObject target)
        {
            return target.scene.name == null;
        }

        public static Vector2 NativeSize(this Sprite pSrite)
        {
            var sizeX = pSrite.bounds.size.x * pSrite.pixelsPerUnit;
            var sizeY = pSrite.bounds.size.y * pSrite.pixelsPerUnit;
            return new Vector2(sizeX, sizeY);
        }

        //public static int GameObjectId<T>(this T target) where T : Component
        //{
        //    return target.GetInstanceID();
        //}

        public static bool IsNull<T>(this T pObj) where T : UnityEngine.Object
        {
            return ReferenceEquals(pObj, null);
        }

        public static bool IsNotNull<T>(this T pObj) where T : UnityEngine.Object
        {
            return !ReferenceEquals(pObj, null);
        }


        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                      TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }

        //===================================================

        #region Image

        /// <summary>
        /// Sketch image following prefered with
        /// </summary>
        public static Vector2 SketchByHeight(this UnityEngine.UI.Image pImage, float pPreferedHeight, bool pPreferNative = false)
        {
            if (pImage.sprite == null)
                return new Vector2(pPreferedHeight, pPreferedHeight);

            var nativeSizeX = pImage.sprite.bounds.size.x * pImage.sprite.pixelsPerUnit;
            var nativeSizeY = pImage.sprite.bounds.size.y * pImage.sprite.pixelsPerUnit;
            float coeff = pPreferedHeight / nativeSizeY;
            float sizeX = nativeSizeX * coeff;
            float sizeY = nativeSizeY * coeff;
            if (pPreferNative && sizeY > nativeSizeY)
            {
                sizeX = nativeSizeX;
                sizeY = nativeSizeY;
            }
            pImage.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            return pImage.rectTransform.sizeDelta;
        }

        /// <summary>
        /// Sketch image following prefered with
        /// </summary>
        public static Vector2 SketchByWidth(this UnityEngine.UI.Image pImage, float pPreferedWith, bool pPreferNative = false)
        {
            if (pImage.sprite == null)
                return new Vector2(pPreferedWith, pPreferedWith);

            var nativeSizeX = pImage.sprite.bounds.size.x * pImage.sprite.pixelsPerUnit;
            var nativeSizeY = pImage.sprite.bounds.size.y * pImage.sprite.pixelsPerUnit;
            float coeff = pPreferedWith / nativeSizeX;
            float sizeX = nativeSizeX * coeff;
            float sizeY = nativeSizeY * coeff;
            if (pPreferNative && sizeX > nativeSizeX)
            {
                sizeX = nativeSizeX;
                sizeY = nativeSizeY;
            }
            pImage.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            return pImage.rectTransform.sizeDelta;
        }

        public static Vector2 SketchByFixedHeight(this UnityEngine.UI.Image pImage, float pFixedHeight)
        {
            if (pImage.sprite == null)
                return new Vector2(pFixedHeight, pFixedHeight);

            var nativeSizeY = pImage.sprite.bounds.size.y * pImage.sprite.pixelsPerUnit;
            var nativeSizeX = pImage.sprite.bounds.size.x * pImage.sprite.pixelsPerUnit;
            float sizeX = pFixedHeight * nativeSizeX / nativeSizeY;
            pImage.rectTransform.sizeDelta = new Vector2(sizeX, pFixedHeight);
            return pImage.rectTransform.sizeDelta;
        }

        public static Vector2 SketchByFixedWidth(this UnityEngine.UI.Image pImage, float pFixedWidth)
        {
            if (pImage.sprite == null)
                return new Vector2(pFixedWidth, pFixedWidth);

            var nativeSizeX = pImage.sprite.bounds.size.x * pImage.sprite.pixelsPerUnit;
            var nativeSizeY = pImage.sprite.bounds.size.y * pImage.sprite.pixelsPerUnit;
            float sizeY = nativeSizeX * nativeSizeY / pFixedWidth;
            pImage.rectTransform.sizeDelta = new Vector2(pFixedWidth, sizeY);
            return pImage.rectTransform.sizeDelta;
        }

        public static Vector2 Sketch(this UnityEngine.UI.Image pImage, Vector2 pPreferedSize, bool pPreferNative = false)
        {
            if (pImage.sprite == null)
                return pPreferedSize;

            var nativeSizeX = pImage.sprite.bounds.size.x * pImage.sprite.pixelsPerUnit;
            var nativeSizeY = pImage.sprite.bounds.size.y * pImage.sprite.pixelsPerUnit;
            float coeffX = pPreferedSize.x / nativeSizeX;
            float coeffY = pPreferedSize.y / nativeSizeY;
            float sizeX = nativeSizeX * coeffX;
            float sizeY = nativeSizeY * coeffY;
            if (coeffX > coeffY)
            {
                sizeX *= coeffY;
                sizeY *= coeffY;
            }
            else
            {
                sizeX *= coeffX;
                sizeY *= coeffX;
            }
            if (pPreferNative && (sizeX > nativeSizeX || sizeY > nativeSizeY))
            {
                sizeX = nativeSizeX;
                sizeY = nativeSizeY;
            }
            pImage.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            return pImage.rectTransform.sizeDelta;
        }

        public static Vector2 SetNativeSize(this UnityEngine.UI.Image pImage, Vector2 pMaxSize)
        {
            if (pImage.sprite == null)
            {
                pImage.rectTransform.sizeDelta = pMaxSize;
                return pImage.rectTransform.sizeDelta;
            }
            var nativeSizeX = pImage.sprite.bounds.size.x * pImage.sprite.pixelsPerUnit;
            var nativeSizeY = pImage.sprite.bounds.size.y * pImage.sprite.pixelsPerUnit;
            if (nativeSizeX > pMaxSize.x)
                nativeSizeX = pMaxSize.x;
            if (nativeSizeY > pMaxSize.y)
                nativeSizeY = pMaxSize.y;
            pImage.rectTransform.sizeDelta = new Vector2(nativeSizeX, nativeSizeY);
            return pImage.rectTransform.sizeDelta;
        }

        #endregion

        //===================================================
    }
}