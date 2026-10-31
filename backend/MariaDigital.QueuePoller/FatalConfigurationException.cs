namespace MariaDigital.QueuePoller;

/// <summary>A failure that retrying cannot fix; the tool stops instead of looping.</summary>
public sealed class FatalConfigurationException(string message) : Exception(message);
