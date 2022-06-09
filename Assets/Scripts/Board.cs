using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private const float UP_POS = 8.53f;
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Pawn pawnPrefab;
    [SerializeField] private Transform slotsContainer;
    [HideInInspector] public bool isClientWhite;
    public static Board Instance { get; set; }
    public static bool GameOver { get; set; }

    private void Awake()
    {
        Instance = this;
        InitializeBoard();
    }

    public void OpponentTryMove(int fromSlot, int toSlot, bool isShelter, float moveTime)
    {
        Pawn pawn = Slot.slots[fromSlot].GetTopPawn(false);
        pawn.OpponentMove(toSlot, isShelter);

        if (isClientWhite) TimeController.Instance.timeLapse[1] = moveTime;
        else TimeController.Instance.timeLapse[0] = moveTime;
    }

    private void InitializeBoard()
    {
        CreateSlots();
        CreatePawns();
    }

    private void CreateSlots()
    {
        Slot.slots = new List<Slot>();
        Vector3 slotPos = new Vector3(0, UP_POS, -0.2f);
        Quaternion slotRot = Quaternion.identity;
        CreateSlot(0, Color.clear, slotPos, slotRot);

        for (int i = 1; i <= 24; i++)
        {
            float xDelta = (i < 13) ? -1.125f : 1.125f;
            float xOffset = (((i - 1) / 6) % 3 == 0) ? 0 : -1.25f;
            float iOffset = (i < 13) ? 1 : 24;
            float ySign = (i < 13) ? 1 : -1;

            Color color = (i % 2 == 0) ? Color.white : Color.gray;

            slotPos = new Vector3(6.81f + (i - iOffset) * xDelta + xOffset, ySign * UP_POS, -0.2f);
            slotRot = (i < 13) ? Quaternion.identity : Quaternion.Euler(new Vector3(0, 0, 180));

            CreateSlot(i, color, slotPos, slotRot);
        }

        slotPos = new Vector3(0, -UP_POS, -0.2f);
        slotRot = Quaternion.Euler(new Vector3(0, 0, 180));
        CreateSlot(25, Color.clear, slotPos, slotRot);
    }

    private void CreateSlot(int slotNo, Color color, Vector3 slotPos, Quaternion slotRot)
    {
        Slot slot = Instantiate(slotPrefab, slotPos, slotRot, slotsContainer);
        slot.name = "slot" + slotNo.ToString();
        slot.slotNo = slotNo;
        slot.spriteRenderer.color = color;
        Slot.slots.Add(slot);
    }

    private void CreatePawns()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < 2) CreatePawn(1, 0);
            if (i < 2) CreatePawn(24, 1);

            if (i < 3) CreatePawn(8, 1);
            if (i < 3) CreatePawn(17, 0);

            CreatePawn(6, 1);
            CreatePawn(12, 0);
            CreatePawn(13, 1);
            CreatePawn(19, 0);
        }

    }

    private void CreatePawn(int slotNo, int isWhite)
    {
        Pawn pawn = Instantiate(pawnPrefab);
        Slot.slots[slotNo].PlacePawn(pawn, isWhite);
    }
}
