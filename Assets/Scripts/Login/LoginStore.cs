using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class LoginStore
{
    const string FILE_NAME = "login_session.bin";
    const string ENTROPY_TEXT = "kalye.login.refresh.v1";
    const string KEY_SALT_TEXT = "kalye.login.store.v1";
    const string KEY_FALLBACK_ID = "kalye.login.no_device_id";
    const int KEY_ITERATIONS = 10000;
    const int KEY_BYTES = 32;
    const int IV_BYTES = 16;
    const int PROTECT_UI_FORBIDDEN = 0x1;

    public static void save(string refresh_token)
    {
        if (string.IsNullOrEmpty(refresh_token))
        {
            clear();
            return;
        }

        try
        {
            write_file(protect(Encoding.UTF8.GetBytes(refresh_token)));
        }
        catch (Exception)
        {
            Debug.LogWarning("login: could not save the session on this device");
            clear();
        }
    }

    public static string load()
    {
        var path = file_path();
        try
        {
            if (!File.Exists(path)) return string.Empty;
            return Encoding.UTF8.GetString(unprotect(File.ReadAllBytes(path)));
        }
        catch (Exception)
        {
            clear();
            return string.Empty;
        }
    }

    public static void clear()
    {
        try
        {
            var path = file_path();
            if (File.Exists(path)) File.Delete(path);
            var temp = path + ".tmp";
            if (File.Exists(temp)) File.Delete(temp);
        }
        catch (Exception)
        {
            Debug.LogWarning("login: could not remove the saved session");
        }
    }

    static string file_path()
    {
        return Path.Combine(Application.persistentDataPath, FILE_NAME);
    }

    static void write_file(byte[] data)
    {
        var path = file_path();
        var temp = path + ".tmp";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(temp, data);
        if (File.Exists(path)) File.Delete(path);
        File.Move(temp, path);
    }

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
    [StructLayout(LayoutKind.Sequential)]
    struct DataBlob
    {
        public int size;
        public IntPtr data;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CryptProtectData(ref DataBlob input, string description, ref DataBlob entropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description, ref DataBlob entropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

    [DllImport("kernel32.dll")]
    static extern IntPtr LocalFree(IntPtr handle);

    static byte[] protect(byte[] plain)
    {
        return transform(plain, true);
    }

    static byte[] unprotect(byte[] sealed_data)
    {
        return transform(sealed_data, false);
    }

    static byte[] transform(byte[] input, bool encrypt)
    {
        var entropy_bytes = Encoding.UTF8.GetBytes(ENTROPY_TEXT);
        var input_blob = new DataBlob();
        var entropy_blob = new DataBlob();
        var output_blob = new DataBlob();
        input_blob.size = input.Length;
        input_blob.data = Marshal.AllocHGlobal(input.Length);
        entropy_blob.size = entropy_bytes.Length;
        entropy_blob.data = Marshal.AllocHGlobal(entropy_bytes.Length);

        try
        {
            Marshal.Copy(input, 0, input_blob.data, input.Length);
            Marshal.Copy(entropy_bytes, 0, entropy_blob.data, entropy_bytes.Length);

            var done = encrypt
                ? CryptProtectData(ref input_blob, null, ref entropy_blob, IntPtr.Zero, IntPtr.Zero, PROTECT_UI_FORBIDDEN, out output_blob)
                : CryptUnprotectData(ref input_blob, IntPtr.Zero, ref entropy_blob, IntPtr.Zero, IntPtr.Zero, PROTECT_UI_FORBIDDEN, out output_blob);
            if (!done) throw new CryptographicException();

            var result = new byte[output_blob.size];
            Marshal.Copy(output_blob.data, result, 0, output_blob.size);
            return result;
        }
        finally
        {
            Marshal.FreeHGlobal(input_blob.data);
            Marshal.FreeHGlobal(entropy_blob.data);
            if (output_blob.data != IntPtr.Zero) LocalFree(output_blob.data);
        }
    }
#else
    static bool weak_warned;
    static byte[] key_cache;

    static byte[] protect(byte[] plain)
    {
        warn_weak();
        using (var aes = Aes.Create())
        {
            aes.Key = derive_key();
            aes.GenerateIV();
            using (var encryptor = aes.CreateEncryptor())
            {
                var cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
                var output = new byte[IV_BYTES + cipher.Length];
                Buffer.BlockCopy(aes.IV, 0, output, 0, IV_BYTES);
                Buffer.BlockCopy(cipher, 0, output, IV_BYTES, cipher.Length);
                return output;
            }
        }
    }

    static byte[] unprotect(byte[] sealed_data)
    {
        if (sealed_data.Length <= IV_BYTES) throw new CryptographicException();

        var iv = new byte[IV_BYTES];
        Buffer.BlockCopy(sealed_data, 0, iv, 0, IV_BYTES);
        using (var aes = Aes.Create())
        {
            aes.Key = derive_key();
            aes.IV = iv;
            using (var decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(sealed_data, IV_BYTES, sealed_data.Length - IV_BYTES);
            }
        }
    }

    static byte[] derive_key()
    {
        if (key_cache != null) return key_cache;

        var device_id = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrEmpty(device_id) || device_id == SystemInfo.unsupportedIdentifier) device_id = KEY_FALLBACK_ID;

        using (var derive = new Rfc2898DeriveBytes(device_id, Encoding.UTF8.GetBytes(KEY_SALT_TEXT), KEY_ITERATIONS))
        {
            key_cache = derive.GetBytes(KEY_BYTES);
        }

        return key_cache;
    }

    static void warn_weak()
    {
        if (weak_warned) return;

        weak_warned = true;
        Debug.LogWarning("login: the saved session on this platform is encrypted with a key derived from the device id, which is weak protection");
    }
#endif
}
