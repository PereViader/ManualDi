﻿using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d.Examples.Example2
{
    internal class Example2EntryPoint : SubordinateEntryPoint<PrimitiveType, Example2Context>
    {
        public Example2Context context;

        public override void Install(DiContainerBindings b)
        {
            b.Bind<Example2Context>().Default().FromInstance(context);
            b.Bind<PrimitiveType>().FromInstance(Data);
        }
    }
}
