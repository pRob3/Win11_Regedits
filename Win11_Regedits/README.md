# Windows 11 – Disable “Show more options”

A small helper tool that toggles the Windows 11 context menu behavior.

It allows you to **disable the “Show more options” entry** and always show the **classic full context menu directly**, or restore the default Windows 11 compact menu.

---

## What this tool does

Windows 11 introduced a compact context menu that hides many entries behind **“Show more options”**.

This tool lets you:

- ✅ **Disable “Show more options”**  
  → Always show the classic full context menu directly

- ✅ **Enable “Show more options”**  
  → Restore the default Windows 11 compact menu

The change is applied **per user** (HKCU) and does **not require administrator rights**.

---

## How it works (technical)

The tool toggles the well-known Windows 11 shell override:

HKCU\Software\Classes\CLSID
{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32
(Default) = ""


- **Key present** → Classic full context menu (no “Show more options”)
- **Key removed** → Windows 11 default menu

This is the same mechanism used by common `.reg` tweaks, implemented safely in a compiled executable.

---

## Important: Restart required

⚠️ **On some Windows 11 builds, a full restart or sign-out is required.**

Even though the tool:
- writes the correct registry values,
- broadcasts shell change notifications,
- and restarts Explorer,

Windows may still cache the context menu for the current session.

### If “Show more options” is still visible:
- 🔁 **Restart Windows**, or
- 🚪 **Sign out and sign back in**

This is a limitation of the Windows shell, not the tool.

---

## Usage

1. Download the executable from **Releases**
2. Run `bin\Release\net10.0\Win11_Regedits.exe`
3. Choose:
   - **Disable “Show more options”** – enables classic menu
   - **Enable “Show more options”** – restores Windows 11 default
4. Restart Windows if prompted

---

## Requirements

- Windows 11
- No administrator rights required
- Per-user setting (does not affect other users)

---

## Notes

- The change is **reversible**
- No system files are modified
- No background services or startup tasks are added

---

## License

MIT License

Copyright (c) 2025 Dennis Asplind

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

