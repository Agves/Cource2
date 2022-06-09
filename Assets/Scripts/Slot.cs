using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public static List<Slot> slots;            
    public SpriteRenderer spriteRenderer;
    public GameObject highlighted;			
    [HideInInspector] public int slotNo;		

    [SerializeField] private Transform pawnsContainer;
    private List<Pawn> pawns = new List<Pawn>();
    private int lastCount;
    private float yOffset = -0.9f;

    public void PlacePawn(Pawn pawn, int isWhite)    
    {
        pawn.transform.SetParent(pawnsContainer, false);
        pawn.transform.localPosition = new Vector3(0, -0.5f + pawns.Count * yOffset, 0);
        pawn.SetColor(isWhite);
        pawn.slotNo = slotNo;                               
        pawn.pawnNo = pawns.Count;                             
        pawns.Add(pawn);
    }

    public Pawn GetTopPawn(bool pop)
    {
        if (pawns.Count > 0)
        {
            Pawn pawn = pawns[pawns.Count - 1];
            if (pop) pawns.RemoveAt(pawns.Count - 1);
            return pawn;
        }

        return null;
    }

    public int Height() => pawns.Count;
    public int IsWhite() => pawns[0].pawnColor;
    public void HightlightMe(bool state) => highlighted.SetActive(state);  


    private void Update() => ModifyPositions();  

    private void ModifyPositions()
    {
        if (pawns.Count != lastCount)
        {
            lastCount = pawns.Count;

            if (pawns.Count > 5)
                for (int i = 1; i < pawnsContainer.childCount; i++)
                {
                    pawnsContainer.GetChild(i).transform.localPosition = new Vector3(0, -0.5f + i * yOffset, 0);
                    float value = (20 - pawnsContainer.childCount) / 15f * 0.85f;
                    float posY = pawnsContainer.GetChild(i).transform.localPosition.y * Mathf.Clamp(value, 0f, 1f);
                    pawnsContainer.GetChild(i).transform.localPosition = new Vector3(0, posY, -i / 150f);
                }
            else
                for (int i = 1; i < pawnsContainer.childCount; i++)
                    pawnsContainer.GetChild(i).transform.localPosition = new Vector3(0, -0.5f + i * yOffset, 0);
        }
    }
}
