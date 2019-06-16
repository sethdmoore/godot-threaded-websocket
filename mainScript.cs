using Godot;
using System;
using System.Text;

public class mainScript : Node2D
{
    chat ch = new chat();
    TextEdit wsURI = new TextEdit();
    Button btn = new Button();
    WebSocketThread websocket = new WebSocketThread();

    bool isConnected;

    public override void _Ready()
    {
        ch = (chat)GetNode("MarginContainer/VBoxContainer/chat");
        ch.ClearDialog();
        websocket = (WebSocketThread)GetNode("websocket");
        btn = (Button)GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button");
        isConnected = false;
        wsURI = (TextEdit)GetNode("MarginContainer/VBoxContainer/ConnectInfo/TextEdit");

        // update state of button
        updateButton();

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
        isConnected = !isConnected;
        updateButton();
    }
}
