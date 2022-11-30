using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Commands.Arguments;
using OoLunar.DSharpPlus.CommandAll.Commands.System.Commands;

namespace OoLunar.DSharpPlus.CommandAll.Converters
{
    public sealed partial class DiscordChannelArgumentConverter : IArgumentConverter<DiscordChannel>
    {
        public static ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Channel;

        [SuppressMessage("Roslyn", "IDE0046", Justification = "Silence the ternary rabbit hole.")]
        public Task<Optional<DiscordChannel>> ConvertAsync(CommandContext context, CommandParameter parameter, string value)
        {
            // Attempt to parse the channel id
            if (!ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong channelId))
            {
                // Value could be a channel mention.
                Match match = GetChannelRegex().Match(value);
                if (!match.Success || !ulong.TryParse(match.Captures[0].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out channelId))
                {
                    return Task.FromResult(Optional.FromNoValue<DiscordChannel>());
                }
            }

            if (context.IsSlashCommand && context.Interaction!.Data.Resolved?.Channels is not null && context.Interaction.Data.Resolved.Channels.TryGetValue(channelId, out DiscordChannel? channel))
            {
                return Task.FromResult(Optional.FromValue(channel));
            }
            else if (context.Guild!.GetChannel(channelId) is DiscordChannel guildChannel)
            {
                return Task.FromResult(Optional.FromValue(guildChannel));
            }
            else
            {
                return Task.FromResult(Optional.FromNoValue<DiscordChannel>());
            }
        }

        [GeneratedRegex(@"^<#(\d+)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
        private static partial Regex GetChannelRegex();
    }
}
