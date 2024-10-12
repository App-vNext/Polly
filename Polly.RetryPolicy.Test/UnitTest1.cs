namespace Polly.RetryPolicy.Test
{
    public class UnitTest1
    {
        [Fact]
        public void RetryPolicy_Should_ExecuteAction_WithoutRetry_OnSuccess()
        {
            // Arrange
            bool actionExecuted = false;
            //_,_,_=exception, timespan, retryCount異常,時間差,幾次重試
            var retryPolicy = Policy
                .Handle<Exception>()
                .Retry(3, (_, _, _) => { });

            // Act設定執行API完成
            retryPolicy.Execute(() =>
            {
                actionExecuted = true;
            });

            // Assert
            Assert.True(actionExecuted, "Action should be executed.");
        }
        [Fact]
        public void RetryPolicy_Should_Retry_WithExponentialBackoff_OnTransientFailure()
        {
            // Arrange
            int retryCount = 0;
            List<TimeSpan> retryDelays = new List<TimeSpan>();

            var retryPolicy = Policy
                .Handle<TimeoutException>()
                .WaitAndRetry(3, // 最多3次
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 指數回退策略
                    (_, timeSpan, currentRetryCount, _) =>
                    {
                        retryCount = currentRetryCount;
                        retryDelays.Add(timeSpan);
                    });

            // Act
            Assert.Throws<TimeoutException>(() =>
            {
                retryPolicy.Execute(() =>
                {
                    throw new TimeoutException("Temporary failure");
                });
            });

            // Assert
            Assert.Equal(3, retryCount); // 應該重試3次
            Assert.Collection(retryDelays,
                delay => Assert.Equal(TimeSpan.FromSeconds(2), delay), // 第一次2秒
                delay => Assert.Equal(TimeSpan.FromSeconds(4), delay), // 第二次4秒
                delay => Assert.Equal(TimeSpan.FromSeconds(8), delay)  // 第三次8秒
            );
            Assert.Equal("Temporary failure", exception.Message);
        }
    }
}
