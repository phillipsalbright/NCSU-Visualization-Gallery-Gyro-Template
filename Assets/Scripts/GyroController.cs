using UnityEngine;
using UnityEngine.InputSystem;

sealed class GyroController : MonoBehaviour
{
    #region Static class members
    

    // Layout extension written in JSON
    const string LayoutJson = @"{
      ""name"": ""DualShock4GamepadHIDCustom"",
      ""extend"": ""DualShock4GamepadHID"",
      ""controls"": [
        {""name"":""gyro"", ""format"":""VC3S"", ""offset"":13,
         ""layout"":""Vector3"", ""processors"":""ScaleVector3(x=-1,y=-1,z=1)""},
        {""name"":""gyro/x"", ""format"":""SHRT"", ""offset"":0 },
        {""name"":""gyro/y"", ""format"":""SHRT"", ""offset"":2 },
        {""name"":""gyro/z"", ""format"":""SHRT"", ""offset"":4 },
        {""name"":""accel"", ""format"":""VC3S"", ""offset"":19,
         ""layout"":""Vector3"", ""processors"":""ScaleVector3(x=-1,y=-1,z=1)""},
        {""name"":""accel/x"", ""format"":""SHRT"", ""offset"":0 },
        {""name"":""accel/y"", ""format"":""SHRT"", ""offset"":2 },
        {""name"":""accel/z"", ""format"":""SHRT"", ""offset"":4 }
      ]}";

    // Gyro vector data to rotation conversion
    private Quaternion GyroInputToRotation(in InputAction.CallbackContext ctx)
    {

        // Gyro input data
        var gyro = ctx.ReadValue<Vector3>();

        // Coefficient converting a gyro data value into a degree
        // Note: The actual constant is undocumented and unknown.
        //       I just put a plasible value by guessing.
        const double GyroToAngle = 17.5f * 360 / System.Math.PI;

        // Delta time from the last event
        var dt = ctx.time - ctx.control.device.lastUpdateTime;
        dt = System.Math.Min(dt, 1.0 / 60); // Discarding large deltas

        return Quaternion.Euler(gyro * (float)(GyroToAngle * dt));
    }

    #endregion

    #region Private members
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float maxAngle = 334;
    [SerializeField] private float minAngle = 4;
    private int playerNum;
    private Quaternion controllerRotation;
    private Vector2 stickMovement = Vector2.zero;
    private int schemenum = 0;
    private Quaternion totalStickRotation = Quaternion.identity;

    // Accumulation of gyro input
    Quaternion _accGyro = Quaternion.identity;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        playerNum = playerInput.playerIndex;
        if (playerInput.currentControlScheme == "PS4")
        { 
            // DS4 input layout extension
            InputSystem.RegisterLayoutOverride(LayoutJson);
            playerInput.currentActionMap.Disable();
            if (playerInput.currentActionMap.FindAction("gyro" + playerNum) == null)
            {
                playerInput.currentActionMap.AddAction("gyro" + playerNum, InputActionType.Value, "<Gamepad>/gyro");


            }
            playerInput.currentActionMap.FindAction("gyro" + playerNum).performed += ctx => _accGyro *= this.GyroInputToRotation(ctx);
            playerInput.currentActionMap.Enable();

        }
       
        controllerRotation = transform.localRotation;

    }

    void Update()
    {
        // Rotation from gyroscope
        if (schemenum == 0) //ps4 with bluetooth
        {
            _accGyro.x = _accGyro.y; // this is good
            _accGyro.y = -_accGyro.z; //This is good
            _accGyro.z = 0; // 
        } else if (schemenum == 1)
        {
            //mapping should be normal
        }
        
        Quaternion yQuatController = Quaternion.AngleAxis(_accGyro.eulerAngles.y, Vector3.up);
        Quaternion xQuatController = Quaternion.AngleAxis(_accGyro.eulerAngles.x, Vector3.right);
        controllerRotation = yQuatController * controllerRotation * xQuatController; // use transform.localRotation to not preserve controller rotation past bounds
        _accGyro = Quaternion.identity;
        Quaternion yQuatStick = Quaternion.AngleAxis(stickMovement.x * Time.deltaTime * 100, Vector3.up);
        Quaternion xQuatStick = Quaternion.AngleAxis(stickMovement.y * Time.deltaTime * -100, Vector3.right);
        totalStickRotation = yQuatStick * totalStickRotation * xQuatStick;
        Quaternion xQuatTotal = Quaternion.AngleAxis(controllerRotation.eulerAngles.y + totalStickRotation.eulerAngles.y, Vector3.up);
        Quaternion yQuatTotal = Quaternion.AngleAxis(controllerRotation.eulerAngles.x + totalStickRotation.eulerAngles.x, Vector3.right);
        transform.localRotation = xQuatTotal * Quaternion.identity * yQuatTotal; //Quaternion.Euler(stickRotVec3 + newRotation);
        if (transform.localEulerAngles.x < maxAngle && transform.localEulerAngles.x > minAngle)
        {
            if (Mathf.Abs(transform.localEulerAngles.x - maxAngle) < Mathf.Abs(transform.localEulerAngles.x - minAngle)) 
            {
                transform.localRotation = Quaternion.Euler(maxAngle, transform.localRotation.eulerAngles.y, 0);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(minAngle, transform.localRotation.eulerAngles.y, 0);
            }
        }
        
    }

    #endregion

    public void ResetView(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //transform.localRotation = Quaternion.identity; use this to not preserve controller rotation past boundaries
            controllerRotation = Quaternion.identity;
            totalStickRotation = Quaternion.identity;
        }
    }

    public void MoveStick(InputAction.CallbackContext ctx)
    {
        stickMovement = ctx.ReadValue<Vector2>();
    }

    //Bluetooth DS4 and plugged in seem to act differently, press share to see if one of these two schemes works better
    public void ChangeScheme(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            schemenum = (schemenum + 1) % 2;

        }
    }
}
