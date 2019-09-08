using System.Text;
using System.Threading.Tasks;
using iText;
using iText.Layout;
using iText.Kernel;
using iText.Kernel.Font;

using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Layer;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Font.Constants;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Xobject;
using iText.IO.Source;

using System.IO;
using System;
using System.Linq;

namespace PDFUpdater
{
    class itext7PDFHdrWatermarkAnnotMS
    {
        public static string CompanyName= "ACME77";
        public static string AnnotName = CompanyName + "WatermarkHDR";

        public byte[] Add(string infile, string outFile)
        {
            PdfFont baseFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD, Encoding.ASCII.EncodingName, false);
            Rectangle pageSize;
            string NoOfPagesPadded = string.Empty;
            string PageNoPadded = string.Empty;
            string HdrLeft = string.Empty;
            string HdrRight = string.Empty;

            int xLeft = 30;
            int xRight = 110;  //was 100
            int xTop = 30;

            float watermarkTrimmingRectangleWidth;  //= 300;
            float watermarkTrimmingRectangleHeight = 300; 

            float formXOffset = 0;  
            float formYOffset = -5;  // was zero  set to -5 because dangling letters (e.g. g) were being cut-off
            byte[] outByteFile;
            byte[] inByteFile = null;

            try
            {
                inByteFile = File.ReadAllBytes(infile);
                IRandomAccessSource inSourceFile = new RandomAccessSourceFactory().CreateSource(inByteFile);
                using (PdfReader reader = new PdfReader(inSourceFile, new ReaderProperties()).SetUnethicalReading(true))
                {
                    using (var outMemoryFile = new MemoryStream())
                    {
                        using (PdfWriter pdfWrite = new PdfWriter(outMemoryFile))
                        {
                            using (PdfDocument pdfDoc = new PdfDocument(reader, pdfWrite))
                            {
                                using (Document doc = new Document(pdfDoc))
                                {
                                    int n = pdfDoc.GetNumberOfPages();
                                    for (var i = 1; i <= n; i++)
                                    {
                                        // Remove annotation if it exists 
                                        PdfDictionary pageDict = pdfDoc.GetPage(i).GetPdfObject();
                                        PdfArray annots = pageDict.GetAsArray(PdfName.Annots);
                                        if (annots != null)
                                        {
                                            for (int j = 0; j < annots.Size(); j++)
                                            {
                                                PdfDictionary annotation = annots.GetAsDictionary(j);
                                                if (PdfName.Watermark.Equals(annotation.GetAsName(PdfName.Subtype)))
                                                {
                                                    string NMvalue = annotation.GetAsString(PdfName.NM).GetValue();
                                                    if (NMvalue == AnnotName)
                                                    {
                                                        annotation.Clear();
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    for (int i = 1; i <= n; i++)
                                    {
                                        PdfPage page = pdfDoc.GetPage(i);
                                        pageSize = page.GetMediaBox();

                                        watermarkTrimmingRectangleWidth = pageSize.GetWidth();
                                        float PageWidth = pageSize.GetWidth();
                                        float PageHeight = pageSize.GetHeight();
                                        int rotation = page.GetRotation();
                                        Rectangle watermarkTrimmingRectangle = new Rectangle(pageSize.GetLeft() + xLeft, pageSize.GetTop() - xTop, watermarkTrimmingRectangleWidth, watermarkTrimmingRectangleHeight);

                                        PdfWatermarkAnnotation watermark = new PdfWatermarkAnnotation(watermarkTrimmingRectangle);

                                        watermark.SetName(new PdfString(AnnotName));
                                        Rectangle formRectangle = new Rectangle(formXOffset, formYOffset, watermarkTrimmingRectangleWidth, watermarkTrimmingRectangleHeight);

                                        PdfFormXObject form = new PdfFormXObject(formRectangle);   //Observation: font XObject will be resized to fit inside the watermark rectangle.  If it is larger, your text will be stretched.
                                        PdfCanvas canvasOver = new PdfCanvas(form, pdfDoc);

                                        canvasOver.SetFillColor(iText.Kernel.Colors.ColorConstants.RED);
                                        canvasOver.SetFontAndSize(baseFont, 10);
                                        canvasOver.SaveState();

                                        string SDSNo = "z2345678";
                                        HdrLeft = CompanyName + $" SDS# {SDSNo}";
                                      //  HdrLeft = $"PWidth: {PageWidth.ToString()} rotation: {rotation.ToString()}";  //qqtempqq
                                        NoOfPagesPadded = (n.ToString());
                                        PageNoPadded = i.ToString();
                                        HdrRight = $"Page {PageNoPadded} of {NoOfPagesPadded}";
                                      //  HdrRight = $"PHeight: {PageHeight.ToString()}";  //qqtempqq
                                        canvasOver.BeginText()
                                                  .SetColor(iText.Kernel.Colors.ColorConstants.RED, true)
                                                  .SetFontAndSize(baseFont, 10)
                                                  .ShowText(HdrLeft)
                                                  .EndText();
                                        canvasOver.BeginText()
                                                  .MoveText(formRectangle.GetRight() - xRight, 0)
                                                  .SetColor(iText.Kernel.Colors.ColorConstants.RED, true)
                                                  .SetFontAndSize(baseFont, 10)
                                                  .ShowText(HdrRight)                                                  
                                                  .EndText();
                                        canvasOver.SaveState();

                                        canvasOver.RestoreState();
                                        canvasOver.Release();

                                        watermark.SetAppearance(PdfName.N, new PdfAnnotationAppearance(form.GetPdfObject()));
                                        watermark.SetFlags(PdfAnnotation.PRINT);

                                        page.AddAnnotation(watermark);
                                    }
                                }
                            }
                            outByteFile = outMemoryFile.ToArray();
                        }
                    }
                }
                File.WriteAllBytes(outFile, outByteFile);

            }    
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.ToString());
                return inByteFile;
                throw;
            }
            return outByteFile;
        } 

        public void RemovetWatermarkPDF(string sourceFile, string destinationPath)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(sourceFile), new PdfWriter(destinationPath));
            var numberOfPages = pdfDoc.GetNumberOfPages();

            for (var i = 1; i <= numberOfPages; i++)
            {
                // PdfAnnotation 
                PdfDictionary pageDict = pdfDoc.GetPage(i).GetPdfObject();
                PdfArray annots = pageDict.GetAsArray(PdfName.Annots);
                for (int j = 0; j < annots.Size(); j++)
                {
                    PdfDictionary annotation = annots.GetAsDictionary(j);
                    if (PdfName.Watermark.Equals(annotation.GetAsName(PdfName.Subtype)))
                    {
                        string x = annotation.GetAsString(PdfName.NM).GetValue();
                        if (x == AnnotName)
                        {
                            annotation.Clear();
                        }
                    }
                }
            }
            pdfDoc.Close();
        }
  
    }
}

