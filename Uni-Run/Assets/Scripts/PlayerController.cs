using UnityEngine;

// PlayerController는 플레이어 캐릭터로서 Player 게임 오브젝트를 제어한다.
public class PlayerController : MonoBehaviour
{
    public AudioClip deathClip; // 사망시 재생할 오디오 클립 //카세트테이프
    public float jumpForce = 700f; // 점프 힘

    private int jumpCount = 0; //누적된 점프횟수
    private bool isGrounded = false; // 바닥에 닿았는지 나타냄
    private bool isDead = false; // 사망 상태

    private Rigidbody2D playerRigidbody; // 사용할 리지드바디 컴포넌트
    private Animator animator; // 사용할 애니메이터 컴포넌트
    private AudioSource playerAudio; // 사용할 오디오 소스 컴포넌트

    private void Start()
    {
        // 초기화
        //게임 오브젝트로부터 사용할 컴포넌트들을 가져와 변수에 할당
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
    }


    private void Update()
    {
        // 사용자 입력을 감지하고 점프하는 처리
        if (isDead)
        {

            //사망시 하단의 처리로 이동하지않고 즉시 종료
            return;
        }
        //바닥에 닿은 상태에서 마우스 왼쪽 버튼을 눌렀다면
        if(Input.GetMouseButtonDown(0) && jumpCount < 3) //0:마우스 왼쪽 1:오른쪽
        {
            jumpCount++; //점프횟수 1증가
            //순간적으로 속도를 제로로 만들기 >>이걸하지않으면 관성이 생겨서 훨씬 높이 점프
            playerRigidbody.velocity = Vector2.zero;
            //Vector2 jumpVector = new Vector2(0, jumpForce);
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            //오디오소스 소리를 재생
            playerAudio.Play();
        }else if(Input.GetMouseButtonUp(0) && playerRigidbody.velocity.y > 0)
        {
            //마우스 왼쪽 버튼에서 손을 떼는 순간 && 속도 y가 양수(위로 상승중)
            //속도를 반토막
            playerRigidbody.velocity = playerRigidbody.velocity * 0.5f;
        }
        //애니메이터의 Grounded 파라미터에 isGrounded값을 덮어쓰기
        animator.SetBool("Grounded", isGrounded); //애니메이터 창에서 설정한 값과 똑같이
    }

    private void Die()
    {
        // 사망 처리
        //애니메이터의 Die 트리거 파라미터를 셋
        animator.SetTrigger("Die");
        //오디오 소스에 할당된 오디오 클립을 deathClip으로 변경
        playerAudio.clip = deathClip;
        //오디오 소스 컴포넌트 재생
        playerAudio.Play();

        //속도를 제로(0, 0)로 변경 new Vector2(0,0)
        playerRigidbody.velocity = Vector2.zero;
        //사망 상태를 true로 변경
        isDead = true;

        //게임 오버 처리 실행
        GameManager.instance.OnPlayerDead();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 트리거 콜라이더를 가진 장애물과의 충돌을 감지
        // 트리거 충돌한 상대방이 Dead 태그를 가짐 && 아직 안죽음
        if (other.tag == "Dead" && !isDead) //&& !isDead를 추가해야함. 죽었는데 또 죽는 에러를 막기위해
        {
            //사망처리실행
            Die();
        }
    }
    /*
    Enter 충돌한 시점
    Exit 떼어지는 순간
    */

    private void OnCollisionEnter2D(Collision2D other) //두 물체가 충돌한상태에서 떼어졌을때 실행됨
    {
        //충돌한 표면의 방향이 위쪽이어야 바닥이다
        if(other.contacts[0].normal.y > 0.5) //표면의 방향을 알려주는 애가 normal //충돌한 지점들의 정보가 포함된다
        {
            jumpCount = 0;
            // 바닥에 닿았음을 감지하는 처리
            isGrounded = true;
        }

    }
    //OnTriggerExit:떨어지는 순간, OnTriggerStay :붙기 시작할때 , OnCollisionStay :붙어있는 동안

    private void OnCollisionExit2D(Collision2D other)
    {
 
        // 바닥에서 벗어났음을 감지하는 처리
        isGrounded = false;
    }
}