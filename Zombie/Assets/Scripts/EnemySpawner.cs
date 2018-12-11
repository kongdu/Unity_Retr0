using System.Collections.Generic;
using UnityEngine;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviour {
    public Enemy enemyPrefab; // 생성할 적 AI

    public Transform[] spawnPoints; // 적 AI를 소환할 위치들

    public float damageMax = 40f; // 최대 공격력
    public float damageMin = 20f; // 최소 공격력

    public float healthMax = 200f; // 최대 체력
    public float healthMin = 100f; // 최소 체력

    public float speedMax = 3f; // 최대 속도
    public float speedMin = 1f; // 최소 속도

    public Color strongEnemyColor = Color.red; // 강한 적 AI가 가지게 될 피부색

    private List<Enemy> enemies = new List<Enemy>(); // 생성된 적들을 담는 리스트 
    /*
     * 리스트는 고무주머니와같다. 적의 남은숫자를 파악하기위해 리스트사용
     */
    private int wave; // 현재 웨이브

    private void Update() {
        // 게임 오버 상태일때는 생성하지 않음 ; 모든코드를 무시하도록
        if (GameManager.instance != null && GameManager.instance.isGameover) //게임매니저가존재하고 게임매니저의게임오버상태가 true라면
        {
            return;
        }

        // 적을 모두 물리친 경우 다음 스폰 실행
        if (enemies.Count <= 0)
        {
            SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI() {
        // 현재 웨이브와 남은 적의 수 표시
        UIManager.instance.UpdateWaveText(wave, enemies.Count);
    }

    // 현재 웨이브에 맞춰 적을 생성
    private void SpawnWave() {
        //웨이브 수를 1증가
        wave++;

        //생성할 적의 갯수는 현재 웨이브*1.5반올림
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        for (int i = 0; i < spawnCount; i++)
        {
            CreateEnemy(Random.Range(0f,1f)); //0.0 ~ 1.0(적이 약할때)
        }
    }

    // 적을 생성하고 생성한 적에게 추적할 대상을 할당
    private void CreateEnemy(float intensity) {
        //입력받은 intensity (강박적인세기)를 기반으로 적의 능력치 결정
        float health = Mathf.Lerp(healthMin, healthMax, intensity); //Lerp 선형보간;중간점찍기
        float damage = Mathf.Lerp(damageMin, damageMax, intensity);
        float speed = Mathf.Lerp(speedMin, speedMax, intensity);

        //intensity를 기반으로 적의 피부색을 결정
        Color skinColor = Color.Lerp(Color.white, strongEnemyColor, intensity);
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        //적 프리팹으로부터 적을 생성
        Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        //왜 GameObject가 아니라 Enemy타입으로 생성할까
        //enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<Enemy>();
        //어자피 겟컴포넌트로 Enemy를 가져와야하니까 귀찮음

        //생성한 적의 능력치 설정
        enemy.Setup(health, damage, speed, skinColor);

        //생성한 적을 추적 가능하도록 리스트에 추가
        enemies.Add(enemy);

        //생성한 적이사망시 실행될 처리를 onDeath이벤트에 구독시키기
        //람다 식을 통한 익명 메서드를 생성하여 등록
        //람다표현식을 사용해 만든 익명함수
        //이름은 없지만 델리게이트타입에 저장해놓고 나중에쓸수있다.
        //Action이 대표적인 델리게이트 타입 중 하나
        enemy.onDeath += () => GameManager.instance.AddScore(100);
        enemy.onDeath += () => Destroy(enemy.gameObject, 10f);
        enemy.onDeath += () => enemies.Remove(enemy);



    }
}