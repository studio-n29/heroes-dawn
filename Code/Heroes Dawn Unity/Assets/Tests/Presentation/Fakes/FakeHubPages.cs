#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeQgPageView : FakeView, IQgPageView
    {
        public event Action<HubPage>? RoomClicked;
        public event Action? DailyClicked;
        public event Action? SecondaryClicked;
        public event Action? QuestsClicked;
        public event Action? MissionClicked;
        public readonly List<QgPageData> Shown = new List<QgPageData>();
        public QgPageData Last => Shown[Shown.Count - 1];
        public void Show(QgPageData d) => Shown.Add(d);
        public void ClickRoom(HubPage p) => RoomClicked?.Invoke(p);
        public void ClickDaily() => DailyClicked?.Invoke();
        public void ClickSecondary() => SecondaryClicked?.Invoke();
        public void ClickQuests() => QuestsClicked?.Invoke();
        public void ClickMission() => MissionClicked?.Invoke();
    }

    public sealed class FakeTeamPageView : FakeView, ITeamPageView
    {
        public event Action<int>? FilterClicked;
        public event Action<MemberSort>? SortClicked;
        public event Action<MemberId>? MemberTapped;
        public event Action<MemberId>? SquadToggled;
        public event Action? MissionClicked;
        public event Action<int>? PresetClicked;
        public event Action? PresetSaveClicked;
        public void ClickPreset(int i) => PresetClicked?.Invoke(i);
        public void ClickPresetSave() => PresetSaveClicked?.Invoke();
        public readonly List<TeamPageData> Shown = new List<TeamPageData>();
        public TeamPageData Last => Shown[Shown.Count - 1];
        public void Show(TeamPageData d) => Shown.Add(d);
        public void ClickFilter(int i) => FilterClicked?.Invoke(i);
        public void ClickSort(MemberSort s) => SortClicked?.Invoke(s);
        public void TapMember(MemberId id) => MemberTapped?.Invoke(id);
        public void ToggleSquad(MemberId id) => SquadToggled?.Invoke(id);
        public void ClickMission() => MissionClicked?.Invoke();
    }

    public sealed class FakeMemberSheetView : FakeView, IMemberSheetView
    {
        public event Action? PreviousClicked;
        public event Action? NextClicked;
        public event Action? PrimaryClicked;
        public event Action<HubPage>? RoomClicked;
        public event Action? LoadoutClicked;
        public event Action? AffinityClicked;
        public void ClickAffinity() => AffinityClicked?.Invoke();
        public event Action<int>? SkillTileClicked;
        public event Action<int>? ItemTileClicked;
        public event Action<int>? PickerChosen;
        public event Action? PickerRemoved;
        public event Action? PickerClosed;
        public PickerData? LastPicker;
        public void ShowPicker(PickerData d) => LastPicker = d;
        public void HidePicker() => LastPicker = null;
        public void ClickSkillTile(int i) => SkillTileClicked?.Invoke(i);
        public void ClickItemTile(int i) => ItemTileClicked?.Invoke(i);
        public void ChoosePicker(int i) => PickerChosen?.Invoke(i);
        public void RemovePicker() => PickerRemoved?.Invoke();
        public void ClosePicker() => PickerClosed?.Invoke();
        public readonly List<MemberSheetData> Shown = new List<MemberSheetData>();
        public MemberSheetData Last => Shown[Shown.Count - 1];
        public void Show(MemberSheetData d) => Shown.Add(d);
        public void ClickPrevious() => PreviousClicked?.Invoke();
        public void ClickNext() => NextClicked?.Invoke();
        public void ClickPrimary() => PrimaryClicked?.Invoke();
        public void ClickRoom(HubPage p) => RoomClicked?.Invoke(p);
        public void ClickLoadout() => LoadoutClicked?.Invoke();
    }

    /// <summary>Cas d'usage qui ne répond qu'à la demande (tests de Dispose pendant un appel).</summary>
    public sealed class PendingTrain : ITrainMemberUseCase
    {
        public int Calls;
        public readonly UniTaskCompletionSource<Result> Done = new UniTaskCompletionSource<Result>();
        public UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default) { Calls++; return Done.Task; }
    }

    public sealed class PendingToggle : IToggleSquadUseCase
    {
        public int Calls;
        public readonly UniTaskCompletionSource<Result> Done = new UniTaskCompletionSource<Result>();
        public UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default) { Calls++; return Done.Task; }
    }
}
