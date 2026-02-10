using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerNametagDisplay : NetworkBehaviour
{

    [SerializeField] TextMeshPro nametag;
    

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
        }

        if (IsOwner) {
            enabled = false;
        }
        
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        SynchronizeNametagClientRpc(nametag.text, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SynchronizeNametagClientRpc(string displayName, RpcParams rpcParams)
    {
        SetNametag(displayName);
    }

    public void SetNametag(string displayName)
    {
        nametag.text = displayName;
    }

    private void LateUpdate()
    {
        nametag.transform.forward = Camera.main.transform.forward;
    }

}
