using KeceK.Input;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Game
{
    public class CharacterMovement : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
    }
}
