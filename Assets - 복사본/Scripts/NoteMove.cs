using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMove : MonoBehaviour
{
    //노트 이동 속도 조절 변수
    public float noteSpeed = 1f; 

    //노트의 종류 속성
    public string NoteType { get; set; } 

    void Update()
    {
        // noteSpeed와 Time.deltaTime을 곱해서 초당 이동 거리를 계산
        transform.Translate(Vector3.left * noteSpeed * Time.deltaTime);
    }
}