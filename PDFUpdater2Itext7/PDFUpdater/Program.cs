using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText;
using iText.Kernel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Common.Logging;
//using iTextSharp;
//using iTextSharp.text;
//using iTextSharp.text.pdf;

using System.Diagnostics;

namespace PDFUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
           string folder = @"C:\PDFTest\";
           System.IO.Directory.CreateDirectory(folder);  // create folder if it does not exist.
            string infile = string.Empty;
            infile = "123865_20060811_USA_EN_CW.pdf";  // editable PDF
            string outFile = infile;
            outFile = folder + outFile.Replace(".pdf", "") + "_afterWatermarkAnnotationStep2.pdf";
             //  infile = folder + infile;

            itext7PDFHdrWatermarkAnnotMS objPDFWatermarkAnnotMS = new itext7PDFHdrWatermarkAnnotMS();
            byte[] byteFile = objPDFWatermarkAnnotMS.Add(infile, outFile);

            string infile2 = outFile;
            string outfile2 = infile2.Replace("_afterWatermarkAnnotationStep2.pdf", "") + "_removedWatermarkAnotationStep3.pdf";

            objPDFWatermarkAnnotMS.RemovetWatermarkPDF(infile2, outfile2);

        }


    }
}
