using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DicesController : MonoBehaviour
{
    public static DicesController Instance { get; set; }
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button diceButton;
    [SerializeField] private Image[] turnImages;
    [SerializeField] private Text[] diceTexts;
    public static int[] dices = new int[2];
    public static bool isDublet;
    public static bool dragEnable;
    public static int turn;
    private static int sign, value, count;
    [HideInInspector] public int sidesAgreed;
    private bool diceEnable = true;
    private void Awake()
    {
        Instance = this;

        Pawn.OnCompleteTurn += Pawn_OnCompleteTurn;

        newGameButton.onClick.AddListener(NewGame);
        diceButton.onClick.AddListener(Generate);
        diceTexts[0].text = diceTexts[1].text = "";

        turn = 0;

        turnImages[0].gameObject.SetActive(turn == 0);
        turnImages[1].gameObject.SetActive(1 - turn == 0);
    }

    private void OnDestroy()
    {
        Pawn.OnCompleteTurn -= Pawn_OnCompleteTurn;

    }

    private void Update()
    {
        if (sidesAgreed == 2)
            LoadGameScene();
    }

    private void Generate()
    {
        if (diceEnable)
        {
            dragEnable = true;
            diceEnable = false;

            CheckIfTurnChange(Random.Range(1, 7), Random.Range(1, 7));
        }
    }

    public void CheckIfTurnChange(int dice0, int dice1)
    {
        diceButton.gameObject.SetActive(false);
        isDublet = false;

        dices[0] = dice0;
        dices[1] = dice1;

        diceTexts[0].text = dices[0].ToString();
        diceTexts[1].text = dices[1].ToString();

        if (dices[0] == dices[1])
            isDublet = true;

        if (!CanMove(2))
        {
            StartCoroutine(ChangeTurn());
        }
    }

    private IEnumerator ChangeTurn()
    {
        yield return new WaitForSeconds(2f);
        Pawn_OnCompleteTurn(turn);
    }

    private void Pawn_OnCompleteTurn(int isWhiteColor)
    {
        diceEnable = true;
        dragEnable = false;

        turn = 1 - turn;

        turnImages[0].gameObject.SetActive(1 - isWhiteColor == 0);
        turnImages[1].gameObject.SetActive(isWhiteColor == 0);

        diceTexts[0].text = diceTexts[1].text = "";

        diceButton.gameObject.SetActive(true);
    }

    private void NewGame()
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        sidesAgreed = 0;
        Board.GameOver = false;
        isDublet = false;
        dragEnable = false;
        turn = 0;
        Pawn.InitializePawn();
        SceneManager.LoadScene(0);
    }


    public static bool CanMove(int amount)
    {
        count = 0;
        sign = turn == 0 ? 1 : -1;
        value = turn == 0 ? 24 : -1;

        if (Pawn.imprisonedSide[turn] > 0)
            return CanMoveFromJail(amount);
        else
        {
            if (Pawn.shelterSide[turn])
                return CanMoveInShelter();
            else if (CanMoveFree())
                return true;
        }

        return false;
    }

    private static bool CanMoveFromJail(int amount)
    {
        int val = turn == 0 ? -1 : 24;

        for (int i = 0; i < 2; i++)
            if (dices[i] != 0)
                if (Slot.slots[(val + 1) + sign * dices[i]].Height() > 1 && Slot.slots[(val + 1) + sign * dices[i]].IsWhite() != turn)
                    count++;

        return !(count == amount);
    }

    private static bool CanMoveFree()
    {
        for (int i = 1; i <= 24; i++)
            if (Slot.slots[i].Height() > 0 && Slot.slots[i].IsWhite() == turn)   // slot with whites
                for (int j = 0; j < 2; j++)
                    if (dices[j] != 0 && dices[j] + sign * i <= value)
                    {
                        if (Slot.slots[i + sign * dices[j]].Height() < 2)
                            return true;
                        else if (Slot.slots[i + sign * dices[j]].Height() > 1 && Slot.slots[i + sign * dices[j]].IsWhite() == turn)
                            return true;
                    }

        return false;
    }

    private static bool CanMoveInShelter()
    {
        int endSlotNo = turn == 0 ? 19 : 6;
        int first = 0;

        for (int j = 0; j < 6; j++)
        {
            if (endSlotNo + sign * j >= 0)
            {
                if (Slot.slots[endSlotNo + sign * j].Height() > 0)
                {
                    if (Slot.slots[endSlotNo + sign * j].IsWhite() == turn)
                    {

                        for (int i = 0; i < 2; i++)
                        {
                            if (dices[i] > 0)
                            {

                                int ind = endSlotNo + sign * (j + dices[i]);

                                if (ind == value + 1)
                                {
                                    Debug.Log("any slots");
                                    return true;
                                }

                                if (first == 0)
                                {
                                    if (value == 24)
                                    {
                                        if (ind > value + 1)
                                        {
                                            Debug.Log("farthest");
                                            return true;
                                        }
                                    }

                                    if (value == -1)
                                    {
                                        if (ind < value + 1)
                                        {
                                            Debug.Log("farthest");
                                            return true;
                                        }
                                    }
                                }

                                if (ind >= 0 && ind < Slot.slots.Count)
                                {
                                    if (Slot.slots[ind].Height() > 0)
                                    {
                                        if (Slot.slots[ind].IsWhite() != turn)
                                        {
                                            if (Slot.slots[ind].Height() < 2)
                                            {
                                                Debug.Log("can capture");
                                                return true;
                                            }
                                        }

                                        if (Slot.slots[ind].IsWhite() == turn)
                                        {
                                            Debug.Log("our pawns");
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("free");
                                        return true;
                                    }
                                }
                            }
                        }

                        first++;
                    }
                }
            }
        }

        return false;
    }
}
