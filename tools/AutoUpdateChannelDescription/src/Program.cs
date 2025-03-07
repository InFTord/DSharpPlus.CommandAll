using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DSharpPlus.CommandAll.Tools.AutoUpdateChannelDescription
{
    public static class Program
    {
        public static async Task Main()
        {
            string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new InvalidOperationException("DISCORD_TOKEN environment variable is not set.");
            string guildId = Environment.GetEnvironmentVariable("DISCORD_GUILD_ID") ?? throw new InvalidOperationException("DISCORD_GUILD_ID environment variable is not set.");
            string channelId = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID") ?? throw new InvalidOperationException("DISCORD_CHANNEL_ID environment variable is not set.");
            string channelTopic = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_TOPIC") ?? throw new InvalidOperationException("DISCORD_DESCRIPTION environment variable is not set.");
            string nugetUrl = Environment.GetEnvironmentVariable("NUGET_URL") ?? throw new InvalidOperationException("NUGET_URL environment variable is not set.");
            string githubUrl = Environment.GetEnvironmentVariable("GITHUB_URL") ?? throw new InvalidOperationException("GITHUB_URL environment variable is not set.");
            string? latestStableVersion = Environment.GetEnvironmentVariable("LATEST_STABLE_VERSION");

            DiscordClient client = new(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot
            });

            client.GuildDownloadCompleted += (client, eventArgs) =>
            {
                DiscordGuild guild = client.Guilds[ulong.Parse(guildId, NumberStyles.Number, CultureInfo.InvariantCulture)];
                DiscordChannel channel = guild.Channels[ulong.Parse(channelId, NumberStyles.Number, CultureInfo.InvariantCulture)];

                // Task.Run in case ratelimit gets hit and event handler is cancelled.
                _ = Task.Run(async () =>
                {
                    StringBuilder builder = new(channelTopic);
                    builder.AppendLine();
                    builder.AppendLine(Formatter.Bold("GitHub") + ": " + githubUrl);

                    // If the latest stable version is not set, try to get it from the channel topic.
                    latestStableVersion ??= channel.Topic.Split('\n').FirstOrDefault(line => line.StartsWith(Formatter.Bold("Latest stable version") + ": " + nugetUrl + "/", false, CultureInfo.InvariantCulture))?.Split('/').LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(latestStableVersion))
                    {
                        builder.AppendLine(Formatter.Bold("Latest stable version") + ": " + nugetUrl + "/" + latestStableVersion);
                    }

                    string nightlyVersion = typeof(CommandAllExtension).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

                    // Default version string is 1.0.0, which happens when the -p:Nightly compiler argument is not set.
                    if (nightlyVersion.Equals("1.0.0", StringComparison.Ordinal))
                    {
                        // Get the previous version from the channel topic.
                        string previousNightlyVersion = channel.Topic.Split('\n').FirstOrDefault(x => x.StartsWith(Formatter.Bold("Latest preview version") + ": " + nugetUrl + "/"))?.Split('/').LastOrDefault() ?? throw new InvalidOperationException("Could not find previous nightly version in channel topic.");
                    }
                    else
                    {
                        // Update the channel topic with the latest nightly version.
                        builder.AppendLine(Formatter.Bold("Latest preview version") + ": " + nugetUrl + "/" + nightlyVersion);
                    }

                    try
                    {
                        await channel.ModifyAsync(channel =>
                        {
                            channel.AuditLogReason = $"Updating channel topic to match stable version {latestStableVersion} and nightly version {nightlyVersion}.";
                            channel.Topic = builder.ToString();
                        });
                    }
                    catch (DiscordException error)
                    {
                        Console.WriteLine($"Error: HTTP {error.Response!.StatusCode}, {error.JsonMessage}");
                    }

                    await client.DisconnectAsync();
                    Environment.Exit(0);
                });

                return Task.CompletedTask;
            };

            await client.ConnectAsync();

            // The program should exit ASAP after the channel description is updated.
            // However it may get caught in a ratelimit, so we'll wait for a bit.
            // The program will exit after 30 seconds no matter what.
            // This includes the time it takes to connect to the Discord gateway.
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }
}
