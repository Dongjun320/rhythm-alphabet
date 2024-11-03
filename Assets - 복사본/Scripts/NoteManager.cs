using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiNote = Melanchall.DryWetMidi.Interaction.Note;
using System.IO;
using UnityEngine.UI;

public class NoteManager : MonoBehaviour
{
    // 노트 생성 위치
    public Transform spawnPoint;

    // MIDI파일 지정변수
    public string midiFileName;

    // 노트의 기본 이동 속도
    public float defaultNoteSpeed = 4.0f;

    // 노트 삭제 위치 지정 변수
    public float destroyXPosition = -10.0f;

    // 노트 초기 속도 변수
    public Vector2 initialVelocity = new Vector2(-1.0f, 0);

    // 노트 알파벳 정보를 담고 있는 CSV 파일 변수
    public TextAsset noteCsvFile;

    // 음표 프리팹 저장 배열
    public GameObject[] notePrefabs;

    // MIDI 파일에서 추출된 음표 저장 리스트
    private List<MidiNote> notes = new List<MidiNote>();

    // MIDI 파일 전체
    private MidiFile midiFile;

    // MIDI 파일 템포 맵
    private TempoMap tempoMap;

    // 이전 음표의 시간 기록
    private double lastNoteTime;

    // 알파벳 시퀀스 저장
    private List<string> alphabetSequence = new List<string>();

    // 현재 음표의 인덱스
    private int currentNoteIndex = 0;
    void Start()
    {
        // MIDI 파일 경로
        string midiFilePath = Path.Combine(Application.dataPath, "Resources/", midiFileName + ".mid");

        // MIDI 파일이 존재하는 경우
        if (File.Exists(midiFilePath))
        {
            // MIDI 파일을 바이트 배열로 읽음
            byte[] midiBytes = File.ReadAllBytes(midiFilePath);

            // 바이트 배열을 스트림으로 변환하고 MIDI 파일로 읽음
            using (var stream = new MemoryStream(midiBytes))
            {
                midiFile = MidiFile.Read(stream);
            }

            // MIDI 파일의 템포 맵을 가져옴
            tempoMap = midiFile.GetTempoMap();

            // 노트 CSV 파일이 존재할때
            if (noteCsvFile != null)
            {
                // CSV 파일의 각 줄을 읽어와서 알파벳 시퀀스에 추가
                string[] lines = noteCsvFile.text.Split('\n');
                foreach (string line in lines)
                {
                    string[] values = line.Split(',');
                    foreach (string value in values)
                    {
                        alphabetSequence.Add(value.Trim());
                    }
                }
            }

            // 음표를 생성하는 코루틴을 시작
            StartCoroutine(SpawnNotes(midiFile.GetNotes()));
        }
        else
        {
            Debug.LogError("MIDI 파일을 찾을 수 없습니다. Resources 폴더에 파일이 존재하는지 확인하십시오.");
        }
    }

    // 음표를 생성하는 코루틴 함수
    IEnumerator SpawnNotes(IEnumerable<MidiNote> midiNotes)
    {
        foreach (var note in midiNotes)
        {
            // MIDI 파일의 노트 시간을 가져옴
            double noteTime = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1000000.0;

            // 이전 음표와의 시간 차이를 계산
            double noteDelay = noteTime - lastNoteTime;
            lastNoteTime = noteTime;

            // 지정된 딜레이 후에 음표를 생성
            yield return new WaitForSeconds((float)noteDelay);

            // 다음 알파벳을 가져오고 해당 알파벳에 대응하는 노트 프리팹을 가져옴
            string selectedAlphabet = GetNextAlphabet();
            GameObject notePrefab = GetNotePrefab(selectedAlphabet);

            // 노트 프리팹이 존재하는 경우
            if (notePrefab != null)
            {
                // 노트를 생성하고 초기 속도와 알파벳을 설정
                GameObject noteObject = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
                Rigidbody2D rb = noteObject.GetComponent<Rigidbody2D>();
                NoteMove noteMove = noteObject.GetComponent<NoteMove>();

                if (noteMove != null)
                {
                    rb.velocity = initialVelocity;
                    noteMove.NoteType = selectedAlphabet;
                }
            }
        }
    }

    // 알파벳에 해당하는 노트 프리팹을 가져옴
    GameObject GetNotePrefab(string noteType)
    {
        GameObject notePrefab = Array.Find(notePrefabs, prefab => prefab.name == "Note_" + noteType);

        // 해당하는 노트 프리팹이 없는 경우 에러를 출력
        if (notePrefab == null)
        {
            Debug.LogError("Note prefab not found for noteType: " + noteType);
        }

        return notePrefab;
    }

    // 다음 알파벳을 가져오는 함수
    string GetNextAlphabet()
    {
        // 알파벳 시퀀스가 비어있는 경우 에러를 출력하고 null을 반환
        if (alphabetSequence.Count == 0)
        {
            Debug.LogError("Alphabet sequence is empty.");
            return null;
        }

        // 현재 인덱스의 알파벳을 가져오고 인덱스 업데이트
        string selectedAlphabet = alphabetSequence[currentNoteIndex];
        currentNoteIndex = (currentNoteIndex + 1) % alphabetSequence.Count;

        return selectedAlphabet;
    }

    // 지정된 노트 인덱스에 해당하는 노트 타입을 가져오는 함수
    public string GetNoteTypeByIndex(int noteIndex)
    {
        // 유효한 노트 인덱스인 경우 해당하는 노트 타입을 반환
        if (noteIndex >= 0 && noteIndex < notePrefabs.Length)
        {
            return notePrefabs[noteIndex].name.Replace("Note_", "");
        }
        else
        {
            return "InvalidNoteType";
        }
    }

    // 입력된 키와 노트 인덱스에 해당하는 노트 타입이 일치하는지 확인하는 함수
    public bool IsNoteTypeCorrect(string inputKey, int noteIndex)
    {
        string correctNoteType = GetNoteTypeByIndex(noteIndex);
        return inputKey == correctNoteType;
    }
}