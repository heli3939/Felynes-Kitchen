using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovePhysicsSafe : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 5f;
    public float deceleration = 10f;
    public float rotateSpeed = 10f;
    public float jumpHeight = 1f;

    [Header("Depth Limits (Z axis)")]
    public float zMin = -3f;
    public float zMax = 3f;

    [Header("Collision")]
    public Collider Capsule;

    [Header("Footstep Sound")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f; 
    private float stepTimer = 0f;

    public LayerMask groundMask = ~0;
    float supportRayDepth = 0.18f;
    int supportSamplesX = 3;
    int supportSamplesZ = 1;
    float minSupportFraction = 0.6f;
    bool requireCenterSupport = true;
    float maxSupportHeightDelta = 0.08f;
    float maxGroundSlope = 55f;

    float fallGravityMultiplier = 2f;
    float lowJumpGravityMultiplier = 4.0f;

    Rigidbody rb;
    Vector3 input;
    bool onGround = true;
    Vector3 lastLookDir;

    private Animator animator;

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

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Input
        float x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float z = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Update grounded state before handle jump
        RefreshGrounded();

        if (animator != null)
        {
            animator.SetBool("isRun", input.sqrMagnitude > 0.01f);
        }

        // Jump (Space)
        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            float g = Mathf.Abs(Physics.gravity.y);
            Vector3 v = rb.linearVelocity;
            v.y = Mathf.Sqrt(2f * g * jumpHeight);
            rb.linearVelocity = v;

            onGround = false;

            if (animator != null)
            {
                animator.SetBool("isJump", true);
            }
        }

        if (onGround && animator != null)
        {
            animator.SetBool("isJump", false);
        }

        // Logic of footstep sound
        if (onGround && input.sqrMagnitude > 0.01f && animator.GetBool("isRun"))
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0 || footstepSource == null) return;

        int index = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[index]);
    }

    void FixedUpdate()
    {
        RefreshGrounded();

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

    // Grounding helper
    void RefreshGrounded()
    {
        if (!Capsule)
        {
            onGround = false;
            return;
        }

        Bounds b = Capsule.bounds;

        Vector3 rayPlaneCenter = b.center + Vector3.up * 0.05f;

        float rayLen = b.extents.y + supportRayDepth + 0.1f;

        int hits = 0;
        int total = (2 * supportSamplesX + 1) * (2 * supportSamplesZ + 1);
        float minHitY = float.MaxValue;
        float maxHitY = float.MinValue;
        bool centerSupported = false;

        for (int ix = -supportSamplesX; ix <= supportSamplesX; ix++)
        {
            for (int iz = -supportSamplesZ; iz <= supportSamplesZ; iz++)
            {
                float tx = (supportSamplesX == 0) ? 0f : (float)ix / (float)supportSamplesX;
                float tz = (supportSamplesZ == 0) ? 0f : (float)iz / (float)supportSamplesZ;

                // spread across capsule footprint
                Vector3 offset = new Vector3(tx * b.extents.x * 0.95f, 0f, tz * b.extents.z * 0.95f);
                Vector3 origin = rayPlaneCenter + offset;

                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLen, groundMask, QueryTriggerInteraction.Ignore))
                {
                    float slope = Vector3.Angle(hit.normal, Vector3.up);
                    if (slope <= maxGroundSlope)
                    {
                        hits++;
                        minHitY = Mathf.Min(minHitY, hit.point.y);
                        maxHitY = Mathf.Max(maxHitY, hit.point.y);
                        if (ix == 0 && iz == 0) centerSupported = true;
                    }
                }
            }
        }

        float supportFrac = (total > 0) ? (float)hits / (float)total : 0f;
        bool heightOk = (maxHitY - minHitY) <= maxSupportHeightDelta;

        bool gridGrounded = (supportFrac >= minSupportFraction) && heightOk && (!requireCenterSupport || centerSupported);

        Vector3 feetStart = b.center + Vector3.up * 0.1f;
        float feetRadius = Mathf.Max(0.05f, Mathf.Min(b.extents.x, b.extents.z) * 0.45f);
        float feetProbe = 0.25f;

        bool feetGrounded = false;
        if (Physics.SphereCast(feetStart, feetRadius, Vector3.down, out RaycastHit footHit, feetProbe, groundMask, QueryTriggerInteraction.Ignore))
        {
            float slope = Vector3.Angle(footHit.normal, Vector3.up);
            if (slope <= maxGroundSlope) feetGrounded = true;
        }
        onGround = gridGrounded || feetGrounded;
    }

}
