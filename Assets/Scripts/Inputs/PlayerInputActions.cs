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
                    ""name"": ""dash"",
                    ""type"": ""Button"",
                    ""id"": ""accb593b-fa1f-45d1-a583-3ffbf250f047"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""interact"",
                    ""type"": ""Button"",
                    ""id"": ""d0048e6d-9883-434b-b045-c6fa71aea51d"",
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
                },
                {
                    ""name"": ""hack"",
                    ""type"": ""Button"",
                    ""id"": ""4ccbf631-dd20-4327-bd3b-c1b7c8a1506d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""hackDirection"",
                    ""type"": ""PassThrough"",
                    ""id"": ""b3a199ca-b0b5-4d60-b9a1-9f06eee85935"",
                    ""expectedControlType"": ""Vector2"",
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
                    ""id"": ""4ce89410-35a6-4fc2-a5f1-577db6a2f960"",
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
                    ""id"": ""5262e0cd-f070-46ed-9583-016bd2b1d03f"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""xbox"",
                    ""action"": ""interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
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
                    ""id"": ""d63eb17d-eb99-4563-8789-006ea5d0662d"",
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
                    ""id"": ""34d12e2d-18a6-445a-99fa-1429f1d9c490"",
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
                    ""id"": ""4bca6e0b-3f7b-451e-b704-82945e3d1ea3"",
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
                    ""id"": ""cf090caa-da71-47f3-b247-af6810818aa0"",
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
                    ""id"": ""32a20702-86bd-4511-8b77-df52e1bab559"",
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
                    ""id"": ""7bbd0b55-f935-40b3-b5c6-d24264fd0d50"",
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
        m_perso_dash = m_perso.FindAction("dash", throwIfNotFound: true);
        m_perso_interact = m_perso.FindAction("interact", throwIfNotFound: true);
        m_perso_inventory = m_perso.FindAction("inventory", throwIfNotFound: true);
        m_perso_map = m_perso.FindAction("map", throwIfNotFound: true);
        m_perso_hit = m_perso.FindAction("hit", throwIfNotFound: true);
        m_perso_run = m_perso.FindAction("run", throwIfNotFound: true);
        m_perso_useConso = m_perso.FindAction("useConso", throwIfNotFound: true);
        m_perso_hack = m_perso.FindAction("hack", throwIfNotFound: true);
        m_perso_hackDirection = m_perso.FindAction("hackDirection", throwIfNotFound: true);
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
    private readonly InputAction m_perso_dash;
    private readonly InputAction m_perso_interact;
    private readonly InputAction m_perso_inventory;
    private readonly InputAction m_perso_map;
    private readonly InputAction m_perso_hit;
    private readonly InputAction m_perso_run;
    private readonly InputAction m_perso_useConso;
    private readonly InputAction m_perso_hack;
    private readonly InputAction m_perso_hackDirection;
    public struct PersoActions
    {
        private @PlayerInputActions m_Wrapper;
        public PersoActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @move => m_Wrapper.m_perso_move;
        public InputAction @dash => m_Wrapper.m_perso_dash;
        public InputAction @interact => m_Wrapper.m_perso_interact;
        public InputAction @inventory => m_Wrapper.m_perso_inventory;
        public InputAction @map => m_Wrapper.m_perso_map;
        public InputAction @hit => m_Wrapper.m_perso_hit;
        public InputAction @run => m_Wrapper.m_perso_run;
        public InputAction @useConso => m_Wrapper.m_perso_useConso;
        public InputAction @hack => m_Wrapper.m_perso_hack;
        public InputAction @hackDirection => m_Wrapper.m_perso_hackDirection;
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
            @dash.started += instance.OnDash;
            @dash.performed += instance.OnDash;
            @dash.canceled += instance.OnDash;
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
            @hack.started += instance.OnHack;
            @hack.performed += instance.OnHack;
            @hack.canceled += instance.OnHack;
            @hackDirection.started += instance.OnHackDirection;
            @hackDirection.performed += instance.OnHackDirection;
            @hackDirection.canceled += instance.OnHackDirection;
        }

        private void UnregisterCallbacks(IPersoActions instance)
        {
            @move.started -= instance.OnMove;
            @move.performed -= instance.OnMove;
            @move.canceled -= instance.OnMove;
            @dash.started -= instance.OnDash;
            @dash.performed -= instance.OnDash;
            @dash.canceled -= instance.OnDash;
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
            @hack.started -= instance.OnHack;
            @hack.performed -= instance.OnHack;
            @hack.canceled -= instance.OnHack;
            @hackDirection.started -= instance.OnHackDirection;
            @hackDirection.performed -= instance.OnHackDirection;
            @hackDirection.canceled -= instance.OnHackDirection;
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
        void OnDash(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnInventory(InputAction.CallbackContext context);
        void OnMap(InputAction.CallbackContext context);
        void OnHit(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnUseConso(InputAction.CallbackContext context);
        void OnHack(InputAction.CallbackContext context);
        void OnHackDirection(InputAction.CallbackContext context);
    }
}
