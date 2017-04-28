// <copyright file="IUsableItem.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator
{
    using Ensage;
    using Ensage.Common.Enums;

    public interface IUsableItem
    {
        ItemId Id { get; }

        bool IsImportant { get; }

        Item Item { get; }

        void Use();
    }
}