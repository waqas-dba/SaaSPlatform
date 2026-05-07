using System;
using System.Threading.Tasks;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Services;
using Xunit;

namespace SaaSPlatform.UnitTests.Billing
{
    public class SubscriptionRuleEngineTests
    {
        [Fact]
        public async Task Should_Allow_Active_Subscription()
        {
            var engine = new SubscriptionRuleEngine();

            var subscription = new Subscription
            {
                Status = SubscriptionStatus.Active
            };

            var result = await engine.CanAccessAsync(subscription, new Plan());

            Assert.True(result);
        }
    }
}