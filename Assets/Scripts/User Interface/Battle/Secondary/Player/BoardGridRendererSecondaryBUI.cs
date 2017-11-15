using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGridRendererSecondaryBUI : PlayerIDBoundSecondaryBUI
{
    public Material gridMaterial;
    public float defaultDisableTime;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                ResetWorldSpaceParent();
                AddGrid();
                break;
            case UIState.DISABLING:
                disableTime = defaultDisableTime;
                break;
            case UIState.DISABLED:
                ResetWorldSpaceParent();
                break;
        }
    }

    public void AddGrid()
    {
        float lineWidth = 1.00f - MiscellaneousVariables.it.boardTileSideLength;
        float lineLength = managedPlayer.board.tiles.GetLength(0);
        float startingPosition = (-managedPlayer.board.tiles.GetLength(0) / 2.0f);
        for (int x = 1; x < managedPlayer.board.tiles.GetLength(0); x++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.transform.SetParent(worldSpaceParent);

            line.transform.localScale = new Vector3(lineWidth, lineLength, 1.0f);
            line.transform.localRotation = new Quaternion(1, 0, 0, 1);
            line.transform.localPosition = Vector3.right * (startingPosition + x) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

            line.GetComponent<Renderer>().material = gridMaterial;
        }

        lineLength = managedPlayer.board.tiles.GetLength(1);
        startingPosition = (-managedPlayer.board.tiles.GetLength(1) / 2.0f);
        for (int y = 1; y < managedPlayer.board.tiles.GetLength(1); y++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.transform.SetParent(worldSpaceParent);

            line.transform.localScale = new Vector3(lineLength, lineWidth, 1.0f);
            line.transform.localRotation = new Quaternion(1, 0, 0, 1);
            line.transform.localPosition = Vector3.forward * (startingPosition + y) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

            line.GetComponent<Renderer>().material = gridMaterial;
        }

        worldSpaceParent.transform.position = managedPlayer.transform.position;
    }

    float disableTime;
    protected override void Update()
    {
        base.Update();
        if (State == UIState.DISABLING)
        {
            if (disableTime < 0)
            {
                State = UIState.DISABLED;
            }
            disableTime -= Time.deltaTime;
        }
    }
}
