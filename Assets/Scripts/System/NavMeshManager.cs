using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    private NavMeshSurface surface;

    private void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
    }

    public void BakeNavMesh()
    {
        surface.BuildNavMesh();
    }
}
