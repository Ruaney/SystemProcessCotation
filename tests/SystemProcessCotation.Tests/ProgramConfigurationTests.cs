using Microsoft.Extensions.Configuration;

namespace SystemProcessCotation.Tests;

public class ProgramConfigurationTests
{
    [Fact]
    public void ResolveAwsServiceUrl_UsesEndpointAliasWhenSectionIsMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_ENDPOINT_URL"] = " http://localhost:4566 "
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localhost:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsServiceUrl_PrefersAppSettingsSectionOverAliases()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Aws:ServiceUrl"] = "http://localstack:4566",
            ["AWS_ENDPOINT_URL"] = "http://localhost:4566"
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localstack:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsServiceUrl_UnquotesConfiguredValue()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_ENDPOINT_URL"] = " \"http://localhost:4566\" "
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localhost:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsRegion_UsesDefaultRegionAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_DEFAULT_REGION"] = " sa-east-1 "
        });

        var region = global::Program.ResolveAwsRegion(configuration);

        Assert.Equal("sa-east-1", region);
    }

    [Fact]
    public void ResolveAwsRegion_DefaultsToUsEastWhenNoRegionIsConfigured()
    {
        var region = global::Program.ResolveAwsRegion(BuildConfiguration());

        Assert.Equal("us-east-1", region);
    }

    [Fact]
    public void ResolveRedisConnectionString_UsesDeploymentAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["REDIS_CONNECTION_STRING"] = " redis:6379 "
        });

        var connectionString = global::Program.ResolveRedisConnectionString(configuration);

        Assert.Equal("redis:6379", connectionString);
    }

    [Fact]
    public void ResolveRedisConnectionString_IgnoresQuotedBlankAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["REDIS_CONNECTION_STRING"] = "' '",
            ["REDIS_URL"] = "redis:6379"
        });

        var connectionString = global::Program.ResolveRedisConnectionString(configuration);

        Assert.Equal("redis:6379", connectionString);
    }

    [Fact]
    public void ResolveRedisConnectionString_DefaultsToLocalhostWhenUnset()
    {
        var connectionString = global::Program.ResolveRedisConnectionString(BuildConfiguration());

        Assert.Equal("localhost:6379", connectionString);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? values = null)
    {
        var builder = new ConfigurationBuilder();
        if (values is not null)
        {
            builder.AddInMemoryCollection(values);
        }

        return builder.Build();
    }
}
