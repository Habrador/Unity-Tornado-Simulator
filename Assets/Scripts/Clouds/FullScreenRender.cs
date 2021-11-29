using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class FullScreenRender : MonoBehaviour 
{
    [SerializeField]
    private Material material;

    [SerializeField, Range(10, 200)]
    private int width = 100;

    [SerializeField, Range(10, 200)]
    private int height = 100;

    private void Awake()
    {
        int lowResRenderTarget = Shader.PropertyToID("_LowResRenderTarget");

        CommandBuffer cb = new CommandBuffer();

        cb.GetTemporaryRT(lowResRenderTarget, this.width, this.height, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);

        // Blit the low-res texture into itself, to re-draw it with the current material
        cb.Blit(lowResRenderTarget, lowResRenderTarget, this.material);

        // Blit the low-res texture into the camera's target render texture, effectively rendering it to the entire screen
        cb.Blit(lowResRenderTarget, BuiltinRenderTextureType.CameraTarget);

        cb.ReleaseTemporaryRT(lowResRenderTarget);

        // Tell the camera to execute our CommandBuffer before the forward opaque pass - that is, just before normal geometry starts rendering
        this.GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
    }
}
