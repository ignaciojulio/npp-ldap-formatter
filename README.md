# NppLdapFormatter

> **Native Notepad++ plugin for beautifully formatting LDAP search filters**  
> Pretty-prints minified LDAP filters with smart parenthesis indentation while preserving whitespace inside attribute values exactly as written.

![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-blueviolet)
![Platform](https://img.shields.io/badge/Platform-Notepad++-brightgreen)
![Release](https://img.shields.io/badge/Release-GitHub-informational)

---

## ✨ Features

- **Smart LDAP indentation** for deeply nested logical operators (`&`, `|`, `!`).
- **Whitespace-safe formatting** inside attribute values (e.g., `(cn=John Doe)` remains intact).
- **UTF-8 safe text handling** to avoid encoding-related corruption.
- **Scintilla-aware memory interop** with careful native buffer management.
- **Zero data corruption mindset**: deterministic transformation with strict preservation rules.
- **Native Notepad++ integration** with plugin-command workflow.

---

## 🔍 Before and After Example

### Before (minified)

```ldap
(&(|(objectCategory=person)(objectCategory=user))(!(userAccountControl:1.2.840.113556.1.4.803:=2))(|(department=Engineering)(department=Research and Development))(|(title=Senior Developer)(title=Principal Engineer))(cn=John Doe))
```

### After (formatted by NppLdapFormatter)

```ldap
(&
  (|
    (objectCategory=person)
    (objectCategory=user)
  )
  (!
    (userAccountControl:1.2.840.113556.1.4.803:=2)
  )
  (|
    (department=Engineering)
    (department=Research and Development)
  )
  (|
    (title=Senior Developer)
    (title=Principal Engineer)
  )
  (cn=John Doe)
)
```

---

## 🧩 Installation

Install the plugin manually in Notepad++:

1. **Close Notepad++** if it is currently running.
2. Locate your Notepad++ plugins directory:
   - Typical path:  
     `C:\Program Files\Notepad++\plugins\`
3. Create a folder named:

   `NppLdapFormatter`

4. Copy the compiled plugin DLL into that folder:
   - Example target:  
     `C:\Program Files\Notepad++\plugins\NppLdapFormatter\NppLdapFormatter.dll`
5. Reopen Notepad++.
6. Verify the plugin appears in the **Plugins** menu.

> If you installed Notepad++ in a custom location, use that installation’s `plugins` folder instead.

---

## 🏗️ Technical Architecture (For Developers)

`NppLdapFormatter` is a native-facing Notepad++ plugin implemented in **C# on .NET 10**, bridged to the host through unmanaged exports and interop boundaries.

Core architecture highlights:

- **Native entry points** are exposed with `[UnmanagedCallersOnly]`.
- Built for **native-compatible loading** from Notepad++ plugin host context.
- Uses **Scintilla API messaging** for text retrieval/replacement in the editor buffer.
- Employs strict, explicit memory handling across managed/unmanaged boundaries.
- Formatting logic is LDAP-aware and designed to preserve semantic content and user-visible whitespace where required.

This design enables modern .NET ergonomics while remaining compatible with Notepad++’s native plugin ecosystem.

---

## 🛠️ Building from Source

From the repository root:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

You can also use an explicit project path if needed:

```bash
dotnet publish .\NppLdapFormatter.csproj -c Release -r win-x64 --self-contained
```

After publishing, copy the generated DLL to your Notepad++ plugin directory as described above.

---

## 📄 License & Credits

This project is licensed under the **MIT License**.

Developed and maintained by **Ignacio Javier Julio Posada**.

If you use this plugin in your workflow, consider starring the repository and sharing feedback via issues/discussions.
