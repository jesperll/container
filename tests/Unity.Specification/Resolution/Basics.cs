﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Specification.Resolution.Basics;

namespace Compiled
{
    [TestClass]
    public class Basics : SpecificationTests
    {
        public override IUnityContainer GetContainer()
        {
            return new UnityContainer().AddExtension(new ForceCompillation());
        }
    }
}


namespace Resolved
{
    [TestClass]
    public class Basics : SpecificationTests
    {
        public override IUnityContainer GetContainer()
        {
            return new UnityContainer().AddExtension(new ForceActivation());
        }
    }
}
