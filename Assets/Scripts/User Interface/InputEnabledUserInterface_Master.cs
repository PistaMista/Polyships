using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEnabledUserInterface_Master : InputEnabledUserInterface
{
    protected override void Update ()
    {
        base.Update();
        if (Application.isMobilePlatform)
        {
            MobileInput();
        }
        else
        {
            PCInput();
        }

        Shared();
    }

    void PCInput ()
    {
        currentInputPosition.screen = new Vector2( Input.mousePosition.x, Input.mousePosition.y );

        if (beginPress)
        {
            beginPress = false;
        }
        else
        {
            beginPress = Input.GetMouseButtonDown( 0 );
        }

        if (endPress)
        {
            endPress = false;
        }
        else
        {
            endPress = Input.GetMouseButtonUp( 0 );
        }
        pressed = Input.GetMouseButton( 0 );

        inputPoints = Input.GetKey( KeyCode.Space ) ? 2 : 1;
    }


    bool lastState;
    void MobileInput ()
    {
        inputPoints = Input.touchCount;
        if (Input.touchCount > 0)
        {
            pressed = true;
            Touch touch = Input.GetTouch( 0 );
            currentInputPosition.screen = new Vector3( touch.position.x, touch.position.y );
        }
        else
        {
            pressed = false;
        }

        if (beginPress)
        {
            beginPress = false;
        }
        else
        {
            beginPress = !lastState && pressed;
        }

        if (endPress)
        {
            endPress = false;
        }
        else
        {
            endPress = lastState && !pressed;

        }

        lastState = pressed;
    }

    void Shared ()
    {
        if (beginPress)
        {
            initialInputPosition.screen = currentInputPosition.screen;
        }

        tap = endPress && !dragging;
    }
}
