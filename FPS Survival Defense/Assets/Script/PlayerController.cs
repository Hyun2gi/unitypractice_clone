using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;


    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    //상태변수
    private bool isRun = false;
    private bool isGround = true; //true만 점프할 수 있게. 공중에서 또 다시 점프 못하게
    private bool isCrouch = false;

    //앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY; //숙이는건 Y가
    private float originPosY; //숙였다가 원래로 돌아가야하는데 원래의 첫 높이
    private float applyCrouchPosY; //crouchPosY와 originPosY를 넣어줄 것이다.


    //땅 착지 여부
    private CapsuleCollider capsuleCollider;

    //민감도
    [SerializeField]
    private float lookSensitivity; 
    
    //카메라한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //정면을 바라보게

    //필요한 컴포넌트
    [SerializeField]
    private Camera theCamera; //FindObjectOfType : Hierarchy에 있는 객체 찾기로 할 수도 있다. 를 START에 넣어주는 방법도 있다


    private Rigidbody myRigid; //실제 육체적인 몸


    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed; //달리기 전엔 무조건 걷는 상태니까

        originPosY = theCamera.transform.localPosition.y; //상대적인거 기준으로 해야하기 때문에
        applyCrouchPosY = originPosY;
    }


    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }

    private void TryCrouch()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouch = !isCrouch;

        if(isCrouch)
        {
            //앉았을때
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY; 
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x,applyCrouchPosY,theCamera.transform.localPosition.z);
    }

    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;

        while(_posY != applyCrouchPosY)
        {
            _posY = Mathf.Lerp();
        }
        yield return new WaitForSeconds(1f);
    }

    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }


    private void TryJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    private void Jump()
    {
        myRigid.velocity = transform.up * jumpForce;
    }

    private void TryRun() //무주건 Move 위에 있어야함. 달리는지 안달리는지 확인해야하기 때문
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel(); //떼는 순간
        }
    }

    private void Running()
    {
        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    
    private void CameraRotation()
    {
        //상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //마우스는 X와 Y가 있으니 그리고 유니티는 3차원, 위아래 고개 드는거
        float _cameraRotationX = _xRotation * lookSensitivity; //마우스를 살짝 올렸다고 순식간에 올라가면 안되니까 sensitivity로 어느정도 천천히 움직이게끔
        currentCameraRotationX -= _cameraRotationX; //여기서 +는 흔히 FPS 옵션에 있는 마우스 y 반전과 관련있다.
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); //limit만큼 가두기

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); //위 아래 움직이기
    }

    private void CharacterRotation()
    {
        //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //vector값을 quaternion으로 바꿔주기
    }

}