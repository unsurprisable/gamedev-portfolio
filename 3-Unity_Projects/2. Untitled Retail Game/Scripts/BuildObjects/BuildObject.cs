using Unity.Netcode;
using UnityEngine;

public class BuildObject : NetworkBehaviour
{
    [Header("Build Object")]
    public BuildObjectSO buildObjectSO;
    [SerializeField] private MeshRenderer buildBoundsRenderer;

    protected virtual void OnSell() {}
    protected virtual void OnPlace() {}

    protected virtual void OnAwake() {}
    protected virtual void OnStart() {}
    protected virtual void OnOnNetworkSpawn() {}


    public override void OnNetworkSpawn()
    {
        OnOnNetworkSpawn();
    }

    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        if (buildBoundsRenderer == null) {
            Debug.LogWarning($"BuildObject \"{name}\" has no buildBoundsRenderer!");
        }

        OnStart();
    }


    public void Place() {
        OnPlace();

        Debug.Log($"Placed \"{buildObjectSO.name}\" for {buildObjectSO.price}");
    }
    public void Sell() {
        OnSell();

        Debug.Log($"Sold \"{buildObjectSO.name}\" for {buildObjectSO.price}");
    }

    public void ShowBuildBounds() {
        buildBoundsRenderer.enabled = true;
    }
    public void HideBuildBounds() {
        buildBoundsRenderer.enabled = false;
    }
}

