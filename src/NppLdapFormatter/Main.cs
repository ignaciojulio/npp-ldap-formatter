using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NppLdapFormatter;

public static unsafe class Main
{
    private const uint NppmGetCurrentScintilla = 0x0400 + 4;
    private const uint SciGetTextLength = 2183;
    private const uint SciGetText = 2182;
    private const uint SciSetText = 2181;
    private const int FuncItemNameLength = 64;

    private static IntPtr _nppHandle;
    private static readonly IntPtr[] _scintillaHandles = new IntPtr[2];

    private static readonly IntPtr _pluginNamePointer;
    private static readonly IntPtr _funcItemsPointer;

    static Main()
    {
        _pluginNamePointer = Marshal.StringToHGlobalUni("NppLdapFormatter");

        var funcItem = new FuncItemNative
        {
            PFunc = &FormatLdapEntry,
            CmdId = 0,
            Init2Check = false,
            PShKey = IntPtr.Zero
        };

        const string menuName = "Format LDAP";
        for (var i = 0; i < FuncItemNameLength; i++)
        {
            funcItem.ItemName[i] = '\0';
        }

        var copyLength = Math.Min(menuName.Length, FuncItemNameLength - 1);
        for (var i = 0; i < copyLength; i++)
        {
            funcItem.ItemName[i] = menuName[i];
        }

        _funcItemsPointer = Marshal.AllocHGlobal(sizeof(FuncItemNative));
        *(FuncItemNative*)_funcItemsPointer = funcItem;
    }

    public static void SetInfo(IntPtr nppHandle, IntPtr scintillaMainHandle, IntPtr scintillaSecondHandle)
    {
        _nppHandle = nppHandle;
        _scintillaHandles[0] = scintillaMainHandle;
        _scintillaHandles[1] = scintillaSecondHandle;
    }

    [UnmanagedCallersOnly(EntryPoint = "isUnicode")]
    public static int IsUnicode() => 1;

    [UnmanagedCallersOnly(EntryPoint = "setInfo")]
    public static void SetInfoExport(IntPtr notepadPlusDataPtr)
    {
        if (notepadPlusDataPtr == IntPtr.Zero)
        {
            return;
        }

        var notepadPlusData = Marshal.PtrToStructure<NotepadPlusData>(notepadPlusDataPtr);
        SetInfo(notepadPlusData.NppHandle, notepadPlusData.ScintillaMainHandle, notepadPlusData.ScintillaSecondHandle);
    }

    [UnmanagedCallersOnly(EntryPoint = "getFuncsArray")]
    public static IntPtr GetFuncsArray(int* numberOfItems)
    {
        if (numberOfItems != null)
        {
            *numberOfItems = 1;
        }

        return _funcItemsPointer;
    }

    [UnmanagedCallersOnly(EntryPoint = "getName")]
    public static IntPtr GetName() => _pluginNamePointer;

    [UnmanagedCallersOnly]
    private static void FormatLdapEntry()
    {
        FormatLdap();
    }

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
        var utf8Bytes = Encoding.UTF8.GetBytes((text ?? string.Empty) + '\0');
        var unmanagedBuffer = Marshal.AllocHGlobal(utf8Bytes.Length);

        try
        {
            Marshal.Copy(utf8Bytes, 0, unmanagedBuffer, utf8Bytes.Length);
            SendMessage(editorHandle, SciSetText, IntPtr.Zero, unmanagedBuffer);
        }
        finally
        {
            Marshal.FreeHGlobal(unmanagedBuffer);
        }
    }

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NotepadPlusData
    {
        public readonly IntPtr NppHandle;
        public readonly IntPtr ScintillaMainHandle;
        public readonly IntPtr ScintillaSecondHandle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private unsafe struct FuncItemNative
    {
        public fixed char ItemName[FuncItemNameLength];
        public delegate* unmanaged<void> PFunc;
        public int CmdId;
        [MarshalAs(UnmanagedType.I1)]
        public bool Init2Check;
        public IntPtr PShKey;
    }
}
