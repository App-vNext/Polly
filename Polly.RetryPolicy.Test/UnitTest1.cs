namespace Polly.RetryPolicy.Test
{
    public class UnitTest1
    {
        [Fact]
        public void RetryPolicy_Should_ExecuteAction_WithoutRetry_OnSuccess()
        {
            // Arrange
            bool actionExecuted = false;
            //_,_,_=exception, timespan, retryCount���`,�ɶ��t,�X������
            var retryPolicy = Policy
                .Handle<Exception>()
                .Retry(3, (_, _, _) => { });

            // Act�]�w����API����
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
                .WaitAndRetry(3, // �̦h3��
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // ���Ʀ^�h����
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
            Assert.Equal(3, retryCount); // ���ӭ���3��
            Assert.Collection(retryDelays,
                delay => Assert.Equal(TimeSpan.FromSeconds(2), delay), // �Ĥ@��2��
                delay => Assert.Equal(TimeSpan.FromSeconds(4), delay), // �ĤG��4��
                delay => Assert.Equal(TimeSpan.FromSeconds(8), delay)  // �ĤT��8��
            );
            Assert.Equal("Temporary failure", exception.Message);
        }
    }
}
