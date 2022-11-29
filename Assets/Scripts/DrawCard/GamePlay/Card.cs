using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Yuuta.Core;

namespace Yuuta.DrawCard.GamePlay
{
    public class Card : MonoBehaviour
    {
        private const int WRONG_FLASH_TIMES = 2;
        private readonly static TimeSpan ONE_FLASH_DURATION = TimeSpan.FromSeconds(0.2f);
        private readonly static Color WRONG_COLOR = Color.red;
        private readonly static Color CORRECT_COLOR = Color.green;
        
        public record Property(bool IsFlipped, string Text, Action OnClick);

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _button;

        private Option<IDisposable> _onClickSubscribeDisposable;

        public void Render(Property property)
        {
            _text.text = property.IsFlipped ? property.Text : string.Empty;

            foreach (var disposable in _onClickSubscribeDisposable)
                disposable.Dispose();

            _onClickSubscribeDisposable = _button.OnClickAsObservable()
                .Subscribe(_ => property.OnClick.Invoke())
                .AddTo(this)
                .Some();
        }
        
        public async UniTask PlayWrongAnimation()
        {
            Color originalColor = _backgroundImage.color;
            
            for (int i = 0; i < WRONG_FLASH_TIMES; i++)
            {
                await _backgroundImage.DOColor(WRONG_COLOR, (float) ONE_FLASH_DURATION.TotalSeconds)
                    .AsyncWaitForCompletion();
                await _backgroundImage.DOColor(originalColor, (float) ONE_FLASH_DURATION.TotalSeconds)
                    .AsyncWaitForCompletion();
            }
        }
        
        public async UniTask PlayCorrectAnimation()
        {
            Color originalColor = _backgroundImage.color;
            
            await _backgroundImage.DOColor(CORRECT_COLOR, (float) ONE_FLASH_DURATION.TotalSeconds)
                .AsyncWaitForCompletion();
            await _backgroundImage.DOColor(originalColor, (float) ONE_FLASH_DURATION.TotalSeconds)
                .AsyncWaitForCompletion();
        }
    }
}

