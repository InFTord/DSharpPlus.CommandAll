using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Commands
{
    public record CommandArgument
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required Type Type { get; init; }
        public IReadOnlyList<Attribute> Attributes { get; init; } = new List<Attribute>();
        public Optional<object?> DefaultValue { get; init; } = Optional.FromNoValue<object?>();
    }
}
