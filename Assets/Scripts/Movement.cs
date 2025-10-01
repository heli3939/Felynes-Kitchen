using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovePhysicsSafe : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float acceleration = 5f;
    public float deceleration = 10f;
    public float rotateSpeed = 10f;   // rotation smoothing
    public float jumpHeight = 3f;

    [Header("Depth Limits (Z axis)")]
    public float zMin = -3f;
    public float zMax = 3f;

    [Header("Collision")]
    public Collider Capsule;

    // >>> New: jump feel controls
    [Header("Jump Feel")]
    [Tooltip("Extra gravity while falling (1 = default gravity)")]
    public float fallGravityMultiplier = 1.5f;
    [Tooltip("Extra gravity for short hops when jump is released early")]
    public float lowJumpGravityMultiplier = 2.0f;

    private Rigidbody rb;
    private Vector3 input;
    private float startY;

    private Boolean onGround = true;
    private Boolean prevOnGround = true; // >>> for clean debug

    // cache last facing so we keep the same heading when idle
    private Vector3 lastLookDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        // allow yaw, block tipping
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startY = transform.position.y;
        lastLookDir = transform.forward; // start facing current forward
    }

    void Update()
    {
        // >>> Print only when state changes
        if (onGround != prevOnGround)
        {
            Debug.Log("isGrounded: " + onGround);
            prevOnGround = onGround;
        }

        float x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

        input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        if (Input.GetKeyDown(KeyCode.Space) && onGround)  // >>> only jump when grounded
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
        }
    }

    void FixedUpdate()
    {
        // --- movement ---
        Vector3 planarVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);  // use velocity
        Vector3 desired = new Vector3(input.x, 0f, input.z) * moveSpeed;
        Vector3 delta = desired - planarVel;
        float accel = (input.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        Vector3 force = delta.sqrMagnitude > 0.0001f ? delta.normalized * accel : Vector3.zero;
        float maxStep = accel * Time.fixedDeltaTime;
        if (delta.magnitude < maxStep) force = delta / Time.fixedDeltaTime;

        rb.AddForce(force, ForceMode.Acceleration);

        // --- rotation ---
        if (input.sqrMagnitude > 0.01f)
        {
            lastLookDir = input; // remember heading
            Quaternion targetRot = Quaternion.LookRotation(lastLookDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // No input: keep facing the last direction and stop physics spin
            rb.angularVelocity = Vector3.zero;
        }

        // --- clamp Z lane ---
        Vector3 pos = rb.position;
        pos.z = Mathf.Clamp(pos.z, zMin, zMax);
        rb.position = pos;

        Vector3 vel = rb.linearVelocity;

        // --- grounded check ---
        onGround = Physics.Raycast(transform.position, Vector3.down, 0.5f);

        // --- Air control limit (yours) ---
        if (!onGround)
        {
            float maxAirSpeed = 0.5f;
            Vector3 horizVel = new Vector3(vel.x, 0, vel.z);
            if (horizVel.magnitude > maxAirSpeed)
            {
                horizVel = horizVel.normalized * maxAirSpeed;
                vel = new Vector3(horizVel.x, vel.y, horizVel.z);
                rb.linearVelocity = vel;
            }
        }

        // >>> Faster-fall / low-jump gravity shaping
        if (!onGround)
        {
            if (rb.linearVelocity.y < 0f)
            {
                // Falling: add extra gravity
                Vector3 extraG = Physics.gravity * (fallGravityMultiplier - 1.5f);
                rb.AddForce(extraG, ForceMode.Acceleration);
            }
            else if (rb.linearVelocity.y > 0f && !Input.GetKey(KeyCode.Space))
            {
                // Rising but jump released: make hop shorter
                Vector3 extraG = Physics.gravity * (lowJumpGravityMultiplier - 1.5f);
                rb.AddForce(extraG, ForceMode.Acceleration);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (contact.thisCollider == Capsule)
            {
                Debug.Log("capsule collide");
            }
        }
    }
}
