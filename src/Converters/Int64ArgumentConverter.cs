using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Commands.Arguments;

namespace OoLunar.DSharpPlus.CommandAll.Converters
{
    public sealed class Int64ArgumentConverter : IArgumentConverter<long>
    {
        public static ApplicationCommandOptionType Type { get; } = ApplicationCommandOptionType.Integer;

        public Task<Optional<long>> ConvertAsync(CommandContext context, CommandParameter parameter, string value) => Task.FromResult(long.TryParse(value, out long result) ? Optional.FromValue(result) : Optional.FromNoValue<long>());
    }
}
