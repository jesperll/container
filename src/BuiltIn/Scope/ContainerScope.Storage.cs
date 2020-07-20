﻿using System;
using System.Diagnostics;

namespace Unity.BuiltIn
{
    public partial class ContainerScope
    {
        [DebuggerDisplay("Identity = { Identity }, Manager = {Manager}", Name = "{ (Contract.Type?.Name ?? string.Empty),nq }")]
        public struct Registry
        {
            public readonly uint Hash;
            public readonly Contract   Contract;
            public RegistrationManager Manager;

            public Registry(uint hash, Type type, RegistrationManager manager)
            {
                Hash = hash;
                Contract = new Contract(type);
                Manager  = manager;
            }

            public Registry(uint hash, Type type, string? name, RegistrationManager manager)
            {
                Hash = hash;
                Contract = new Contract(type, name);
                Manager = manager;
            }
        }

        [DebuggerDisplay("{ Name }")]
        public struct Identity
        {
            public readonly uint Hash;
            public readonly string? Name;
            public int[] References;

            public Identity(uint hash, string? name, int size)
            {
                Hash = hash;
                Name = name;
                References = new int[size];
            }
        }
    }
}