using Godot;
using System;
using System.Text;

public class WebSocketThread : Node
{
    private static WebSocketClient client = new WebSocketClient();

    chat log = new chat();
    TextEdit wsURI = new TextEdit();
    mainScript main = new mainScript();

    // signal callbacks
    private void SignalClientConnected(string msg) {
        log.Println("INFO: Client connected, connection status: " + msg);
    }

    private void SignalServerCloseRequest() {
        log.Println("WARN: Closing connection by request of server");
    }

    private void SignalClientClosed(bool cleanClose) {
        log.Println("INFO: Connection Closed.");
    }

    private void SignalClientError() {
        log.Println("Warn: Connection error.");
    }

    private void SignalPacketReceived() {
        Encoding toText = Encoding.UTF8;
        Boolean isString = false;
        byte[] packet;

        try {
            packet = client.GetPeer(1).GetPacket();

        } catch (Exception e) {
            log.Println(String.Format("ERROR: Could not GetPacket(): {0}", e.ToString()));
            return;
        }

        // not sure if this is necessary, as WasStringPacket returns a bool
        try {
            isString = client.GetPeer(1).WasStringPacket();
        } catch (Exception e) {
            log.Println(String.Format("ERROR: Couldn't tell type of packet: {0}", e.ToString()));
        }

        if (isString) {
            string msg;
            msg = toText.GetString(packet);
            log.Println(String.Format(toText.GetString(packet)));
        }
    }

	public Error ConnectToWs() {
        Error connectErr;
        string uri;
        uri = main.GetConnectionURI();
        log.Println(String.Format("INFO: Connecting to {0}", uri));
        try {
            connectErr = client.ConnectToUrl(uri, null, false);
        } catch (Exception e) {
            log.Println(String.Format("ERROR: Could not connect to chat!: {0}", e.ToString()));
            return Error.Failed;
        }

        if (connectErr != Error.Ok) {
            GD.Print("ERROR: Connection error: " + connectErr);
            return connectErr;
        }
		return Error.Ok;
}

    public override void _Ready()
    {
        log = (chat)GetNode("/root/main/MarginContainer/VBoxContainer/chat");
        main = (mainScript)GetNode("/root/main");

        client.Connect("connection_established", this, nameof(SignalClientConnected));
        client.Connect("connection_error", this, nameof(SignalClientError));
        client.Connect("connection_closed", this, nameof(SignalClientClosed));
        client.Connect("server_close_request", this, nameof(SignalServerCloseRequest));
        client.Connect("data_received", this, nameof(SignalPacketReceived));

    }
}
