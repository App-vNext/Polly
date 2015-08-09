using System;
using System.Diagnostics;

namespace Polly
{
    /// <summary>
    /// 
    /// </summary>
    public static class HandledPolicySyntax
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="message"></param>
        public static HandledPolicy Trace(this HandledPolicy policy, string message)
        {
            return Trace(policy, message, TraceLevel.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static HandledPolicy Trace(this HandledPolicy policy, string message, TraceLevel level)
        {
            if (!policy.HasException) return policy;

            switch (level)
            {
                case TraceLevel.Error:
                    System.Diagnostics.Trace.TraceError("{0}: {1}", message, policy.InnerException.Message);
                    break;
                case TraceLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning("{0}: {1}", message, policy.InnerException.Message);
                    break;
                case TraceLevel.Info:
                    System.Diagnostics.Trace.TraceInformation("{0}: {1}", message, policy.InnerException.Message);
                    break;
                case TraceLevel.Verbose:
                    System.Diagnostics.Trace.WriteLine(string.Format("{0}: {1}", message, policy.InnerException.Message));
                    break;
            }

            return policy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HandledPolicy RollbackWith(this HandledPolicy policy, Action action)
        {
            if (!policy.HasException) return policy;

            action();

            return policy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static HandledPolicy Rethrow(
            this HandledPolicy policy)
        {
            if (!policy.HasException) return policy;

            throw policy.InnerException;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HandledPolicy FollowedBy(this HandledPolicy policy, Action<HandledPolicy> action)
        {
            if (!policy.HasException) return policy;
            
            action(policy);

            return policy;
        }
    }
}