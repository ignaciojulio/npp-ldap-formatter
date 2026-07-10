using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NppLdapFormatter;

public static class Main
{
    private const uint NppmGetCurrentScintilla = 0x0400 + 4;
    private const uint SciGetTextLength = 2183;
    private const uint SciGetText = 2182;
    private const uint SciSetText = 2181;

    private static IntPtr _nppHandle;
    private static readonly IntPtr[] _scintillaHandles = new IntPtr[2];
    private static readonly FuncItem[] _funcItems =
    [
        new FuncItem("Format LDAP", FormatLdap)
    ];

    public static void SetInfo(IntPtr nppHandle, IntPtr scintillaMainHandle, IntPtr scintillaSecondHandle)
    {
        _nppHandle = nppHandle;
        _scintillaHandles[0] = scintillaMainHandle;
        _scintillaHandles[1] = scintillaSecondHandle;
    }

    public static FuncItem[] GetMenuItems() => _funcItems;

    private static void FormatLdap()
    {
        var editorHandle = GetCurrentScintillaHandle();
        if (editorHandle == IntPtr.Zero)
        {
            return;
        }

        var currentText = ReadEditorText(editorHandle);
        var formattedText = LdapFormatterEngine.Format(currentText);
        SetEditorText(editorHandle, formattedText);
    }

    private static IntPtr GetCurrentScintillaHandle()
    {
        var whichEditor = IntPtr.Zero;
        SendMessage(_nppHandle, NppmGetCurrentScintilla, IntPtr.Zero, ref whichEditor);

        var editorIndex = whichEditor.ToInt32();
        return editorIndex is 0 or 1 ? _scintillaHandles[editorIndex] : IntPtr.Zero;
    }

    private static string ReadEditorText(IntPtr editorHandle)
    {
        var textLength = SendMessage(editorHandle, SciGetTextLength, IntPtr.Zero, IntPtr.Zero).ToInt32();
        if (textLength <= 0)
        {
            return string.Empty;
        }

        var buffer = new byte[textLength + 1];
        var unmanagedBuffer = Marshal.AllocHGlobal(buffer.Length);

        try
        {
            SendMessage(editorHandle, SciGetText, (IntPtr)buffer.Length, unmanagedBuffer);
            Marshal.Copy(unmanagedBuffer, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }
        finally
        {
            Marshal.FreeHGlobal(unmanagedBuffer);
        }
    }

    private static void SetEditorText(IntPtr editorHandle, string text)
    {
        SendMessage(editorHandle, SciSetText, IntPtr.Zero, text ?? string.Empty);
    }

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref IntPtr lParam);
}

public sealed record FuncItem(string Name, Action Action);
