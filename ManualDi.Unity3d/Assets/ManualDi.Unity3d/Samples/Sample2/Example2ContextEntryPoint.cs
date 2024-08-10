using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d.Examples.Example2
{
    public class Example2ContextEntryPoint : BaseContextEntryPoint<PrimitiveType, Example2Context>
    {
        public Example2Context context;

        public override void Install(DiContainerBindings bindings)
        {
            bindings.Bind<Example2Context>()
                .FromInstance(context)
                .Inject((o, c) => o.Inject(Data));
        }
    }
}
