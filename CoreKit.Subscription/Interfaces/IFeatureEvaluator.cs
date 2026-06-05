using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Interfaces;

public interface IFeatureEvaluator
{
    string FeatureCode { get; }
    bool Evaluate(Plan plan);
}