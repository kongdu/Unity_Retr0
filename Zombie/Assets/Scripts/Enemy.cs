using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드를 가져오기

// 적 AI를 구현한다
public class Enemy : LivingEntity {
    //주변을 검색하면서 자기자신의 추적대상을 찾는다

    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적할 대상
    private NavMeshAgent pathFinder; // 경로계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격시 재생할 파티클 효과
    public AudioClip deathSound; // 사망시 재생할 소리
    public AudioClip hitSound; // 피격시 재생할 소리

    private Animator enemyAnimator; // 애니메이터 컴포넌트
    private AudioSource enemyAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer enemyRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    //프로퍼티: 변수인척하는 함수  (겉은 변수 내부적으로는 함수)
    //get만 있어 >> 읽을순있는데 쓸 수 없다
    //변수지만 값이 자동으로 변경되는 변수
    private bool hasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake() {
        // 초기화
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAudioPlayer = GetComponent<AudioSource>();
        enemyAnimator = GetComponent<Animator>();

        //렌더러 컴포넌트는 자식에 있으므로 GetComponentInChindren사용
        enemyRenderer = GetComponentInChildren<Renderer>();
    }

    // 적 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newSpeed, Color skinColor) {
        //체력 설정
        startingHealth = newHealth;
        health = newHealth;

        //공격력 설정
        damage = newDamage;

        //내비메시 에이전트의 이동속도 변경
        pathFinder.speed = newSpeed;

        //랜더러의 컬러 색을 변경
        enemyRenderer.material.color = skinColor;
    }

    private void Start() {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션을 재생
        enemyAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로를 갱신
    private IEnumerator UpdatePath() {
        // 살아있는 동안 무한 루프
        while (!dead)
        {
            if (hasTarget)
            {
                //추적 대상이 존재: 경로를 갱신하고 이동을 계속 진행
                pathFinder.isStopped = false;
                pathFinder.SetDestination(targetEntity.transform.position);


            }
            else
            {
                //추적 대상이 없음: AI이동을 중지하고, 새로운 추적대상 찾기

                // 20 미터(유닛)의 반지름을 가진 가상의 구를 그렸을때, 
                //구와 겹치는 모든 콜라이더를 가져옴
                //단, 성능을 위해 whatIsTarget에 포함되는 레이어만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                //찾아온 모든 콜라이더를 순회하면서 살아있는 LivingEntity찾기
                for(int i = 0; i<colliders.Length; i++)
                {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    if(livingEntity != null && !livingEntity.dead)
                    {
                        targetEntity = livingEntity;
                        pathFinder.isStopped = false;
                    }
                }

            }

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) {
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
        hitEffect.transform.position = hitPoint;
        hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
        hitEffect.Play();

        enemyAudioPlayer.PlayOneShot(hitSound);
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        //다른 AI들을 방해하지 않도록 자신의 콜라이더를 해제

        Collider[] enemyColliders = GetComponents<Collider>();

        //어떤 집합에 대응되는 배열처럼 한땀한땀 넘겨갈수있ㄴ는 타입에대해 하나씩하나씩 전부꺼내서 순회하는 키워드
        //하나씩꺼내서 하나씩 실행. 순번을 안따지고
        //foreach문이좀더 직관적 코드량도 짧다. 0번째 1번째 따질필요없음
        //foreach문은 컬렉션 타입이 들어온다

        foreach(Collider collider in enemyColliders)
        {
            collider.enabled = false;
        }
        /*
        for(int i = 0; i<enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }*/

        // AI추적을 중단
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        //사망 애니메이션 재생 
        enemyAnimator.SetTrigger("Die");

        //사망 효과음 재생
        enemyAudioPlayer.PlayOneShot(deathSound);

    }

    //겹쳐있는 동안 매번 실행된다
    private void OnTriggerStay(Collider other) {
        // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행   
        // 자신이 사망하지 않은 상태 && 최근 공격시점에서 timeBetAttack이상 시간 지남
        if(!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            //상대방의 LivingEntity 타입 가져오기 시도
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();
            
            //상대방의 LivingEntity가 본래 추적 대상이 맞다면 공격 실행
            if(attackTarget != null && attackTarget == targetEntity)
            {
                //최근공격시점을 갱신
                lastAttackTime = Time.time;
                //상대방의 피격 위치와 피격 방향을 근삿값으로 계싼
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                //>일단 점을 찍고 그점과 컬라이더사이의 가장 가까운 지점을 찍어준다. >> 대충 그 지점이 공격당했다고 취급하기위해
                //상대방의 위치에서 자신의 위치까지의 방향을 피격방향으로 사용
                Vector3 hitNormal = transform.position - other.transform.position;

                //공격을 실행
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }

    }
}