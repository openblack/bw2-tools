solution "mod_tools"
	configurations { "Debug", "Release" }
	location  "../project"
	targetdir "../bin"

	configuration "Debug"
		targetdir "../bin/debug"
		defines   "DEBUG"
	configuration "Release"
		targetdir "../bin/release"
		defines   "RELEASE"


project "unstuff"
	targetname "unstuff"
	language   "C#"
	kind       "ConsoleApp"
	defines   { "TRACE" }
	files { "app/unstuff/**" }
	links { "System", "System.Core", "Microsoft.CSharp" }

    configuration { "Debug*" }
      defines { "DEBUG" }
      flags   { "Symbols" }

project "chlextract"
	targetname "chlex"
	language   "C#"
	kind       "ConsoleApp"
	defines   { "TRACE" }
	files { "app/chlextract/**" }
	links { "System", "System.Core", "Microsoft.CSharp" }

    configuration { "Debug*" }
      defines { "DEBUG" }
      flags   { "Symbols" }

if _ACTION == "clean" then
	os.rmdir("../bin")
	os.rmdir("../project")
end