using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //UI관련코드 가져오기
using UnityEngine.SceneManagement;  //씬 관리코드 가져오기

//1122 목
public class GameManager : MonoBehaviour {
    public GameObject gameOverPannel; // 게임오버 패널 /평소에 꺼놨다가 게임오버가 되면 켜질수있게
    public Text timeText; //시간표시용 UI텍스트
    public Text recordText; //최고기록 표시용 UI텍스트

    private bool isGameOver = false; //게임오버상태를 표시
    private float surviveTime = 0f; //생존시간
    
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        //게임오버가 아닌동안
        if (!isGameOver)
        {
            //생존시간을 갱신
            surviveTime = surviveTime + Time.deltaTime;
            //텍스트 컴포넌트의 text필트값을 생존 시간으로 변경
            timeText.text = "Time: " + Mathf.Round(surviveTime);
        }
        else
        {
            //게임오버가 게임 재시작 가능
            if (Input.GetKeyDown(KeyCode.R))
            {
                //SampleScene씬을 로드
                //같은 씬을 로드 >> 게임 재시작이랑 같은 의미

                SceneManager.LoadScene("SampleScene");
            }
        }	
	}

    //게임매니저가 매프래임마다 죽었는지 안죽었는지 체크하는방법도 있음 >>쓸만하다
    //현재 게임 상태를 게임 오버 상태로 만들기
    public void EndGame()
    {
        isGameOver = true;
        //게임 오버 표시용 UI를 활성화
        gameOverPannel.SetActive(true);

        //과거에 BestTime이라는 키로 저장된 값을 가져오기
        //PlayerPrefs 유니티가 제공해주는 함수. 현재 기기에 저장
        //[key | value]로 저장하라고 만든것

        float bestTime = PlayerPrefs.GetFloat("BestTime");

        //이전의 최고기록와 지금의 기록을 비교
        if (surviveTime > bestTime)
        {
            //최고 기록값을 현재 기록으로 덮어쓰기
            bestTime = surviveTime;
            //변경된 최고 기록을 BestTime이라는 키로 저장
            PlayerPrefs.SetFloat("BestTime", bestTime);

        }

        //최고기록을 텍스트로 표시
        recordText.text = "Best Time : " + Mathf.Round(bestTime);

    }

}
