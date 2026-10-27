#nullable enable
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Fenêtre Compte : onglets, e-mail, mot de passe, pseudo, Valider, Mot de passe oublié, Google, Apple, Fermer, attente.</summary>
    public sealed class AccountPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/AccountPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("AccountPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1100, 900, Theme.Primary);

            var tabIn = Ui.Btn(content, "", Theme.Surface2, Theme.Text, null!, 34, FontKind.Title, "TabSignIn");
            tabIn.Rect.At(0f, 1f, 0, 0, 500, 96);
            var tabUp = Ui.Btn(content, "", Theme.Surface2, Theme.Text, null!, 34, FontKind.Title, "TabSignUp");
            tabUp.Rect.At(1f, 1f, 0, 0, 500, 96);

            var (email, emailHint) = InputBox(content, "Email", -106, false);
            var (password, passwordHint) = InputBox(content, "Password", -206, true);
            var (name, nameHint) = InputBox(content, "Name", -306, false);
            var nameRow = name.gameObject;

            var message = Ui.Txt(content, "", 30, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Body, "Message");
            message.rectTransform.At(0.5f, 1f, 0, -402, 1000, 52);

            var submit = Ui.Btn(content, "", Theme.Success, Theme.TextDark, null!, 38, FontKind.Title, "Submit");
            submit.Rect.At(0f, 1f, 0, -460, 600, 96);
            var forgot = Ui.Btn(content, "", Theme.Surface3, Theme.Text, null!, 28, FontKind.Body, "Forgot");
            forgot.Rect.At(1f, 1f, 0, -460, 400, 96);

            var google = Ui.Btn(content, "", Theme.Surface2, Theme.Text, null!, 32, FontKind.Title, "Google");
            google.Rect.At(0f, 1f, 0, -566, 500, 96);
            var apple = Ui.Btn(content, "", Theme.Surface2, Theme.Text, null!, 32, FontKind.Title, "Apple");
            apple.Rect.At(1f, 1f, 0, -566, 500, 96);

            var buttons = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            buttons.HLayout(40, 0, TextAnchor.MiddleCenter);
            var close = Ui.Btn(buttons, "", Theme.Surface3, Theme.Text, null!, 34, FontKind.Title, "Close");
            close.Rect.Pref(340, 96);

            // Attente : voile sur toute la fenêtre pendant la requête (bloque les clics).
            var busy = Ui.Img(content, "Busy", Theme.Overlay, null, true);
            busy.rectTransform.Stretch();
            var busyText = Ui.Txt(busy.transform, "", 40, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "BusyText");
            busyText.rectTransform.Stretch(40, 40, 40, 40);
            busyText.Inked();
            busy.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<AccountPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "tabSignIn", tabIn);
            ctx.Wire(view, "tabSignUp", tabUp);
            ctx.Wire(view, "emailField", email);
            ctx.Wire(view, "passwordField", password);
            ctx.Wire(view, "nameField", name);
            ctx.Wire(view, "emailHint", emailHint);
            ctx.Wire(view, "passwordHint", passwordHint);
            ctx.Wire(view, "nameHint", nameHint);
            ctx.Wire(view, "nameRow", nameRow);
            ctx.Wire(view, "message", message);
            ctx.Wire(view, "submit", submit);
            ctx.Wire(view, "google", google);
            ctx.Wire(view, "apple", apple);
            ctx.Wire(view, "forgot", forgot);
            ctx.Wire(view, "close", close);
            ctx.Wire(view, "busyOverlay", busy.gameObject);
            ctx.Wire(view, "busyText", busyText);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }

        /// <summary>Champ de saisie sur une ligne (modèle : FriendAddRecipes) ; un mot de passe est masqué.</summary>
        static (TMP_InputField Field, TMP_Text Hint) InputBox(Transform parent, string name, float y, bool password)
        {
            var box = Ui.Img(parent, name, Theme.Surface3, Sprites.Panel, true);
            box.rectTransform.At(0f, 1f, 0, y, 1028, 90);
            var viewport = Ui.Rect(box.transform, "Viewport").Stretch(20, 8, 20, 8);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Ui.Txt(viewport, "", 36, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Text");
            text.rectTransform.Stretch();
            text.raycastTarget = true;
            var hint = Ui.Txt(viewport, "", 36, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Hint");
            hint.rectTransform.Stretch();
            var field = box.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = hint;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.characterLimit = 64;
            if (password) field.contentType = TMP_InputField.ContentType.Password;
            else if (name == "Email") field.contentType = TMP_InputField.ContentType.EmailAddress;
            return (field, hint);
        }
    }

    /// <summary>Choix de progression : deux cartes (appareil, compte) avec chiffres et bouton chacune, Plus tard.</summary>
    public sealed class ProgressChoicePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ProgressChoicePopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ProgressChoicePopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1400, 800, Theme.Primary);

            var hint = Ui.Txt(content, "", 30, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Hint");
            hint.rectTransform.At(0.5f, 1f, 0, 0, 1280, 90);

            var (deviceHeader, deviceSummary, keepDevice) = Card(content, "Device", 0f, Theme.Info);
            var (accountHeader, accountSummary, keepAccount) = Card(content, "Account", 1f, Theme.Success);

            var buttons = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            buttons.HLayout(40, 0, TextAnchor.MiddleCenter);
            var later = Ui.Btn(buttons, "", Theme.Surface3, Theme.Text, null!, 34, FontKind.Title, "Later");
            later.Rect.Pref(340, 96);

            var view = root.gameObject.AddComponent<ProgressChoicePopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "deviceHeader", deviceHeader);
            ctx.Wire(view, "deviceSummary", deviceSummary);
            ctx.Wire(view, "accountHeader", accountHeader);
            ctx.Wire(view, "accountSummary", accountSummary);
            ctx.Wire(view, "keepDevice", keepDevice);
            ctx.Wire(view, "keepAccount", keepAccount);
            ctx.Wire(view, "later", later);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }

        /// <summary>Une carte : intitulé, chiffres, bouton « Garder … » (ancrée à gauche ou à droite).</summary>
        static (TMP_Text Header, TMP_Text Summary, UiButton Keep) Card(Transform parent, string name, float side, Color accent)
        {
            var frame = Ui.Panel(parent, name, Theme.Surface3, 4f, 6f, false).Frame();
            frame.At(side, 1f, 0, -100, 640, 410);
            var header = Ui.Txt(frame, "", 44, accent, TextAnchor.MiddleCenter, FontKind.Title, "Header");
            header.rectTransform.At(0.5f, 1f, 0, -20, 600, 70);
            header.Inked();
            var summary = Ui.Txt(frame, "", 34, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Summary");
            summary.rectTransform.At(0.5f, 1f, 0, -100, 600, 150);
            var keep = Ui.Btn(frame, "", accent, Theme.TextDark, null!, 34, FontKind.Title, "Keep");
            keep.Rect.At(0.5f, 0f, 0, 24, 560, 110);
            return (header, summary, keep);
        }
    }
}
