using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Yuuta.Core;
using Random = UnityEngine.Random;

namespace Yuuta.DrawCard.GamePlay
{
    public class Board : MonoBehaviour
    {
        private readonly static string[] POSSIBLE_TEXT_VALUES = new[]
        { "悠", "太", "翼", "寫", "程", "式", "語", "言" };
        
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Transform _boardRootTransform;
        [SerializeField] private TextMeshProUGUI _stepCountText;

        private IReactiveProperty<Card.Property>[] _cardProperties;
        private Card[] _cards;
        private Option<int> _previousFlippedCardIndex;
        private bool _isPause;
        private IReactiveProperty<int> _stepCount = new ReactiveProperty<int>(0);
        
        private void Start()
        {
            _cardProperties = POSSIBLE_TEXT_VALUES
                .SelectMany(text => new [] { text, text })
                .Shuffle()
                .Select((text, index) => new ReactiveProperty<Card.Property>(
                    new Card.Property(
                        IsFlipped: false, 
                        text, 
                        () => UniTask.Void(async () => await _ClickCard(index)))))
                .ToArray();
            _cards = _cardProperties
                .Select(_ => Instantiate<Card>(_cardPrefab, _boardRootTransform))
                .ToArray();

            foreach (var (card, property) in _cards.ZipTuple(_cardProperties))
            {
                property.Subscribe(propertyValue => card.Render(propertyValue))
                    .AddTo(this);
            }
            
            _stepCount.Subscribe(stepCount =>
                _stepCountText.text = $"步數： {stepCount} 步")
                .AddTo(this);
        }

        private void _FlipCard(int index)
            => _cardProperties[index].Value = _cardProperties[index].Value with
            {
                IsFlipped = !_cardProperties[index].Value.IsFlipped
            };
        
        private async UniTask _ClickCard(int index)
        {
            if (_isPause)
                return;

            if (_cardProperties[index].Value.IsFlipped)
                return;

            _FlipCard(index);

            await _previousFlippedCardIndex.Match(
                previousFlippedCardIndex => UniTask.Create(async () =>
                {
                    if (_cardProperties[previousFlippedCardIndex].Value.Text != _cardProperties[index].Value.Text)
                    {
                        _isPause = true;
                        await UniTask.WhenAll(
                            _cards[index].PlayWrongAnimation(),
                            _cards[previousFlippedCardIndex].PlayWrongAnimation());
                        _isPause = false;
                        _FlipCard(index);
                        _FlipCard(previousFlippedCardIndex);
                    }
                    else
                    {
                        _isPause = true;
                        await UniTask.WhenAll(
                            _cards[index].PlayCorrectAnimation(),
                            _cards[previousFlippedCardIndex].PlayCorrectAnimation());
                        _isPause = false;
                    }

                    _stepCount.Value++;
                    _previousFlippedCardIndex = Option<int>.None();
                }),
                () =>
                {
                    _previousFlippedCardIndex = index.Some();
                    return UniTask.CompletedTask;
                });

        }
    }
}