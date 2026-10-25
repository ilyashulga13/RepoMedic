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
                Console.WriteLine("The limit of requests has expired!!!");
            }



            return check;
        }


        static void Main(string[] args)
        {

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
            try
            {
                Console.Write("Please enter your github token: ");
                token = Console.ReadLine();
            }
            catch (AuthorizationException)
            {
                Console.WriteLine("The token is incorrect, please enter the correct one!!!");
            }

            if (token != null)
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



        }
    }
}
