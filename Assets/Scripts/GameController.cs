using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    private const float TickTime = 1f;
    private float tickTimer = 0f;

    [SerializeField] private BaseCard ghostCard; 
	
	
	private Card mimicedCard = null; 

	public static GameController Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}


    private void Update()
    {
        // Tick
        tickTimer += Time.deltaTime;

        if (tickTimer >= TickTime) {
			Tick();
			tickTimer = 0f;
		}


        if (Mouse.current.leftButton.wasReleasedThisFrame) {
			Debug.Log("Mouse Released this frame");
			StopDrag();
        }

		if (mimicedCard != null) {
			Vector2Int highlight = GetHighlightIndex();
            GameAreaController.Instance.HighlightSlot(highlight.x,highlight.y);
        }
    }

    private void Tick()
    {
		GameAreaController.Instance.Tick();
    }

    public void StartDrag(Card cardToDrag)
	{
		// Request to start Dragging this card
		//Debug.Log("Mouse pressed: [" + Mouse.current.position.ReadValue().x+"," + Mouse.current.position.ReadValue().y + "]");

		
		ghostCard.gameObject.SetActive(true);


		mimicedCard = cardToDrag;

        mimicedCard?.gameObject.SetActive(false);


        // Set Ghostcard to mimic this Card
        ghostCard.Mimic(cardToDrag);

		// Hide the dragged card - fix later

	}
	
	private Vector2Int GetHighlightIndex()
	{

        RectTransform gameArea = GameAreaController.Instance.GetComponent<RectTransform>();

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameArea,
            Mouse.current.position.ReadValue(),
            null,   // Use null if the Canvas is Screen Space - Overlay
            out localPoint
        );

        Vector2 localPos = localPoint + new Vector2(984 / 2, 704 / 2);
		Vector2Int localIndex = new Vector2Int(Mathf.FloorToInt(localPos.x / 140), Mathf.FloorToInt(-localPos.y / 140 + 5));

		return localIndex;
    }


	public void StopDrag()
	{
		if (mimicedCard == null)
			return;


        // Request to stop dragging
        ghostCard.gameObject.SetActive(false);

        Vector2Int localIndex = GetHighlightIndex();

        // Move or return the mimicedCard 
        mimicedCard?.gameObject.SetActive(true);

		// Also need to handle invalid placement to unset the mimic
        if (localIndex.x < 0 || localIndex.y < 0 || localIndex.x >= 7 || localIndex.y >= 5) {
			// return Item its outside
			Debug.Log("Return Item its outside.");

			if(localIndex.y >= 5) {
				// Place the item back in inventory
				Debug.Log("Place in Inventory + remove it from old position");
                InventoryController.Instance.PlaceCard(mimicedCard);
            }
			return;
		}

		if(mimicedCard != null) {
			GameAreaController.Instance.PlaceCard(mimicedCard,localIndex.x,localIndex.y);
        }
		else
			Debug.Log("Mimic is NUll shouldnt reach here");
        mimicedCard = null;
    }
}
