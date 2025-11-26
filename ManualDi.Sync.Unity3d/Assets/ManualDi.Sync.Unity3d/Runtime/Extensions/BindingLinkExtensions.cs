using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ManualDi.Sync.Unity3d
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
        
        public static Binding<TConcrete> LinkButtonOnClick<TInterface, TConcrete>(
            this Binding<TConcrete> binding,
            Button button,
            Action<TConcrete, IDiContainer> onClick
            )
        {
            binding.Inject((o, c) =>
            {
                var to = (TConcrete)o;
                UnityAction action = () => onClick.Invoke(to, c);
                (button.onClick ??= new Button.ButtonClickedEvent()).AddListener(action);
                
                c.QueueDispose(() => {
                    button.onClick.RemoveListener(action);
                });
            });
            return binding;
        }
        
        public static Binding<TConcrete> LinkToggleOnValueChanged<TInterface, TConcrete>(
            this Binding<TConcrete> binding,
            Toggle toggle,
            Action<TConcrete, IDiContainer, bool> onValueChanged
        )
        {
            binding.Inject((o, c) =>
            {
                var to = (TConcrete)o;
                UnityAction<bool> action = v => onValueChanged.Invoke(to, c, v);
                (toggle.onValueChanged ??= new Toggle.ToggleEvent()).AddListener(action);
                
                c.QueueDispose(() => {
                    toggle.onValueChanged.RemoveListener(action);
                });
            });
            return binding;
        }
        
        public static Binding<TConcrete> LinkSliderOnValueChanged<TInterface, TConcrete>(
            this Binding<TConcrete> binding,
            Slider slider,
            Action<TConcrete, IDiContainer, float> onValueChanged
        )
        {
            UnityAction<float>? action = null;
            binding.Inject((o, c) =>
            {
                var to = (TConcrete)o;
                action = v => onValueChanged.Invoke(to, c, v);
                (slider.onValueChanged ??= new Slider.SliderEvent()).AddListener(action);
                
                c.QueueDispose(() => {
                    slider.onValueChanged.RemoveListener(action);
                });
            });
            return binding;
        }
    }
}