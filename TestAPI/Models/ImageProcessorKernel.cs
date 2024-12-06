using Microsoft.SemanticKernel;

namespace TestAPI.Models
{
    public class ImageProcessorKernel
    {
        private readonly Kernel kernel;
        public ImageProcessorKernel(Kernel kernel)
        {
            this.kernel = kernel;
        }

        public IAsyncEnumerable<StreamingKernelContent> InvokePromptStreamingAsync(
            string promptTemplate,
            KernelArguments? arguments = null)
            => this.kernel.InvokePromptStreamingAsync(promptTemplate, arguments);
    }
}
