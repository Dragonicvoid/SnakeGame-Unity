using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
  [SerializeField] List<Sprite> volumeSprite = new List<Sprite>();
  [SerializeField] Image? image;

  float percentPerVol = 0.5f;

  int currVolume = 2;

  void Awake()
  {
    currVolume = SaveManager.Instance.SaveData.AudioVolume ?? 2;
    SaveManager.Instance.SaveData.AudioVolume = currVolume;
    SaveManager.Instance.Save();

    updateVolume();
    updateAudioSprite();
  }

  public void onButtonClick()
  {
    currVolume = (currVolume + 1) % volumeSprite.Count;
    updateVolume();
    updateAudioSprite();
    AudioManager.Instance.PlaySFX(ASSET_KEY.SFX_BUTTON_CLICK);
  }

  void updateVolume()
  {
    AudioManager.Instance.SetVolume((float)currVolume / (volumeSprite.Count - 1));
    SaveManager.Instance.SaveData.AudioVolume = currVolume;
    SaveManager.Instance.Save();
  }

  void updateAudioSprite()
  {
    if (image == null) return;

    image.sprite = volumeSprite[currVolume];
  }
}
