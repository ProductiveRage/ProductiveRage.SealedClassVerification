@echo off

%~d0
cd "%~p0"

del *.nu* 2> nul
del *.dll 2> nul
del *.pdb 2> nul
del *.xml 2> nul
del *.ps1 2> nul

copy ..\ProductiveRage.SealedClassVerification\bin\Release\ProductiveRage.SealedClassVerification.dll > nul
copy ..\ProductiveRage.SealedClassVerification\bin\Release\ProductiveRage.SealedClassVerification.xml > nul

copy ..\Analyser\bin\Release\*.dll > nul
copy ..\Analyser\bin\Release\tools\* > nul

copy ..\ProductiveRage.SealedClassVerification.Bridge.nuspec > nul
..\packages\NuGet.CommandLine.2.8.5\tools\nuget pack -NoPackageAnalysis ProductiveRage.SealedClassVerification.Bridge.nuspec
