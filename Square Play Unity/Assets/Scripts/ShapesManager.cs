using System;
using System.Collections.Generic;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    [HideInInspector]
    
    public GameObject mPiecePrefab;

    private List<BaseShape> player1Pieces = new List<BaseShape>();
    private List<BaseShape> player2Pieces = new List<BaseShape>();
    private List<BaseShape> player3Pieces = new List<BaseShape>();
    private List<BaseShape> player4Pieces = new List<BaseShape>();
    private List<BaseShape> playablePieces = new List<BaseShape>();
    public List<CellClass> occupiedCells;

    private string[] mPieceOrder = new string[2]
    {
        "6","H",
    };

    private Dictionary<string, Type> pieceLibrary = new Dictionary<string, Type>()
    {
        //{"6",  typeof(Six)},
        //{"H",  typeof(H)},
    };

    public void Setup(BoardClass board)
    {
        player1Pieces = CreatePieces(Color.white, new Color32(80, 124, 159, 255),1);

        player2Pieces = CreatePieces(Color.black, new Color32(210, 95, 64, 255),2);

        player3Pieces = CreatePieces(Color.red, new Color32(210, 95, 64, 255),3); //other team color.

        player4Pieces = CreatePieces(Color.green, new Color32(210, 95, 64, 255),4); //another different team color.

        occupiedCells = board.occupiedCells;

        SwitchTurn(1);
    }

    private List<BaseShape> CreatePieces(Color teamColor, Color32 spriteColor,int teamNum)
    {
        //Fix
        List<BaseShape> newPieces = new List<BaseShape>();

        for (int i = 0; i < mPieceOrder.Length; i++)
        {
            string key = mPieceOrder[i];
            Type pieceType = pieceLibrary[key];
            BaseShape newPiece = CreatePiece(pieceType);
            newPieces.Add(newPiece);
            newPiece.Setup(teamColor, spriteColor, teamNum, this,newPiece.transform.position);
        }

        return newPieces;
    }

    private BaseShape CreatePiece(Type pieceType)
    {
        //Fix
        GameObject newPieceObject = Instantiate(mPiecePrefab);

        newPieceObject.transform.SetParent(transform);
        newPieceObject.transform.localScale = new Vector3(1, 1, 0);
        newPieceObject.transform.localRotation = Quaternion.identity;

        BaseShape newPiece = (BaseShape)newPieceObject.AddComponent(pieceType);

        return newPiece;
    }

    private void PlacePieces(int pawnRow, int royaltyRow, List<BaseShape> pieces, BoardClass board)
    {
        //Fix
        for (int i = 0; i < 8; i++)
        {
            // Place pawns    
            //pieces[i].Place(board.mAllCells[i, pawnRow]);

            // Place royalty
            //pieces[i + 8].Place(board.mAllCells[i, royaltyRow]);
        }
    }

    private void SetInteractive(List<BaseShape> allPieces, bool value)
    {
        foreach (BaseShape piece in allPieces)
            piece.enabled = value;
    }


    public void SwitchTurn(int playerNum)
    {
        playerNum = (playerNum + 1) % 4;
        SetInteractive(player1Pieces, playerNum == 1);

        SetInteractive(player2Pieces, playerNum == 2);

        SetInteractive(player3Pieces, playerNum == 3);

        SetInteractive(player4Pieces, playerNum == 4);

        // Set promoted interactivity
        foreach (BaseShape piece in playablePieces)
        {
            piece.enabled = playerNum==piece.teamNum;
        }
    }

    public void ResetPieces()
    {
        foreach (BaseShape piece in playablePieces)
        {
            piece.Kill();
            Destroy(piece.gameObject);
        }

        playablePieces.Clear();

        foreach (BaseShape piece in player1Pieces)
            piece.Reset();
        foreach (BaseShape piece in player2Pieces)
            piece.Reset();
        foreach (BaseShape piece in player3Pieces)
            piece.Reset();
        foreach (BaseShape piece in player4Pieces)
            piece.Reset();
    }

}