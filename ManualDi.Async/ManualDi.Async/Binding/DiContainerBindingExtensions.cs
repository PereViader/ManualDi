using System;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

namespace ManualDi.Async
{
    public static class DiContainerBindingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> Bind<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            Binding<TConcrete, TConcrete> binding = new();
            diContainerBindings.AddBinding(binding, typeof(TConcrete));
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Bind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            Binding<TApparent, TConcrete> binding = new();
            diContainerBindings.AddBinding(binding, typeof(TApparent));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent1, TConcrete> Bind<TApparent1, TApparent2, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2
        {
            diContainerBindings.BindingRedirection<TApparent2, TApparent1>();
            
            return Bind<TApparent1, TConcrete>(diContainerBindings);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent1, TConcrete> Bind<TApparent1, TApparent2, TApparent3, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3
        {
            diContainerBindings.BindingRedirection<TApparent2, TApparent1>();
            diContainerBindings.BindingRedirection<TApparent3, TApparent1>();
            
            return Bind<TApparent1, TConcrete>(diContainerBindings);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent1, TConcrete> Bind<TApparent1, TApparent2, TApparent3, TApparent4, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3, TApparent4
        {
            diContainerBindings.BindingRedirection<TApparent2, TApparent1>();
            diContainerBindings.BindingRedirection<TApparent3, TApparent1>();
            diContainerBindings.BindingRedirection<TApparent4, TApparent1>();
            
            return Bind<TApparent1, TConcrete>(diContainerBindings);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> BindAll<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            diContainerBindings.BindingRedirection<TApparent, TConcrete>();
            
            return Bind<TConcrete>(diContainerBindings);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> BindAll<TApparent1, TApparent2, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2
        {
            diContainerBindings.BindingRedirection<TApparent1, TConcrete>();
            diContainerBindings.BindingRedirection<TApparent2, TConcrete>();
            
            return Bind<TConcrete>(diContainerBindings);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> BindAll<TApparent1, TApparent2, TApparent3, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3
        {
            diContainerBindings.BindingRedirection<TApparent1, TConcrete>();
            diContainerBindings.BindingRedirection<TApparent2, TConcrete>();
            diContainerBindings.BindingRedirection<TApparent3, TConcrete>();
            
            return Bind<TConcrete>(diContainerBindings);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> BindAll<TApparent1, TApparent2, TApparent3, TApparent4, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3, TApparent4
        {
            diContainerBindings.BindingRedirection<TApparent1, TConcrete>();
            diContainerBindings.BindingRedirection<TApparent2, TConcrete>();
            diContainerBindings.BindingRedirection<TApparent3, TConcrete>();
            diContainerBindings.BindingRedirection<TApparent4, TConcrete>();

            return Bind<TConcrete>(diContainerBindings);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BindingRedirection<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
        {
            var binding = new Binding<TApparent, TConcrete>()
                .FromContainerResolve()
                .DependsOn(static d => d.ConstructorDependency<TConcrete>());
            
            diContainerBindings.AddBinding(binding, typeof(TApparent));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings QueueStartup<T>(this DiContainerBindings diContainerBindings, Action<T> startup)
        {
            diContainerBindings.QueueStartup(c =>
            {
                var resolved = c.Resolve<T>();
                startup.Invoke(resolved);
            });
            return diContainerBindings;
        }
    }
}
