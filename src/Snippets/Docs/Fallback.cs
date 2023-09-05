using Polly;
using Polly.Fallback;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Fallback
{
    public static void FallbackExample()
    {
        #region fallback

        // Use a substitute value if an operation fails.
        new ResiliencePipelineBuilder<UserAvatar>()
            .AddFallback(new FallbackStrategyOptions<UserAvatar>
            {
                ShouldHandle = new PredicateBuilder<UserAvatar>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(r => r is null),
                FallbackAction = args =>
                {
                    return Outcome.FromResultAsValueTask(UserAvatar.Blank);
                }
            });

        // Use a dynamically generated value if an operation fails.
        new ResiliencePipelineBuilder<UserAvatar>()
            .AddFallback(new FallbackStrategyOptions<UserAvatar>
            {
                ShouldHandle = new PredicateBuilder<UserAvatar>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(r => r is null),
                FallbackAction = args =>
                {
                    var avatar = UserAvatar.GetRandomAvatar();
                    return Outcome.FromResultAsValueTask(avatar);
                }
            });

        // Use a default or dynamically generated value, and execute an additional action if the fallback is triggered.
        new ResiliencePipelineBuilder<UserAvatar>()
            .AddFallback(new FallbackStrategyOptions<UserAvatar>
            {
                ShouldHandle = new PredicateBuilder<UserAvatar>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(r => r is null),
                FallbackAction = args =>
                {
                    var avatar = UserAvatar.GetRandomAvatar();
                    return Outcome.FromResultAsValueTask(UserAvatar.Blank);
                },
                OnFallback = args =>
                {
                    // Add extra logic to be executed when the fallback is triggered, such as logging.
                    return default; // returns an empty ValueTask
                }
            });

        #endregion
    }

    public class UserAvatar
    {
        public static readonly UserAvatar Blank = new();

        public static UserAvatar GetRandomAvatar() => new();
    }
}
