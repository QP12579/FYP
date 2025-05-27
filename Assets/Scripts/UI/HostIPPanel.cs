using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HostIPPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text hostIPText;

    // Start is called before the first frame update
    void Start()
    {
        // Get the local IP address and display it in the hostIPText
        string localIP = "Not available";

        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch
        {
            localIP = "Error retrieving IP";
        }

        if (hostIPText != null)
        {
            hostIPText.text = $"Host IP: {localIP}";
        }
    }
}
