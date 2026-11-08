using Octokit;
using System.Text.Json;
using System.Linq;
using System.IO;

namespace repomedic
{
    public class RepoMedicConfig
    {
        public bool SkipReadme { get; set; } = false;
        public bool SkipLicense { get; set; } = false;
        public bool SkipGitignore { get; set; } = false;
        public bool SkipActions { get; set; } = false;
        public bool SkipContributing { get; set; } = false;
        public bool SkipDescription { get; set; } = false;
    }

    internal class Program
    {
        private static string achievementsPath = "achievements.txt";

        private static async Task<bool> FileExists(GitHubClient github, string? owner, string? repo, string? filename)
        {
            try
            {
                await github.Repository.Content.GetAllContents(owner, repo, filename);
                return true;
            }
            catch (NotFoundException)
            {
                return false; // no file
            }
            catch (RateLimitExceededException)
            {
                Console.WriteLine("Error: The limit of requests has expired!!!");
                return false;
            }
        }

        private static async Task<bool> HasWorkflows(GitHubClient github, string? owner, string? repo)
        {
            try
            {
                var files = await github.Repository.Content.GetAllContents(owner, repo, ".github/workflows");
                return files.Count > 0;
            }
            catch (NotFoundException)
            {
                return false;
            }
        }

        private static void PrintResult(string checkName, bool isSuccess, string tip, bool isPro)
        {
            if (isSuccess)
            {
                if (!isPro) Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(isPro ? $"[OK] {checkName}" : $"[OK] {checkName} found!\n");
            }
            else
            {
                if (!isPro) Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(isPro ? $"[X] {checkName}" : $"[X] {checkName} not found.");

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"    Tip: {tip}\n");
                }
            }
            Console.ResetColor();
        }

        private static void UnlockAchievement(string achievement, bool isPro)
        {
            List<string> achievements = new List<string>();
            if (File.Exists(achievementsPath))
            {
                achievements = File.ReadAllLines(achievementsPath).ToList();
            }

            if (!achievements.Contains(achievement))
            {
                achievements.Add(achievement);
                File.WriteAllLines(achievementsPath, achievements);

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n[NEW ACHIEVEMENT] {achievement}");
                    Console.ResetColor();
                }
            }
        }

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Check for --achievements command
            if (args.Contains("--achievements") || args.Contains("-a"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("========================================");
                Console.WriteLine("         RepoMedic Achievements         ");
                Console.WriteLine("========================================\n");
                Console.ResetColor();

                if (File.Exists(achievementsPath))
                {
                    string[] unlocked = File.ReadAllLines(achievementsPath);
                    if (unlocked.Length == 0)
                    {
                        Console.WriteLine("No achievements yet. Go check some repos!");
                    }
                    else
                    {
                        foreach (var item in unlocked)
                        {
                            Console.WriteLine($"[Unlocked] {item}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No achievements yet. Go check some repos!");
                }
                return;
            }

            bool skipLicenceArg = args.Contains("--skip-license");
            bool skipReadmeArg = args.Contains("--skip-readme");
            bool isPro = args.Contains("--pro") || args.Contains("--silent") || args.Contains("--quiet");

            RepoMedicConfig config = new RepoMedicConfig();
            string configPath = ".repomedic.json";

            if (File.Exists(configPath))
            {
                string jsonString = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<RepoMedicConfig>(jsonString) ?? new RepoMedicConfig();

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("[INFO] Loaded settings from .repomedic.json\n");
                    Console.ResetColor();
                }
            }

            while (true)
            {
                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("========================================");
                    Console.WriteLine("         RepoMedic v1.3.0               ");
                    Console.WriteLine("  GitHub Repository Health Checker      ");
                    Console.WriteLine("========================================\n");
                    Console.ResetColor();
                }

                GitHubClient? github = null;
                string? token = null;
                Repository repoInfo;

                string? input = null;

                if (isPro)
                {
                    input = args.FirstOrDefault(a => !a.StartsWith("--"));
                }

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Please enter GitHub repository (URL or owner/repo). You can add flags like '--pro' or '--skip-license': ");
                    input = Console.ReadLine();
                }

                if (!string.IsNullOrEmpty(input))
                {
                    string[] inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (inputParts.Contains("--pro") || inputParts.Contains("--silent") || inputParts.Contains("--quiet")) isPro = true;
                    if (inputParts.Contains("--skip-readme")) skipReadmeArg = true;
                    if (inputParts.Contains("--skip-license")) skipLicenceArg = true;

                    input = inputParts.FirstOrDefault(a => !a.StartsWith("--"));

                    var unknownFlags = inputParts.Where(a => a.StartsWith("--") && a != "--pro" && a != "--silent" && a != "--quiet" && a != "--skip-readme" && a != "--skip-license").ToList();
                    if (unknownFlags.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[WARNING] Unknown flag(s) ignored: {string.Join(", ", unknownFlags)}");
                        Console.ResetColor();
                    }

                    if (string.IsNullOrEmpty(input))
                    {
                        Console.Write("Please enter the repository to check (owner/repo): ");
                        input = Console.ReadLine();

                        if (input != null && input.StartsWith("--"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ERROR] You entered a flag instead of a repository. Restarting...\n");
                            Console.ResetColor();
                            continue;
                        }
                    }
                }

                string owner = "";
                string repo = "";

                // Easter Egg
                if (input?.ToLower() == "saturn" || input?.ToLower() == "v1.3.0")
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\n          .         .          ");
                    Console.WriteLine("      .         .         .    ");
                    Console.WriteLine("          .-'''''-.          ");
                    Console.WriteLine("        .'         '.        ");
                    Console.WriteLine("       /             \\       ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("  ----|---------------|----  ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("       \\             /       ");
                    Console.WriteLine("        '.         .'        ");
                    Console.WriteLine("          '-.....-'          ");
                    Console.WriteLine("      .         .         .    ");
                    Console.WriteLine("          .         .          ");

                    Console.WriteLine("\nSecret found!");
                    Console.WriteLine("You actually typed that? Nice.");
                    Console.WriteLine("Saturn is pretty cool. I spent like 10 minutes trying to draw these rings using just text characters.");
                    Console.WriteLine("Anyway, let's go check some repositories!\n");
                    Console.ResetColor();
                    continue;
                }

                if (!string.IsNullOrEmpty(input))
                {
                    input = input.Replace(".git", "");
                    string[] parts = input.Split('/');

                    if (input.Contains("github.com"))
                    {
                        if (parts.Length >= 2)
                        {
                            owner = parts[parts.Length - 2];
                            repo = parts[parts.Length - 1];
                        }
                    }
                    else
                    {
                        if (parts.Length == 2)
                        {
                            owner = parts[0];
                            repo = parts[1];
                        }
                    }
                }

                // Checking for errors
                if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                {
                    Console.WriteLine("Error: invalid format. Please use 'owner/repo' or a valid GitHub URL.\n");
                    continue;
                }

                // Enter token
                if (isPro)
                {
                    token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                }
                else
                {
                    Console.WriteLine("\nTip: A GitHub token increases your API limit from 60 to 5000 requests/hour.");
                    Console.Write("Please enter your GitHub token (or press Enter to skip): ");
                    token = Console.ReadLine();
                }

                bool skipReadmeInteractive = false;
                bool skipLicenceInteractive = false;

                if (!isPro)
                {
                    Console.WriteLine("\nDo you want to skip any checks?");
                    Console.Write("Skip README.md check? (y/n): ");
                    skipReadmeInteractive = Console.ReadLine()?.ToLower() == "y";

                    Console.Write("Skip LICENSE check? (y/n): ");
                    skipLicenceInteractive = Console.ReadLine()?.ToLower() == "y";
                }

                bool skipReadme = skipReadmeArg || skipReadmeInteractive || config.SkipReadme;
                bool skipLicence = skipLicenceArg || skipLicenceInteractive || config.SkipLicense;

                if (!string.IsNullOrEmpty(token))
                {
                    github = new GitHubClient(new ProductHeaderValue("RepoMedic"))
                    {
                        Credentials = new Credentials(token)
                    };
                }
                else
                {
                    github = new GitHubClient(new ProductHeaderValue("RepoMedic"));
                }

                try
                {
                    repoInfo = await github.Repository.Get(owner, repo);
                }
                catch (NotFoundException)
                {
                    Console.WriteLine("Error: Repository not found!\n");
                    if (isPro) Environment.Exit(1);
                    continue;
                }
                catch (AuthorizationException)
                {
                    Console.WriteLine("Error: Invalid GitHub token. Please check your credentials.\n");
                    if (isPro) Environment.Exit(1);
                    continue;
                }

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n----------------------------------------");
                    Console.WriteLine($"Checking: {owner}/{repo}");
                    Console.WriteLine("----------------------------------------\n");
                    Console.ResetColor();
                }

                Console.WriteLine("\nChecking repository...\n");

                bool hasREADME = skipReadme ? false : await FileExists(github, owner, repo, "README.md");
                bool hasGitignore = config.SkipGitignore ? false : await FileExists(github, owner, repo, ".gitignore");
                bool hasLICENSE = skipLicence ? false : (await FileExists(github, owner, repo, "LICENSE") || await FileExists(github, owner, repo, "LICENSE.md") || await FileExists(github, owner, repo, "LICENSE.txt"));
                bool hasWorkflows = config.SkipActions ? false : await HasWorkflows(github, owner, repo);
                bool hasContributing = config.SkipContributing ? false : await FileExists(github, owner, repo, "CONTRIBUTING.md");
                bool hasDescription = config.SkipDescription ? false : !string.IsNullOrEmpty(repoInfo.Description);

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("----------------------------------------\n");
                    Console.ResetColor();
                }

                if (!skipReadme) PrintResult("README.md", hasREADME, "Add README.md in your repository", isPro);
                if (!config.SkipGitignore) PrintResult(".gitignore", hasGitignore, "Add .gitignore to ignore bin/ and obj/", isPro);
                if (!skipLicence) PrintResult("LICENSE", hasLICENSE, "Add LICENSE to your repository", isPro);
                if (!config.SkipActions) PrintResult("GitHub Actions", hasWorkflows, "Add a .github/workflows folder for CI/CD", isPro);
                if (!config.SkipContributing) PrintResult("CONTRIBUTING.md", hasContributing, "Add CONTRIBUTING.md to guide new contributors.", isPro);
                if (!config.SkipDescription) PrintResult("Repository Description", hasDescription, "Add a short description in the 'About' section on GitHub.", isPro);

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("----------------------------------------\n");
                    Console.ResetColor();
                }

                int passed = 0;
                int total = 0;

                if (!skipReadme) { total++; if (hasREADME) passed++; }
                if (!config.SkipGitignore) { total++; if (hasGitignore) passed++; }
                if (!skipLicence) { total++; if (hasLICENSE) passed++; }
                if (!config.SkipActions) { total++; if (hasWorkflows) passed++; }
                if (!config.SkipContributing) { total++; if (hasContributing) passed++; }
                if (!config.SkipDescription) { total++; if (hasDescription) passed++; }

                int score = total == 0 ? 100 : (passed * 100) / total;

                if (!isPro)
                {
                    if (score >= 75) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine($"\nYour score: {score}/100"); Console.ResetColor(); }
                    else if (score >= 50) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"\nYour score: {score}/100"); Console.ResetColor(); }
                    else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"\nYour score: {score}/100"); Console.ResetColor(); }

                    Console.WriteLine();

                    if (score == 100) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("PERFECT! Your repository is in ideal health!"); }
                    else if (score >= 50) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("Not bad, but your repo needs a little checkup."); }
                    else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Ouch... Your repository is sick. Let's fix it!"); }
                    Console.ResetColor();

                    // SAVE ACHIEVEMENTS
                    if (score == 100) UnlockAchievement("Perfect Health", isPro);
                    if (isPro) UnlockAchievement("Silent Assassin", isPro);
                    if (total == 0) UnlockAchievement("Ghost Repo", isPro);
                    else if (skipReadme || skipLicence || config.SkipGitignore || config.SkipActions) UnlockAchievement("Selective Surgeon", isPro);
                }
                else
                {
                    if (score == 100) Console.WriteLine($"Score: {score}/100\n[SYSTEM] Perfect health detected. You may proceed, human.");
                    else if (score < 50) Console.WriteLine($"Score: {score}/100\n[SYSTEM] Critical failure. Your repository is beyond repair.");
                    else Console.WriteLine($"Score: {score}/100");

                    // Save silently in Pro mode
                    if (score == 100) UnlockAchievement("Perfect Health", true);
                    if (isPro) UnlockAchievement("Silent Assassin", true);
                    if (total == 0) UnlockAchievement("Ghost Repo", true);
                    else if (skipReadme || skipLicence || config.SkipGitignore || config.SkipActions) UnlockAchievement("Selective Surgeon", true);
                }

                if (isPro)
                {
                    Environment.Exit(score < 50 ? 1 : 0);
                }

                Console.WriteLine("\nDo you want to check another repository? (y/n)");
                var answer = Console.ReadLine();

                if (answer?.ToLower() != "y") break;

                Console.Clear();
            }
        }
    }
}