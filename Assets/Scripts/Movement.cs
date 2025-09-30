using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovePhysicsSafe : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public float rotateSpeed = 10f;   // rotation smoothing

    [Header("Depth Limits (Z axis)")]
    public float zMin = -3f;
    public float zMax = 3f;

    private Rigidbody rb;
    private Vector3 input;
    private float startY;

    // NEW: cache last facing so we keep the same heading when idle
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
        float x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

        input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 6f, ForceMode.VelocityChange);
        }
    }

    void FixedUpdate()
    {
        // --- movement (unchanged) ---
        Vector3 planarVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 desired = new Vector3(input.x, 0f, input.z) * moveSpeed;
        Vector3 delta = desired - planarVel;
        float accel = (input.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        Vector3 force = delta.sqrMagnitude > 0.0001f ? delta.normalized * accel : Vector3.zero;
        float maxStep = accel * Time.fixedDeltaTime;
        if (delta.magnitude < maxStep) force = delta / Time.fixedDeltaTime;

        rb.AddForce(force, ForceMode.Acceleration);

        // --- rotation (only when input) ---
        if (input.sqrMagnitude > 0.01f)
        {
            lastLookDir = input; // remember heading
            Quaternion targetRot = Quaternion.LookRotation(lastLookDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // No input: keep facing the last direction and stop physics spin
            rb.angularVelocity = Vector3.zero; // kill collision-induced spin
            // do NOT call MoveRotation so it stays as-is
        }

        // --- clamp Z lane ---
        Vector3 pos = rb.position;
        pos.z = Mathf.Clamp(pos.z, zMin, zMax);
        rb.position = pos;
    }
}
