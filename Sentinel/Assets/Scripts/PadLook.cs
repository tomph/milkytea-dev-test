using UnityEngine;
using UnityEngine.VR;

public class PadLook : MonoBehaviour {

    public bool lockCursor = true;
    public bool clampVerticalRotation = true;
    public bool smooth = true;
    public float smoothTime = 5.0f;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public Vector2 sensitivity = new Vector2(2f, 2f);

    private Transform m_characterTransform = null;
    private Transform m_cameraTransform = null;
    private Quaternion m_cameraTargetRot;
    private bool m_cursorIsLocked = true;

    void Awake()
    {
        m_characterTransform = transform;
        m_cameraTransform = m_characterTransform.GetChild(0);

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;

        m_cameraTargetRot = m_cameraTransform.localRotation;
    }

    //look at a point instantly without any slerps (teleport)
    public void LookAt(Vector3 point)
    {
        Vector3 targetDir = point - m_characterTransform.position;
        float angle = m_characterTransform.localRotation.eulerAngles.y + Vector3.Angle(targetDir, m_characterTransform.forward);
        m_characterTransform.localRotation = Quaternion.Euler(0f, angle, 0f);

        //targetDir = point - m_cameraTransform.position;
        //m_cameraTransform.localRotation = Quaternion.Euler(Vector3.Angle(targetDir, m_cameraTransform.right), 0f, 0f);
        //m_cameraTargetRot = m_cameraTransform.localRotation;
    }
	
	// Update is called once per frame
	void Update () {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon)
            return;

        //dont use mouse look in VR
        if (UnityEngine.XR.XRDevice.isPresent)
            return;

        UpdateCursorLock();

        if (!m_cursorIsLocked)
            return;

        float yRot = Input.GetAxis("Horizontal") * sensitivity.y;
        float xRot = Input.GetAxis("Vertical") * sensitivity.x;

        //m_characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
       // m_cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
        m_cameraTargetRot *= Quaternion.Euler(-xRot, yRot, 0f);

        if (clampVerticalRotation)
            m_cameraTargetRot = ClampRotationAroundXAxis(m_cameraTargetRot);

        if (smooth)
        {
            //m_characterTransform.localRotation = Quaternion.Slerp(m_characterTransform.localRotation, m_characterTargetRot, smoothTime * Time.deltaTime);
            m_cameraTransform.localRotation = Quaternion.Slerp(m_cameraTransform.localRotation, m_cameraTargetRot, smoothTime * Time.deltaTime);
        }
        else
        {
            //m_characterTransform.localRotation = m_characterTargetRot;
            m_cameraTransform.localRotation = m_cameraTargetRot;
        }

        
    }

    public void UpdateCursorLock()
    {
        //if the user set "lockCursor" we check & properly lock the cursos
        if (lockCursor)
            InternalLockUpdate();
    }

    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_cursorIsLocked = true;
        }

        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
