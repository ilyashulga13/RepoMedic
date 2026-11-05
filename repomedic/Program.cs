/**
 * !                         !
 * !                         !
 * !                         !
 * !                         !
 *    v1.3.0 I'M STILL DOING!!
 * !                         !
 * !                         !
 * !                         !
 * !                         !
**/


using Octokit;
using System.Text.Json;

namespace repomedic
{
    public class RepoMedicConfig
    {
        public bool SkipReadme { get; set; } = false;
        public bool SkipLicense { get; set; } = false;
    }

    internal class Program
    {

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
                // Ask GitHub to show the contents of the.github/workflows folder
                var files = await github.Repository.Content.GetAllContents(owner, repo, ".github/workflows");

                // If there are files (their number is greater than 0), then Actions is configured
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

        static async Task Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

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

                // Initilization github
                GitHubClient? github = null;

                // Initilization token
                string? token = null;

                // Initilization repoInfo
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

                    // 1. Check known flags
                    if (inputParts.Contains("--pro") || inputParts.Contains("--silent") || inputParts.Contains("--quiet")) isPro = true;
                    if (inputParts.Contains("--skip-readme")) skipReadmeArg = true;
                    if (inputParts.Contains("--skip-license")) skipLicenceArg = true;

                    // 2. Find the repository link (the argument that does not start with "--")
                    input = inputParts.FirstOrDefault(a => !a.StartsWith("--"));

                    // 3. Find unknown flags and warn the user
                    var unknownFlags = inputParts.Where(a => a.StartsWith("--") && a != "--pro" && a != "--silent" && a != "--quiet" && a != "--skip-readme" && a != "--skip-license").ToList();

                    if (unknownFlags.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[WARNING] Unknown flag(s) ignored: {string.Join(", ", unknownFlags)}");
                        Console.ResetColor();
                    }

                    // 4. If the link is not found ask for it
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


                // Initilization owner/repo
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
                            // parts. Length - 1 is always the last element
                            // parts. Length - 2 is the penultimate

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
                // 1. First, ask for the token (and read it immediately)
                if (isPro)
                {
                    token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                }
                else
                {
                    // Normal mode
                    Console.WriteLine("\nTip: A GitHub token increases your API limit from 60 to 5000 requests/hour.");
                    Console.Write("Please enter your GitHub token (or press Enter to skip): ");
                    token = Console.ReadLine();
                }

                bool skipReadmeInteractive = false;
                bool skipLicenceInteractive = false;

                if (!isPro)
                {
                    // 2. Then we ask about the absences
                    Console.WriteLine("\nDo you want to skip any checks?");

                    Console.Write("Skip README.md check? (y/n): ");
                    skipReadmeInteractive = Console.ReadLine()?.ToLower() == "y";

                    Console.Write("Skip LICENSE check? (y/n): ");
                    skipLicenceInteractive = Console.ReadLine()?.ToLower() == "y";
                }

                // 3. Combine: if the user entered the flag OR answered 'y', then skip
                bool skipReadme = skipReadmeArg || skipReadmeInteractive || config.SkipReadme;
                bool skipLicence = skipLicenceArg || skipLicenceInteractive || config.SkipLicense;


                if (!string.IsNullOrEmpty(token))
                {
                    // If you have entered your token, you will have 5000 requests per hour

                    github = new GitHubClient(new ProductHeaderValue("RepoMedic"))
                    {
                        Credentials = new Credentials(token)
                    };

                }
                else
                {
                    // If not then 60 requests per hour

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


                // Conclusion
                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n----------------------------------------");
                    Console.WriteLine($"Checking: {owner}/{repo}");
                    Console.WriteLine("----------------------------------------\n");
                    Console.ResetColor();
                }

                Console.WriteLine("\nChecking repository...\n");

                // bool
                bool hasREADME = false;
                if(!skipReadme)
                {
                    hasREADME = await FileExists(github, owner, repo, "README.md");
                }

                bool hasGitignore = await FileExists(github, owner, repo, ".gitignore");

                bool hasLICENSE = false;
                if (!skipLicence)
                {
                    hasLICENSE = await FileExists(github, owner, repo, "LICENSE")
                    || await FileExists(github, owner, repo, "LICENSE.md")
                    || await FileExists(github, owner, repo, "LICENSE.txt");
                }

                bool hasWorkflows = await HasWorkflows(github, owner, repo);
                bool hasContributing = await FileExists(github, owner, repo, "CONTRIBUTING.md");
                bool hasDescription = !string.IsNullOrEmpty(repoInfo.Description);

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("----------------------------------------\n");
                    Console.ResetColor();
                }

                // Output result
                if (!skipReadme)
                {
                    PrintResult("README.md", hasREADME, "Add README.md in your repository", isPro);
                }
                else
                {
                    if (!isPro)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("[INFO] Skipped README.md check (--skip-readme)\n");
                        Console.ResetColor();
                    }
                }

                PrintResult(".gitignore", hasGitignore, "Add .gitignore to ignore bin/ and obj/", isPro);

                if (!skipLicence)
                {
                    PrintResult("LICENSE", hasLICENSE, "Add LICENSE to your repository", isPro);
                }
                else
                {
                    if (!isPro)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("[INFO] Skipped LICENSE check (--skip-license)\n");
                        Console.ResetColor();
                    }
                }

                PrintResult("GitHub Actions", hasWorkflows, "Add a .github/workflows folder for CI/CD", isPro);
                PrintResult("CONTRIBUTING.md", hasContributing, "Add CONTRIBUTING.md to guide new contributors.", isPro);
                PrintResult("Repository Description", hasDescription, "Add a short description in the 'About' section on GitHub.", isPro);

                if (!isPro)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("----------------------------------------\n");
                    Console.ResetColor();
                }

                // SCORE system
                int passed = 0;
                int total = 0;

                if (!skipReadme)
                {
                    total++;
                    if (hasREADME) passed++;
                }

                total++;
                if (hasGitignore) passed++;

                if (!skipLicence)
                {
                    total++;
                    if (hasLICENSE) passed++;
                }

                total++;
                if (hasWorkflows) passed++;

                total++;
                if (hasContributing) passed++;

                total++;
                if (hasDescription) passed++;

                int score = (passed * 100) / total;

                // output grade and score
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
                }
                else
                {
                    if (score == 100)
                    {
                        Console.WriteLine($"Score: {score}/100");
                        Console.WriteLine("[SYSTEM] Perfect health detected. You may proceed, human.");
                    }
                    else if (score < 50)
                    {
                        Console.WriteLine($"Score: {score}/100");
                        Console.WriteLine("[SYSTEM] Critical failure. Your repository is beyond repair.");
                    }
                    else
                    {
                        Console.WriteLine($"Score: {score}/100");
                    }
                }

                if (isPro)
                {
                    // If the repository is bad(less than 50), return code 1.If it is good, return 0.
                    Environment.Exit(score < 50 ? 1 : 0);
                }

                // I request from the user :3
                Console.WriteLine("\nDo you want to check another repository? (y/n)");
                var answer = Console.ReadLine();



                if (answer?.ToLower() != "y")
                {
                      break;
                }

                Console.Clear();

            }

        }
    }
}
