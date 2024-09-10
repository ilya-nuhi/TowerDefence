using UnityEngine;

public class WallHealth : Health
{
    private Wall _wall;

    protected override void Start()
    {
        base.Start();
        _wall = GetComponent<Wall>();
    }

    protected override void HandleZeroHealth()
    {
        _wall.DestroyWall();
        EventManager.Instance.UpdateNavMesh();
    }
}