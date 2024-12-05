using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Airplane
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlaneController : MonoBehaviour
    {
        [SerializeField] internal string planeName;
        
        [FormerlySerializedAs("Maxspeed")]
        [Header("Speed")]
        [SerializeField] internal float maxSpeed;
        [FormerlySerializedAs("minsSpeed")] [SerializeField] internal float minSpeed;
        [SerializeField][ReadOnly] internal float actualSpeed;
        
        [Header("Directional")]
        [SerializeField] internal float cursorDeadZone = 0.05f;
        [Space]
        [SerializeField] internal float rollSpeed;
        [SerializeField] internal float pitchSpeed;
        [SerializeField] internal float yawSpeed;
        [Space]
        [SerializeField] internal float rollSpeedMouse;
        [SerializeField] internal float pitchSpeedMouse;
        [SerializeField] internal float yawSpeedMouse;
        
        [Header("External effect")]
        [SerializeField] internal float speedCoeffPrecision;
        [SerializeField] internal AnimationCurve heightSpeedThrottle;
        [FormerlySerializedAs("heightLiftThrottle")] [SerializeField] internal AnimationCurve heightSpeedDecelerate;
        
        [Space] 
        [SerializeField] internal float liftCoefficient;
        [SerializeField][ReadOnly] internal float actualLiftCoefficient;
        [SerializeField] internal float dragCoefficient;

        internal Rigidbody _rb;
        internal float pitch = 0;
        internal float roll = 0;
        internal float yaw = 0;

        internal Vector2 mousePos;
        private Camera _camera;
        private float screenWidth;
        private float screenHeight;
        private float ratio;

        [Header("Show Debug")] [SerializeField]
        private bool showDebug;
        
        internal void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        internal void Start()
        {
            _camera = Camera.main;
            screenHeight = Screen.height;
            screenWidth = Screen.width;
            ratio = screenWidth / screenHeight;
        }

        internal void Update()
        {
            RotatePlane();
            GetAcceleratorInput();

            if (boostDuration > 0)
            {
                boostDuration -= Time.deltaTime;
            }
            else
            {
                boostSpeedModificator = 0;
            }
            
            mousePos = Input.mousePosition;
        }

        #region Controller

        private float rollCoef;
        private float pitchCoef;
        private float yawCoef;
        internal void RotatePlane()
        {
            roll = 0;
            pitch = 0;
            yaw = 0;

            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2) + new Vector2(0,-.5f);
            Vector2 mouseDist = (mousePos - screenCenter); // distance to the screen center 
            Vector2 mouseDir = mouseDist / new Vector2(screenWidth, screenHeight);
            
            if (mouseDir.magnitude > cursorDeadZone)
            {
                var sensitiveControl = (mouseDir.magnitude - cursorDeadZone) / (1 - cursorDeadZone); 
                mouseDir = mouseDir.normalized * (sensitiveControl);
                
                rollCoef = Mathf.Clamp(mouseDir.x, -1f, 1f); // clamp for both direction
                roll = rollCoef * rollSpeedMouse * Time.deltaTime;
                
                pitchCoef = Mathf.Clamp(-mouseDir.y, -1f, 1f); 
                pitch = pitchCoef * pitchSpeedMouse * Time.deltaTime;
                
                yawCoef = Mathf.Clamp(mouseDir.x, -1f, 1f); 
                yaw += yawCoef * yawSpeedMouse * Time.deltaTime;
            }
            
            // Keyboard has authority over mouse
            if (GetInput.Instance.GetIsMovement()) 
            {
                roll = GetInput.Instance.GetMovementVector().x * rollSpeed * Time.deltaTime;
                pitch = GetInput.Instance.GetMovementVector().y * pitchSpeed * Time.deltaTime;

                if (Input.GetKey(KeyCode.Q))
                {
                    yaw -= yawSpeed * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    yaw += yawSpeed * Time.deltaTime;
                }
            }
            
            transform.Rotate(pitch, yaw, -roll);
        }

        private float acceleratorFactor = 0;
        private void GetAcceleratorInput()
        {
            acceleratorFactor += Time.deltaTime * GetInput.Instance.GetAccelerating();
            acceleratorFactor = Mathf.Lerp(0, 1, acceleratorFactor);
        }
        internal float GetAcceleratorFactor()
        {
            float accelerator = Mathf.Lerp(minSpeed, maxSpeed, acceleratorFactor);
            return accelerator;
        } 
        
        internal float speedCoeff = 0;
        internal float SpeedCalculator()
        {
            if (Mathf.Abs(Quaternion.Dot(this.transform.rotation, Quaternion.Euler(-90,transform.rotation.y,transform.rotation.z))) > speedCoeffPrecision) // check if plane is going up
            {
                if(speedCoeff > -maxSpeed - 20) speedCoeff -= heightSpeedThrottle.Evaluate(actualSpeed) * Time.deltaTime;
            }
            else if(speedCoeff < 0) speedCoeff += heightSpeedDecelerate.Evaluate(actualSpeed) * Time.deltaTime;
            
            actualSpeed = GetAcceleratorFactor() + speedCoeff + boostSpeedModificator; 

            return actualSpeed;
        }
        
        internal float liftCoeff = 0;
        internal Vector3 liftCalculator()
        {
            if (Mathf.Abs(Quaternion.Dot(this.transform.rotation, Quaternion.Euler(-90,0,0))) > speedCoeffPrecision)
            {
                liftCoeff -= heightSpeedDecelerate.Evaluate(actualSpeed) * Time.deltaTime;
            }
            else liftCoeff += heightSpeedDecelerate.Evaluate(actualSpeed) * Time.deltaTime;
            
            actualLiftCoefficient = liftCoefficient; // + liftCoeff
                
            Vector3 liftDir = Vector3.up;
            float liftMagnitude = _rb.linearVelocity.magnitude * actualLiftCoefficient; //* _rb.velocity.magnitude
            Vector3 lift = liftDir * liftMagnitude;

            return lift;
        }

        #endregion

        #region Aim

        internal Vector2 GetAimPosVector2()
        {
            Vector3 pos = (transform.forward * 1500) + transform.position;
            Vector3 screenPos = _camera.WorldToScreenPoint((pos));
            
            return screenPos;
        }

        #endregion
        
        internal void FixedUpdate()
        {
            _rb.AddForce((transform.forward * SpeedCalculator()));
            _rb.AddForce((liftCalculator()));
            _rb.linearDamping = dragCoefficient;
        }

        private float boostDuration;
        private float boostSpeedModificator;
        internal void BoostSpeed(float duration, float speedModificator)
        {
            boostDuration = duration;
            boostSpeedModificator = speedModificator;
        }
        
        
        #if UNITY_EDITOR

        internal void OnDrawGizmos()
        {
            if(!showDebug) return;
            DrawLift();
            DrawGroundDir();
            Gizmos.DrawSphere(mousePos, .1f);

            Gizmos.DrawRay(transform.position, transform.forward * 500);
        }

        private void DrawLift()
        {
            if(!Application.isPlaying) return;
            Handles.color = Color.green;
            Handles.DrawLine(transform.position, transform.position + liftCalculator(), 2);
        }

        private void DrawGroundDir()
        {
            if(!Application.isPlaying) return;
            Handles.color = new Color32(138, 43, 226, 255);
            Handles.DrawLine(transform.position, transform.position + Vector3.down * 3, 2);
        }
        
        void OnGUI()
        {
            if(!showDebug) return;
            DisplaySpeed();
            DisplayMousePos();

            GUI.Label(new Rect(10, 60, 500, 20), $"{transform.rotation.eulerAngles.x}");
        }
        
        private void DisplaySpeed()
        {
            if(!Application.isPlaying) return;
            GUI.Label(new Rect(10, 10, 500, 20), $"plane speed {SpeedCalculator()}");
            GUI.Label(new Rect(10, 30, 500, 20), $"plane magnitude {_rb.linearVelocity.magnitude}");
        }
        
        private void DisplayMousePos()
        {
            if(!Application.isPlaying) return;
            GUI.Label(new Rect(10, 100, 500, 20), $"Mouse Pos {mousePos}");
            GUI.Label(new Rect(10, 120, 500, 20), $"Mouse Pos {ratio}");
        }
#endif
    }
}