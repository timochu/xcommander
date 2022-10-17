Remove-Item –path .\bin\publish –recurse
dotnet publish -c Release -r win-x64 -o ".\bin\publish\" -p:DebugType=None -p:PublishSingleFile=true --self-contained false
# 7z a ".\bin\publish\xcommander v$(Get-Date -format 'yyyyMMdd.HHmm').7z" .\bin\publish\* -mx=9