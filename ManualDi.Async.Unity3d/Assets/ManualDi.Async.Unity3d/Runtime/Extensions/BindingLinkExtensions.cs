namespace ManualDi.Async.Unity3d
{
    public static class BindingLinkExtensions
    {
        public static Binding<TConcrete> LinkDontDestroyOnLoad<TConcrete>(
            this Binding<TConcrete> binding,
            bool setAsRootTransform = true,
            bool keepPreviousParent = true,
            bool destroyIfPreviousParentDestroyed = true
        )
            where TConcrete : UnityEngine.Component
        {
            binding.Inject((o, c) =>
            {
                var to = (UnityEngine.Component)o;
                var transform = to.transform;
                var previousParent = transform.parent;
                if (setAsRootTransform)
                {
                    transform.parent = null;
                }
                UnityEngine.Object.DontDestroyOnLoad(to);

                if (keepPreviousParent)
                {
                    c.QueueDispose(() =>
                    {
                        if (previousParent == null)
                        {
                            if (destroyIfPreviousParentDestroyed && to != null)
                            {
                                UnityEngine.Object.Destroy(to.gameObject);
                            }

                            return;
                        }

                        to.transform.parent = previousParent;
                    });
                }
            });
            return binding;
        }
    }
}