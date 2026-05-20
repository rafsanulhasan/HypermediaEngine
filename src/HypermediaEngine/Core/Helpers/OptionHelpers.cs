using LanguageExt;

namespace HypermediaEngine.Helpers;

/// <summary>
/// Provides extension methods for working with <see cref="Option{T}"/> types.
/// </summary>
public static class OptionHelpers
{
    extension<T>(Option<T> option)
        where T : class
    {
        /// <summary>
        /// Converts the option to a nullable value.
        /// </summary>
        /// <returns>The wrapped value if present; otherwise, <see langword="null"/>.</returns>
        public T? Flatten()
            => option.IsNone
             ? null
             : option.Match(
                Some: value => value is null ? null : (T?)value,
                None: () => null);
    }
}

/// <summary>
/// Provides extension methods for working with <see cref="Option{T}"/> types.
/// </summary>
public static class OptionCollectionHelper
{
    extension<T, TCollection>(Option<TCollection> option)
        where T : class
        where TCollection : class, IEnumerable<T>
    {
        /// <summary>
        /// Converts the option to a nullable value.
        /// </summary>
        /// <returns>The wrapped value if present; otherwise, <see langword="null"/>.</returns>
        public TCollection? Flatten()
            => option.IsNone
             ? null
             : option.Match(
                Some: value => (TCollection?)value,
                None: () => null);
    }
}