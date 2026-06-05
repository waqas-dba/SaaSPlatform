// CoreKit.Subscription/Services/FeatureEvaluators.cs
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Interfaces;

namespace CoreKit.Subscription.Services;

public sealed class CustomDomainEvaluator : IFeatureEvaluator
{
    public string FeatureCode => FeatureFlags.CustomDomain;
    public bool Evaluate(Plan plan) => plan.CustomDomainEnabled;
}

public sealed class ThemeCustomizationEvaluator : IFeatureEvaluator
{
    public string FeatureCode => FeatureFlags.ThemeCustomization;
    public bool Evaluate(Plan plan) => plan.ThemeCustomizationEnabled;
}