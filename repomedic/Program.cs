using Octokit;

namespace repomedic
{
    internal class Program
    {
        private static async Task<bool[]> Checks(GitHubClient github, string? owner, string? repo)
        {

            bool[] check = { false, false, false};

            try
            {

                var file = await github.Repository.Content.GetAllContents(owner, repo, "README.md");
                check[0] = true;

            }
            catch (NotFoundException) { check[0] = false; }
            catch (RateLimitExceededException)
            {
                Console.WriteLine("Error: The limit of requests has expired!!!");
            }

            try
            {

                var file = await github.Repository.Content.GetAllContents(owner, repo, ".gitignore");
                check[1] = true;

            }
            catch (NotFoundException) { check[1] = false; }
            catch (RateLimitExceededException)
            {
                Console.WriteLine("Error: The limit of requests has expired!!!");
            }

            try
            {

                var file = await github.Repository.Content.GetAllContents(owner, repo, "LICENSE");
                check[2] = true;

            }
            catch (NotFoundException) { check[2] = false; }
            catch (RateLimitExceededException)
            {
                Console.WriteLine("Error: The limit of requests has expired!!!");
            }



            return check;
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

            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                Console.WriteLine("Error: username and repository cannot be empty.");
                return;
            }

            //Conclusion
            Console.WriteLine("\nChecking repository...");
            bool[] result = await Checks(github, owner, repo);

            //README.md
            if (result[0])
            {
                Console.WriteLine("[OK] README.md found!");
            }
            else
            {
                Console.WriteLine("[X] README.md not found.");
            }

            //.gitignore
            if (result[1])
            {
                Console.WriteLine("[OK] .gitignore found!");
            }
            else
            {
                Console.WriteLine("[X] .gitignore not found.");
            }

            //LICENSE
            if (result[2])
            {
                Console.WriteLine("[OK] LICENSE found!");
            }
            else
            {
                Console.WriteLine("[X] LICENSE not found.");
            }

        }
    }
}
