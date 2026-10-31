using System.ComponentModel;
using System.Diagnostics;

namespace MariaDigital.LocalScanner;

/// <summary>Runs `ollama serve` as a child process; disposing it stops the server.</summary>
public sealed class OllamaServerProcess : IDisposable
{
	private readonly Process _process;
	private bool _disposed;

	private OllamaServerProcess(Process process)
	{
		_process = process;
	}

	public static IDisposable Start()
	{
		var startInfo = new ProcessStartInfo("ollama", "serve")
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};

		try
		{
			var process =
				Process.Start(startInfo)
				?? throw new InvalidOperationException("Could not start `ollama serve`.");
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			return new OllamaServerProcess(process);
		}
		catch (Win32Exception ex)
		{
			throw new InvalidOperationException(
				"Could not run `ollama`. Is Ollama installed and on PATH?",
				ex
			);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		if (!_process.HasExited)
		{
			_process.Kill(entireProcessTree: true);
		}

		_process.Dispose();
	}
}
