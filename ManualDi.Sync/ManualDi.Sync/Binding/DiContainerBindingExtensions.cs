using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerBindingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Bind<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            Binding<TConcrete> binding = new();
            diContainerBindings.AddBinding(binding, typeof(TConcrete));
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Bind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            Binding<TConcrete> binding = new();
            diContainerBindings.AddBinding(binding, typeof(TApparent));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Bind<TApparent1, TApparent2, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2
        {
            var binding = diContainerBindings.Bind<TApparent1, TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent2));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Bind<TApparent1, TApparent2, TApparent3, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3
        {
            var binding = diContainerBindings.Bind<TApparent1, TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent2));
            diContainerBindings.AddBinding(binding, typeof(TApparent3));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Bind<TApparent1, TApparent2, TApparent3, TApparent4, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3, TApparent4
        {
            var binding = diContainerBindings.Bind<TApparent1, TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent2));
            diContainerBindings.AddBinding(binding, typeof(TApparent3));
            diContainerBindings.AddBinding(binding, typeof(TApparent4));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> BindAll<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            var binding = diContainerBindings.Bind<TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> BindAll<TApparent1, TApparent2, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2
        {
            var binding = diContainerBindings.Bind<TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent1));
            diContainerBindings.AddBinding(binding, typeof(TApparent2));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> BindAll<TApparent1, TApparent2, TApparent3, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3
        {
            var binding = diContainerBindings.Bind<TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent1));
            diContainerBindings.AddBinding(binding, typeof(TApparent2));
            diContainerBindings.AddBinding(binding, typeof(TApparent3));
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> BindAll<TApparent1, TApparent2, TApparent3, TApparent4, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3, TApparent4
        {
            var binding = diContainerBindings.Bind<TConcrete>();
            diContainerBindings.AddBinding(binding, typeof(TApparent1));
            diContainerBindings.AddBinding(binding, typeof(TApparent2));
            diContainerBindings.AddBinding(binding, typeof(TApparent3));
            diContainerBindings.AddBinding(binding, typeof(TApparent4));
            return binding;
        }
        
        [Obsolete("Use QueueStartup instead.")]
        public static DiContainerBindings WithStartup<T>(this DiContainerBindings diContainerBindings, Action<T> startup)
        {
            return QueueStartup<T>(diContainerBindings, startup);
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
        
        /// <summary>
        /// Resolve instance can be used during the binding process to resolve instances during installation.
        /// It enables data-driven creation of the object graph.
        /// It can resolve instances from a parent container after WithParentContainer has been called.
        /// It can resolve instances when FromInstance is used for a binding on the current container.
        /// </summary>
        /// <code>
        /// <![CDATA[
        /// var config = bindings.ResolveInstance<SomeFeatureConfig>();
        /// if(config.IsEnabled)
        /// {
        ///    bindings.Bind<ISomeFeature, EnabledSomeFeature>().Default().FromConstructor();
        /// }
        /// else
        /// {
        ///    bindings.Bind<ISomeFeature, DisabledSomeFeature>().Default().FromConstructor();
        /// }
        /// ]]>
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TConfig ResolveInstance<TConfig>(this DiContainerBindings bindings)
        {
            if (!bindings.TryResolveInstance<TConfig>(out var config))
            {
                throw new InvalidOperationException($"Could not resolve instance for binding of type {typeof(TConfig).FullName}");
            }
            
            return config;
        }
        
        /// <inheritdoc cref="ResolveInstance{TConfig}(DiContainerBindings)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolveInstance<TConfig>(this DiContainerBindings bindings, [NotNullWhen(true)] out TConfig? config)
        {
            var binding = bindings.GetBinding(typeof(TConfig));
            if (binding is { FromDelegate: TConfig instance })
            {
                config = instance;
                return true;
            }
            
            if(bindings.parentDiContainerBindings is not null && bindings.parentDiContainerBindings.TryResolveInstance<TConfig>(out config))
            {
                return true;
            }

            if (bindings.parentDiContainer is null)
            {
                config = default;
                return false;
            }
            
            return bindings.parentDiContainer.TryResolve<TConfig>(out config);
        }
        
        /// <inheritdoc cref="ResolveInstance{TConfig}(DiContainerBindings)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TConfig? ResolveInstanceNullable<TConfig>(this DiContainerBindings bindings)
            where TConfig : class
        {
            var binding = bindings.GetBinding(typeof(TConfig));
            if (binding is { FromDelegate: TConfig instance })
            {
                return instance;
            }
            
            if(bindings.parentDiContainerBindings is not null && bindings.parentDiContainerBindings.TryResolveInstance<TConfig>(out var parentInstance))
            {
                return parentInstance;
            }

            return bindings.parentDiContainer?.ResolveNullable<TConfig>();
        }
        
        /// <inheritdoc cref="ResolveInstance{TConfig}(DiContainerBindings)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TConfig? ResolveInstanceNullableValue<TConfig>(this DiContainerBindings bindings)
            where TConfig : struct
        {
            var binding = bindings.GetBinding(typeof(TConfig));
            if (binding is { FromDelegate: TConfig instance })
            {
                return instance;
            }
            
            if(bindings.parentDiContainerBindings is not null && bindings.parentDiContainerBindings.TryResolveInstance<TConfig>(out var parentInstance))
            {
                return parentInstance;
            }

            return bindings.parentDiContainer?.ResolveNullableValue<TConfig>();
        }
        
        //This method is a duplicate of the one in DiContainer so that we can use it to resolve instances during the binding
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Binding? GetBinding(this DiContainerBindings diContainerBindings, Type type)
        {
            if (!diContainerBindings.bindingsByType.TryGetValue(type.TypeHandle.Value, out Binding? binding))
            {
                return null;
            }

            if (binding.FilterBindingDelegate is null)
            {
                return binding;
            }

            //bindingContext.InjectedIntoBinding = injectedBinding;
            do
            {
                diContainerBindings.bindingContext.Binding = binding;

                if (binding.FilterBindingDelegate?.Invoke(diContainerBindings.bindingContext) ?? true)
                {
                    return binding;
                }

                binding = binding.NextBinding;
            } while (binding is not null);
            
            return null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent> BindSubContainer<TApparent>(
            this DiContainerBindings bindings,
            InstallDelegate installDelegate
        )
        {
            var binding = bindings.Bind<TApparent>();
            
            FromDelegate fromDelegate = c =>
            {
                IDiContainer subContainer = new DiContainerBindings()
                    .WithParentBindings(bindings)
                    .Install(installDelegate)
                    .Build();
                
                binding.Dispose((_, _) => subContainer.Dispose());
                return subContainer.Resolve<TApparent>();
            };
            
            binding.FromDelegate = fromDelegate;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent> BindIsolatedSubContainer<TApparent>(
            this DiContainerBindings bindings,
            InstallDelegate installDelegate
        )
        {
            var binding = bindings.Bind<TApparent>();
            
            FromDelegate fromDelegate = c =>
            {
                IDiContainer subContainer = new DiContainerBindings()
                    .Install(installDelegate)
                    .Build();
                
                binding.Dispose((_, _) => subContainer.Dispose());
                return subContainer.Resolve<TApparent>();
            };
            
            binding.FromDelegate = fromDelegate;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings Unbind<TApparent>(this DiContainerBindings diContainerBindings)
        {
            diContainerBindings.RemoveBinding(typeof(TApparent));
            return diContainerBindings;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Rebind<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            return diContainerBindings
                .Unbind<TConcrete>()
                .Bind<TConcrete>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Rebind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            return diContainerBindings
                .Unbind<TApparent>()
                .Bind<TApparent, TConcrete>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Rebind<TApparent1, TApparent2, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2
        {
            return diContainerBindings
                .Unbind<TApparent1>()
                .Unbind<TApparent2>()
                .Bind<TApparent1, TApparent2, TConcrete>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Rebind<TApparent1, TApparent2, TApparent3, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3
        {
            return diContainerBindings
                .Unbind<TApparent1>()
                .Unbind<TApparent2>()
                .Unbind<TApparent3>()
                .Bind<TApparent1, TApparent2, TApparent3, TConcrete>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> Rebind<TApparent1, TApparent2, TApparent3, TApparent4, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent1, TApparent2, TApparent3, TApparent4
        {
            return diContainerBindings
                .Unbind<TApparent1>()
                .Unbind<TApparent2>()
                .Unbind<TApparent3>()
                .Unbind<TApparent4>()
                .Bind<TApparent1, TApparent2, TApparent3, TApparent4, TConcrete>();
        }
    }
}
