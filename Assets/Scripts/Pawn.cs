using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public static event Action<int> OnCompleteTurn = delegate { };
    public static event Action<bool> OnGameOver = delegate { };

    public static int[] imprisonedSide = new int[2];
    public static bool[] shelterSide = new bool[2];

    public static int endSlotNo;
    private static int moves;

    public int pawnColor;
    public int slotNo;
    public int pawnNo;

    private Slot slot;
    private Vector3 startPos;
    private GameObject go;
    private bool isDown;
    private bool imprisoned;
    private bool shelter;
    private int rescuedPawns;
    private int maxMoves;

    public void SetColor(int color)
    {
        GetComponent<SpriteRenderer>().color = color == 0 ? Color.yellow : Color.blue;
        pawnColor = color;
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Slot"))
            slot = other.GetComponent<Slot>();
        else if (other.CompareTag("Shelter"))
            if (shelterSide[0] || shelterSide[1])
                shelter = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Slot"))
            slot = Slot.slots[slotNo];
        else if (other.CompareTag("Shelter"))
            shelter = false;
    }

    private void OnMouseDown()
    {
        if (!Board.GameOver)
        {
            if (!imprisoned && ((imprisonedSide[0] > 0 && pawnColor == 0) || (imprisonedSide[1] > 0 && pawnColor == 1)))
                return;

            TrySelectPawn();
        }
    }

    private void TrySelectPawn()
    {
        if (DicesController.dragEnable && DicesController.turn == pawnColor)
            if (Slot.slots[slotNo].Height() - 1 == pawnNo)
            {
                startPos = transform.position;
                isDown = true;
                TryHighlight(true);
            }
    }

    private void OnMouseDrag()
    {
        if (isDown)
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, -1);
        }
    }

    private void OnMouseUp()
    {
        if (isDown)
        {
            TryHighlight(false);
            isDown = false;

            if (IsPatology())
                return;
            CheckShelterStage();

            if (TryPlace())
                CheckShelterAndMore();

            CheckIfNextTurn();
        }
    }

    public void OpponentMove(int toSlot, bool isShelter)
    {
        shelter = isShelter;
        slot = Slot.slots[toSlot];

        CheckShelterStage();

        if (TryPlace())
            CheckShelterAndMore();

        CheckIfNextTurn();
    }

    private void CheckIfNextTurn()
    {
        if (moves == maxMoves && !Board.GameOver)
        {
            moves = 0;
            OnCompleteTurn(pawnColor);
        }
    }

    private void TryRemovePawnFromJail()
    {
        if (imprisonedSide[pawnColor] > 0 && imprisoned)
        {
            imprisoned = false;
            imprisonedSide[pawnColor]--;
        }
    }

    private void CheckShelterAndMore()
    {
        if (slotNo != 0) TryRemovePawnFromJail();
        if (slotNo != 25) TryRemovePawnFromJail();

        if (CheckShelterStage())
            shelterSide[pawnColor] = true;

        if (++moves < maxMoves)
        {
            if (!DicesController.CanMove(1))
            {
                moves = 0;
                OnCompleteTurn(pawnColor);
            }
        }
    }

    private bool IsPatology()
    {
        if (slot.slotNo == 0 || slot.slotNo == 25)
        {
            transform.position = startPos;
            return true;
        }

        if (slot.Height() > 1 && slot.IsWhite() != pawnColor)
        {
            transform.position = startPos;
            return true;
        }

        return false;
    }

    private bool TryPlace()
    {
        if (shelter)
        {
            if (shelterSide[pawnColor])
                if (CanPlaceShelter())
                    return true;

            transform.position = startPos;
            return false;
        }
        else
        {
            if (slot.slotNo == slotNo)
            {
                transform.position = startPos;
                return false;
            }

            int sign = pawnColor == 0 ? 1 : -1;

            if (slot.slotNo == slotNo + sign * DicesController.dices[0])
                DoCorrectMove(0);
            else if (slot.slotNo == slotNo + sign * DicesController.dices[1])
                DoCorrectMove(1);
            else
            {
                transform.position = startPos;
                return false;
            }

            return true;
        }
    }

    private void TryHighlight(bool state)
    {
        int sign = pawnColor == 0 ? 1 : -1;

        int slot0 = slotNo + sign * DicesController.dices[0];
        int slot1 = slotNo + sign * DicesController.dices[1];

        if (slot0 > 0 && slot0 < 25 && slot0 != slotNo)
            if (!(Slot.slots[slot0].Height() > 1 && Slot.slots[slot0].IsWhite() != pawnColor))
                Slot.slots[slot0].HightlightMe(state);

        if (slot1 > 0 && slot1 < 25 && slot1 != slotNo)
            if (!(Slot.slots[slot1].Height() > 1 && Slot.slots[slot1].IsWhite() != pawnColor))
                Slot.slots[slot1].HightlightMe(state);
    }

    private void DoCorrectMove(int diceNo)
    {
        if (slot.Height() == 1 && slot.IsWhite() != pawnColor)
            PlaceJail();

        Slot.slots[slotNo].GetTopPawn(true);
        slot.PlacePawn(this, pawnColor);

        if (!DicesController.isDublet)
            DicesController.dices[diceNo] = 0;

    }

    private void PlaceJail()
    {
        Pawn pawn = slot.GetTopPawn(true);
        pawn.imprisoned = true;

        Slot.slots[pawn.pawnColor == 0 ? 0 : 25].PlacePawn(pawn, pawn.pawnColor);
        imprisonedSide[pawn.pawnColor]++;
        shelterSide[pawn.pawnColor] = false;

    }

    //-------- private methods related to shelter mode support

    private bool CanPlaceShelter()
    {
        int value = pawnColor == 0 ? 25 : 0;

        if (slotNo == endSlotNo)
        {
            if (CanPlaceShelter(0, value, true) || CanPlaceShelter(1, value, true))
                return true;
        }
        else if (CanPlaceShelter(0, value, false) || CanPlaceShelter(1, value, false))
            return true;

        return false;
    }

    private bool CanPlaceShelter(int ind, int value, bool lastOrNearer)
    {
        int sign = pawnColor == 0 ? -1 : 1;
        int val = value + sign * slotNo;
        int diceVal = DicesController.dices[ind];
        bool result = lastOrNearer ? diceVal >= val : diceVal == val;

        if (result)
        {
            DicesController.dices[ind] = DicesController.isDublet ? DicesController.dices[ind] : 0;
            PlaceInShelter();
        }

        return result;
    }

    private void PlaceInShelter()
    {
        go.transform.GetChild(rescuedPawns++).gameObject.SetActive(true);

        if (rescuedPawns == 15)
        {
            OnGameOver(pawnColor == 0);
            Board.GameOver = true;
        }

        Slot.slots[slotNo].GetTopPawn(true);
        gameObject.transform.localScale = Vector3.zero;
        Destroy(gameObject, 1f);
    }

    private bool CheckShelterStage()
    {
        maxMoves = DicesController.isDublet ? 4 : 2;

        go = GameObject.Find((pawnColor == 0 ? "Yellow" : "Blue") + " House");
        rescuedPawns = go.GetComponentsInChildren<SpriteRenderer>().Length - 1;

        int count = 0;
        int offset = pawnColor == 0 ? 18 : 0;
        int b = pawnColor == 0 ? -1 : 1;

        for (int i = 1 + offset; i <= 6 + offset; i++)
            if (Slot.slots[7 * pawnColor - b * i].Height() > 0 && Slot.slots[7 * pawnColor - b * i].IsWhite() == pawnColor)
            {
                if (count == 0)
                    endSlotNo = 7 * pawnColor - b * i;

                count += Slot.slots[7 * pawnColor - b * i].Height();
            }

        return (count == 15 - rescuedPawns);
    }

    public static void InitializePawn()
    {
        Board.GameOver = false;
        moves = 0;
        imprisonedSide = new int[2];
        shelterSide = new bool[2];
    }
}
