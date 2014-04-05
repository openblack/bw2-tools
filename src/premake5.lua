solution "mod_tools"
	configurations { "release", "debug" }
	location ( "../project" )
	flags { "NoEditAndContinue", "NoPCH" }
	includedirs { "lib", "../lib" }

project "unstuff"
	targetname "unstuff"
	language   "C"
	kind       "ConsoleApp"

	configuration "debug"
		targetdir "../bin/debug"
		defines   "DEBUG"
		flags     { "Symbols" }
	configuration "release"
		targetdir "../bin/release"
		defines   "RELEASE"
		optimize  "Speed"
	configuration "vs*"
		defines   { "_CRT_SECURE_NO_WARNINGS" }

	files
	{
		"app/unstuff/**.h",
		"app/unstuff/**.c"
	}

