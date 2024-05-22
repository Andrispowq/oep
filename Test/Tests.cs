using Microsoft.VisualStudio.TestPlatform.TestHost;
using OEP;

namespace Test
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Test1()
        {
            string test = "../../../tests/1.test";
            string expected = "../../../tests/1.out";

            string[] input = File.ReadAllLines(test);
            string output = File.ReadAllText(expected);

            Assert.AreEqual(DoTesting(input, output), true);
        }

        [TestMethod]
        public void Test2()
        {
            string test = "../../../tests/2.test";
            string expected = "../../../tests/2.out";

            string[] input = File.ReadAllLines(test);
            string output = File.ReadAllText(expected);

            Assert.AreEqual(DoTesting(input, output), true);
        }

        [TestMethod]
        public void Test3()
        {
            string test = "../../../tests/3.test";
            string expected = "../../../tests/3.out";

            string[] input = File.ReadAllLines(test);
            string output = File.ReadAllText(expected);

            Assert.AreEqual(DoTesting(input, output), true);
        }

        public bool DoTesting(string[] input, string output)
        {
            TextWriter outputWriter = new StringWriter();
            var original = Console.Out;

            Console.SetOut(outputWriter);

            OEP.Program.Test(input);

            outputWriter.Close();
            var result = outputWriter.ToString()?.TrimEnd(' ', '\n', '\r') ?? null;

            Console.SetOut(original);
            if (output.TrimEnd(' ', '\n', '\r') != result)
            {
                return false;
            }

            return true;
        }
    }
}