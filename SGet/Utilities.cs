
using System.Net.Sockets;
using System.Net;

namespace SGet
{
    public static class Utilities
    {
        // Get the number of occurences of a specified character in a string
        public static int CountOccurence(string input, char c)
        {
            int count = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (c == input[i])
                    count++;
            }
            return count;
        }

        public static bool IsInternetAvailable()
        {
            return IsInternetAvailable("www.google.com");
        }

        public static bool IsInternetAvailable( string urlToCheck )
        {
            try
            {
                Dns.GetHostEntry(urlToCheck); //using System.Net;
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
