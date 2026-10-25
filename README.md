# RepoMedic 🩺

**RepoMedic** is a simple and fast CLI tool written in C# that checks the health of your GitHub repository. 
It verifies if your project has the essential files needed for open-source development and gives it a score out of 100!

## ✨ Features
- Checks for a `README.md` file.
- Checks for a `.gitignore` file.
- Checks for a `LICENSE` file (supports `LICENSE`, `LICENSE.md`, `LICENSE.txt`).
- Provides helpful tips on what to add if a file is missing.
- Gives a final health score out of 100.

## 🚀 How to run

1. Make sure you have [.NET SDK](https://dotnet.microsoft.com/download) installed.
2. Clone this repository:
   ```bash
   git clone https://github.com/ilyashulga13/RepoMedic.git
