using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSetUpScreenUserInterface : SlidingUserInterface
{
    public ExclusiveTogglingButtonGroup tutorialButtonGroup;
    public ExclusiveTogglingButtonGroup boardSizeButtonGroup;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        if (GameLoaderUserInterface.newBattleData.attacker.board.tiles == null && state == UIState.ENABLING)
        {
            SetBoardSize(1);
            SetTutorialMode(GameLoaderUserInterface.neverPlayed ? 1 : 0);
        }
    }
    public void SetTutorialMode(int enabled)
    {
        GameLoaderUserInterface.newBattleData.tutorialStage = enabled;
        tutorialButtonGroup.ResetColors(enabled);
    }

    public void SetBoardSize(int size)
    {
        int actualDimensions = MiscellaneousVariables.it.boardSizes[size];
        boardSizeButtonGroup.ResetColors(size);

        Board.BoardData boardData = new Board.BoardData();
        boardData.ownedByAttacker = true;
        boardData.tiles = new Tile.TileData[actualDimensions, actualDimensions];
        for (int x = 0; x < actualDimensions; x++)
        {
            for (int y = 0; y < actualDimensions; y++)
            {
                boardData.tiles[x, y].coordinates = new int[] { x, y };
                boardData.tiles[x, y].ownedByAttacker = true;
            }
        }

        GameLoaderUserInterface.newBattleData.attacker.board = boardData;

        boardData = new Board.BoardData();
        boardData.tiles = new Tile.TileData[actualDimensions, actualDimensions];
        for (int x = 0; x < actualDimensions; x++)
        {
            for (int y = 0; y < actualDimensions; y++)
            {
                boardData.tiles[x, y].coordinates = new int[] { x, y };
            }
        }

        GameLoaderUserInterface.newBattleData.defender.board = boardData;
    }


}
