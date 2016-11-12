@echo off

%~d0
cd "%~p0"

del *.nu* 2> nul
del *.dll 2> nul
del *.pdb 2> nul
del *.xml 2> nul
del *.ps1 2> nul
rd /s/q netstandard1.6 2> null

copy ..\Analyser\bin\Release\*.dll > nul
copy ..\Analyser\bin\Release\tools\* > nul

copy ..\"ProductiveRage.SealedClassVerification (Bridge)"\bin\Release\ProductiveRage.SealedClassVerification.dll > nul
copy ..\"ProductiveRage.SealedClassVerification (Bridge)"\bin\Release\ProductiveRage.SealedClassVerification.xml > nul

copy ..\ProductiveRage.SealedClassVerification.Bridge.nuspec > nul
..\packages\NuGet.CommandLine.2.8.5\tools\nuget pack -NoPackageAnalysis ProductiveRage.SealedClassVerification.Bridge.nuspec

copy ..\"ProductiveRage.SealedClassVerification (.NET)"\bin\Release\net45\ProductiveRage.SealedClassVerification.Net.dll > nul
md netstandard1.6
copy ..\"ProductiveRage.SealedClassVerification (.NET)"\bin\Release\netstandard1.6\ProductiveRage.SealedClassVerification.Net.dll netstandard1.6 > nul

copy ..\ProductiveRage.SealedClassVerification.NET.nuspec > nul
..\packages\NuGet.CommandLine.2.8.5\tools\nuget pack -NoPackageAnalysis ProductiveRage.SealedClassVerification.NET.nuspec

