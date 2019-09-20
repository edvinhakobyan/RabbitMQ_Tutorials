using System;
using System.IO;

namespace GenerationMesage
{
    class GenerationMesage
    {
        private static string GetKeyValueString()
        {
            int r = new Random().Next(4);

            if (r == 0)
                return "info " + Path.GetRandomFileName();
            if (r == 1)
                return "warning " + Path.GetRandomFileName();
            if (r == 2)
                return "error " + Path.GetRandomFileName();
            else
                return Path.GetRandomFileName() + Path.GetRandomFileName();
        }
    }
}
