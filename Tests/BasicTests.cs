﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IDisposerCore;

namespace Tests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void DisposerRegistryTest()
        {
            DisposerRegistry.Clear();
            Assert.AreEqual(0, DisposerRegistry.LeakedObjects.Count);

            using (var f = new SampleDisposable())
            {
                DisposerRegistry.Add(f);
                Assert.AreEqual(1, DisposerRegistry.LeakedObjects.Count);
                Assert.IsTrue(DisposerRegistry.LeakedObjects.Keys.Contains(f));

                DisposerRegistry.Remove(f);
                Assert.AreEqual(0, DisposerRegistry.LeakedObjects.Count);
            }
        }
    }
}