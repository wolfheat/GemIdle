using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

public class GameController : MonoBehaviour
{
    private const float TickTime = 1f;
    private float tickTimer = 0f;

    [SerializeField] private BaseCard ghostCard; 
    [SerializeField] private Transform ghostHolder; 
	
	
	private Card mimicCard = null; 

	public static GameController Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;

		//Initiate Stats
		Stats.Initiate();

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
			//Debug.Log("Mouse Released this frame");
			StopDrag();
        }

		if (mimicCard != null) {
			Vector2Int highlight = GetHighlightIndex();
            GameAreaController.Instance.HighlightSlot(highlight.x,highlight.y);

			ghostCard.transform.position = Mouse.current.position.ReadValue();


            // Scale it to fit the Box
            RectTransform rect = ghostCard.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);

		}
		
    }

    private void Tick()
    {
		//Debug.Log("TICK");
		GameAreaController.Instance.Tick();
    }

    public void StartDrag(Card cardToDrag)
	{
		// Request to start Dragging this card
		//Debug.Log("Mouse pressed: [" + Mouse.current.position.ReadValue().x+"," + Mouse.current.position.ReadValue().y + "]");

		
		//ghostCard.gameObject.SetActive(true);

		// Try instantiating and cloning the dragged card

		if(ghostCard != null)
			Destroy(ghostCard.gameObject);

		ghostCard = Instantiate(cardToDrag, ghostHolder);


		mimicCard = cardToDrag;
		// Deactivate the dragged one
        mimicCard?.gameObject.SetActive(false);

        // Set Ghostcard to mimic this Card
        ghostCard.Mimic(cardToDrag);

		// Hide the dragged card - fix later
		if(mimicCard == null)
			Debug.Log("Lost Mimic Card");
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
		if (mimicCard == null) {
			Debug.Log("Stop Drag, no mimic Card issue....");
			return;
		}


        // Request to stop dragging
        ghostCard.gameObject.SetActive(false);

        Vector2Int localIndex = GetHighlightIndex();

        // Move or return the mimicedCard 
        mimicCard?.gameObject.SetActive(true);

        Debug.Log("Activate Card again");

        // Also need to handle invalid placement to unset the mimic
        if (localIndex.x < 0 || localIndex.y < 0 || localIndex.x >= 7 || localIndex.y >= 5) {
			// return Item its outside
			Debug.Log("Return Item its outside.");

			if(localIndex.y >= 5) {
				// Place the item back in inventory
				Debug.Log("Place in Inventory + remove it from old position");
                InventoryController.Instance.PlaceCard(mimicCard);
			}
			else {

                SoundMaster.Instance.PlaySound(SoundName.PlaceError);
            }
			return;
		}

		if(mimicCard != null) {
			Card placedCard = GameAreaController.Instance.Occupier(localIndex.x, localIndex.y);

			if (placedCard != null) {

                // SwapCards
				GameAreaController.Instance.SwapCards(mimicCard, placedCard);
            }
			else {
				GameAreaController.Instance.PlaceCard(mimicCard,localIndex.x,localIndex.y);
            }
        }
		else {

			Debug.Log("Mimic is NUll shouldnt reach here");
		}
		mimicCard = null;
    }
}
