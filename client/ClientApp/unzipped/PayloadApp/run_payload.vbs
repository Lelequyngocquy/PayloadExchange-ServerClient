Set objShell = CreateObject("WScript.Shell")

' Lấy thư mục chứa file .vbs
Set fso = CreateObject("Scripting.FileSystemObject")
currentFolder = fso.GetParentFolderName(WScript.ScriptFullName)

' Chuyển thư mục làm việc sang thư mục này
objShell.CurrentDirectory = currentFolder

' Chạy Python + script (giữ console mở sau khi chạy)
objShell.Run "cmd /k python.exe PayloadApp.py", 1, True