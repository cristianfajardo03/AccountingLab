using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP05Singleton;

public sealed record GlobalAccountingRegistry(string Version, int MonetaryScale, string Scope);
public interface IGlobalRegistryChallenge{GlobalAccountingRegistry Acquire(); }

public sealed class GlobalRegistryChallenge : IGlobalRegistryChallenge
{
    private static readonly GlobalAccountingRegistry RegistroGlobal =
        new GlobalAccountingRegistry(
            Version: "DEMO-1",
            MonetaryScale: 4,
            Scope: "process"
        );

    public GlobalAccountingRegistry Acquire()
    {
        return RegistroGlobal;
    }
}

