
class SlidingUserInterfaceMaster : InputEnabledUserInterface { 
SlidingUserInterface[] interfaces;
public int selectedPosition;
int defaultPosition;
float transitionVelocity;
float transitionDistance;
RectTransform rectTransform;
public static bool locked;
bool afterDragLock;

void Start () 
{
	interfaces = gameObject.GetComponentsInChildren<SlidingUserInterface> ();
  	rectTransform = gameObject.GetComponent<RectTransform> ();
	defaultPosition = selectedPosition;
}

void Update () 
{
	rectTransform.anchoredPosition = Vector2.right * Mathf.SmoothDamp(rectTransform.anchoredPosition.x, (defaultPosition - selectedPosition) * Screen.width, ref transitionVelocity, 0.2f, Mathf.Infinity );
	if (!locked && !afterDragLock) 
	{
		ProcessInput();
	}
	
	if (!dragging) 
	{
		afterDragLock = false;
	}
}

void ProcessInput () 
{
	int moveDirection = Mathf.Sign(Mathf.Floor((initialInputPosition.screen - currentInputPosition.screen) / (Screen.width / 3.0f)));
	if (moveDirection != 0) 
	{
		afterDragLock = true;
		selectedPosition += moveDirection;
		
		int widthOffset;
		
		for (int i = 0; i < interfaces.Length; i++) 
		{
			int absolutePosition = i + widthOffset - defaultPosition;
			int relativePosition = absolutePosition - selectedPosition;
			interfaces[i].rect.anchoredPosition = Vector2.right * absolutePosition * width;
			widthOffset += interfaces[i].width - 1;
		}
	}
}
}
