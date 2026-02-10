using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    public static PlayerController LocalInstance { get; private set; }


    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    public Transform orientation;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float drag;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpInputBuffer;
    private float jumpInputBufferLeft;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float collisionWidth;
    [SerializeField] private float maxGroundDistance;
    [SerializeField] private Vector3 collisionReduction; // no idea what this is lol (i forgot)

    [Header("Interaction")]
    public Transform cameraAnchor;
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private Transform itemAnchor;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private float playerRotateSpeed;
    [SerializeField] private float interactionDistance;
    [SerializeField] private IInteractableObject hoveredItem;
    [SerializeField] private float throwForce;
    private HoldableItem heldItem;

    [Header("Interaction Held")]
    private bool mainInteractHeld;
    private bool alternateInteractHeld;
    [SerializeField] private IInteractableObject interactHeldItem; // the item that was interacted with at the start of the hold
    [SerializeField] private float interactHeldTime;
    private bool InteractHeld => mainInteractHeld || alternateInteractHeld;
    private bool InteractHeldIsHovering => interactHeldItem != null && interactHeldItem.IsHovered();

    [Header("Interaction Outlines")]
    public Outline.Mode outlineMode;
    public Color outlineColor;
    public float outlineWidth;

    private bool controlsDisabled;
    private BuildModeController buildMode;


    // TODO:
    //
    
    // DONE:
    // X Interaction Held detection
    // X Alternate Interaction Held detection


    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
        }

        if (IsOwner) {
            LocalInstance = this;
            buildMode = GetComponent<BuildModeController>();
            playerModelTransform.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            playerModelTransform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        } else {
            enabled = false;
        }
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        if (heldItem != null) {
            PickupItemClientRpc(heldItem, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    private void Start()
    {
        rb.isKinematic = false;

        FirstPersonCamera.LocalInstance.Enable(true);
        SettingsMenuUI.Instance.OnPlayerSpawned();





        #region Events

        // jump is actually detected in HandleMovement(); this indirectly causes the player to jump by resetting the buffer
        GameInput.Instance.OnJump += (sender, args) => {
            jumpInputBufferLeft = jumpInputBuffer;
            
            // if (buildMode.IsActive()) {
            //     buildMode.Deactivate();
            // } else {
            //     EnterBuildMode();
            // }
        };


        // Main Action (LMB)
        // used for: Using held items, Interacting with objects, Picking up objects, Beginning interact held functionality
        GameInput.Instance.MainAction += (sender, args) => {
            if (controlsDisabled || buildMode.IsActive()) return;

            if (heldItem != null && heldItem.hasUse) {
                heldItem.OnUse(this);

                if (!InteractHeld) { // so only LMB or RMB can be held at once
                    mainInteractHeld = true;
                    interactHeldItem = heldItem;
                }

            } else if (heldItem == null && hoveredItem != null && hoveredItem is HoldableItem item) {
                PickupItem(item);
            
            } else if (hoveredItem != null) {
                hoveredItem.OnInteract(this);
                
                if (!InteractHeld) {
                    mainInteractHeld = true;
                    interactHeldItem = hoveredItem;
                }
            }
        };


        // Secondary Action (RMB)
        // used for: Using secondary ability of held items, Interacting with second ability of objects
        GameInput.Instance.SecondaryAction += (sender, args) => {
            if (controlsDisabled || buildMode.IsActive()) return;
            if (hoveredItem == null) return; 

            if (heldItem != null && heldItem.hasUse) {
                heldItem.OnUseSecondary(this);

                if (!InteractHeld) { // so only LMB or RMB can be held at once
                    alternateInteractHeld = true;
                    interactHeldItem = heldItem;
                }
            } else {
                hoveredItem.OnInteractSecondary(this);
                
                if (!InteractHeld) {
                    alternateInteractHeld = true;
                    interactHeldItem = hoveredItem;
                }
            }
        };


        // Releasing Main Action (LMB)
        // used for: Detecting when interact held is cancelled
        GameInput.Instance.MainActionReleased += (sender, args) => {
            mainInteractHeld = false;
            interactHeldItem = null;
        };


        // Releasing Alternate Action (RMB)
        // used for: Detecting when alternate interact held is cancelled
        GameInput.Instance.SecondaryActionReleased += (sender, args) => {
            alternateInteractHeld = false;
            interactHeldItem = null;
        };

        
        // Dropping Held Item (F)
        GameInput.Instance.OnDrop += (sender, args) => {
            if (controlsDisabled || buildMode.IsActive()) return;
            if (heldItem == null) return;
            
            DropHeldItem();
        };

    }

    #endregion







    #region Update Functions

    private void Update()
    {
        if (controlsDisabled) return;

        if (!buildMode.IsActive()) {
            HandleItemHovering();
            HandleInteractHeld();
        }

        // this should probably move but its here for now because idk where to put it
        if (heldItem != null) {
            heldItem.HeldUpdate(this);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void LateUpdate()
    {
        HandlePlayerVisual();
    }



    private void HandlePlayerVisual() 
    {
        playerModelTransform.transform.rotation = Quaternion.Slerp(
            playerModelTransform.transform.rotation, 
            Quaternion.Euler(Vector3.up * orientation.rotation.eulerAngles.y),
            Time.deltaTime * playerRotateSpeed);

        if (heldItem != null) {
            // TODO: add heldItem.heldPositionValues to item position (but it needs to be adjusted for the angle using trig)
            heldItem.transform.SetPositionAndRotation(itemAnchor.position + Vector3.up * heldItem.heldPositionOffset.y, Quaternion.Euler(playerModelTransform.rotation.eulerAngles + heldItem.heldRotationValues));
        }
    }

    private void HandleItemHovering() 
    {
        if (Physics.Raycast(cameraAnchor.position, orientation.forward, out RaycastHit hit, interactionDistance, itemLayerMask)) {
            if (hit.transform.TryGetComponent(out IInteractableObject otherItem))
            {
                // player is looking at an item
                if (hoveredItem != otherItem) {
                    if (hoveredItem != null) {
                        hoveredItem.Unhover();
                    }
                    if (heldItem == null || ItemInteractions.HeldItemCanInteractWithOther(heldItem, otherItem)) {
                        otherItem.Hover(); // only tell an item it's hovered if it can be interacted with by the player's item
                        hoveredItem = otherItem;
                    }
                }
            } else {
                Debug.LogError("Item is missing an 'IInteractableItem' component! (make sure the only collider is on the parent object with the component)");
            }
        }
        else
        {
            // player is not looking at an item
            if (hoveredItem != null) {

                // as far as i know, interactHeld should only be true if interactHeldItem is !null... if this errors, that's probably why
                if (InteractHeld && interactHeldItem == hoveredItem) {
                    if (mainInteractHeld) {
                        interactHeldItem.OnInteractHeldLookAway(this);
                    } else {
                        interactHeldItem.OnAlternateHeldLookAway(this);
                    }
                }

                hoveredItem.Unhover();
                hoveredItem = null;
            }
        }
    }

    private void HandleMovement() 
    {
        // horizontal drag
        Vector3 dragVelocity = -rb.linearVelocity;
        dragVelocity.y = 0;
        rb.AddForce(drag * dragVelocity);

        if (controlsDisabled) return;
        
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        // movement relative to camera direction
        float theta = -orientation.localRotation.eulerAngles.y * Mathf.Deg2Rad;
        Vector2 relMoveDir = new Vector2(
            inputVector.x * Mathf.Cos(theta) - inputVector.y * Mathf.Sin(theta),
            inputVector.x * Mathf.Sin(theta) + inputVector.y * Mathf.Cos(theta)
        );

        float moveSpeed = GameInput.Instance.GetIsSprinting() ? runSpeed : walkSpeed;

        Vector3 moveDir = new Vector3(relMoveDir.x, 0f, relMoveDir.y) * moveSpeed;

        rb.AddForce(moveDir);
        
        // jump detection
        jumpInputBufferLeft -= Time.fixedDeltaTime;
        if (jumpInputBufferLeft > 0 && IsGrounded(out Collider[] groundObjects)) {
            rb.linearVelocity -= Vector3.up * rb.linearVelocity.y; // cancel current velocity
            rb.AddForce(Vector3.up * jumpForce);
            jumpInputBufferLeft = 0f;

            // newtons 3rd law because im bored lol
            // also just realized this isn't sent through the server so it's probably multiplayer buggy (havent tested)
            ApplyThirdLawOnJump(groundObjects);
        }
    }

    // private bool IsGrounded() {
    //     return Physics.OverlapBox(transform.position + Vector3.down * maxGroundDistance, new Vector3(collisionWidth/2, maxGroundDistance/2, collisionWidth/2) - collisionReduction, Quaternion.identity, groundLayerMask).Length != 0;
    // }
    private bool IsGrounded(out Collider[] groundObjects) 
    {
        groundObjects = Physics.OverlapBox(transform.position + Vector3.down * maxGroundDistance, new Vector3(collisionWidth/2, maxGroundDistance/2, collisionWidth/2) - collisionReduction, Quaternion.identity, groundLayerMask);
        return groundObjects.Length != 0;
    }
    private void ApplyThirdLawOnJump(Collider[] groundObjects) {
        float distributedForce = jumpForce / groundObjects.Length;
        foreach (Collider collider in groundObjects) {
            if (collider.TryGetComponent(out Rigidbody rb)) {
                rb.AddForce(Vector3.down * distributedForce);
            }
        }
    }

    private void HandleInteractHeld() 
    {
        if (InteractHeld && InteractHeldIsHovering) {
            interactHeldTime += Time.deltaTime;
            if (mainInteractHeld) {
                interactHeldItem.OnInteractHeld(this, interactHeldTime);
            } else {
                interactHeldItem.OnAlternateHeld(this, interactHeldTime);
            }
        } else {
            interactHeldTime = 0f;
        }
    }
    
    #endregion







    #region Interactions

    public void PickupItem(HoldableItem item) 
    {
        PickupItemServerRpc(item);
    }

    [Rpc(SendTo.Server)]
    private void PickupItemServerRpc(NetworkBehaviourReference itemObject, RpcParams rpcParams = default) 
    {
        if (itemObject.TryGet(out HoldableItem item))
        {
            item.NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
            PickupItemClientRpc(item, RpcTarget.ClientsAndHost);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void PickupItemClientRpc(NetworkBehaviourReference itemObject, RpcParams rpcParams) 
    {
        if (itemObject.TryGet(out HoldableItem item))
        {
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Collider>().enabled = false;

            item.OnPickup(this);

            heldItem = item;
        }
    }

    public void DropHeldItem() {
        DropHeldItemServerRpc(orientation.forward);
    }

    [Rpc(SendTo.Server)]
    private void DropHeldItemServerRpc(Vector3 clientThrowDirection) {
        // errors here rn because heldItem will be null to any late clients who didn't see the player pick up the item
        heldItem.NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
        HoldableItem item = heldItem;
        DropHeldItemClientRpc();

        item.GetComponent<Rigidbody>().AddForce(clientThrowDirection * throwForce);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void DropHeldItemClientRpc() {
        heldItem.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.GetComponent<Collider>().enabled = true;
        
        heldItem.OnDrop();

        heldItem = null;
    }

    public HoldableItem GetHeldItem() {
        return heldItem;
    }

    #endregion







    public void DisableControls(bool changeMouseState = true) {
        controlsDisabled = true;
        FirstPersonCamera.LocalInstance.Disable(changeMouseState);
    }
    public void EnableControls(bool changeMouseState = true) {
        controlsDisabled = false;
        FirstPersonCamera.LocalInstance.Enable(changeMouseState);
    }






    #region Build Mode

    public void EnterBuildMode(BuildObjectSO buildObjectSO) {
        buildMode.Activate();
        buildMode.SetBuildObject(buildObjectSO);
        
        if (hoveredItem != null) {
            hoveredItem.Unhover();
            hoveredItem = null;
        }
    }

    #endregion
}
