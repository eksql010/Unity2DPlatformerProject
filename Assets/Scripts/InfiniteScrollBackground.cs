using UnityEngine;

public class InfiniteScrollBackground : MonoBehaviour
{
    public Transform[] backgroundPieces; // 배경 조각들 (2~3개)
    public Camera mainCamera;
    private float pieceWidth;

    void Start()
    {
        // 스프라이트 하나 기준 가로 길이 자동 계산
        SpriteRenderer sr = backgroundPieces[0].GetComponent<SpriteRenderer>();
        pieceWidth = sr.bounds.size.x;
    }

    void Update()
    {
        foreach (Transform piece in backgroundPieces)
        {
            // 카메라가 이 조각의 오른쪽 끝을 완전히 지나쳤으면
            if (mainCamera.transform.position.x - piece.position.x > pieceWidth)
            {
                // 가장 오른쪽에 있는 조각 뒤로 재배치
                float rightMostX = GetRightMostX();
                piece.position = new Vector3(rightMostX + pieceWidth, piece.position.y, piece.position.z);
            }
        }
    }

    float GetRightMostX()
    {
        float maxX = backgroundPieces[0].position.x;
        foreach (Transform piece in backgroundPieces)
        {
            if (piece.position.x > maxX)
                maxX = piece.position.x;
        }
        return maxX;
    }
}
