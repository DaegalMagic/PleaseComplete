using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchHandler : MonoBehaviour
{
    public GameObject tilePrefab;
    private Tile[,] field = new Tile[ConstValue.SIZE, ConstValue.SIZE];
    GameState gameState;
    private Tile dragStartTile = null;
    private Vector2 dragStartScreenPos;
    private bool hasSwapped = false;
    float tileDistance = 5f / 10.6f; // 타일 간 간격
    private const int AREA_X = 4;
    private const int AREA_Y = 4;
    private CheckMatches checkMatches;

    float offsetX = 13f;
    float offsetY = 22f;

    void Start()
    {
        gameState = GameState.Idle;
        GetField(field);
        checkMatches = CheckMatches.Instance;
        CheckAndRefillMatches();
        GetPos();
    }

    void GetPos()
    {
        //이전 위치 가져오기
        offsetX = 13f;
        offsetY = 22f;
        float targetX = (offsetX - (ConstValue.SIZE - 1) / 2f) * tileDistance;
        float targetY = (offsetY - (ConstValue.SIZE - 1) / 2f) * tileDistance;
        Vector3 endPos = new Vector3(targetX, targetY, transform.position.z);
        transform.position = endPos;
    }

    void GetField(Tile[,] field)
    {
        for (int x = 0; x < ConstValue.SIZE; x++)
        {
            for (int y = 0; y < ConstValue.SIZE; y++)
            {
                Vector3 position = GetWorldPosition(x, y);

                GameObject obj = Instantiate(tilePrefab, position, Quaternion.identity);

                Tile tile = obj.GetComponent<Tile>();
                tile.type = (TileType)Random.Range(0, 4);//(TileType)((x + y) % 4);
                tile.position = new Position(x, y);
                field[x, y] = tile;
            }
        }
    }

    public bool CheckAndRefillMatches(Position? originalPosA = null, Position? originalPosB = null)
    {
        int startX = (int)offsetX - AREA_X;
        startX = Mathf.Max(0, startX);
        int startY = (int)offsetY - AREA_Y;
        startY = Mathf.Max(0, startY);
        int endX = AREA_X + (int)offsetX;
        endX = Mathf.Min(26, endX);
        int endY = AREA_Y + (int)offsetY;
        endY = Mathf.Min(26, endY);

        bool needRefill = checkMatches.Check(field, startX, startY, endX, endY, originalPosA, originalPosB);
        if (needRefill) StartCoroutine(DestroyAndRefill());
        return needRefill;
    }

    int MatchingCount = 0;
    public IEnumerator DestroyAndRefill()
    {
        gameState = GameState.Matching;
        MatchingCount++;
        yield return new WaitForSeconds(0.1f);

        for (int x = 0; x < ConstValue.SIZE; x++)
        {
            for (int y = 0; y < ConstValue.SIZE; y++)
            {
                if (field[x, y] != null && field[x, y].isMatched)
                {
                    Destroy(field[x, y].gameObject);
                    field[x, y] = null;
                }
            }
        }

        RefillField();
        ResetMatchedFlags();
        yield return new WaitForSeconds(0.4f);
        CheckAndRefillMatches(); // 연쇄 콤보
        MatchingCount--;
        if (MatchingCount == 0) gameState = GameState.Idle;
    }

    void RefillField()
    {
        for (int x = 0; x < ConstValue.SIZE; x++)
        {
            int emptyCount = 0;

            // 1️⃣ 빈 공간 개수 체크 및 블록 아래로 이동
            for (int y = 0; y < ConstValue.SIZE; y++)
            {
                if (field[x, y] == null)
                {
                    emptyCount++;
                }
                else if (emptyCount > 0)
                {
                    field[x, y - emptyCount] = field[x, y];
                    field[x, y] = null;
                    field[x, y - emptyCount].position = new Position(x, y - emptyCount);
                }
            }

            // 2️⃣ 새로운 블록 생성 (최상단부터 채우기)
            for (int i = 0; i < emptyCount; i++)
            {
                int y = ConstValue.SIZE - emptyCount + i; // 새 블록이 들어갈 위치
                int spawnY = ConstValue.SIZE + i; // 최상단에서 점진적으로 떨어지도록 설정

                Vector3 spawnPosition = GetWorldPosition(x, spawnY);
                GameObject obj = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
                Tile tile = obj.GetComponent<Tile>();
                tile.type = (TileType)Random.Range(0, 4);
                tile.position = new Position(x, y);
                field[x, y] = tile;

                string spritePath = $"blocks/{tile.type.ToString()}";
                Sprite tileSprite = Resources.Load<Sprite>(spritePath);
                if (tileSprite != null)
                {
                    obj.GetComponent<SpriteRenderer>().sprite = tileSprite;
                }
            }
        }

        // ✅ 블록 드롭 애니메이션 실행
        BlockDrop();
    }

    void BlockDrop()
    {
        for (int x = 0; x < ConstValue.SIZE; x++)
        {
            for (int y = 0; y < ConstValue.SIZE; y++)
            {
                if (field[x, y] != null)
                {
                    Vector3 targetPosition = GetWorldPosition(x, y);
                    StartCoroutine(AnimateDrop(field[x, y].gameObject, targetPosition));
                }
            }
        }
    }

    bool isCameraMoving = false;
    public void moveOffset(int way)
    {
        if (!isCameraMoving && gameState == GameState.Idle)
        {
            float oriX = offsetX;
            float oriY = offsetY;
            switch (way)
            {
                case 0:
                    if (offsetY > 0) offsetY--;
                    break;
                case 1:
                    if (offsetX < ConstValue.SIZE - 1) offsetX++;
                    break;
                case 2:
                    if (offsetY < ConstValue.SIZE - 1) offsetY++;
                    break;
                case 3:
                    if (offsetX > 0) offsetX--;
                    break;
            }
            if (oriX != offsetX || oriY != offsetY) StartCoroutine(moveCamera());
        }
    }

    IEnumerator moveCamera()
    {
        isCameraMoving = true;

        Vector3 startPos = transform.position;
        float targetX = (offsetX - (ConstValue.SIZE - 1) / 2f) * tileDistance;
        float targetY = (offsetY - (ConstValue.SIZE - 1) / 2f) * tileDistance;
        Vector3 endPos = new Vector3(targetX, targetY, transform.position.z);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // 위치 보정
        isCameraMoving = false;

        CheckAndRefillMatches();
    }

    Vector3 GetWorldPosition(int x, int y)
    {
        float worldX = (x - (ConstValue.SIZE - 1) / 2f) * tileDistance;
        float worldY = (y - (ConstValue.SIZE - 1) / 2f) * tileDistance;

        return new Vector3(worldX, worldY, 0);
    }

    Vector3 GetWorldPosition(Position pos)
    {
        return GetWorldPosition(pos.x, pos.y);
    }

    void ResetMatchedFlags()
    {
        foreach (var tile in field)
        {
            if (tile != null)
                tile.isMatched = false;
        }
    }

    IEnumerator AnimateDrop(GameObject obj, Vector3 targetPosition)
    {
        float duration = 0.3f; // 1초 동안 이동
        float elapsed = 0f;
        Vector3 startPosition = obj.transform.position;

        while (elapsed < duration)
        {
            if (obj == null) break;
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPosition; // 정확한 위치로 보정
    }

    void HandleDragInput()
    {
        Vector2 inputPos = Vector2.zero;
        bool isPressed = false;
        bool isDragging = false;
        bool isReleased = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            inputPos = Input.mousePosition;
            isPressed = true;
        }
        else if (Input.GetMouseButton(0))
        {
            inputPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isReleased = true;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPos = touch.position;

            if (touch.phase == TouchPhase.Began) isPressed = true;
            else if (touch.phase == TouchPhase.Moved) isDragging = true;
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isReleased = true;
        }
#endif

        if (isPressed)
        {
            dragStartScreenPos = inputPos;
            dragStartTile = GetTileFromScreenPosition(inputPos);
            hasSwapped = false; // ✨ 드래그 시작 시 초기화
        }

        else if (isDragging && dragStartTile != null && !hasSwapped)
        {
            Vector2 dragDelta = (Vector2)inputPos - dragStartScreenPos;

            if (dragDelta.magnitude > 15f)
            { // ✨ 거리 임계값 (ex: 30px 이상 이동)
                Vector2 dir = dragDelta.normalized;
                int dx = 0, dy = 0;

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) dx = dir.x > 0 ? 1 : -1;
                else dy = dir.y > 0 ? 1 : -1;

                int newX = dragStartTile.position.x + dx;
                int newY = dragStartTile.position.y + dy;

                if (IsInBounds(newX, newY))
                {
                    Tile targetTile = field[newX, newY];
                    if (targetTile != null)
                    {
                        SwapTiles(dragStartTile, targetTile);
                        hasSwapped = true; // ✅ 한 번만 스왑되도록
                    }
                }
            }
        }

        else if (isReleased)
        {
            dragStartTile = null;
            hasSwapped = false;
        }
    }

    bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < ConstValue.SIZE && y < ConstValue.SIZE;
    }

    void SwapTiles(Tile a, Tile b)
    {
        if (a == null || b == null) return;

        // 스왑
        Position posA = a.position;
        Position posB = b.position;

        field[posA.x, posA.y] = b;
        field[posB.x, posB.y] = a;

        (a.position, b.position) = (b.position, a.position);

        // 위치 이동 애니메이션
        StartCoroutine(SwapAnimation(a, GetWorldPosition(a.position)));
        StartCoroutine(SwapAnimation(b, GetWorldPosition(b.position)));

        // 매치 검사 후 복구 필요 여부 확인
        StartCoroutine(CheckMatchAfterSwap(a, b, posA, posB));
    }

    IEnumerator SwapAnimation(Tile tile, Vector3 targetPos)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 start = tile.transform.position;

        while (elapsed < duration)
        {
            tile.transform.position = Vector3.Lerp(start, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        tile.transform.position = targetPos;
    }

    IEnumerator CheckMatchAfterSwap(Tile a, Tile b, Position originalPosA, Position originalPosB)
    {
        yield return new WaitForSeconds(0.25f); // 스왑 애니메이션 후 딜레이

        bool matched = CheckAndRefillMatches(originalPosA, originalPosB);

        if (!matched)
        {
            // ❌ 매칭이 없으면 다시 스왑
            field[originalPosA.x, originalPosA.y] = a;
            field[originalPosB.x, originalPosB.y] = b;

            (a.position, b.position) = (originalPosA, originalPosB);

            StartCoroutine(SwapAnimation(a, GetWorldPosition(a.position)));
            StartCoroutine(SwapAnimation(b, GetWorldPosition(b.position)));
        }
    }

    Tile GetTileFromScreenPosition(Vector2 screenPos)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hit = Physics2D.OverlapPoint(touchPos);
        if (hit != null) return hit.GetComponent<Tile>();

        return null;
    }

    void Update()
    {
        if (gameState == GameState.Idle) HandleDragInput();
    }
}