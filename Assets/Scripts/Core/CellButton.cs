using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayPerfect.UI
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class CellButton : MonoBehaviour
    {
        [SerializeField] int _rowIndex;
        [SerializeField] int _columnIndex;
        Button _button;
        Image _image;

        public void Initialize(int row, int column, Action<CellButton> onCellClickedCallback)
        {
            if (TryGetComponent(out Button button))
            {
                _button = button;
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => onCellClickedCallback?.Invoke(this));
            }

            if (TryGetComponent(out Image image))
            {
                _image = image;
            }
            
            SetCoordinates(row, column);
        }

        void SetCoordinates(int row, int column)
        {
            _rowIndex = row;
            _columnIndex = column;
        }

        public Tuple<int, int> GetCoordinates()
        {
            return new Tuple<int, int>(_rowIndex, _columnIndex);
        }

        public void UpdateSprite(Sprite sprite)
        {
            var color = _image.color;
            color.a = sprite != null ? 1 : 0;
            _image.color = color;
            _image.sprite = sprite;
        }

        public void ToggleInteraction(bool state)
        {
            _button.interactable = state;
        }
    }
}