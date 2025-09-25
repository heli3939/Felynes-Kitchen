using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovePhysicsSafe : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;          // target max speed
    public float acceleration = 25f;      // how quickly to speed up
    public float deceleration = 15f;      // how quickly to slow down

    [Header("Depth Limits (Z axis)")]
    public float zMin = -3f;              // adjust to match your lane
    public float zMax = 3f;

    private Rigidbody rb;
    private Vector3 input;
    private float startY;                 // lock Y position for now

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;  // enable gravity for physics
        rb.constraints = RigidbodyConstraints.FreezeRotation; // only freeze rotation
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startY = transform.position.y;
    }

    void Update()
    {
        // WASD and space input
        float x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

        input = new Vector3(x, 0f, z);

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        // Jump input (simple)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 6f, ForceMode.VelocityChange);
        }
    }

    void FixedUpdate()
    {
        // Current velocity on X/Z
        Vector3 planarVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Desired velocity (note: use input.z here!)
        Vector3 desired = new Vector3(input.x, 0f, input.z) * moveSpeed;

        // Difference
        Vector3 delta = desired - planarVel;

        // Accel vs decel
        float accel = (input.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        // Force in direction of delta
        Vector3 force = delta.normalized * accel;

        // Clamp so we don’t overshoot
        if (delta.magnitude < force.magnitude * Time.fixedDeltaTime)
            force = delta / Time.fixedDeltaTime;

        rb.AddForce(force, ForceMode.Acceleration);

        // Clamp Z movement
        Vector3 pos = rb.position;
        pos.z = Mathf.Clamp(pos.z, zMin, zMax);
        rb.position = pos;
    }
}
