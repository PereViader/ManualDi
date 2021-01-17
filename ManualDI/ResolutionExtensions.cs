namespace ManualDI
{
    public static class ResolutionExtensions
    {
        public static IResolutionConstraints Id(this IResolutionConstraints resolution, object identifier)
        {
            resolution.Identifier = identifier;
            return resolution;
        }
    }
}
