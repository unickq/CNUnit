# CNUnit
Command line tool for tests execution in multiple NUnit console instances.

[![Build status](https://ci.appveyor.com/api/projects/status/o8ysmwhw2bcn3yxh?svg=true)](https://ci.appveyor.com/project/unickq/cnunit)
[![Test status](http://teststatusbadge.azurewebsites.net/api/status/unickq/cnunit)](https://ci.appveyor.com/project/unickq/cnunit)
[![NuGet cnunit](http://flauschig.ch/nubadge.php?id=cnunit)](https://www.nuget.org/packages/cnunit)

## Use cases:

- You need to execute not thread safe tests in parallel.
- You need to generate multiple test lists and shuffle tests.
- You don't know how to get JUnit XML files from NUnit console 😋


## Example:


```
> CNUnit.exe --dll ".\build\TestAssembly.dll" --parse="Chrome" 
-w=10 --outdir="build\cnunit-reports" 
--format=junit --shuffle
```

Tests in TestAssembly.dll containing "Chrome" will be shuffled and run with 10 NUnit3-consoles with JUnit XML reports.

## Parameters:
| Command          | Description                                                                                                                                         |
|------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| --executable, -e | NUnit3-console.exe path. If not specified - app will try to found it by itself using %PATH%                                                         |
| --dll, -t        | Tests assembly path                                                                                                                                 |
| --workers, -w    | NUnit instances count;                                                                                                                              |
| --shuffle, -s    | Shuffle tests in test lists                                                                                                                         |
| --quite, -q      | Don't show tests output in CNUnit console                                                                                                           |
| --format, -f     | XML output format. **Options:** nunit3, nunit2, junit. nunit3 by default                                                                            |
| --where          | Parse EXPRESSION indicating what tests will be run. See [NUnit Test Selection Language](https://github.com/nunit/docs/wiki/Test-Selection-Language) |
| --parse          | Simpler expression indicating what tests will be run. Test full tests name should contains VALUES. Separated by **;**                               |
| --oudtdir        | Output directory for XML and testlists. %CD%\cnunit-reports by default                                                                              |
| --tlGenerate     | Do not run test. Generate testlists only                                                                                                            |
| --tlKeep         | Do not remove testlists after test are run                                                                                                          |
| --wait           | Do not close NUnit consoles after test finished                                                                                                     |
| --debug          | Debug CNunit                                                                                                                                        |
