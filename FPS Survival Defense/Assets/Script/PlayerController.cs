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

        //초기화
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

    //앉기 시도
    private void TryCrouch()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    //앉기
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
            //일어날때
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());
    }

    //부드러운 앉기 동작
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;


        //자연스럽게 앉게하도록
        while(_posY != applyCrouchPosY)
        {
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0); //상대적인위치
            //0과 1 딱 떨어지게 나오지 않기 때문에 15번만 해보고 걍 바로 도달하게끔
            if(count > 15)
            {
                break;
            }
            yield return null; //null은 한 프레임 대기. 한프레임마다 반복.
            theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
        }
    }

    //지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    //점프시도
    private void TryJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    //점프
    private void Jump()
    {
        //앉은 상태에서 점프 시 앉은 상태 해제
        if(isCrouch)
            Crouch();

        myRigid.velocity = transform.up * jumpForce;
    }

    //달리기 시도
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

    //달리기 실행
    private void Running()
    {
        //앉은 상태에서 달리기 할때 앉은 상태 해제
        if(isCrouch)
            Crouch();
        isRun = true;
        applySpeed = runSpeed;
    }

    //달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    //움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    //상하 카메라 회전    
    private void CameraRotation()
    {
        //상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //마우스는 X와 Y가 있으니 그리고 유니티는 3차원, 위아래 고개 드는거
        float _cameraRotationX = _xRotation * lookSensitivity; //마우스를 살짝 올렸다고 순식간에 올라가면 안되니까 sensitivity로 어느정도 천천히 움직이게끔
        currentCameraRotationX -= _cameraRotationX; //여기서 +는 흔히 FPS 옵션에 있는 마우스 y 반전과 관련있다.
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); //limit만큼 가두기

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); //위 아래 움직이기
    }

    //좌우 캐릭터 회전
    private void CharacterRotation()
    {
        //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //vector값을 quaternion으로 바꿔주기
    }

}