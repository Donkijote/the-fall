using System;
using UnityEngine;

namespace TheFall.Presentation.Match
{
    /// <summary>
    /// Persistent scene-authored geometry and anchors for the first-playable table. Runtime match
    /// state remains dynamic, but edits to these transforms are the presentation source of truth.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class FirstPlayableTableLayout : MonoBehaviour
    {
        [SerializeField] private GameObject _environment;
        [SerializeField] private GameObject _table;
        [SerializeField] private GameObject _localSeat;
        [SerializeField] private GameObject _opponentSeat;
        [SerializeField] private Transform _cardZonesRoot;
        [SerializeField] private Transform _dealerSpreadAnchor;
        [SerializeField] private Transform _deckAnchor;
        [SerializeField] private Transform _tableCardsAnchor;
        [SerializeField] private Transform _localHandAnchor;
        [SerializeField] private Transform _opponentHandAnchor;
        [SerializeField] private Transform _localCapturedAnchor;
        [SerializeField] private Transform _opponentCapturedAnchor;
        [SerializeField] private Transform _cardSizeReference;
        [SerializeField] private Transform[] _cardPreviews = Array.Empty<Transform>();

        public GameObject Environment => _environment;

        public GameObject Table => _table;

        public GameObject LocalSeat => _localSeat;

        public GameObject OpponentSeat => _opponentSeat;

        public Transform CardZonesRoot => _cardZonesRoot;

        public Transform DealerSpreadAnchor => _dealerSpreadAnchor;

        public Transform DeckAnchor => _deckAnchor;

        public Transform TableCardsAnchor => _tableCardsAnchor;

        public Transform LocalHandAnchor => _localHandAnchor;

        public Transform OpponentHandAnchor => _opponentHandAnchor;

        public Transform LocalCapturedAnchor => _localCapturedAnchor;

        public Transform OpponentCapturedAnchor => _opponentCapturedAnchor;

        public Vector3 CardScale => _cardSizeReference != null
            ? _cardSizeReference.localScale
            : new Vector3(0.19f, 0.012f, 0.19f * 88f / 63f);

        public bool IsConfigured =>
            _environment != null &&
            _table != null &&
            _localSeat != null &&
            _opponentSeat != null &&
            _cardZonesRoot != null &&
            _dealerSpreadAnchor != null &&
            _deckAnchor != null &&
            _tableCardsAnchor != null &&
            _localHandAnchor != null &&
            _opponentHandAnchor != null &&
            _localCapturedAnchor != null &&
            _opponentCapturedAnchor != null &&
            _cardSizeReference != null;

#if UNITY_EDITOR
        public void Configure(
            GameObject environment,
            GameObject table,
            GameObject localSeat,
            GameObject opponentSeat,
            Transform cardZonesRoot,
            Transform dealerSpreadAnchor,
            Transform deckAnchor,
            Transform tableCardsAnchor,
            Transform localHandAnchor,
            Transform opponentHandAnchor,
            Transform localCapturedAnchor,
            Transform opponentCapturedAnchor,
            Transform cardSizeReference,
            Transform[] cardPreviews)
        {
            _environment = environment;
            _table = table;
            _localSeat = localSeat;
            _opponentSeat = opponentSeat;
            _cardZonesRoot = cardZonesRoot;
            _dealerSpreadAnchor = dealerSpreadAnchor;
            _deckAnchor = deckAnchor;
            _tableCardsAnchor = tableCardsAnchor;
            _localHandAnchor = localHandAnchor;
            _opponentHandAnchor = opponentHandAnchor;
            _localCapturedAnchor = localCapturedAnchor;
            _opponentCapturedAnchor = opponentCapturedAnchor;
            _cardSizeReference = cardSizeReference;
            _cardPreviews = cardPreviews ?? Array.Empty<Transform>();
            SynchronizeCardPreviews();
        }
#endif

        private void Update()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                SynchronizeCardPreviews();
            }
        }

        private void OnValidate()
        {
            SynchronizeCardPreviews();
        }

        private void SynchronizeCardPreviews()
        {
            if (_cardSizeReference == null)
            {
                return;
            }

            var width = Mathf.Max(0.02f, _cardSizeReference.localScale.x);
            var expected = new Vector3(width, 0.012f, width * 88f / 63f);
            if (_cardSizeReference.localScale != expected)
            {
                _cardSizeReference.localScale = expected;
            }

            foreach (var preview in _cardPreviews)
            {
                if (preview != null && preview.localScale != expected)
                {
                    preview.localScale = expected;
                }
            }
        }
    }
}
