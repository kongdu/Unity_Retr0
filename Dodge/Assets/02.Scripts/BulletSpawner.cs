using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour {

    public GameObject bulletPrefab; //생성할 총알 원본 프리팹
    public float spawnRateMin = 0.5f;  //최소 생성 주기
    public float spawnRateMax = 3; //최대 생성 주기

    private Transform target; //발사할 대상
    private float spawnRate; //생성 주기 //현재시점에서 다음번기다릴때까지 명시할까
    private float timeAfterSpawn; //최근 생성 시점에서 지난 시간

    void Start () {
        timeAfterSpawn = 0f;
        //spawnRateMin과 spawnRateMax사이의 랜덤값을 사용
        spawnRate = Random.Range(spawnRateMin, spawnRateMax); 
        target = FindObjectOfType<PlayerController>().transform; //메모리를 많이먹기때문에 단발적으로 사용해야함 /Update에서는 사용하지말기
        //FindObjectOfType은 씬에 존재하는 모든 오브젝트를 검색

        /*
        PlayerController playerctrl = GetComponent<PlayerController>();
        target = playerctrl.transform; */
    }

    //게임화면이 한번 갱신될때 한번 실행됨
    void Update () {
        //Time.deltaTime은 직전의 Update와 현재 Update 실행 시점사이의 시간 간격
        timeAfterSpawn = timeAfterSpawn + Time.deltaTime;
        //누적된 시간이 생성 주기보다 크거나 같다
        if(timeAfterSpawn >= spawnRate)
        {
            timeAfterSpawn = 0f; //누적된 시간을 리셋
            //bulletPrefab의 복제복을 생성
            //위치와 회전은 자신의 위치/회전으로 지정
            //생성한 총알 복제본을 bullet이라는 변수로 다루기
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation); 
            //Instantiate 프리팹이 아니어도 생성할 수 있다. >>생성한 프리팹을 리턴값으로 준다

            //총알이 타겟을 바라보도록 회전
            bullet.transform.LookAt(target);

            //다음번 생성 간격을 랜덤하게 변경
            spawnRate = Random.Range(spawnRateMin, spawnRateMax);

        }
    }
}
