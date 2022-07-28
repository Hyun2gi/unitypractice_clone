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
    private float cmaeraRotationLimit;
    private float currentCameraRotationX = 0; //정면을 바라보게

    void Start()
    {
        myRigid = GetComponent<Rigidbody>();
    }


    void Update()
    {
        Move();
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
}
