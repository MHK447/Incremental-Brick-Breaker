using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleStoreKitTestTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ovXzkbeQtaapiiboJletoamhtqjDCzc6i2OKeHASQXX6rZm7z8eICXdE2AgPCM0HYXrIjcZsb4mr8IxDlZKTlfq3rZSQkJOSl5GXlZKTlfpg54jrS/phi6n2jAUKqaPbkpF8SOszG6pYAYGeaMF5hGj+v9WeejN7rRuYMMumZFytRTXwbTFoGoVU+Gmrmexr785gE/8XNRoZCP3q1AtPz6SjrKjz1M/SxevJ1JGxkK+mo/WkkP+RsZCvpqP1pKOsqPPUz9LF68lv2Az9Gp7/8Iv8iSx9YneKt3+CFDlbEyr1P0+9iBj/inHTP5XnlxvmiiboJletoaGrpaCQ/5GxkK+mo/WCkK2mqYom6CZXraGhoaWgoyKhr/PUz9LF68nUkL63rZKQkJSQkZGXrKjz1M/SxevJ1JGrkKmmo/WkprMdYHa94D73GZgw19bycgjr0M4Yx7xPqSjYxW4hUi3SR6e6zoayV5IRq5CppqP1pKazovXzkbeQtaapiiZvxJzNIPmTLGqWpoqxuy3pbqhfxsDWl/JMXwy5/ay1jyqvVSlod4u9qqyo89TP0sXrydSRsZCvpqP1pKvUkbGQr6aj9aSqrKjz1M/SxevJ1HPWsFJQyUxtwjnRWcAqJOiYZB0rKwxVPxCpmAPO22uG8Kq6QSlx0taM1UbsEbJzXUO//q3dGPh3ANxE31etoaGrpaCjIqGhoBKgQJxRSEnC6CZXraGpobao89TP0sXrydSQIqGyI3Yn2M05Hf4TqkA4blZKYy7NySxB0Xvq5VONuTaj9iEG5E7V1/vzkCKj1JAiovwAo6KhoqKhoZCtpqlLF+AhnSed0PFxZP0tBXzfyvi6fseFGQvMk8UNeGI62OivSWxhh61mYltG/7JhoqOhoKEDm5CZkK+mo/WhX6Sko6KiJJC2pqP1vYWhoV+krEfPQLtGrchthc5j1qhV6qsO2tSckboxIBkfRaxrnBuu5MKGWqSJFumQqqaoi6ahpaWno6OQraapiiboJuEF3qktGp2dbdD3eE1PdXrXaDOkoJAioaqiIqGhoHvfMJyM0697N4Yl2OhuYGWyscqvrA6PpWrP2sTfgL2zoaFfpKWQo6GhX5CupqP1va+hZAmrE2/Cr1sUHFd7JP6IS+etUU2RsZCvpqP1pKusqPPUz9LF68nUkb5MizGlB53v");
        private static int[] order = new int[] { 5,40,26,7,37,15,23,20,26,26,25,33,40,13,23,37,32,22,33,36,43,43,26,34,36,39,31,40,41,36,37,42,35,40,36,42,42,37,43,42,43,41,43,43,44 };
        private static int key = 160;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

