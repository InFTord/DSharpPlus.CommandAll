using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class SlashChoiceProviderAttribute(Type providerType) : Attribute
    {
        public Type ProviderType { get; init; } = providerType ?? throw new ArgumentNullException(nameof(providerType));

        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> GrabChoicesAsync(IServiceProvider serviceProvider, CommandArgument argument)
        {
            IChoiceProvider choiceProvider;
            try
            {
                choiceProvider = (IChoiceProvider)ActivatorUtilities.CreateInstance(serviceProvider, ProviderType);
            }
            catch (Exception)
            {
                return [];
            }

            List<DiscordApplicationCommandOptionChoice> choices = [];
            foreach ((string name, object value) in await choiceProvider.ProvideAsync(argument))
            {
                choices.Add(new(name, value));
            }

            return choices;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class SlashChoiceProviderAttribute<T> : SlashChoiceProviderAttribute where T : IChoiceProvider
    {
        public SlashChoiceProviderAttribute() : base(typeof(T)) { }
    }

    public interface IChoiceProvider
    {
        public Task<Dictionary<string, object>> ProvideAsync(CommandArgument argument);
    }
}
