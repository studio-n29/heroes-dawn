#nullable enable
using System;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Monnaie reconnue dans un libellé de prix (« 120 PR », « RECRUTER · 450 PR »).</summary>
    public enum PriceCurrency { None, Credits, Pr, Gems }

    /// <summary>
    /// Contenu composé d'un bouton de la boutique et des réglages : icône, libellé, puis icône de monnaie et montant quand le texte
    /// se termine par un prix. Le texte du bouton reste en place (les parcours de test le lisent) mais n'est plus dessiné : les
    /// éléments enfants le remplacent à l'écran et donnent sa largeur au bouton.
    /// </summary>
    public static class HdButtonContent
    {
        const string RootClass = "hd-bc", IconClass = "hd-bc__icon", HeadClass = "hd-bc__head", CoinClass = "hd-bc__coin", AmountClass = "hd-bc__amount";
        const string Separator = " · ";

        /// <summary>
        /// Découpe un texte autour de son premier segment de prix (« 450 PR ») : ce qui précède, le montant sans le code de monnaie,
        /// ce qui suit. Sans prix reconnu : <see cref="PriceCurrency.None"/> et le texte entier dans <paramref name="head"/>.
        /// </summary>
        public static PriceCurrency Split(string? text, out string head, out string amount, out string tail)
        {
            head = text ?? ""; amount = ""; tail = "";
            if (string.IsNullOrEmpty(text)) return PriceCurrency.None;
            var parts = text!.Split(new[] { Separator }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
            {
                var currency = Code(parts[i].Trim(), out string value);
                if (currency == PriceCurrency.None) continue;
                head = string.Join(Separator, parts, 0, i).Trim();
                amount = value;
                tail = string.Join(Separator, parts, i + 1, parts.Length - i - 1).Trim();
                return currency;
            }
            return PriceCurrency.None;
        }

        /// <summary>« 450 PR » donne Pr et « 450 » ; le montant doit finir par un chiffre (« Coût : 20 Gemmes » n'est pas un prix à icône).</summary>
        static PriceCurrency Code(string segment, out string value)
        {
            value = "";
            if (segment.Length < 4 || segment[segment.Length - 3] != ' ') return PriceCurrency.None;
            string code = segment.Substring(segment.Length - 2);
            var currency = code == "PR" ? PriceCurrency.Pr : code == "CR" ? PriceCurrency.Credits : code == "GM" ? PriceCurrency.Gems : PriceCurrency.None;
            if (currency == PriceCurrency.None) return PriceCurrency.None;
            string body = segment.Substring(0, segment.Length - 3).TrimEnd();
            if (body.Length == 0 || !char.IsDigit(body[body.Length - 1])) return PriceCurrency.None;
            value = body;
            return currency;
        }

        static T Part<T>(VisualElement host, string cls) where T : VisualElement, new()
        {
            var e = new T { pickingMode = PickingMode.Ignore };
            e.AddToClassList(cls);
            host.Add(e);
            return e;
        }

        static void Ensure(Button b)
        {
            if (b.ClassListContains(RootClass)) return;
            b.AddToClassList(RootClass);
            // Le contenu composé porte sa propre icône : les ornements automatiques du bouton principal (HdButtonDeco) sont refusés.
            b.AddToClassList("hd-btn--plain");
            Part<VisualElement>(b, IconClass);
            Part<Label>(b, HeadClass);
            // Pièce commune du thème (disque à la couleur de la monnaie, glyphe sombre).
            var coin = Part<VisualElement>(b, CoinClass);
            coin.AddToClassList("hd-coin"); coin.AddToClassList("hd-coin--sm");
            Part<Label>(b, AmountClass);
        }

        static void Show(VisualElement e, bool shown) => e.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;

        static void SetCoin(VisualElement e, PriceCurrency currency, string prefix)
        {
            e.EnableInClassList(prefix + "credits", currency == PriceCurrency.Credits);
            e.EnableInClassList(prefix + "pr", currency == PriceCurrency.Pr);
            e.EnableInClassList(prefix + "gems", currency == PriceCurrency.Gems);
        }

        static void SetIcon(Button b, string? icon)
        {
            var e = b.Q<VisualElement>(className: IconClass);
            if (e == null) return;
            if (e.userData is string old && old != icon) e.RemoveFromClassList(old);
            e.userData = icon;
            if (!string.IsNullOrEmpty(icon)) e.AddToClassList(icon!);
            Show(e, !string.IsNullOrEmpty(icon));
        }

        /// <summary>Libellé simple, précédé d'une icône (classe USS « hd-ico--… ») quand <paramref name="icon"/> est donné.</summary>
        public static void Fill(Button b, string? text, string? icon = null)
        {
            Ensure(b);
            b.text = text ?? "";
            SetIcon(b, icon);
            var head = b.Q<Label>(className: HeadClass);
            head.text = text ?? "";
            Show(head, !string.IsNullOrEmpty(text));
            Show(b.Q<VisualElement>(className: CoinClass), false);
            Show(b.Q<Label>(className: AmountClass), false);
        }

        /// <summary>
        /// Libellé dont le prix (« … · 450 PR ») est rendu par l'icône de la monnaie suivie du montant.
        /// <paramref name="amountOnly"/> : seul le prix est dessiné (pastille de prix d'une carte).
        /// </summary>
        public static void FillPrice(Button b, string? text, string? icon = null, bool amountOnly = false)
        {
            var currency = Split(text, out string head, out string amount, out string tail);
            if (currency == PriceCurrency.None) { Fill(b, text, icon); return; }
            Ensure(b);
            b.text = text ?? "";
            SetIcon(b, icon);
            var headLabel = b.Q<Label>(className: HeadClass);
            headLabel.text = head;
            Show(headLabel, head.Length > 0 && !amountOnly);
            var coin = b.Q<VisualElement>(className: CoinClass);
            SetCoin(coin, currency, "hd-coin--");
            Show(coin, true);
            var amountLabel = b.Q<Label>(className: AmountClass);
            amountLabel.text = tail.Length > 0 ? amount + Separator + tail : amount;
            Show(amountLabel, true);
        }

        /// <summary>
        /// Texte de prix d'un libellé (« 120 PR », « 20 GM · ×2 ») : le code de monnaie laisse la place à l'icône posée à gauche du texte
        /// par la classe « hd-price ». Un texte sans prix en tête est affiché tel quel.
        /// </summary>
        public static void Price(Label label, string? text)
        {
            var currency = Split(text, out string head, out string amount, out string tail);
            if (head.Length > 0) currency = PriceCurrency.None;
            label.text = currency == PriceCurrency.None ? text ?? "" : tail.Length > 0 ? amount + Separator + tail : amount;
            label.EnableInClassList("hd-price", currency != PriceCurrency.None);
            SetCoin(label, currency, "hd-price--");
        }
    }
}
