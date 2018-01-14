using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.IO.Path;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Cake.VisualStudio.Helpers;
using System.ComponentModel.Composition;

namespace Cake.VisualStudio.Language
{
    [ContentType(Constants.CakeContentType)]
    [Export(typeof(ILanguageClient))]
    class CakeLanguageClient : ILanguageClient
    {
        public string Name => Constants.LanguageClientName;

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            var info = new ProcessStartInfo();
            info.FileName = Combine(@"C:\", "TEMP", "win7-x64", "omnisharp.exe");
            info.Arguments = "-lsp -stdio";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process process = new Process();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.StartInfo = info;

            if (process.Start())
            {
                //process.BeginOutputReadLine();
                return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            }

            return null;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine($"LSP Output: {e.Data}");
        }

        public async Task OnLoadedAsync()
        {
            await StartAsync?.InvokeAsync(this, EventArgs.Empty);
        }
    }
}
