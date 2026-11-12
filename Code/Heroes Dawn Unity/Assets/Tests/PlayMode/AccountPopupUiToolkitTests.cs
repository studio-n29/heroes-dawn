#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.IO;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour la fenêtre Compte (vue de production, UI Toolkit) : quatre formats, focus manette piégé dans la fenêtre, Retour.</summary>
    public sealed class AccountPopupUiToolkitTests
    {
        static readonly (int w, int h, string name)[] Formats = { (1920, 1080, "16x9"), (2160, 972, "20x9"), (1280, 960, "4x3"), (1280, 720, "720p") };
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-account"));

        static (AccountPopupView view, RenderTexture rt, GameObject go) Spawn(int w, int h, AccountMode mode)
        {
            var rt = new RenderTexture(w, h, 24);
            var settings = Object.Instantiate(Resources.Load<PanelSettings>("UiToolkit/HdPanelSettings"));
            settings.targetTexture = rt;
            var go = new GameObject("AccountUnderTest", typeof(RectTransform));
            go.SetActive(false);
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = settings;
#if UNITY_EDITOR
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/UIToolkit/Screens/Account/Account.uxml");
#endif
            var view = go.AddComponent<AccountPopupView>();
            go.SetActive(true);
            bool up = mode == AccountMode.SignUp;
            view.Show(new AccountPopupData
            {
                Mode = mode, Title = Loc.T(up ? "account.title.signup" : "account.title.signin"),
                TabSignIn = Loc.T("account.tab.signin"), TabSignUp = Loc.T("account.tab.signup"),
                EmailHint = Loc.T("account.email"), PasswordHint = Loc.T("account.password"), NameHint = Loc.T("account.name"),
                Submit = Loc.T(up ? "account.submit.signup" : "account.submit.signin"),
                Google = Loc.T("account.google"), Apple = Loc.T("account.apple"), Forgot = Loc.T("account.forgot"), Close = Loc.T("account.close"),
            });
            // Message long : la fenêtre ne doit pas déborder.
            view.ShowMessage(Loc.T("account.confirm_sent"), true);
            return (view, rt, go);
        }

        static void Save(RenderTexture rt, string name)
        {
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        static IEnumerable<VisualElement> Controls(VisualElement window)
        {
            foreach (var e in window.Query<VisualElement>().ToList())
                if (e is UnityEngine.UIElements.Button || e is TextField) yield return e;
        }

        [UnityTest, Timeout(180000), Description("Vérifie que la fenêtre Compte (connexion et création, avec un long message) reste entière dans le panneau, avec champs et boutons visibles et à taille de manette, sur 16:9, 20:9, 4:3 et 1280×720 — parce que le formulaire est le plus haut de l'écran de connexion et déborde en premier sur les formats courts")]
        public IEnumerator Account_FitsAndStaysTappable_OnTheFourFormats()
        {
            foreach (var mode in new[] { AccountMode.SignIn, AccountMode.SignUp })
                foreach (var (w, h, name) in Formats)
                {
                    var (view, rt, go) = Spawn(w, h, mode);
                    for (int i = 0; i < 12; i++) yield return null;
                    yield return new WaitForSeconds(0.5f);
                    var panel = view.Root.panel.visualTree.worldBound;
                    var window = view.Root.Q<VisualElement>("window");
                    var wb = window.worldBound;
                    string tag = $"{mode} {name}";
                    Assert.That(wb.yMin, Is.GreaterThanOrEqualTo(panel.yMin - 0.5f), tag + " : fenêtre coupée en haut");
                    Assert.That(wb.yMax, Is.LessThanOrEqualTo(panel.yMax + 0.5f), tag + " : fenêtre coupée en bas");
                    Assert.That(wb.xMin, Is.GreaterThanOrEqualTo(panel.xMin - 0.5f), tag + " : fenêtre coupée à gauche");
                    Assert.That(wb.xMax, Is.LessThanOrEqualTo(panel.xMax + 0.5f), tag + " : fenêtre coupée à droite");
                    foreach (var c in Controls(window))
                    {
                        if (c.resolvedStyle.display == DisplayStyle.None) continue;
                        var b = c.worldBound;
                        Assert.That(b.yMin >= wb.yMin - 0.5f && b.yMax <= wb.yMax + 0.5f && b.xMin >= wb.xMin - 0.5f && b.xMax <= wb.xMax + 0.5f, Is.True, $"{tag} : « {c.name} » sort de la fenêtre ({b} dans {wb})");
                        Assert.That(b.height, Is.GreaterThanOrEqualTo(56f), $"{tag} : « {c.name} » sous 56 px de référence pour une manette ({b.height} px)");
                    }
                    Save(rt, $"account-{mode}-{name}");
                    Object.Destroy(go); rt.Release();
                    yield return null;
                }
        }

        [UnityTest, Timeout(120000), Description("Vérifie que le focus de la manette part du champ e-mail, parcourt champs et boutons sans jamais sortir de la fenêtre, et que Retour ferme la fenêtre — parce que le focus ne doit pas se perdre (spec §3)")]
        public IEnumerator Account_Gamepad_FocusIsTrappedAndBackCloses()
        {
            var (view, rt, go) = Spawn(1920, 1080, AccountMode.SignUp);
            for (int i = 0; i < 12; i++) yield return null;
            yield return new WaitForSeconds(0.2f);
            int closed = 0; view.CloseClicked += () => closed++;
            var fc = view.Root.panel.focusController;
            var window = view.Root.Q<VisualElement>("window");
            Assert.That(fc.focusedElement, Is.Not.Null, "un élément a le focus à l'ouverture");
            var seen = new HashSet<string>();
            for (int step = 0; step < 14; step++)
            {
                var fe = (VisualElement)fc.focusedElement!;
                var named = fe.name; var owner = fe.GetFirstAncestorOfType<TextField>();
                if (string.IsNullOrEmpty(named) && owner != null) named = owner.name;
                seen.Add(named);
                Assert.That(window.Contains(fe), Is.True, $"le focus reste dans la fenêtre (pas {step}, « {named} »)");
                using (var e = NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Down)) { e.target = fe; fe.SendEvent(e); }
                yield return null;
            }
            foreach (var expected in new[] { "email", "password", "name", "submit" })
                Assert.That(seen, Does.Contain(expected), "le focus atteint « " + expected + " » : " + string.Join(", ", seen));
            using (var cancel = NavigationCancelEvent.GetPooled()) { var t = (VisualElement)fc.focusedElement!; cancel.target = t; t.SendEvent(cancel); }
            yield return null;
            Assert.That(closed, Is.EqualTo(1), "Retour (B) ferme la fenêtre");
            Object.Destroy(go); rt.Release();
        }
    }
}
