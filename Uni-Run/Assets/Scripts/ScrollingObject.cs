using UnityEngine;

//1127 화
// 게임 오브젝트를 계속 왼쪽으로 움직이는 스크립트
public class ScrollingObject : MonoBehaviour {
    public float speed = 10f; // 이동 속도

    private void Update() {
        // 게임 오브젝트를 왼쪽으로 일정 속도로 평행 이동하는 처리
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.Self); 
        //Translate() 평행이동하는 함수 // 기본적으로 로컬좌표(오브젝트좌표계)로 동작

    }
}