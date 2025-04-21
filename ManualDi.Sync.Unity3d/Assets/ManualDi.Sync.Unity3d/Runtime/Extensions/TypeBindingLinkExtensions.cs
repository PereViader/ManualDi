using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ManualDi.Sync.Unity3d
{
    public static class BindingLinkExtensions
    {
        public static Binding<TInterface, TConcrete> LinkDontDestroyOnLoad<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            bool setAsRootTransform = true,
            bool keepPreviousParent = true,
            bool destroyIfPreviousParentDestroyed = true
        )
            where TConcrete : UnityEngine.Component
        {
            Transform? previousParent = null;
            binding.Inject((o, c) =>
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
                binding.Dispose((o, c) =>
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
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> LinkButtonOnClick<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            Button button,
            InstanceContainerDelegate<TConcrete> onClick
            )
        {
            UnityAction? action = null;
            binding.Inject((o, c) =>
            {
                action = () => onClick.Invoke(o, c);
                (button.onClick ??= new Button.ButtonClickedEvent()).AddListener(action);
            });
            binding.Dispose((o, c) =>
            {
                if (action is not null)
                {
                    button.onClick.RemoveListener(action);
                }
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> LinkToggleOnValueChanged<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            Toggle toggle,
            InstanceContainerDelegate<(bool value, TConcrete o)> onValueChanged
        )
        {
            UnityAction<bool>? action = null;
            binding.Inject((o, c) =>
            {
                action = v => onValueChanged.Invoke((v, o), c);
                (toggle.onValueChanged ??= new Toggle.ToggleEvent()).AddListener(action);
            });
            binding.Dispose((o, c) =>
            {
                if (action is not null)
                {
                    toggle.onValueChanged.RemoveListener(action);
                }
            });
            return binding;
        }
        
        public static Binding<TInterface, TConcrete> LinkSliderOnValueChanged<TInterface, TConcrete>(
            this Binding<TInterface, TConcrete> binding,
            Slider slider,
            InstanceContainerDelegate<(float value, TConcrete o)> onValueChanged
        )
        {
            UnityAction<float>? action = null;
            binding.Inject((o, c) =>
            {
                action = v => onValueChanged.Invoke((v,o), c);
                (slider.onValueChanged ??= new Slider.SliderEvent()).AddListener(action);
            });
            binding.Dispose((o, c) =>
            {
                if (action is not null)
                {
                    slider.onValueChanged.RemoveListener(action);
                }
            });
            return binding;
        }
    }
}