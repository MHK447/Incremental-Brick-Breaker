// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("FqQnBBYrIC8MoG6g0SsnJycjJiXhTUBY4IhmKFRdYiIlL9CDxL8h5TJcLS7JFbl8YcepW+fmffgKARdmeb0UcihNAfstph9jZJyVBJqPLQPHmmR+9gn0PcIGU+9LJ35XDtHVThwuC6/VmY/y1kIxkuiV3Egl0QNUgxmzUL6CVtwzF5DAFM+qNBfF4iFaH961iUnhCP+XYe+zegmw71wtFzA9dvpA8hVq5oDhJYQgjALgIB8icaIWSgCYB0pCEpUomuNYGuVbTXdEUzU+qx201XlOP2M/xqSRtCOKrZ8KO2spTWvjhRCc38HplwoD3aPWpCcpJhakJywkpCcnJpIUzeImKgtmzjuUKhQdNTzdm8686ksC8zDYQykQq+NLM88VYSQlJyYn");
        private static int[] order = new int[] { 0,9,5,5,6,7,6,7,12,11,12,13,13,13,14 };
        private static int key = 38;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
