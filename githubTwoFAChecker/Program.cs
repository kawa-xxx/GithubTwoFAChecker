using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Slack.Webhooks;
using Emoji = Slack.Webhooks.Emoji;

namespace githubTwoFAChecker
{
	class Program
	{
		static void Main(string[] args)
		{
			var githubSample = new GithubSample();
			var result = githubSample.Check2FA().Result;
		}
	}

	class GithubSample
	{
		public async Task<int> Check2FA()
		{
			var github = new GitHubClient(new ProductHeaderValue(ConfigurationManager.AppSettings["GithubProductHeader"]))
				{ Credentials = new Credentials(ConfigurationManager.AppSettings["GithubClientToken"]) };

			var users = await github.Organization.Member.GetAll("yourOrganization", OrganizationMembersFilter.TwoFactorAuthenticationDisabled);

			SlackPost(users.Any()
				? users.Aggregate("2FA 無効ユーザ\n", (current, user) => current + ("Name: " + user.Login))
				: "2FA が無効のユーザはいません！セキュアデス！");
			return 0;
		}

		private void SlackPost(string message)
		{
			var client = new SlackClient(ConfigurationManager.AppSettings["SlackWebHookUrl"]);
			var slackMessage = new SlackMessage
			{
				Channel = "#test",
				Text = message,
				IconEmoji = Emoji.Octocat,
				Username = "Github2FAChecker"
			};
			client.Post(slackMessage);
		}
	}
}
