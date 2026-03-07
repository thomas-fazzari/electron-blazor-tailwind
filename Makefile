install:
	cd src/ElectronApp && bun install

build:
	dotnet build

run:
	cd src/ElectronApp && electronize start

web-http:
	dotnet watch --project src/ElectronApp --launch-profile http

web-https:
	dotnet watch --project src/ElectronApp --launch-profile https

test:
	dotnet test

format:
	dotnet csharpier format .

check:
	dotnet csharpier check .

outdated:
	dotnet dotnet-outdated
	cd src/ElectronApp && bun outdated
