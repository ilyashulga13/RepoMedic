using Octokit;

namespace repomedic
{
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
                return false; //no file
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
                //Ask GitHub to show the contents of the.github/workflows folder
                var files = await github.Repository.Content.GetAllContents(owner, repo, ".github/workflows");

                //If there are files (their number is greater than 0), then Actions is configured
                return files.Count > 0; 
            }
            catch (NotFoundException)
            {
                return false;                
            }
        }

        private static void PrintResult(string checkName, bool isSucces, string tip)
        {
            if (isSucces)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK] {checkName} found!\n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[X] {checkName} not found.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    Tip: {tip}\n");
            }
            Console.ResetColor();
        }

        static async Task Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool skipLicenceArg = args.Contains("--skip-license");
            bool skipReadmeArg = args.Contains("--skip-readme");

            while (true)
            {

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========================================");
                Console.WriteLine("         RepoMedic v1.2.0               ");
                Console.WriteLine("  GitHub Repository Health Checker      ");
                Console.WriteLine("========================================\n");
                Console.ResetColor();

                //Initilization github
                GitHubClient? github = null;

                //Initilization token
                string? token = null;

                //Initilization repoInfo
                Repository repoInfo;



                Console.WriteLine("Please enter GitHub repository (URL or owner/repo): ");
                var input = Console.ReadLine();


                //Initilization owner/repo
                string owner = "";
                string repo = "";


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

                //Checking for errors
                if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                {
                    Console.WriteLine("Error: invalid format. Please use 'owner/repo' or a valid GitHub URL.");
                    continue;
                }


                //Enter token
                // 1. First, ask for the token (and read it immediately)
                Console.WriteLine("\nTip: A GitHub token increases your API limit from 60 to 5000 requests/hour.");
                Console.Write("Please enter your GitHub token (or press Enter to skip): ");
                token = Console.ReadLine();

                // 2. Then we ask about the absences
                Console.WriteLine("\nDo you want to skip any checks?");

                Console.Write("Skip README.md check? (y/n): ");
                bool skipReadmeInteractive = Console.ReadLine()?.ToLower() == "y";

                Console.Write("Skip LICENSE check? (y/n): ");
                bool skipLicenceInteractive = Console.ReadLine()?.ToLower() == "y";

                // 3. Combine: if the user entered the flag OR answered 'y', then skip
                bool skipReadme = skipReadmeArg || skipReadmeInteractive;
                bool skipLicence = skipLicenceArg || skipLicenceInteractive;


                if (!string.IsNullOrEmpty(token))
                {
                    //If you have entered your token, you will have 5000 requests per hour

                    github = new GitHubClient(new ProductHeaderValue("RepoMedic"))
                    {
                        Credentials = new Credentials(token)
                    };

                }
                else
                {
                    //If not then 60 requests per hour

                    github = new GitHubClient(new ProductHeaderValue("RepoMedic"));

                }

                try
                {
                    repoInfo = await github.Repository.Get(owner, repo);
                }
                catch (NotFoundException)
                {
                    Console.WriteLine("Error: Repository not found!");
                    continue;
                }


                //Conclusion
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n----------------------------------------");
                Console.WriteLine($"Checking: {owner}/{repo}");
                Console.WriteLine("----------------------------------------\n");
                Console.ResetColor();

                Console.WriteLine("\nChecking repository...\n");

                //bool
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


                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("----------------------------------------\n");
                Console.ResetColor();

                // Output result
                if (!skipReadme)
                {
                    PrintResult("README.md", hasREADME, "Add README.md in your repository");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("[INFO] Skipped README.md check (--skip-readme)\n");
                    Console.ResetColor();
                }

                PrintResult(".gitignore", hasGitignore, "Add .gitignore to ignore bin/ and obj/");

                if (!skipLicence)
                {
                    PrintResult("LICENSE", hasLICENSE, "Add LICENSE to your repository");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("[INFO] Skipped LICENSE check (--skip-license)\n");
                    Console.ResetColor();
                }

                PrintResult("GitHub Actions", hasWorkflows, "Add a .github/workflows folder for CI/CD");
                PrintResult("CONTRIBUTING.md", hasContributing, "Add CONTRIBUTING.md to guide new contributors.");
                PrintResult("Repository Description", hasDescription, "Add a short description in the 'About' section on GitHub.");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("----------------------------------------");
                Console.ResetColor();

                //SCORE system
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

                //output score
                if (score >= 75)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nYour score: {score}/100");
                    Console.ResetColor();
                }
                else if (score >= 50)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nYour score: {score}/100");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nYour score: {score}/100");
                    Console.ResetColor();
                }

                Console.WriteLine();

                //output grade
                if (score == 100)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("PERFECT! Your repository is in ideal health!");
                }
                else if (score >= 50)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Not bad, but your repo needs a little checkup.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ouch... Your repository is sick. Let's fix it!");
                }
                Console.ResetColor();

                //I request from the user :3
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
