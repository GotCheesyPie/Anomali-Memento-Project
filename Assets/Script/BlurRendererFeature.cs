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
        BlurMaterial = settings.blurMaterial;
        // Kita inisialisasi pass, tapi material akan dikirim saat AddRenderPasses atau setup
        if (settings.blurMaterial != null)
        {
            blurRenderPass = new BlurRenderPass(settings.blurMaterial);
            blurRenderPass.renderPassEvent = settings.renderPassEvent;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blurMaterial == null)
        {
            // Warning ini sering spam, bisa di-comment kalau mengganggu
            // Debug.LogWarningFormat("Missing Blur Material. Blur pass will not be executed.");
            return;
        }
        
        // Penting: Di Unity 6/URP 17, kita harus set input texture di sini jika perlu
        blurRenderPass.SetTarget(renderer.cameraColorTargetHandle);
        
        renderer.EnqueuePass(blurRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        // Bersihkan memory RTHandle saat game berhenti atau feature dihancurkan
        blurRenderPass?.Dispose();
    }

    // The class that performs the blur
    class BlurRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle source;      // Ganti RenderTargetIdentifier/Handle lama
        private RTHandle tempTexture; // Ganti RenderTargetHandle lama

        public BlurRenderPass(Material material)
        {
            this.material = material;
        }

        // Method baru untuk menerima target kamera (pengganti OnCameraSetup yang lama)
        public void SetTarget(RTHandle cameraColorTargetHandle)
        {
            this.source = cameraColorTargetHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Di Unity 6, pengambilan target kamera sebaiknya lewat AddRenderPasses atau Configure
            // Tapi kita sudah handle via SetTarget di atas.
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;
            if (renderingData.cameraData.isSceneViewCamera) return;

            CommandBuffer cmd = CommandBufferPool.Get("BlurPass");

            // Setup Descriptor
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // --- PERUBAHAN UTAMA DI SINI ---
            // 1. Alokasi texture menggunakan sistem RTHandle baru (ReAllocateIfNeeded)
            // Ini menggantikan cmd.GetTemporaryRT
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, opaqueDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempBlurTexture");

            // 2. Gunakan Blitter API (Standard baru Unity 6) atau cmd.Blit dengan RTHandle
            // Kita pakai cmd.Blit biasa agar Shader kamu tidak perlu diubah (_MainTex vs _BlitTexture)
            
            // Copy screen ke temp
            cmd.Blit(source, tempTexture);

            // Apply blur dari temp balik ke screen
            cmd.Blit(tempTexture, source, material);
            // -------------------------------

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup di Unity 6 berbeda, kita buat fungsi Dispose manual
        public void Dispose()
        {
            tempTexture?.Release();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // cmd.ReleaseTemporaryRT tidak lagi digunakan untuk RTHandle
            // Memory diurus oleh Dispose atau sistem RTHandle
        }
    }
}