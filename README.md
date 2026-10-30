# RepoMedic

[![NuGet](https://img.shields.io/nuget/v/RepoMedic.svg)](https://www.nuget.org/packages/RepoMedic)
[![Publish to NuGet](https://github.com/ilyashulga13/RepoMedic/actions/workflows/publish.yml/badge.svg)](https://github.com/ilyashulga13/RepoMedic/actions/workflows/publish.yml)

RepoMedic is a simple and fast CLI tool written in C# that checks the health of your GitHub repository. It verifies if your project has the essential files needed for open-source development and gives it a score out of 100.

## Why use RepoMedic?

When starting a new project, it is easy to forget the small but crucial details that make a repository look professional. RepoMedic automates this check. It acts like a doctor for your repository, telling you exactly what is missing and how to fix it before you share your project with the world.

## Features

- Checks for `README.md`, `.gitignore`, and `LICENSE` (supports `LICENSE`, `LICENSE.md`, `LICENSE.txt`).
- Checks if GitHub Actions are set up (in the `.github/workflows` folder).
- Checks for `CONTRIBUTING.md` and Repository Description.
- **Flexible:** You can skip checks using interactive prompts or command-line flags (`--skip-license`, `--skip-readme`).
- **Interactive:** Check multiple repositories without restarting the program.
- Gives a final "Doctor's Verdict" based on your score.

## Installation

Make sure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed. Then open your terminal and run:

```bash
dotnet tool install --global RepoMedic
```

*Note: To update to the latest version in the future, use `dotnet tool update --global RepoMedic`.*

## Usage

Once installed, simply run the command in your terminal:

```bash
repomedic
```

The app will ask you for a GitHub repository, your token (optional), and if you want to skip any specific checks.

Here is what it looks like:

```text
Please enter GitHub repository (URL or owner/repo): 
ilyashulga13/RepoMedic

Tip: A GitHub token increases your API limit from 60 to 5000 requests/hour.
Please enter your GitHub token (or press Enter to skip): 

Do you want to skip any checks?
Skip README.md check? (y/n): n
Skip LICENSE check? (y/n): n

----------------------------------------
Checking: ilyashulga13/RepoMedic
----------------------------------------

Checking repository...

[OK] README.md found!
[OK] .gitignore found!
[OK] LICENSE found!
[OK] GitHub Actions found!
[OK] CONTRIBUTING.md found!
[OK] Repository Description found!

----------------------------------------
Your score: 100/100
PERFECT! Your repository is in ideal health!
```

### How the Scoring Works

Every repository starts with 0 points. RepoMedic checks for 6 essential things:
- **README.md**
- **.gitignore**
- **LICENSE**
- **GitHub Actions**
- **CONTRIBUTING.md**
- **Repository Description**

Each check is worth an equal percentage of the final score. If you skip a check using the interactive prompt or flags, the total score is recalculated dynamically.

### Why should I use a token?

If you skip the token, GitHub allows 60 requests per hour. By providing a Personal Access Token (PAT), the limit increases to 5000 requests per hour. The token is not saved anywhere; it is only used for the current session.

## Build from source

If you want to test the code or add something new:

1. Clone the repo:
   ```bash
   git clone https://github.com/ilyashulga13/RepoMedic.git
   ```
2. Go to the folder:
   ```bash
   cd RepoMedic
   ```
3. Run it:
   ```bash
   dotnet run
   ```

## Contributing

I am still learning, so any help is awesome! If you have ideas (like checking for `SECURITY.md` or letting people auto-generate missing files), just open an Issue or send a Pull Request.

## About me

Hey! I'm Ilya, a 13-year-old developer. I built this tool to practice C# and help beginners. I wrote the whole thing myself (with some advice from a mentor). I'm constantly updating it, so let me know what you think!

## License

This project uses the MIT License. You can read the details in the [LICENSE](LICENSE) file.
