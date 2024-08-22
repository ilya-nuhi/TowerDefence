using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T: MonoBehaviour 
{

	static T m_instance;
    protected static bool _isApplicationQuitting = false;

	public static T Instance
	{
		get 
		{
			if (m_instance == null) 
			{
				m_instance = GameObject.FindObjectOfType<T> ();

				if (m_instance == null && !_isApplicationQuitting) 
				{
					GameObject singleton = new GameObject (typeof(T).Name);
					m_instance = singleton.AddComponent<T> ();
				}
			}
			return m_instance;
		}
	}

	protected virtual void Awake()
	{
		if (m_instance == null) 
		{
			m_instance = this as T;
            transform.parent = null;
			//DontDestroyOnLoad (this.gameObject);
		} 
		else 
		{
			Destroy (gameObject);
		}
	}

	protected virtual void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }
}
