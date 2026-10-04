#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Salon (prefab Pages/Salon) : bandeau « version simple », une ligne par membre.</summary>
    public sealed class SalonPageView : ViewBase, ISalonPageView
    {
        [SerializeField] TMP_Text banner = null!;
        [SerializeField] RectTransform list = null!;
        [SerializeField] MemberRowView rowPrefab = null!;

        readonly List<MemberRowView> _rows = new List<MemberRowView>();

        public event Action<MemberId>? TrainClicked;

        public IReadOnlyList<MemberRowView> Rows => _rows;

        public void Show(SalonPageData d)
        {
            banner.text = d.Banner;
            list.Clear();
            _rows.Clear();
            foreach (var r in d.Rows)
            {
                var row = Instantiate(rowPrefab, list);
                var id = r.Id;
                row.ActionClicked += () => TrainClicked?.Invoke(id);
                row.Show(r);
                _rows.Add(row);
            }
        }
    }
}
