# RepoMedic

[![NuGet](https://img.shields.io/nuget/v/RepoMedic.svg)](https://www.nuget.org/packages/RepoMedic)
[![Publish to NuGet](https://github.com/ilyashulga13/RepoMedic/actions/workflows/publish.yml/badge.svg)](https://github.com/ilyashulga13/RepoMedic/actions/workflows/publish.yml)

**RepoMedic** is a simple and fast CLI tool written in C# that checks the health of your GitHub repository. It verifies if your project has the essential files needed for open-source development and gives it a score out of 100.

## Why use RepoMedic?

When starting a new project, it is easy to forget the small but crucial details that make a repository look professional. Recruiters, open-source contributors, and other developers look for things like a README, a license, and a `.gitignore`. 

RepoMedic automates this check. It acts like a doctor for your repository, telling you exactly what is missing and how to fix it before you share your project with the world.

## Features

- Checks for a `README.md` file (essential for explaining your project).
- Checks for a `.gitignore` file (prevents you from committing unnecessary files like `bin/` or `obj/`).
- Checks for a `LICENSE` file (supports `LICENSE`, `LICENSE.md`, and `LICENSE.txt`).
- Checks if GitHub Actions are set up (in the `.github/workflows` folder).
- Provides helpful tips on what to add if a file is missing.
- Gives a final health score out of 100 to gamify the process.

## Installation

You can install RepoMedic globally as a .NET tool. Make sure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed on your computer.

Open your terminal and run:

```bash
dotnet tool install --global RepoMedic
```

*Note: To update to the latest version in the future, use `dotnet tool update --global RepoMedic`.*

## Usage

Once installed, simply run the command in your terminal:

```bash
repomedic
```


The app will ask you for a GitHub repository (you can paste a full URL like `https://github.com/user/repo` or just `user/repo`), and optionally your GitHub token.

Here is what it looks like:

```text
Please enter GitHub repository (URL or owner/repo): 
ilyashulga13/RepoMedic

Tip: A GitHub token increases your API limit from 60 to 5000 requests/hour.
Please enter your GitHub token (or press Enter to skip): 

----------------------------------------
Checking: ilyashulga13/RepoMedic
----------------------------------------

Checking repository...

[OK] README.md found!

[OK] .gitignore found!

[OK] LICENSE found!

[OK] GitHub Actions found!

[OK] Repository Description found!

----------------------------------------
Your score: 100/100
```

### How the Scoring Works

Every repository starts with 0 points. RepoMedic checks for 4 essential things:
- **README.md**: +25 points
- **.gitignore**: +25 points
- **LICENSE**: +25 points
- **GitHub Actions**: +25 points

If you have all four, you get a final score of 100/100.

### Why should I use a token?

If you skip the token, GitHub allows 60 requests per hour. This is usually enough for a few checks, but if you plan to check many repositories, you might hit the limit.
By providing a Personal Access Token (PAT), the limit increases to 5000 requests per hour. The token is not saved anywhere on your computer; it is only used for the current session.

## Build from source

If you want to contribute to the code, test new features, or just see how it works under the hood:

1. Clone the repository:
   ```bash
   git clone https://github.com/ilyashulga13/RepoMedic.git
   ```
2. Navigate to the project folder:
   ```bash
   cd RepoMedic
   ```
3. Run the app:
   ```bash
   dotnet run
   ```

## Contributing

I would love some help with this project! If you have an idea for a new feature (like checking for `CONTRIBUTING.md` or accepting full GitHub URLs instead of separate owner/repo names), please open an Issue or send a Pull Request. Any advice or feedback is welcome!

## About the Author

Hi, I'm Ilya, a 13-year-old developer. I built this tool to practice C# and to help beginners build better open-source projects. I wrote the code myself with some guidance from a mentor. I am actively improving this project and listening to feedback, so feel free to reach out!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
