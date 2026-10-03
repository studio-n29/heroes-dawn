#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class NoCodeBuiltScreensTests
    {
        static readonly HashSet<string> Factory = new HashSet<string> { "Txt", "Btn", "Img", "Panel", "Rect" };
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        [Test, Description("Vérifie qu'aucun type d'exécution de la présentation n'appelle Ui.Txt/Btn/Img/Panel/Rect — parce que les écrans viennent désormais de prefabs générés, retouchables à la main ; une interface construite par code échapperait à ces retouches (spec §4)")]
        public void RuntimeAssembliesDontCallUiFactory()
        {
            var ui = typeof(Ui);
            var offenders = new SortedSet<string>();
            foreach (var t in ui.Assembly.GetTypes())
            {
                if (t == ui || t.DeclaringType == ui) continue;
                var methods = t.GetMethods(All).Cast<MethodBase>().Concat(t.GetConstructors(All));
                foreach (var m in methods)
                    if (CallsFactory(m, ui)) offenders.Add(t.FullName + "." + m.Name);
            }
            Assert.That(offenders, Is.Empty, "Interface construite par code :\n" + string.Join("\n", offenders));
        }

        static bool CallsFactory(MethodBase m, Type ui)
        {
            byte[]? il;
            try { il = m.GetMethodBody()?.GetILAsByteArray(); }
            catch (Exception) { return false; }
            if (il == null) return false;
            var typeArgs = m.DeclaringType!.IsGenericType ? m.DeclaringType.GetGenericArguments() : null;
            var methodArgs = m.IsGenericMethod ? m.GetGenericArguments() : null;
            for (int i = 0; i + 4 < il.Length; i++)
            {
                if (il[i] != 0x28 && il[i] != 0x6F) continue; // call, callvirt
                int token = BitConverter.ToInt32(il, i + 1);
                if ((token >> 24) != 0x0A && (token >> 24) != 0x06 && (token >> 24) != 0x2B) continue; // MemberRef, MethodDef, MethodSpec
                MethodBase? callee;
                try { callee = m.Module.ResolveMethod(token, typeArgs, methodArgs); }
                catch (Exception) { continue; }
                if (callee != null && callee.DeclaringType == ui && Factory.Contains(callee.Name)) return true;
            }
            return false;
        }
    }
}
