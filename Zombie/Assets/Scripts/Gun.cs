using System.Collections;
using UnityEngine;

// 총을 구현한다
public class Gun : MonoBehaviour
{
    // 총의 상태를 표현하는데 사용할 타입을 선언한다
    public enum State
    {
        Ready, // 발사 준비됨 0
        Empty, // 탄창이 빔 1
        Reloading // 재장전 중 2
    }

    public State state { get; private set; } // 현재 총의 상태 //외부에서는 값을 읽을수만있고 내부에서는 set 

    public Transform fireTransform; // 총알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 총알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 재장전 소리

    public float damage = 25; // 공격력
    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄약
    public int magCapacity = 25; // 탄창 용량
    public int magAmmo; // 현재 탄창에 남아있는 탄약


    public float timeBetFire = 0.12f; // 총알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간
    private float lastFireTime; // 총을 마지막으로 발사한 시점


    private void Awake()
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        bulletLineRenderer = GetComponent<LineRenderer>();
        gunAudioPlayer = GetComponent<AudioSource>();

        //라인렌더러의 사용할 점의 갯수를 2로 변경
        bulletLineRenderer.positionCount = 2;
        //라인렌더러를 비활성화
        bulletLineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        // 총 상태 초기화
        //부활할때마다 매번 실행 //플레이어가 나중에 부활할 수도 있다. (향후 멀티플레이어 게임 구현할 경우)
        //탄창을 가득채우기
        magAmmo = magCapacity;
        //총의 상태를 준비된 상태로 변경
        state = State.Ready;
        //마지막으로 총을 발사한 시점을 리셋
        lastFireTime = 0;
    }

    // 발사 시도
    public void Fire()
    {
        //총이 준비된 상태 && 마지막 발사 시점에서 timeBetFire 이상 시간이 지남
        if (state == State.Ready && Time.time >= lastFireTime + timeBetFire)
        {
            //마지막 총 발사 시점을 갱신
            lastFireTime = Time.time;
            //실제 발사 처리 실행
            Shot(); //private 필터링을 위해 쪼갬
        }
    }

    // 실제 발사 처리
    private void Shot()
    {
        //레이캐스트에 의한 충돌 정보 저장 컨테이너
        RaycastHit hit;
        //총알이 맞은 위치를 저장할 변수
        Vector3 hitPosition = Vector3.zero;

        //레이캐스트(시작지점, 방향, 충돌정보 그릇, 사정거리)
        //out은 입력으로 들어간 변수가 변경사항을 받아 빠져나옴
        //(함수가 두번째 출력을 만드는 방법 중 하나)
        if (Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            //레이(광선)가 어떤 충돌체와 충돌한 경우
            //충돌한 상대방으로부터 IDamageble 가져오기 시도
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            //상대방으로부터 IDamageble 오브젝트를 가져오는데 성공
            if(target != null)
            {
                //상대방의 타입을 구체적으로 파악할 필요없이 OnDamage()실행
                target.OnDamage(damage, hit.point, hit.normal);
            }
            //레이가 충돌한 지점 저장(총알이 맞은 지점)
            hitPosition = hit.point;
        }
        else
        {
            //레이가 충돌하지않았다면, 총알이 최대 사정거리까지 날아갔을때의 위치를 충돌 위치로 사용
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }
        //남은 탄약수를 수정
        //탄약 -1
        magAmmo--;
        //탄창에 남은 탄약이 없다면, 총의 현재 상태를 Empty로 갱신
        if(magAmmo <= 0)
        {
            state = State.Empty; //이넘타입 1
        }

        //발사 이펙트 재생 시작
        StartCoroutine(ShotEffect(hitPosition));
    }

    // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        //총구 화염 효과 재생
        muzzleFlashEffect.Play();
        //탄피 배출효과 재생
        shellEjectEffect.Play();

        //총격 소리를 재생
        gunAudioPlayer.PlayOneShot(shotClip); //Play를 쓰면 뚝뚝끊기는 느낌이든다

        //라인렌더러를 활성화하여 총알 궤적을 그린다
        bulletLineRenderer.enabled = true;
        //선의 시작점은 총구의 위치
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        //선의 끝지점은 충돌 위치
        bulletLineRenderer.SetPosition(1, hitPosition);

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 총알 궤적을 지운다
        bulletLineRenderer.enabled = false;
    }

    // 재장전 시도
    public bool Reload()
    {
        //이미 재장전 중 || 남은 탄약 없음 || 탄창 가득참
        if(state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity)
        {
        return false;

        }
        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    private IEnumerator ReloadRoutine()
    {
        // 현재 상태를 재장전 중 상태로 전환
        state = State.Reloading;
        //재장전 소리 재생
        gunAudioPlayer.PlayOneShot(reloadClip);
        // 재장전 소요 시간 만큼 처리를 쉬기
        yield return new WaitForSeconds(reloadTime);

        //탄창에 채울 탄약을 계산
        int ammoToFill = magCapacity - magAmmo;

        //탄창에 채울 탄약이, 남은 탄약보다 많다면
        //탄창에 채울 탄약을 남은 탄약에 맞춰 줄이기

        if(ammoRemain < ammoToFill)
        {
            ammoToFill = ammoRemain;
        }

        //탄창을 채우기
        magAmmo += ammoToFill;

        //채운 탄약만큼, 남은 탄약을 빼기
        ammoRemain -= ammoToFill;

        // 총의 현재 상태를 발사 준비된 상태로 변경
        state = State.Ready;

    }
}