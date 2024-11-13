using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Th;

namespace ICU4N.Tests.LuceneNetIntegration;

public class TestThaiAnalyzer : BaseTokenStreamTestCase
{
    [Test]
    [Repeat(100)]
    public virtual void TestRandomHugeStrings()
    {
        Random random = Random;
        CheckRandomData(random, new ThaiAnalyzer(TEST_VERSION_CURRENT), 100 * RandomMultiplier, 8192);
    }
}
