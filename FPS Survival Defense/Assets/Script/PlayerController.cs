using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid; //실제 육체적인 몸

    [SerializeField]
    private float lookSensitivity; //민감도

    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //정면을 바라보게

    void Start()
    {
        myRigid = GetComponent<Rigidbody>();
        //FindObjectOfType : Hierarchy에 있는 객체 찾기로 할 수도 있다.
    }


    void Update()
    {
        Move();
        CameraRotation();
        CharacterRotation();
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * walkSpeed;

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