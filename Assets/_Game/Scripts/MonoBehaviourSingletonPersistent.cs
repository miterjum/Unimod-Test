using Sirenix.OdinInspector;
using UnityEngine;

public class MonoBehaviourSingletonPersistent<T> : SerializedMonoBehaviour
	where T : Component
{
	public static T Instance { get; private set; }
	
	public virtual void Awake ()
	{
		if (Instance == null) {
			Instance = this as T;
			DontDestroyOnLoad (this);
		} else {
			DestroyImmediate (gameObject);
		}
	}
}