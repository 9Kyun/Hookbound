using UnityEngine;

public class PersistentManagersBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject _persistentManagersPrefab;

    private void Awake()
    {
        if (FindAnyObjectByType<PersistentManagers>() != null)
        {
            return;
        }

        Instantiate(_persistentManagersPrefab);
    }
}