namespace CIMSystemGUI.Helpers

open System
open System.IO
open Avalonia.Media.Imaging
open OpenCvSharp

module CameraHelper =
    
    let matToBitmap (mat: Mat) =
        try
            let bytes = mat.ImEncode(".png")
            use stream = new MemoryStream(bytes)
            new Bitmap(stream)
        with _ -> null