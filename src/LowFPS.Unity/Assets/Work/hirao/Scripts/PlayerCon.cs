using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCon : MonoBehaviour
{
    [Header("References")]
    public Transform cameraRoot;
    public Camera playerCamera;
    public Transform gunPivot;

    // アイアンサイト位置
    private Transform adsPoint;

    [Header("Mouse")]
    public float sensitivity = 2f;
    public float adsSensitivity = 0.7f;
    public float maxLookAngle = 85f;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 5f;
    public float airControl = 0.4f;

    [Header("Ground Check")]
    public float groundDistance = 1.1f;
    public LayerMask groundLayer;

    [Header("Gun Lag")]
    public float gunMaxOffset = 10f;
    public float gunLagSpeed = 18f;
    public float gunReturnSpeed = 6f;

    [Header("ADS")]
    public float adsFOV = 50f;
    public float adsSpeed = 12f;
    public float adsCameraSpeed = 18f;
    public float adsRotationSpeed = 18f;
    public float adsGunLagMultiplier = 0.15f;
    public Vector3 adsPosition = new Vector3(0f, -0.04f, 0.12f);

    [Header("Gun Movement")]
    public float moveTiltAmount = 2f;

    [Header("Lean")]
    public float leanAngle = 12f;
    public float leanOffset = 0.15f;
    public float leanSpeed = 10f;

    [Header("Head Bob")]
    public float bobSpeed = 8f;
    public float bobAmount = 0.04f;

    [Header("FOV")]
    public float walkFOV = 75f;
    public float sprintFOV = 82f;
    public float fovSmooth = 6f;

    private Rigidbody rb;

    // RECOIL
    [Header("Recoil")]
    public float recoilReturnSpeed = 8f;
    public float recoilSnappiness = 18f;

    // DEATH / SPECTATE
    public bool isDead = false;
    private Transform spectateTarget;

    [Header("Spectate")]
    public Vector3 spectateOffset = new Vector3(0, 8f, -4f);
    public float spectateFollowSpeed = 8f;

    private float currentRecoilX;
    private float targetRecoilX;

    private float currentRecoilY;
    private float targetRecoilY;

    // LOOK
    private float yaw;
    private float pitch;

    // GUN LAG
    private float gunYaw;
    private float gunPitch;

    // LEAN
    private float currentLean;
    private int leanState = 0;

    // GROUND
    private bool grounded;

    // CAMERA
    private Vector3 camOriginPos;

    // GUN
    private Quaternion gunOriginRot;
    private Vector3 gunOriginPos;

    // CAMERA ORIGINAL
    private Vector3 cameraOriginLocalPos;
    private Quaternion cameraOriginLocalRot;

    // HEAD BOB
    private float bobTimer;

    // ADS
    private bool aiming;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        camOriginPos = cameraRoot.localPosition;

        if (gunPivot != null)
        {
            gunOriginRot = gunPivot.localRotation;
            gunOriginPos = gunPivot.localPosition;
        }

        Transform found = gunPivot.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.ToLower().Contains("adspoint"));

        if (found != null)
        {
            adsPoint = found;
        }
        else
        {
            Debug.LogWarning("ADSPoint not found.\nCreate child object named 'ADSPoint'.");
        }

        cameraOriginLocalPos = playerCamera.transform.localPosition;
        cameraOriginLocalRot = playerCamera.transform.localRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isDead)
        {
            SpectateUpdate();
            return;
        }

        Look();
        Lean();
        ADS();
        Jump();
        HeadBob();
        UpdateFOV();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        GroundCheck();
        Move();
    }

    // =====================================
    // LOOK
    // =====================================

    void Look()
    {
        float currentSensitivity = aiming ? adsSensitivity : sensitivity;

        float mouseX = Input.GetAxisRaw("Mouse X") * currentSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * currentSensitivity;

        yaw += mouseX;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        currentRecoilX = Mathf.Lerp(currentRecoilX, targetRecoilX, Time.deltaTime * recoilSnappiness);

        currentRecoilY = Mathf.Lerp(currentRecoilY, targetRecoilY, Time.deltaTime * recoilSnappiness);

        targetRecoilX = Mathf.Lerp(targetRecoilX, 0f, Time.deltaTime * recoilReturnSpeed);

        targetRecoilY = Mathf.Lerp(targetRecoilY, 0f, Time.deltaTime * recoilReturnSpeed);

        Quaternion camRot = Quaternion.Euler(pitch - currentRecoilX, currentRecoilY, currentLean);

        cameraRoot.localRotation = Quaternion.Slerp(cameraRoot.localRotation, camRot, Time.deltaTime * 20f);

        gunYaw += mouseX;
        gunPitch -= mouseY;

        gunYaw = Mathf.Clamp(gunYaw, -gunMaxOffset, gunMaxOffset);
        gunPitch = Mathf.Clamp(gunPitch, -gunMaxOffset, gunMaxOffset);

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        float tiltX = -moveZ * moveTiltAmount;
        float tiltZ = -moveX * moveTiltAmount;

        Quaternion targetGunRot = gunOriginRot * Quaternion.Euler(gunPitch + tiltX, gunYaw, -gunYaw * 0.5f + tiltZ);

        float lagSpeed = aiming ? gunLagSpeed * adsGunLagMultiplier : gunLagSpeed;

        gunPivot.localRotation = Quaternion.Slerp(gunPivot.localRotation, targetGunRot, Time.deltaTime * lagSpeed);

        gunYaw = Mathf.Lerp(gunYaw, 0f, Time.deltaTime * gunReturnSpeed);
        gunPitch = Mathf.Lerp(gunPitch, 0f, Time.deltaTime * gunReturnSpeed);
    }

    // =====================================
    // MOVE
    // =====================================

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;

        bool sprinting = Input.GetKey(KeyCode.LeftShift) && !aiming;

        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        Vector3 targetVelocity = moveDir * speed;

        Vector3 vel = rb.linearVelocity;

        float control = grounded ? 1f : airControl;

        vel.x = Mathf.Lerp(vel.x, targetVelocity.x, control);
        vel.z = Mathf.Lerp(vel.z, targetVelocity.z, control);

        rb.linearVelocity = vel;
    }

    // =====================================
    // JUMP
    // =====================================

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // =====================================
    // ADS
    // =====================================

    void ADS()
    {
        aiming = Input.GetMouseButton(1);

        if (!aiming || adsPoint == null)
        {
            gunPivot.localPosition = Vector3.Lerp(gunPivot.localPosition, gunOriginPos, Time.deltaTime * adsSpeed);
            return;
        }

        Quaternion rotationOffset = playerCamera.transform.rotation * Quaternion.Inverse(adsPoint.rotation);
        Quaternion targetRot = rotationOffset * gunPivot.rotation;
        Vector3 worldOffset = playerCamera.transform.position - adsPoint.position;
        Vector3 targetPos = gunPivot.localPosition + gunPivot.parent.InverseTransformVector(worldOffset);
        gunPivot.localPosition = Vector3.Lerp( gunPivot.localPosition,targetPos, Time.deltaTime * adsSpeed);
        gunPivot.rotation = Quaternion.Slerp(gunPivot.rotation, targetRot, Time.deltaTime * adsRotationSpeed);
    }

    // =====================================
    // LEAN
    // =====================================

    void Lean()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (leanState == -1) leanState = 0;
            else leanState = -1;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (leanState == 1) leanState = 0;
            else leanState = 1;
        }

        float targetLean = 0f;
        float targetOffset = 0f;

        switch (leanState)
        {
            case -1:
                targetLean = leanAngle;
                targetOffset = -leanOffset;
                break;

            case 1:
                targetLean = -leanAngle;
                targetOffset = leanOffset;
                break;
        }

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        Vector3 targetPos = camOriginPos + new Vector3(targetOffset, 0f, 0f);

        cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetPos, Time.deltaTime * leanSpeed);
    }

    // =====================================
    // HEAD BOB
    // =====================================

    void HeadBob()
    {
        if (!grounded) return;
        if (aiming) return;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVel.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            float bob = Mathf.Sin(bobTimer) * bobAmount;

            Vector3 pos = cameraRoot.localPosition;

            pos.y = camOriginPos.y + bob;

            cameraRoot.localPosition = pos;
        }
        else
        {
            bobTimer = 0f;

            Vector3 pos = cameraRoot.localPosition;

            pos.y = Mathf.Lerp(pos.y, camOriginPos.y, Time.deltaTime * 8f);

            cameraRoot.localPosition = pos;
        }
    }

    // =====================================
    // FOV
    // =====================================

    void UpdateFOV()
    {
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && !aiming;

        float targetFOV;

        if (aiming)
        {
            targetFOV = adsFOV;
        }
        else
        {
            targetFOV = sprinting ? sprintFOV : walkFOV;
        }

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
    }

    // =====================================
    // GROUND CHECK
    // =====================================

    void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, groundDistance, groundLayer);

