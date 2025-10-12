using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TutorialSpike : BaseTutorial
{
  [SerializeField] GameObject? tutorialPanel;
  [SerializeField] SpikeVfx? spike;
  [SerializeField] RawImage? image;
  [SerializeField] GameManager? gameManager;
  [SerializeField] float highlightSize = 5;
  [SerializeField] Material? highlightMat;
  Material? blurMat;
  RenderTexture? rendTex;
  RenderTexture? blurTex;
  RenderTexture? spikeTex;
  CommandBuffer? cmdBuffer;
  int blurIteration = 5;

  void Awake()
  {
    cmdBuffer = new CommandBuffer();
    rendTex = new RenderTexture(
      (int)ARENA_DEFAULT_SIZE.WIDTH,
      (int)ARENA_DEFAULT_SIZE.HEIGHT,
      Util.GetGraphicFormat(),
      Util.GetDepthFormat()
    );
    blurTex = new RenderTexture(
      (int)ARENA_DEFAULT_SIZE.WIDTH,
      (int)ARENA_DEFAULT_SIZE.HEIGHT,
      Util.GetGraphicFormat(),
      Util.GetDepthFormat()
    );
    if (image)
    {
      image.texture = rendTex;
      tutorialPanel.gameObject.SetActive(false);
    }

    if (spike)
    {
      spikeTex = spike.GetTexture();
    }
    Util.ClearDepthRT(rendTex, cmdBuffer, true);

    setMat();
  }

  void setMat()
  {
    if (!blurMat)
    {
      Shader shader = Shader.Find("Transparent/Blur");
      blurMat = new Material(shader);
    }

    blurMat.SetFloat("_Intensity", 0f);

    if (!highlightMat)
    {
      Shader shader = Shader.Find("Transparent/HighlightShader");
      highlightMat = new Material(shader);
    }

    highlightMat.SetTexture("_MainTex", spikeTex);
    highlightMat.SetTexture("_BlurTex", blurTex);
  }

  void render()
  {
    if (cmdBuffer == null) return;

    cmdBuffer.Clear();

    int blur1 = Shader.PropertyToID("_Temp1");
    int blur2 = Shader.PropertyToID("_Temp2");

    cmdBuffer.GetTemporaryRT(blur1, (int)ARENA_DEFAULT_SIZE.WIDTH, (int)ARENA_DEFAULT_SIZE.HEIGHT, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
    cmdBuffer.GetTemporaryRT(blur2, (int)ARENA_DEFAULT_SIZE.WIDTH, (int)ARENA_DEFAULT_SIZE.HEIGHT, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);

    cmdBuffer.SetRenderTarget(rendTex);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
    cmdBuffer.SetRenderTarget(blurTex);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
    cmdBuffer.SetRenderTarget(blur1);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
    cmdBuffer.SetRenderTarget(blur2);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

    cmdBuffer.Blit(spikeTex, blur1);

    for (var i = 0; i < blurIteration; i++)
    {
      cmdBuffer.Blit(blur1, blur2, blurMat, 0);
      cmdBuffer.Blit(blur2, blur1, blurMat, 0);
    }
    cmdBuffer.Blit(blur1, blurTex);

    cmdBuffer.ReleaseTemporaryRT(blur1);
    cmdBuffer.ReleaseTemporaryRT(blur2);

    cmdBuffer.Blit(null, rendTex, highlightMat, 0);

    cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
    cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);

    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  public override void Init()
  {
    if (!SaveManager.Instance.shouldDoTutorial)
    {
      Next();
      return;
    }
    PersistentData.Instance.isPaused = true;
    StartCoroutine(showSpikeTutorial());
  }

  IEnumerator showSpikeTutorial()
  {
    yield return null;
    tutorialPanel.gameObject.SetActive(true);
    StartCoroutine(startHighlightAnim());
  }

  IEnumerator startHighlightAnim()
  {
    yield return null;

    float timePass = 0;

    while (true)
    {
      yield return null;
      render();
      timePass += Time.deltaTime * 3f;
      float dist = (Mathf.Sin(timePass) + 1f) / 2f;
      blurMat.SetFloat("_Intensity", dist * highlightSize);
    }
  }

  public void onSpikeTutorialConfirm()
  {
    StopAllCoroutines();
    StartCoroutine(hideSpikeTutorial());
  }

  IEnumerator hideSpikeTutorial()
  {
    yield return null;
    tutorialPanel.gameObject.SetActive(false);
    PersistentData.Instance.isPaused = false;
    Next();
  }

  public override void OnChange()
  {
  }
}
