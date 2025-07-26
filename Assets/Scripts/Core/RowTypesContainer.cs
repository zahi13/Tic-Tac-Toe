using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RowTypesContainer : MonoBehaviour
{
    [SerializeField] RowTypeObject[] _rowTypeObjects;
    Sprite _rowSprite;
    Sprite _diagonalRowSprite;
    
    public enum RowType
    {
        None,
        HorizontalTop,
        HorizontalMiddle,
        HorizontalBottom,
        VerticalLeft,
        VerticalMiddle,
        VerticalRight,
        ForwardSlash,
        BackwardSlash,
    }

    [Serializable]
    public class RowTypeObject
    {
        public RowType Type;
        public Image Image;

        public void Toggle(bool state)
        {
            Image.enabled = state;
        }
        
        public void SetSprite(Sprite sprite)
        {
            Image.sprite = sprite;
        }
    }

    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        foreach (var row in _rowTypeObjects)
            row.Toggle(false);
    }

    public void ActivateRowType(RowType rowType)
    {
        var rowObject = _rowTypeObjects.FirstOrDefault(x => x.Type == rowType);
        if (rowObject == null) return;
        rowObject.Toggle(true);

        switch (rowType)
        {
            case RowType.None:
                break;
            
            case RowType.HorizontalTop:
            case RowType.HorizontalMiddle:
            case RowType.HorizontalBottom:
            case RowType.VerticalLeft:
            case RowType.VerticalMiddle:
            case RowType.VerticalRight:
                rowObject.SetSprite(_rowSprite);
                break;
            
            case RowType.ForwardSlash:
            case RowType.BackwardSlash:
                rowObject.SetSprite(_diagonalRowSprite);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(rowType), rowType, null);
        }
    }

    public void SetSprites(Sprite rowSprite, Sprite diagonalRowSprite)
    {
        _rowSprite = rowSprite;
        _diagonalRowSprite = diagonalRowSprite;
    }
}
