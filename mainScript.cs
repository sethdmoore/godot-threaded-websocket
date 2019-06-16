using Godot;
using System;
using System.Text;

public class mainScript : Node2D
{
    chat ch = new chat();
    Godot.TextEdit wsURI = new Godot.TextEdit();
    Button btn = new Button();
    WebSocketThread websocket = new WebSocketThread();
    Thread wsThread = new Thread();

    bool isConnected;

    public override void _Ready()
    {
        ch = (chat)GetNode("MarginContainer/VBoxContainer/chat");

        websocket = (WebSocketThread)GetNode("websocket");
        btn = (Button)GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button");
        isConnected = false;
        wsURI = (Godot.TextEdit)GetNode("MarginContainer/VBoxContainer/HBoxContainer/TextEdit");
        ch.Println(wsURI.GetType().ToString());

        // update state of button
        updateButton();

    }

    private void startWs() {
        wsThread.Start(websocket, "ConnectToWs");
        //wsThread.Start(websocket, "ConnectToWs", wsURI.Text, 1);
    }

    public string GetConnectionURI() {
        return wsURI.Text;
    }

    private void updateButton() {
        if (isConnected) {
            btn.Text = "Disconnect";
        } else {
            btn.Text = "Connect";
        }
    }

    private void _on_Button_button_up()
    {
        if (!isConnected) {
            startWs();
        }
        updateButton();
    }

    public override void _Process(float delta) {
        updateButton();
    }
}
