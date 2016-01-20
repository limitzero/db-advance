#-------------------------------------------------------------------------------  
# basic build file Powershell script for psake
#-------------------------------------------------------------------------------  
properties { 
  $base_dir  = resolve-path .
  $lib_dir = "$base_dir\lib"
  $source_dir = "$base_dir\src"
  $tests_dir = "$base_dir\tests"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\DbAdvance.sln" 
  $test_lib = ""
  $version = "1.1.0.0"
  $tools_dir = "$base_dir\tools"
  $release_dir = "$base_dir\release"
} 

#-------------------------------------------------------------------------------  
# entry task to start the build script
#-------------------------------------------------------------------------------  
task default -depends release

#-------------------------------------------------------------------------------  
# clean the "build" directory and make ready for the build actions
#-------------------------------------------------------------------------------  
task clean { 
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue
  remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue 
} 

#-------------------------------------------------------------------------------  
# initializes all of the directories and other objects in preparation for the 
# build process
#-------------------------------------------------------------------------------  
task init -depends clean { 
	new-item $release_dir -itemType directory 
	new-item $buildartifacts_dir -itemType directory 
} 
#-------------------------------------------------------------------------------  
# compiles the solution for the test process
#-------------------------------------------------------------------------------  
task compile -depends init { 
  copy-item "$tools_dir\*" $build_dir
  copy-item "$tools_dir\xUnit\*" $build_dir
  exec  {msbuild  /p:TargetFrameworkVersion=v4.0 /p:OutDir="$buildartifacts_dir" "$sln_file" }
} 
#-------------------------------------------------------------------------------  
# task to run all unit tests in the solution
#-------------------------------------------------------------------------------  
task test -depends compile {
  $old = pwd
  cd $build_dir

  # -- grab each test:
   $tests = $test_lib.split(","); 

  # using .NET 4.0 runner for xunit:
  $xunit = "$build_dir\xunit.console.clr4.x86.exe"
  
  if($tests.length -gt 0)
  {
	  foreach($test in $tests)
	  {
		$library = $test.trim()
	   
		if($library.trim() -eq "")
		{
			continue;
		}
		
		.$xunit "$build_dir\$library"
	  }
 }

  cd $old		
}

task release -depends compile, test {
	$old = pwd
    cd $build_dir
	
	# build db-advance.exe merged with dependencies
	remove-item db-advance.partial.exe -ErrorAction SilentlyContinue 
	rename-item db-advance.exe db-advance.partial.exe
	start-sleep 3 #-- rename operation may not have finished before the ilmerge process...
	
	& $tools_dir\ilmerge\ILMerge.exe db-advance.partial.exe `
		Castle.Core.dll `
		Castle.Facilities.Logging.dll `
		Castle.Services.Logging.NLogIntegration.dll `
		Castle.Windsor.dll `
		Dapper.Contrib.dll `
		Dapper.dll `
		ICSharpCode.SharpZipLib.dll `
		NLog.dll `
		/targetplatform:"v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319" `
		/out:db-advance.exe `
		/t:exe 
	
	if ($lastExitCode -ne 0) {
        throw "Error: Failed to merge assemblies for db-advance executable!"
    }
	
	copy-item db-advance.exe $release_dir
	copy-item db-advance.exe.config $release_dir
	
	cd $old
}


