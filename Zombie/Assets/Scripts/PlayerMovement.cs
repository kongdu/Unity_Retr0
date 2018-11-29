using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float rotateSpeed = 180f; // 좌우 회전 속도

    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private void Start() {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    // 화면이 깜빡일때마다가 아니라 물리갱신에 따라. 
    // 물리적으로 좀더 세심한 처리할때 사용
    private void FixedUpdate() {
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        //회전 실행
        Rotate();
        
        // 움직임 실행
        Move();

        //애니메이터의 Move파라미터의 값을 move값으로 변경
        //-1부터 1까지
        playerAnimator.SetFloat("Move", playerInput.move);
    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move() {
        //상대적으로 이동할 거리
        //입력값 * 방향 * 속도
        Vector3 moveDistance =  playerInput.move * transform.forward * moveSpeed * Time.deltaTime;

        //리지드바디 컴포넌트를 통해 게임오브젝트 위치를 변경 
        //물리처리가 끼어들어간다. a에서 b로 이동할때 중간에 물체가있으면 못지나가겠지
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance);

        //이건 물리처리를 무시하고 순간이동하는 것과 같다.
        //transform.position = transform.position + moveDistance;
    }

    // 입력값에 따라 캐릭터를 좌우로 회전
    private void Rotate() {
        //상대적으로 회전할 수치 계산
        float turn = playerInput.rotate * rotateSpeed * Time.deltaTime;

        //리지드바디 컴포넌트를 통해 게임오브젝트 회전 변경
        playerRigidbody.MoveRotation(playerRigidbody.rotation * Quaternion.Euler(0f, turn, 0f));
    }
}