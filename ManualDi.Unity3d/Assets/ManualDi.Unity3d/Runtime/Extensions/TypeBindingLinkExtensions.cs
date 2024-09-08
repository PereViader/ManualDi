using ManualDi.Main;
using UnityEngine;

namespace ManualDi.Unity3d
{
    public static class TypeBindingLinkExtensions
    {
        public static TypeBinding<TInterface, TConcrete> LinkDontDestroyOnLoad<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            bool setAsRootTransform = true,
            bool keepPreviousParent = true,
            bool destroyIfPreviousParentDestroyed = true
        )
            where TConcrete : UnityEngine.Component
        {
            Transform? previousParent = null;
            typeBinding.Inject((o, c) =>
            {
                var transform = o.transform;
                previousParent = transform.parent;
                if (setAsRootTransform)
                {
                    transform.parent = null;
                }
                UnityEngine.Object.DontDestroyOnLoad(o);
            });

            if (keepPreviousParent)
            {
                typeBinding.Dispose((o, c) =>
                {
                    if (previousParent == null)
                    {
                        if (destroyIfPreviousParentDestroyed)
                        {
                            UnityEngine.Object.Destroy(o.gameObject);
                        }
                        return;
                    }

                    o.transform.parent = previousParent;
                });
            }
            return typeBinding;
        }
    }
}