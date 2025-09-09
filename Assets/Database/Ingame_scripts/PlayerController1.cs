using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public class PlayerController1 : NetworkBehaviour
{
    public float moveSpeed = 5f;

    CharacterController cc;

    void Awake() => cc = GetComponent<CharacterController>();

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        float h = Input.GetAxisRaw("Horizontal");  // A/D
        float v = Input.GetAxisRaw("Vertical");    // W/S
        Vector3 dir = new Vector3(h, 0, v).normalized;

        if (dir != Vector3.zero)
            cc.Move(dir * moveSpeed * Runner.DeltaTime);
    }
}
