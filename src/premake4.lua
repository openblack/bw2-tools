solution "mod_tools"
	configurations { "debug", "release" }
	location  "../project"
	targetdir "../bin"
	flags { "NoEditAndContinue", "NoPCH" }
	includedirs { "include" }
	libdirs { "lib" }

	vpaths
	{
		["headers/*"] = { "**.h", "**.hpp" },
		["sources/*"] = { "**.c", "**.cpp" }
	}

	configuration "debug"
		targetdir "../bin/debug"
		defines   "DEBUG"
		flags     { "Symbols" }
	configuration "release"
		targetdir "../bin/release"
		defines   "RELEASE"
	configuration "vs*"
		defines   { "_CRT_SECURE_NO_WARNINGS" }


project "unstuff"
	targetname "unstuff"
	language   "C++"
	kind       "ConsoleApp"

	configuration "release"
		flags "OptimizeSpeed" -- sonic

	files
	{
		"app/unstuff/**.cpp"
	}

	configuration "windows"
		links { "ole32" }


if _ACTION == "clean" then
	os.rmdir("../bin")
	os.rmdir("../project")
end