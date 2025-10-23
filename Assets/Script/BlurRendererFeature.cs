using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRendererFeature : ScriptableRendererFeature
{
    // Public material that will be controlled by other scripts
    public static Material BlurMaterial;

    [System.Serializable]
    public class BlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Tooltip("A material with the ScreenBlurShader.")]
        public Material blurMaterial = null;
    }

    public BlurSettings settings = new BlurSettings();
    private BlurRenderPass blurRenderPass;

    public override void Create()
    {
        BlurMaterial = settings.blurMaterial; // Make material accessible statically
        blurRenderPass = new BlurRenderPass(settings.blurMaterial);
        blurRenderPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blurMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blur Material. Blur pass will not be executed.");
            return;
        }
        // Hand the pass to the renderer.
        renderer.EnqueuePass(blurRenderPass);
    }

    // The class that performs the blur
    class BlurRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;

        public BlurRenderPass(Material material)
        {
            this.material = material;
            tempTexture.Init("_TempBlurTexture");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            // Don't run in the editor Scene view
            if (renderingData.cameraData.isSceneViewCamera) return;

            CommandBuffer cmd = CommandBufferPool.Get("BlurPass");

            // Copy the screen to a temporary texture
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(tempTexture.id, opaqueDesc);
            cmd.Blit(source, tempTexture.Identifier());

            // Apply the blur from the temporary texture back to the screen
            cmd.Blit(tempTexture.Identifier(), source, material);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}