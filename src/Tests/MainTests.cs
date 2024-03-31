namespace Tests;

[UnitTest]
[UsesVerify]
public class MainTests
{

    public MainTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    [Fact]
    public async Task ExecuteAsync_ResultsInSomething()
    {
        // Arrange
        var services = new ServiceCollection()
                     .AddLogging((builder) => builder.AddXUnit(OutputHelper))
                     .AddSingleton<TestSubject>();

        var sut = services
            .BuildServiceProvider()
            .GetRequiredService<TestSubject>();

        // Act
        int actual = sut.GetResult();

        // Assert
        actual.ShouldBe(1);

    }

    public ITestOutputHelper OutputHelper { get; }

}


public class TestSubject
{
    public int GetResult()
    {
        return 1;
    }  
}
