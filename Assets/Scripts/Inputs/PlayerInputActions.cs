//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Scripts/Inputs/PlayerInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""perso"",
            ""id"": ""0b618132-4be4-4b59-a576-51538f00d6ab"",
            ""actions"": [
                {
                    ""name"": ""move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e3286fd1-c991-4227-bd74-d6aad86a17ec"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""interact"",
                    ""type"": ""Button"",
                    ""id"": ""430686be-26cd-40e4-9321-059ac16b6dbd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""inventory"",
                    ""type"": ""Button"",
                    ""id"": ""fbfa0ea4-b180-4324-8049-589af83809b1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""map"",
                    ""type"": ""Button"",
                    ""id"": ""2009546e-be2f-4aec-89d1-3576c067877c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""hit"",
                    ""type"": ""Button"",
                    ""id"": ""a71c9118-1699-4f9c-96b6-f90a0e6134e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""run"",
                    ""type"": ""Value"",
                    ""id"": ""0948f1ba-942c-47be-b02f-249a908482fc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""useConso"",
                    ""type"": ""Button"",
                    ""id"": ""af9057fb-3fd3-4bb3-bb36-27d0f1b16911"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""main"",
                    ""id"": ""7da63991-cb3c-430b-977c-698b30b90db7"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""cd5cb580-e676-4ba8-80e1-d63578a65114"",
                    ""path"": ""<XInputController>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""6760f575-c485-4c4d-8260-6622ba5c842e"",
                    ""path"": ""<XInputController>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""3a3703c7-05a4-441b-97e5-e552adca641d"",
                    ""path"": ""<XInputController>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""16c3713d-c67f-4acf-b510-8b7f38817b07"",
                    ""path"": ""<XInputController>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""96585a0b-19a0-4a12-b30c-ee5c5664ed8c"",
                    ""path"": ""<XInputController>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a78382af-fd0b-4b13-a37d-a064cac28d6f"",
                    ""path"": ""<XInputController>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""map"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""93b1a724-d3ff-4856-88c9-ac6ce6414518"",
                    ""path"": ""<XInputController>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""hit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8e26258b-b4ea-4ef6-a1eb-9a22f3f02dcf"",
                    ""path"": ""<XInputController>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""75dfc87d-e548-4042-8a15-def507bbd239"",
                    ""path"": ""<XInputController>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""useConso"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c733c319-9b13-4a35-970c-ef0700b874a0"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""eabb19b5-c974-4c9a-ba69-71b4bad173e5"",
            ""actions"": [
                {
                    ""name"": ""navigate"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6081c15c-8c3f-4213-beb2-816695cc650c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""activate"",
                    ""type"": ""Button"",
                    ""id"": ""bfb0aa0c-6049-4ece-a264-cceaa2a55caa"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""close"",
                    ""type"": ""Button"",
                    ""id"": ""df6b9bf3-aee1-4184-abc3-7153ec7e1a6f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""main"",
                    ""id"": ""33ef89ea-5a45-430e-ac93-d53dfee50cbf"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""81755a8d-61be-48a9-8fbf-1f5d6e2789cd"",
                    ""path"": ""<XInputController>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2c5c333e-9484-4f0c-a652-e3385fe00ce5"",
                    ""path"": ""<XInputController>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""59adbb4a-f239-4404-ba67-9bcb5b0f0bdd"",
                    ""path"": ""<XInputController>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8bcedef8-4a78-4d85-ad3f-14f5835063d0"",
                    ""path"": ""<XInputController>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""5bbfd291-7f88-4772-9484-02def75ea68a"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""activate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2ec2c4ba-8c51-42d3-b555-e85958480e1f"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""close"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""enhanced_perso"",
            ""id"": ""496fb706-37aa-4ede-8ba7-f8a5dfa615a7"",
            ""actions"": [
                {
                    ""name"": ""dash"",
                    ""type"": ""Button"",
                    ""id"": ""a9789480-6c9d-480f-aa99-098e4a684ac2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""hack"",
                    ""type"": ""Button"",
                    ""id"": ""7ccb0cc6-c229-46ca-974b-ed8cb2b5d5ef"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""hackDirection"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d41e8cc6-f0a5-4d64-b135-7e5433b38655"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""74a37704-95d7-41bd-a178-f90badf1e6ba"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""30be72d1-30ce-48e3-aa76-32f91b036399"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""hack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""main"",
                    ""id"": ""8e057bc5-8757-40be-b237-903be937e396"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""hackDirection"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""09eb962e-60a2-4161-a347-5c262b5efdc7"",
                    ""path"": ""<XInputController>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""hackDirection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""fa4c2ceb-4f91-40c7-95bf-d6129d404951"",
                    ""path"": ""<XInputController>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""hackDirection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""37850b98-b822-4000-9a19-9fa8421862cf"",
                    ""path"": ""<XInputController>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""hackDirection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""612be825-b9b3-4c13-b81a-7d17431309e0"",
                    ""path"": ""<XInputController>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""hackDirection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""xbox"",
            ""bindingGroup"": ""xbox"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // perso
        m_perso = asset.FindActionMap("perso", throwIfNotFound: true);
        m_perso_move = m_perso.FindAction("move", throwIfNotFound: true);
        m_perso_interact = m_perso.FindAction("interact", throwIfNotFound: true);
        m_perso_inventory = m_perso.FindAction("inventory", throwIfNotFound: true);
        m_perso_map = m_perso.FindAction("map", throwIfNotFound: true);
        m_perso_hit = m_perso.FindAction("hit", throwIfNotFound: true);
        m_perso_run = m_perso.FindAction("run", throwIfNotFound: true);
        m_perso_useConso = m_perso.FindAction("useConso", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_navigate = m_UI.FindAction("navigate", throwIfNotFound: true);
        m_UI_activate = m_UI.FindAction("activate", throwIfNotFound: true);
        m_UI_close = m_UI.FindAction("close", throwIfNotFound: true);
        // enhanced_perso
        m_enhanced_perso = asset.FindActionMap("enhanced_perso", throwIfNotFound: true);
        m_enhanced_perso_dash = m_enhanced_perso.FindAction("dash", throwIfNotFound: true);
        m_enhanced_perso_hack = m_enhanced_perso.FindAction("hack", throwIfNotFound: true);
        m_enhanced_perso_hackDirection = m_enhanced_perso.FindAction("hackDirection", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // perso
    private readonly InputActionMap m_perso;
    private List<IPersoActions> m_PersoActionsCallbackInterfaces = new List<IPersoActions>();
    private readonly InputAction m_perso_move;
    private readonly InputAction m_perso_interact;
    private readonly InputAction m_perso_inventory;
    private readonly InputAction m_perso_map;
    private readonly InputAction m_perso_hit;
    private readonly InputAction m_perso_run;
    private readonly InputAction m_perso_useConso;
    public struct PersoActions
    {
        private @PlayerInputActions m_Wrapper;
        public PersoActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @move => m_Wrapper.m_perso_move;
        public InputAction @interact => m_Wrapper.m_perso_interact;
        public InputAction @inventory => m_Wrapper.m_perso_inventory;
        public InputAction @map => m_Wrapper.m_perso_map;
        public InputAction @hit => m_Wrapper.m_perso_hit;
        public InputAction @run => m_Wrapper.m_perso_run;
        public InputAction @useConso => m_Wrapper.m_perso_useConso;
        public InputActionMap Get() { return m_Wrapper.m_perso; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PersoActions set) { return set.Get(); }
        public void AddCallbacks(IPersoActions instance)
        {
            if (instance == null || m_Wrapper.m_PersoActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PersoActionsCallbackInterfaces.Add(instance);
            @move.started += instance.OnMove;
            @move.performed += instance.OnMove;
            @move.canceled += instance.OnMove;
            @interact.started += instance.OnInteract;
            @interact.performed += instance.OnInteract;
            @interact.canceled += instance.OnInteract;
            @inventory.started += instance.OnInventory;
            @inventory.performed += instance.OnInventory;
            @inventory.canceled += instance.OnInventory;
            @map.started += instance.OnMap;
            @map.performed += instance.OnMap;
            @map.canceled += instance.OnMap;
            @hit.started += instance.OnHit;
            @hit.performed += instance.OnHit;
            @hit.canceled += instance.OnHit;
            @run.started += instance.OnRun;
            @run.performed += instance.OnRun;
            @run.canceled += instance.OnRun;
            @useConso.started += instance.OnUseConso;
            @useConso.performed += instance.OnUseConso;
            @useConso.canceled += instance.OnUseConso;
        }

        private void UnregisterCallbacks(IPersoActions instance)
        {
            @move.started -= instance.OnMove;
            @move.performed -= instance.OnMove;
            @move.canceled -= instance.OnMove;
            @interact.started -= instance.OnInteract;
            @interact.performed -= instance.OnInteract;
            @interact.canceled -= instance.OnInteract;
            @inventory.started -= instance.OnInventory;
            @inventory.performed -= instance.OnInventory;
            @inventory.canceled -= instance.OnInventory;
            @map.started -= instance.OnMap;
            @map.performed -= instance.OnMap;
            @map.canceled -= instance.OnMap;
            @hit.started -= instance.OnHit;
            @hit.performed -= instance.OnHit;
            @hit.canceled -= instance.OnHit;
            @run.started -= instance.OnRun;
            @run.performed -= instance.OnRun;
            @run.canceled -= instance.OnRun;
            @useConso.started -= instance.OnUseConso;
            @useConso.performed -= instance.OnUseConso;
            @useConso.canceled -= instance.OnUseConso;
        }

        public void RemoveCallbacks(IPersoActions instance)
        {
            if (m_Wrapper.m_PersoActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPersoActions instance)
        {
            foreach (var item in m_Wrapper.m_PersoActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PersoActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PersoActions @perso => new PersoActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();
    private readonly InputAction m_UI_navigate;
    private readonly InputAction m_UI_activate;
    private readonly InputAction m_UI_close;
    public struct UIActions
    {
        private @PlayerInputActions m_Wrapper;
        public UIActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @navigate => m_Wrapper.m_UI_navigate;
        public InputAction @activate => m_Wrapper.m_UI_activate;
        public InputAction @close => m_Wrapper.m_UI_close;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void AddCallbacks(IUIActions instance)
        {
            if (instance == null || m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
            @navigate.started += instance.OnNavigate;
            @navigate.performed += instance.OnNavigate;
            @navigate.canceled += instance.OnNavigate;
            @activate.started += instance.OnActivate;
            @activate.performed += instance.OnActivate;
            @activate.canceled += instance.OnActivate;
            @close.started += instance.OnClose;
            @close.performed += instance.OnClose;
            @close.canceled += instance.OnClose;
        }

        private void UnregisterCallbacks(IUIActions instance)
        {
            @navigate.started -= instance.OnNavigate;
            @navigate.performed -= instance.OnNavigate;
            @navigate.canceled -= instance.OnNavigate;
            @activate.started -= instance.OnActivate;
            @activate.performed -= instance.OnActivate;
            @activate.canceled -= instance.OnActivate;
            @close.started -= instance.OnClose;
            @close.performed -= instance.OnClose;
            @close.canceled -= instance.OnClose;
        }

        public void RemoveCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IUIActions instance)
        {
            foreach (var item in m_Wrapper.m_UIActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public UIActions @UI => new UIActions(this);

    // enhanced_perso
    private readonly InputActionMap m_enhanced_perso;
    private List<IEnhanced_persoActions> m_Enhanced_persoActionsCallbackInterfaces = new List<IEnhanced_persoActions>();
    private readonly InputAction m_enhanced_perso_dash;
    private readonly InputAction m_enhanced_perso_hack;
    private readonly InputAction m_enhanced_perso_hackDirection;
    public struct Enhanced_persoActions
    {
        private @PlayerInputActions m_Wrapper;
        public Enhanced_persoActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @dash => m_Wrapper.m_enhanced_perso_dash;
        public InputAction @hack => m_Wrapper.m_enhanced_perso_hack;
        public InputAction @hackDirection => m_Wrapper.m_enhanced_perso_hackDirection;
        public InputActionMap Get() { return m_Wrapper.m_enhanced_perso; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(Enhanced_persoActions set) { return set.Get(); }
        public void AddCallbacks(IEnhanced_persoActions instance)
        {
            if (instance == null || m_Wrapper.m_Enhanced_persoActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_Enhanced_persoActionsCallbackInterfaces.Add(instance);
            @dash.started += instance.OnDash;
            @dash.performed += instance.OnDash;
            @dash.canceled += instance.OnDash;
            @hack.started += instance.OnHack;
            @hack.performed += instance.OnHack;
            @hack.canceled += instance.OnHack;
            @hackDirection.started += instance.OnHackDirection;
            @hackDirection.performed += instance.OnHackDirection;
            @hackDirection.canceled += instance.OnHackDirection;
        }

        private void UnregisterCallbacks(IEnhanced_persoActions instance)
        {
            @dash.started -= instance.OnDash;
            @dash.performed -= instance.OnDash;
            @dash.canceled -= instance.OnDash;
            @hack.started -= instance.OnHack;
            @hack.performed -= instance.OnHack;
            @hack.canceled -= instance.OnHack;
            @hackDirection.started -= instance.OnHackDirection;
            @hackDirection.performed -= instance.OnHackDirection;
            @hackDirection.canceled -= instance.OnHackDirection;
        }

        public void RemoveCallbacks(IEnhanced_persoActions instance)
        {
            if (m_Wrapper.m_Enhanced_persoActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IEnhanced_persoActions instance)
        {
            foreach (var item in m_Wrapper.m_Enhanced_persoActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_Enhanced_persoActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public Enhanced_persoActions @enhanced_perso => new Enhanced_persoActions(this);
    private int m_xboxSchemeIndex = -1;
    public InputControlScheme xboxScheme
    {
        get
        {
            if (m_xboxSchemeIndex == -1) m_xboxSchemeIndex = asset.FindControlSchemeIndex("xbox");
            return asset.controlSchemes[m_xboxSchemeIndex];
        }
    }
    public interface IPersoActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnInventory(InputAction.CallbackContext context);
        void OnMap(InputAction.CallbackContext context);
        void OnHit(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnUseConso(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnNavigate(InputAction.CallbackContext context);
        void OnActivate(InputAction.CallbackContext context);
        void OnClose(InputAction.CallbackContext context);
    }
    public interface IEnhanced_persoActions
    {
        void OnDash(InputAction.CallbackContext context);
        void OnHack(InputAction.CallbackContext context);
        void OnHackDirection(InputAction.CallbackContext context);
    }
}
