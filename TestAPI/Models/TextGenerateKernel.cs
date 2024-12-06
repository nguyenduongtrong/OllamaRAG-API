using Microsoft.SemanticKernel;

namespace TestAPI.Models
{
    public class TextGenerateKernel
    {
        private readonly Kernel kernel;
        public TextGenerateKernel(Kernel kernel)
        {
            this.kernel = kernel;
        }

        public IAsyncEnumerable<StreamingKernelContent> InvokePromptStreamingAsync(
            string promptTemplate,
            KernelArguments? arguments = null)
            => this.kernel.InvokePromptStreamingAsync(promptTemplate, arguments);
    }
}
