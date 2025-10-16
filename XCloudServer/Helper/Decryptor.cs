using System.Text;

public static class Decryptor {
    public static string XorEncrypt(string text, int k = 73) {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] newBytes = new byte[bytes.Length];

        for (int i = 0; i < bytes.Length; i++) {
            newBytes[i] = (byte)(bytes[i] ^ k);
        }

        return Convert.ToBase64String(newBytes);
    }

    public static string XorDecrypt(string base64, int k = 73) {
        byte[] bytes = Convert.FromBase64String(base64);
        byte[] newBytes = new byte[bytes.Length];

        for (int i = 0; i < bytes.Length; i++) {
            newBytes[i] = (byte)(bytes[i] ^ k);
        }

        return Encoding.UTF8.GetString(newBytes);
    }
}