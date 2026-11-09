using System;
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Sources vectorielles du kit Aube tactique, rasterisées par UiAssetBaker.</summary>
    public static class TacticalGlyphs
    {
        public static Sprite Create(string name)
        {
            const int size = 96;
            var lines = new List<Vector4>();
            void Line(float x, float y, float u, float v) => lines.Add(new Vector4(x, y, u, v));
            void Box(float x, float y, float w, float h)
            { Line(x,y,x+w,y); Line(x+w,y,x+w,y+h); Line(x+w,y+h,x,y+h); Line(x,y+h,x,y); }
            void Circle(float x, float y, float r)
            {
                for (int i=0;i<32;i++)
                { float a=i*Mathf.PI/16, b=(i+1)*Mathf.PI/16; Line(x+r*Mathf.Cos(a),y+r*Mathf.Sin(a),x+r*Mathf.Cos(b),y+r*Mathf.Sin(b)); }
            }
            switch (name)
            {
                case "DawnStar": break;
                case "GlyphTeam":
                    Circle(32,60,9); Circle(64,60,9); Line(15,25,15,39); Line(15,39,24,46); Line(24,46,40,46); Line(40,46,48,39);
                    Line(48,39,56,46); Line(56,46,72,46); Line(72,46,81,39); Line(81,39,81,25); Line(15,25,81,25); break;
                case "GlyphPass":
                    Box(19,22,58,52); Line(19,49,77,49); Line(38,22,38,74); Line(58,22,58,74); break;
                case "GlyphShop":
                    Box(22,21,52,46); Line(22,67,74,67); Line(30,67,36,78); Line(36,78,60,78); Line(60,78,66,67); Line(40,34,56,34); break;
                case "GlyphProfile":
                    Circle(48,62,13); Line(22,20,22,31); Line(22,31,34,43); Line(34,43,62,43); Line(62,43,74,31); Line(74,31,74,20); Line(22,20,74,20); break;
                case "GlyphWorkshop":
                    Line(22,22,65,65); Circle(25,25,7); Line(60,60,58,72); Line(58,72,69,81); Line(69,81,72,69); Line(72,69,81,66); Line(81,66,72,56); Line(72,56,60,60); break;
                case "GlyphOperations":
                    Circle(48,48,24); Line(48,14,48,37); Line(48,59,48,82); Line(14,48,37,48); Line(59,48,82,48); Circle(48,48,5); break;
                case "GlyphGrimoire":
                    Line(48,22,48,71); Line(48,71,24,79); Line(24,79,14,74); Line(14,74,14,27); Line(14,27,25,31); Line(25,31,48,22);
                    Line(48,71,72,79); Line(72,79,82,74); Line(82,74,82,27); Line(82,27,71,31); Line(71,31,48,22); break;
                case "GlyphInventory":
                    Line(18,29,48,13); Line(48,13,78,29); Line(78,29,78,66); Line(78,66,48,83); Line(48,83,18,66); Line(18,66,18,29);
                    Line(18,66,48,49); Line(48,49,78,66); Line(48,49,48,13); break;
                case "GlyphRegister":
                    Box(23,16,53,66); Line(15,30,30,30); Line(15,48,30,48); Line(15,66,30,66); Circle(51,57,9); Line(37,32,41,40); Line(41,40,61,40); Line(61,40,65,32); break;
                case "GlyphQuest":
                    Box(23,15,50,64); Box(36,72,24,10); Line(33,49,43,39); Line(43,39,62,60); Line(35,28,60,28); break;
                default: throw new ArgumentException("Sprite d'interface inconnu : " + name);
            }
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false)
            { wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear,hideFlags=HideFlags.DontSave };
            var pixels = new Color32[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                var p=new Vector2(x+0.5f,y+0.5f);
                float alpha=0;
                if(name=="DawnStar")
                {
                    // Quadrilatères concaves : quatre pointes, jamais l'étoile d'impact des combats.
                    float dx=Mathf.Abs(p.x-48),dy=Mathf.Abs(p.y-48);
                    float edge=dx>=dy ? 42-dx-3*dy : 42-dy-3*dx;
                    alpha=Mathf.Clamp01(edge+0.5f);
                }
                else foreach(var l in lines)
                {
                    var a=new Vector2(l.x,l.y);var b=new Vector2(l.z,l.w);var d=b-a;
                    float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
                    alpha=Mathf.Max(alpha,Mathf.Clamp01(2.1f-Vector2.Distance(p,a+t*d)));
                }
                pixels[y*size+x]=new Color32(255,255,255,(byte)(alpha*255));
            }
            texture.SetPixels32(pixels);texture.Apply();
            return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f);
        }
    }
}
