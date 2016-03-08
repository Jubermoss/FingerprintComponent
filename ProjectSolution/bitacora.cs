using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace FingerColaborator
{
    internal class Bitacora
   {
    static string strArchivo = "K:\\LOGS\\Temp\\" + Environment.MachineName.ToUpper() + "_FingerReader.log";
    internal  void regBitacora(string strTexto)
    {
        StreamWriter oWriter = default(StreamWriter);
        
        try
        {
            VerificaTamaño();
                        oWriter = File.AppendText(strArchivo);
            oWriter.WriteLine("{0} | {1}", DateTime.Now.ToString(), strTexto);
            oWriter.Close();

        }
        catch
        {
        }
    }

    private static void VerificaTamaño()
    {
        FileInfo archivo = default(FileInfo);

        try
        {
            if (File.Exists(strArchivo))
            {
                archivo = new FileInfo(strArchivo);
                //¿File > 100 KB?
                if ((archivo.Length / 1024) > 100)
                {
                    File.Delete(strArchivo);
                }

                archivo = null;
            }

        }
        catch 
        {
        }
    }

}
}







