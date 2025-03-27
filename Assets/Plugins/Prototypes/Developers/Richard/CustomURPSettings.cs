using UnityEngine;
using UnityEngine.Rendering;

namespace Scarecrow
{
    public class CustomURPSettings : MonoBehaviour
    {
        [SerializeField] private RenderPipelineAsset customPipelineAsset;

        private void OnEnable()
        {
            if (customPipelineAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = customPipelineAsset;
            }
        }

        private void OnDisable()
        {
            // Optionally reset to the default pipeline
            GraphicsSettings.defaultRenderPipeline = null;
        }
    }
}
