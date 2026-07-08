using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class DropdownReference
    {
        private enum Type
        {
            Unity = 0,
            TMP = 1
        }

        // private const int MAX_CHARACTER_COUNT = 99999;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Type m_Type = Type.Unity;
        [SerializeField] private Dropdown m_Unity;
        [SerializeField] private TMP_Dropdown m_TMP;
        
        // MEMBERS: -------------------------------------------------------------------------------

        // [NonSerialized] private string m_Value;
        // [NonSerialized] private int m_CharactersVisible = MAX_CHARACTER_COUNT;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        /*public string Text
        {
            get => m_Value;
            set
            {
                m_Value = value;
                Refresh();
            }
        }*/
        
        public GameObject GameObject => m_Type switch
        {
            Type.Unity => m_Unity.gameObject,
            Type.TMP => m_TMP.gameObject,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        public bool Interactable
        {
            get =>
                m_Type switch
                {
                    Type.Unity => m_Unity.interactable,
                    Type.TMP => m_TMP.interactable,
                    _ => throw new ArgumentOutOfRangeException()
                };
            set
            {
                switch (m_Type)
                {
                    case Type.Unity:
                        m_Unity.interactable = value;
                        break;
                    case Type.TMP:
                        m_TMP.interactable = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Component component => m_Type switch
        {
            Type.Unity => m_Unity,
            Type.TMP => m_TMP,
            _ => throw new ArgumentOutOfRangeException()
        };

        /*public Color Color
        {
            get => m_Type switch
            {
                Type.Unity => m_Unity.textComponent.color,
                Type.TMP => m_TMP.textComponent.color,
                _ => throw new ArgumentOutOfRangeException()
            };
            set
            {
                switch (m_Type)
                {
                    case Type.Unity: m_Unity.textComponent.color = value; break;
                    case Type.TMP: m_TMP.textComponent.color = value; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }*/

        // public bool AreAllCharactersVisible => this.m_Value.Length <= this.CharactersVisible;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DropdownReference()
        {
            // m_Value = Text;
        }

        public DropdownReference(Dropdown unity) : this()
        {
            m_Type = Type.Unity;
            m_Unity = unity;
        }

        public DropdownReference(TMP_Dropdown text)
        {
            m_Type = Type.TMP;
            m_TMP = text;
        }

        public void SubscribeOnValueChanged(UnityAction<int> onValueChanged)
        {
            switch (m_Type)
            {
                case Type.Unity: m_Unity.onValueChanged.AddListener(onValueChanged); break;
                case Type.TMP: m_TMP.onValueChanged.AddListener(onValueChanged); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public void UnsubscribeOnValueChanged(UnityAction<int> onValueChanged)
        {
            switch (m_Type)
            {
                case Type.Unity: m_Unity.onValueChanged.RemoveListener(onValueChanged); break;
                case Type.TMP: m_TMP.onValueChanged.RemoveListener(onValueChanged); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        // TO STRING: -----------------------------------------------------------------------------

        public override string ToString()
        {
            return m_Type switch
            {
                Type.Unity => m_Unity != null ? m_Unity.gameObject.name : "(none)",
                Type.TMP => m_TMP != null ? m_TMP.gameObject.name : "(none)",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        /*private void Refresh()
        {
            switch (m_Type)
            {
                case Type.Unity:
                    if (m_Unity == null) return;
                    // int count = Math.Min(this.m_Value.Length, this.CharactersVisible);
                    m_Unity.text = m_Value;
                    break;
                
                case Type.TMP:
                    if (m_TMP == null) return;
                    m_TMP.text = m_Value;
                    // this.m_TMP.maxVisibleCharacters = this.CharactersVisible;
                    break;
                
                default: throw new ArgumentOutOfRangeException();
            }
        }*/

        /*public void DeactivateInputField()
        {
            switch (m_Type)
            {
                case Type.Unity: m_Unity.DeactivateInputField(); break;
                case Type.TMP: m_TMP.DeactivateInputField(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void ActivateInputField()
        {
            switch (m_Type)
            {
                case Type.Unity: m_Unity.ActivateInputField(); break;
                case Type.TMP: m_TMP.ActivateInputField(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }*/

        public void Select()
        {
            switch (m_Type)
            {
                case Type.Unity: m_Unity.Select(); break;
                case Type.TMP: m_TMP.Select(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            switch (m_Type)
            {
                case Type.Unity: m_Unity.OnPointerClick(pointerEventData); break;
                case Type.TMP: m_TMP.OnPointerClick(pointerEventData); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}