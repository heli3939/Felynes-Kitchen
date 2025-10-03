using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovePhysicsSafe : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 5f;
    public float deceleration = 10f;
    public float rotateSpeed = 10f;
    public float jumpHeight = 1f; // meters

    [Header("Depth Limits (Z axis)")]
    public float zMin = -3f;
    public float zMax = 3f;

    [Header("Collision")]
    public Collider Capsule; // solid capsule on the root

    [Header("Grounding / Anti-Edge-Hang")]
    public LayerMask groundMask = ~0;
    public float supportRaySkin = 0.02f;
    public float supportRayDepth = 0.18f;
    [Range(1, 7)] public int supportSamplesX = 3;     // L, C, R
    [Range(1, 5)] public int supportSamplesZ = 1;     // 1 for 2.5D
    [Range(0f, 1f)] public float minSupportFraction = 0.6f;
    public bool requireCenterSupport = true;
    public float maxSupportHeightDelta = 0.08f;
    public float maxGroundSlope = 55f;

    [Header("Jump Feel")]
    public float fallGravityMultiplier = 1.5f;
    public float lowJumpGravityMultiplier = 2.0f;

    Rigidbody rb;
    Vector3 input;
    bool onGround = true;
    Vector3 lastLookDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!Capsule) Capsule = GetComponent<CapsuleCollider>();
        if (Capsule) Capsule.isTrigger = false;

        lastLookDir = transform.forward;
    }

    void Update()
    {
        // WASD input
        float x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Jump (Space) — sets upward velocity for desired height
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float g = Mathf.Abs(Physics.gravity.y);
            Vector3 v = rb.linearVelocity;
            v.y = Mathf.Sqrt(2f * g * jumpHeight);
            rb.linearVelocity = v;
        }
    }

    void FixedUpdate()
    {
        // Grounded (strict, anti-edge-hang)
        onGround = IsWellSupported();

        // Movement (planar accel/decel)
        Vector3 planarVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 desired = input * moveSpeed;
        Vector3 delta = desired - planarVel;
        float accel = (input.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        Vector3 force = delta.sqrMagnitude > 0.0001f ? delta.normalized * accel : Vector3.zero;
        float maxStep = accel * Time.fixedDeltaTime;
        if (delta.magnitude < maxStep) force = delta / Time.fixedDeltaTime;
        rb.AddForce(force, ForceMode.Acceleration);

        // Rotation
        if (input.sqrMagnitude > 0.01f)
        {
            lastLookDir = input;
            Quaternion targetRot = Quaternion.LookRotation(lastLookDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }

        // Z-lane clamp
        Vector3 pos = rb.position;
        pos.z = Mathf.Clamp(pos.z, zMin, zMax);
        rb.position = pos;

        // Better gravity feel
        if (!onGround)
        {
            if (rb.linearVelocity.y < 0f)
                rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
            else if (rb.linearVelocity.y > 0f && !Input.GetKey(KeyCode.Space))
                rb.AddForce(Physics.gravity * (lowJumpGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    // Strict support: grid of downward rays across capsule bottom
    bool IsWellSupported()
    {
        if (!Capsule) return false;

        Bounds b = Capsule.bounds; // world-space
        float y = b.min.y + supportRaySkin;

        float halfX = Mathf.Max(b.extents.x - 0.005f, 0.001f);
        float halfZ = Mathf.Max(b.extents.z - 0.005f, 0.001f);

        int sx = Mathf.Max(supportSamplesX, 1);
        int sz = Mathf.Max(supportSamplesZ, 1);
        int total = sx * sz;

        int hits = 0;
        bool centerHit = false;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        int cx = sx / 2;
        int cz = sz / 2;

        float stepX = (sx == 1) ? 0f : (2f * halfX) / (sx - 1);
        float stepZ = (sz == 1) ? 0f : (2f * halfZ) / (sz - 1);

        float maxSlopeDot = Mathf.Cos(maxGroundSlope * Mathf.Deg2Rad);
        int mask = (groundMask == 0) ? ~0 : groundMask.value;

        for (int ix = 0; ix < sx; ix++)
        for (int iz = 0; iz < sz; iz++)
        {
            float offX = -halfX + ix * stepX;
            float offZ = -halfZ + iz * stepZ;

            Vector3 origin = new Vector3(b.center.x + offX, y, b.center.z + offZ);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, supportRayDepth, mask, QueryTriggerInteraction.Ignore))
            {
                float upDot = Vector3.Dot(hit.normal, Vector3.up);
                if (upDot >= maxSlopeDot)
                {
                    hits++;
                    minY = Mathf.Min(minY, hit.point.y);
                    maxY = Mathf.Max(maxY, hit.point.y);
                    if (ix == cx && iz == cz) centerHit = true;

                    Debug.DrawRay(origin, Vector3.down * hit.distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(origin, Vector3.down * supportRayDepth, Color.yellow);
                }
            }
            else
            {
                Debug.DrawRay(origin, Vector3.down * supportRayDepth, Color.red);
            }
        }

        float frac = (total > 0) ? (hits / (float)total) : 0f;
        float heightSpread = (hits > 0) ? (maxY - minY) : float.MaxValue;

        return frac >= minSupportFraction
            && (!requireCenterSupport || centerHit)
            && heightSpread <= maxSupportHeightDelta;
    }
}
