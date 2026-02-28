install:
	cd src/ElectronApp && bun install

build:
	dotnet build

run:
	cd src/ElectronApp && electronize start

run-web:
	dotnet run --project src/ElectronApp

test:
	dotnet test

format:
	dotnet csharpier .

check:
	dotnet csharpier --check .
