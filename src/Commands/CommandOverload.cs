using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.DSharpPlus.CommandAll.Commands.Builders;
using OoLunar.DSharpPlus.CommandAll.Commands.Enums;

namespace OoLunar.DSharpPlus.CommandAll.Commands
{
    /// <summary>
    /// A command overload.
    /// </summary>
    public sealed class CommandOverload
    {
        /// <summary>
        /// The command this overload belongs to.
        /// </summary>
        public readonly Command Command;

        /// <summary>
        /// The method this overload calls.
        /// </summary>
        public readonly MethodInfo Method;

        /// <summary>
        /// The parameters of this overload, NOT including the command context parameter.
        /// </summary>
        public readonly IReadOnlyList<CommandParameter> Parameters;

        /// <summary>
        /// The flags of this overload.
        /// </summary>
        public readonly CommandOverloadFlags Flags;

        /// <summary>
        /// The priority of this overload. If this overload is <see cref="CommandOverloadFlags.SlashPreferred"/>, this will be set to 0.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// The slash metadata of this overload.
        /// </summary>
        public readonly CommandSlashMetadata SlashMetadata;

        /// <summary>
        /// Creates a new command overload.
        /// </summary>
        /// <param name="builder">The builder used to create this overload.</param>
        /// <param name="command">The command this overload belongs to.</param>
        public CommandOverload(CommandOverloadBuilder builder, Command command)
        {
            builder.Verify();
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Method = builder.Method;
            Priority = builder.Priority;
            Flags = builder.Flags;
            Parameters = builder.Parameters.Select(parameterBuilder => new CommandParameter(parameterBuilder, this)).ToArray();
            SlashMetadata = new(builder.SlashMetadata);
        }

        public override string ToString() => $"{Command.FullName} {string.Join(" ", Parameters.Select(parameter => parameter.ParameterInfo.ParameterType.Name))}{(Flags.HasFlag(CommandOverloadFlags.Disabled) ? " Disabled " : "")}";

        public static explicit operator DiscordApplicationCommandOption(CommandOverload overload) => new(
            overload.Command.Name.Underscore(),
            overload.Command.Description,
            ApplicationCommandOptionType.SubCommand,
            null, null,
            overload.Parameters.Select(parameter => (DiscordApplicationCommandOption)parameter),
            null, null, null, null,
            overload.SlashMetadata.LocalizedNames.ToDictionary(x => x.Key.Parent.TwoLetterISOLanguageName == x.Key.TwoLetterISOLanguageName ? x.Key.Parent.TwoLetterISOLanguageName : $"{x.Key.Parent.TwoLetterISOLanguageName}-{x.Key.TwoLetterISOLanguageName}", x => x.Value),
            overload.SlashMetadata.LocalizedDescriptions.ToDictionary(x => x.Key.Parent.TwoLetterISOLanguageName == x.Key.TwoLetterISOLanguageName ? x.Key.Parent.TwoLetterISOLanguageName : $"{x.Key.Parent.TwoLetterISOLanguageName}-{x.Key.TwoLetterISOLanguageName}", x => x.Value));
    }
}
