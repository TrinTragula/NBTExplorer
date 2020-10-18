using System;
using NBTParser;
using NUnit.Framework;

namespace NBTParserTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var path = @"C:\Users\danie\AppData\Roaming\.minecraft\saves\La vita - Copia\level.dat";
            var nbt = new NBT(path);
            Console.WriteLine(nbt);
            Assert.Pass();
        }
    }
}