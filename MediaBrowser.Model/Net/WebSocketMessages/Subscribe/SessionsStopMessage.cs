using MediaBrowser.Model.Session;

namespace MediaBrowser.Model.Net.WebSocketMessages.Subscribe;

/// <summary>
/// Sessions stop message.
/// TODO use SessionInfo for Data.
/// </summary>
public class SessionsStopMessage : WebSocketMessage<object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsStopMessage"/> class.
    /// </summary>
    /// <param name="data">Session info.</param>
    public SessionsStopMessage(object data)
        : base(data)
    {
    }

    /// <inheritdoc />
    public override SessionMessageType MessageType => SessionMessageType.SessionsStop;
}