#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Vector3.down * groundDistance, grounded ? Color.green : Color.red);
#endif
    }
    public void AddRecoil(float recoilAmount)
    {
        targetRecoilX += recoilAmount;

        targetRecoilY += Random.Range(
            -recoilAmount * 0.2f,
             recoilAmount * 0.2f
        );
    }

    private void SpectateUpdate()
    {
        if (spectateTarget == null)
            return;

        Vector3 targetPos =
            spectateTarget.position + spectateOffset;

        playerCamera.transform.position =
            Vector3.Lerp(
                playerCamera.transform.position,
                targetPos,
                Time.deltaTime * spectateFollowSpeed);

        playerCamera.transform.LookAt(
            spectateTarget.position + Vector3.up * 1.5f);
    }

    public void Dead(System.Guid killerConnectionId)
    {
        Debug.Log($"killerConnectionId = {killerConnectionId}");

        isDead = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, -10, this.gameObject.transform.position.z);
        spectateTarget = InRoomPlayerData.I.PlayerList[killerConnectionId].playerObj.transform;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn(Vector3 spawnPos)
    {
        isDead = false;

        spectateTarget = null;
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = spawnPos;

        pitch = 0f;
        yaw = transform.eulerAngles.y;

        currentRecoilX = 0f;
        currentRecoilY = 0f;

        targetRecoilX = 0f;
        targetRecoilY = 0f;

        cameraRoot.localPosition = camOriginPos;
        cameraRoot.localRotation = Quaternion.identity;

        playerCamera.transform.localPosition =
            cameraOriginLocalPos;

        playerCamera.transform.localRotation =
            cameraOriginLocalRot;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}