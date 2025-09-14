using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace HalliGalli_Server
{
     internal class HalliGalli_Server
    {
        public static void Main()
        {
            Manager.Instance.Init();
        }
    }
}