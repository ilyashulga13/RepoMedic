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

        static async Task Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //Initilization github
            GitHubClient? github = null;

            //Initilization token
            string? token = null;



            //Enter owner
            Console.WriteLine("Please enter your username: ");
            var owner = Console.ReadLine();


            //Enter repo/repository
            Console.WriteLine("Please enter your repository: ");
            var repo = Console.ReadLine();


            //Enter token
            Console.Write("Please enter your github token (or press Enter to skip): ");
            token = Console.ReadLine();


            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                Console.WriteLine("Error: username and repository cannot be empty.");
                return;
            }


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


            //Conclusion
            Console.WriteLine("\nChecking repository...\n");

            bool hasREADME = await FileExists(github, owner, repo, "README.md");
            bool hasGitignore = await FileExists(github, owner, repo, ".gitignore");
            bool hasLICENSE = await FileExists(github, owner, repo, "LICENSE")
                || await FileExists(github, owner, repo, "LICENSE.md")
                || await FileExists(github, owner, repo, "LICENSE.txt");

            //output and solution
            Console.WriteLine(hasREADME ? "[OK] README.md found!\n" : "[X] README.md not found.\n Add README.md in your repository\n");

            Console.WriteLine(hasGitignore ? "[OK] .gitignore found!\n" : "[X] .gitignore not found.\n Add .gitignore in your repository\n");

            Console.WriteLine(hasLICENSE ? "[OK] LICENSE found!\n" : "[X] LICENSE not found.\n Add LICENSE in your repository\n");
        }
    }
}
