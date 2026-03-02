install:
	cd src/ElectronApp && bun install

build:
	dotnet build

run:
	cd src/ElectronApp && electronize start

run-web:
	dotnet watch --project src/ElectronApp

test:
	dotnet test

format:
	dotnet csharpier format .

check:
	dotnet csharpier check .

outdated:
	dotnet dotnet-outdated
	cd src/ElectronApp && bun outdated
