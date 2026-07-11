using UnityEngine;
using UnityEngine.UI;
using Wolfheat.StartMenu;

public class AudioButtonController : MonoBehaviour
{
    [SerializeField] private Image soundImage; 
    [SerializeField] private Sprite soundOnSprite; 
    [SerializeField] private Sprite soundOffSprite; 

    public void ToggleMusic()
    {
        SoundMaster.Instance.ToggleMusic();
        soundImage.sprite = SoundMaster.Instance.IsMusicOn ? soundOnSprite : soundOffSprite;
    }
}
