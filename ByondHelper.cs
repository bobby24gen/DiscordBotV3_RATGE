namespace DiscordBotV3;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ByondHelper
{
    private string _host = "";
    private int _port = 1337;

    public bool IsServerOnline()
    {
        try
        {
            using (var client = new TcpClient())
            {
                // Set a timeout for the connection attempt
                client.SendTimeout = 3000;
                client.ReceiveTimeout = 3000;

                // Attempt to connect to the server
                client.Connect(_host, _port);

                // If successful, the server is online
                return true;
            }
        }
        catch (SocketException)
        {
            // Connection failed - server may be offline or unreachable
            return false;
        }
    }

    public string QueryServerStatus()
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(_host, _port);

                NetworkStream networkStream = client.GetStream();

                string message = "ping";
                byte[] data = Encoding.ASCII.GetBytes(message);

                networkStream.Write(data, 0, data.Length);

                byte[] buffer = new byte[256];
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                return response;
                
            }
        }
        catch (SocketException)
        {
            return "Could not connect to server.";
        }
        catch (Exception ex)
        {
            return $"An error occurred: {ex.Message}";
        }
    }

    public int GetPlayerCount()
    {
        string response = QueryServerStatus();

        // Example response format might be something like "players=5"
        if (response.Contains("players="))
        {
            string playerCountString = response.Split(new[] { "players=" }, StringSplitOptions.None)[1];
            if (int.TryParse(playerCountString, out int playerCount))
            {
                return playerCount;
            }
        }

        return -1; // Return -1 if the player count couldn't be parsed
    }


}