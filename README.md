# RepoMedic

[![NuGet](https://img.shields.io/nuget/v/RepoMedic.svg)](https://www.nuget.org/packages/RepoMedic)
[![Publish to NuGet](https://github.com/ilyashulga13/RepoMedic/actions/workflows/publish.yml/badge.svg)](https://github.com/ilyashulga13/RepoMedic/actions/workflows/publish.yml)

**RepoMedic** is a simple and fast CLI tool written in C# that checks the health of your GitHub repository. It verifies if your project has the essential files needed for open-source development and gives it a score out of 100.

## Why use RepoMedic?

When you are starting a new project, it's easy to forget the small but crucial details that make a repository look professional. Recruiters, open-source contributors, and other developers look for things like a README, a license, and a `.gitignore`. 

RepoMedic automates this check. It acts like a doctor for your repository, telling you exactly what is missing and how to fix it before you share your project with the world.

## Features

- Checks for a `README.md` file (essential for explaining your project).
- Checks for a `.gitignore` file (prevents you from committing unnecessary files like `bin/` or `obj/`).
- Checks for a `LICENSE` file (supports `LICENSE`, `LICENSE.md`, and `LICENSE.txt`).
- Provides helpful tips on what to add if a file is missing.
- Gives a final health score out of 100 to gamify the process.

## Installation

You can install RepoMedic globally as a .NET tool. Make sure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed on your computer.

Open your terminal and run:

```bash
dotnet tool install --global RepoMedic
