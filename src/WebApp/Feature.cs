using System.Diagnostics;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

public enum DeploymentRing
{
    Canary = 0,      // Internal team testing
    EarlyAdopters = 1, // Opted-in preview users  
    GeneralAvailability = 2,   // General availability
}

public class RingDeploymentSettings
{
    public DeploymentRing ReleaseRing { get; set; } = DeploymentRing.Canary;
    public List<string> Regions { get; set; } = [];
}

public class ReleaseContext
{
    public DeploymentRing Ring { get; set; } = DeploymentRing.GeneralAvailability;
    public string Region { get; set; } = "global";
    public string BuildVersion { get; set; } = string.Empty;
}

[FilterAlias("RingDeployment")]
public class RingDeploymentFeatureFilter : IContextualFeatureFilter<ReleaseContext>
{
    private ILogger<RingDeploymentFeatureFilter> logger;

    public RingDeploymentFeatureFilter(ILogger<RingDeploymentFeatureFilter> logger)
    {
        this.logger = logger;
    } 

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context, 
        ReleaseContext releaseContext)
    {
        logger.LogError($"FF: Evaluating feature {context.FeatureName} for ring '{releaseContext.Ring}' in region '{releaseContext.Region}'");
        
        var settings = context.Parameters.Get<RingDeploymentSettings>() 
            ?? new RingDeploymentSettings();
        logger.LogError($"FF: Feature {context.FeatureName} settings: ReleaseRing={settings.ReleaseRing}, Regions=[{string.Join(", ", settings.Regions)}]");
        
        // Feature is enabled if user's ring is at or before the release ring
        var ringEnabled = releaseContext.Ring <= settings.ReleaseRing;
        
        // Optionally restrict to specific regions during rollout
        var regionEnabled = settings.Regions.Count == 0 
            || settings.Regions.Contains(releaseContext.Region);
        
        return Task.FromResult(ringEnabled && regionEnabled);
//        return Task.FromResult(true);
    }
}