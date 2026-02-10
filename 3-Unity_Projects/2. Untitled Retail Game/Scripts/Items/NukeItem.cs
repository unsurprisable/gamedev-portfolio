
using Unity.Netcode;
using UnityEngine;

public class NukeItem : HoldableItem
{
    [Header("Nuke")]
    [SerializeField] private float interactDistance;
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] BuildObject selectedObject;

    public override void OnUse(PlayerController player)
    {
        if (selectedObject != null) {
            SellObjectServerRpc(selectedObject);
        }
    }

    public override void HeldUpdate(PlayerController player) {
        if (Physics.Raycast(player.cameraAnchor.position, player.orientation.forward, out RaycastHit hit, interactDistance, interactLayerMask)) {
            if (hit.transform.parent.TryGetComponent(out BuildObject buildObject)) {
                if (selectedObject != buildObject) {
                    if (selectedObject != null) {
                        selectedObject.HideBuildBounds();
                    }
                    selectedObject = buildObject;
                    selectedObject.ShowBuildBounds();
                }
            } else {
                Debug.LogWarning($"Object \"{hit.transform.name}\" is on BuildBounds layer without a BuildObject parent! oh no D:");
            }
        } else {
            if (selectedObject != null) {
                selectedObject.HideBuildBounds();
            }
            selectedObject = null;
        }
    }


    // SELLING IS HANDLED HERE BECAUSE IDK WHERE TO PUT IT YET (as of writing this i havent made a selling mode yet so if ur here because thats a thing now then hi o/ from 2/1/2025)
    [Rpc(SendTo.Server)]
    private void SellObjectServerRpc(NetworkBehaviourReference buildObjectReference)
    {
        if (buildObjectReference.TryGet(out BuildObject buildObject)) {
            EconomyManager.Instance.AddToBalance(buildObject.buildObjectSO.price); // 100% sellback rate
            SellObjectClientRpc(buildObjectReference);
            Destroy(buildObject.gameObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SellObjectClientRpc(NetworkBehaviourReference buildObjectReference)
    {
        if (buildObjectReference.TryGet(out BuildObject buildObject)) {
            buildObject.Sell();
        }
    }

}
