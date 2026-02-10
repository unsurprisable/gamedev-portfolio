
using Unity.Netcode;
using UnityEngine;

public static class ClientDebug
{
    [Rpc(SendTo.SpecifiedInParams)]
    public static void LogRpc(string message, BaseRpcTarget rpcTarget)
    {
        Debug.Log(message);
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public static void LogWarningRpc(string message, BaseRpcTarget rpcTarget)
    {
        Debug.LogWarning(message);
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public static void LogErrorRpc(string message, BaseRpcTarget rpcTarget)
    {
        Debug.LogError(message);
    }
}
