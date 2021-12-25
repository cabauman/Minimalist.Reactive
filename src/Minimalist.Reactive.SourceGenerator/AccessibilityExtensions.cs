using Microsoft.CodeAnalysis;

namespace Minimalist.Reactive.SourceGenerator;

internal static class AccessibilityExtensions
{
    public static string ToFriendlyString(this Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.NotApplicable => string.Empty,
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => string.Empty,
        };
}
