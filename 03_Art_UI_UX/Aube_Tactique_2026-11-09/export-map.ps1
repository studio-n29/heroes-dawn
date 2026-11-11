$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension'
$source="$root/Scenes/map-new-york.png"

$dir="$root/Exports/Map3840x2160";[IO.Directory]::CreateDirectory($dir)|Out-Null
Add-Type -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
public static class BoroughAlpha {
 public static void Apply(Bitmap image,Bitmap mask) {
  var r=new Rectangle(0,0,image.Width,image.Height);
  var a=image.LockBits(r,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);
  var b=mask.LockBits(r,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
  try {var row=new byte[image.Width*4];var mr=new byte[image.Width*4];
   for(int y=0;y<image.Height;y++){var p=IntPtr.Add(a.Scan0,y*a.Stride);Marshal.Copy(p,row,0,row.Length);Marshal.Copy(IntPtr.Add(b.Scan0,y*b.Stride),mr,0,mr.Length);for(int x=3;x<row.Length;x+=4){row[x]=(byte)(row[x]*mr[x]/255);if(row[x]==0){row[x-3]=0;row[x-2]=0;row[x-1]=0;}}Marshal.Copy(row,0,p,row.Length);}
  }finally{image.UnlockBits(a);mask.UnlockBits(b);}
 }
}
'@ -ReferencedAssemblies @([Drawing.Bitmap].Assembly.Location,[Drawing.Rectangle].Assembly.Location,'System.Runtime.InteropServices')
if(!(Test-Path $source)){Write-Output 'Map source not generated yet.';return}
$sourceImage=[Drawing.Image]::FromFile($source)
$records=@()
try{foreach($borough in @('bronx','brooklyn','manhattan','queens','staten')){foreach($state in @('normal','selected')){
 $dest="$dir/$borough-$state.png";$maskPath="$root/MapMasks/$borough.png";$overlayPath="$root/MapOverlays/$borough-$state.png"
 $bm=[Drawing.Bitmap]::new(3840,2160,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $mask=[Drawing.Bitmap]::FromFile($maskPath);$overlay=[Drawing.Image]::FromFile($overlayPath);$g=[Drawing.Graphics]::FromImage($bm)
 try{$g.InterpolationMode='HighQualityBicubic';$g.DrawImage($sourceImage,0,0,3840,2160);$g.DrawImage($overlay,0,0,3840,2160);$g.Dispose();$g=$null;[BoroughAlpha]::Apply($bm,$mask);$bm.Save($dest,[Drawing.Imaging.ImageFormat]::Png)}finally{if($g){$g.Dispose()};$bm.Dispose();$mask.Dispose();$overlay.Dispose()}
 $records+=@{file=$dest;source=$source;mask=$maskPath;overlay=$overlayPath;width=3840;height=2160;upscaled=($sourceImage.Width -lt 3840);note='Exact CityMapLayout polygon mask; painted geography remains illustrative.'}
}}}finally{$sourceImage.Dispose()}
$records|ConvertTo-Json -Depth 5|Set-Content "$PSScriptRoot/manifest-map-exports.json"
Write-Output '10 borough layers exported.'


