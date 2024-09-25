using ManualDi.Main;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        
        public static TypeBinding<TInterface, TConcrete> LinkButtonOnClick<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            Button button,
            InstanceContainerDelegate<TConcrete> onClick
            )
        {
            UnityAction? action = null;
            typeBinding.Inject((o, c) =>
            {
                action = () => onClick.Invoke(o, c);
                (button.onClick ??= new Button.ButtonClickedEvent()).AddListener(action);
            });
            typeBinding.Dispose((o, c) =>
            {
                if (action is not null)
                {
                    button.onClick.RemoveListener(action);
                }
            });
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> LinkToggleOnValueChanged<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            Toggle toggle,
            InstanceContainerDelegate<(bool value, TConcrete o)> onValueChanged
        )
        {
            UnityAction<bool>? action = null;
            typeBinding.Inject((o, c) =>
            {
                action = v => onValueChanged.Invoke((v, o), c);
                (toggle.onValueChanged ??= new Toggle.ToggleEvent()).AddListener(action);
            });
            typeBinding.Dispose((o, c) =>
            {
                if (action is not null)
                {
                    toggle.onValueChanged.RemoveListener(action);
                }
            });
            return typeBinding;
        }
        
        public static TypeBinding<TInterface, TConcrete> LinkSliderOnValueChanged<TInterface, TConcrete>(
            this TypeBinding<TInterface, TConcrete> typeBinding,
            Slider slider,
            InstanceContainerDelegate<(float value, TConcrete o)> onValueChanged
        )
        {
            UnityAction<float>? action = null;
            typeBinding.Inject((o, c) =>
            {
                action = v => onValueChanged.Invoke((v,o), c);
                (slider.onValueChanged ??= new Slider.SliderEvent()).AddListener(action);
            });
            typeBinding.Dispose((o, c) =>
            {
                if (action is not null)
                {
                    slider.onValueChanged.RemoveListener(action);
                }
            });
            return typeBinding;
        }
    }
}