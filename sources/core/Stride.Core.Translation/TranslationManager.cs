// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Globalization;
using System.Reflection;

namespace Stride.Core.Translation;

public static class TranslationManager
{
    private static readonly Lazy<ITranslationManager> Lazy = new(() => new TranslationManagerImpl());

    /// <summary>
    /// Gets the instance of the <see cref="ITranslationManager"/>.
    /// </summary>
    public static ITranslationManager Instance => Lazy.Value;

    /// <summary>
    /// Implementation of <see cref="ITranslationManager"/>.
    /// </summary>
    private sealed class TranslationManagerImpl : ITranslationManager
    {
        private readonly Dictionary<string, ITranslationProvider> translationProviders = [];

        /// <inheritdoc />
        public CultureInfo CurrentLanguage
        {
            get => CultureInfo.CurrentUICulture;
            set
            {
                if (Equals(CultureInfo.CurrentUICulture, value))
                    return;

                CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = value;
                OnLanguageChanged();
            }
        }

        /// <inheritdoc />
        public event EventHandler? LanguageChanged;

        /// <inheritdoc />
        string ITranslationProvider.BaseName => nameof(TranslationManager);

        /// <inheritdoc />
        public string GetString(string text)
        {
            return GetString(text, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc />
        public string GetString(string text, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            return GetProvider(assembly)?.GetString(text) ?? text;
        }

        /// <inheritdoc />
        public string GetPluralString(string text, string textPlural, long count)
        {
            return GetPluralString(text, textPlural, count, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc />
        public string GetPluralString(string text, string textPlural, long count, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            return GetProvider(assembly)?.GetPluralString(text, textPlural, count) ?? text;
        }

        /// <inheritdoc />
        public string GetParticularString(string context, string text)
        {
            return GetParticularString(context, text, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc />
        public string GetParticularString(string context, string text, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            return GetProvider(assembly)?.GetParticularString(context, text) ?? text;
        }

        /// <inheritdoc />
        public string GetParticularPluralString(string context, string text, string textPlural, long count)
        {
            return GetParticularPluralString(context, text, textPlural, count, Assembly.GetCallingAssembly());
        }

        /// <inheritdoc />
        public string GetParticularPluralString(string context, string text, string textPlural, long count, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            return GetProvider(assembly)?.GetParticularPluralString(context, text, textPlural, count) ?? text;
        }

        /// <inheritdoc />
        public void RegisterProvider(ITranslationProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            translationProviders.Add(provider.BaseName, provider);
        }

        private ITranslationProvider? GetProvider(Assembly assembly)
        {
            translationProviders.TryGetValue(assembly.GetName().Name!, out var provider);
            return provider;
        }

        private void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
