using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;



namespace FingerColaborator
{
    internal partial class frmMatchFingerPrint : Form
    {


        #region Variables Globales
        private DpSdkOps.FPGetTemplateClass verifyTemple = new DpSdkOps.FPGetTemplateClass();
        private DpSdkEng.AISecureModeMask secureMode = new DpSdkEng.AISecureModeMask();
        private object _FingerPrintTemplate;
        private int intentos = 0;
        private Bitacora log;
    

        #endregion


        #region Propiedades

        private bool resultadoVerificacion;

        private int contador = 0;
               
        private int coincidencias=0;

        public int Coincidencias
        {
            get
            {
                return coincidencias;
            }
            set
            {
                coincidencias = value;
            }
        }

        private HuellaDigital[] coleccionHuellas;

        public HuellaDigital[] ColeccionHuellas
        {
            get
            {
                return coleccionHuellas;
            }
            set
            {
                coleccionHuellas = value;
            }
        }
        
        #endregion

        #region Metodos del Form

        public frmMatchFingerPrint()
        {
            InitializeComponent();
        }
        
        private void frmMatchFingerPrint_Load(object sender, EventArgs e)
        {
            log = new Bitacora(); 
            progressBar1.Visible = false;
            ComparaHuellas();
        }

        #endregion

        #region Metodos para la verificacion de huellas

        private void ComparaHuellas()
        {
            try
            {
                log.regBitacora("frmMatchFingerPrint.ComparaHuellas:        Asignando Events Handlers");
                verifyTemple.SampleReady += new DpSdkOps._IFPGetTemplateEvents_SampleReadyEventHandler(SampleReady);
                verifyTemple.Done += new DpSdkOps._IFPGetTemplateEvents_DoneEventHandler(RegDone);
                verifyTemple.Run(0);
            }
            catch (Exception ex)
            {
                log.regBitacora("frmMatchFingerPrint.ComparaHuellas:        Message: " + ex.Message.ToString());
            }

        }

        private void SampleReady(object obj)
        {
            try
            {
                log.regBitacora("frmMatchFingerPrint.SampleReady:        Entra al SampleReady");
                //aqui se hace la tarea de leer la huella en el sensor
                DpSdkEng.FPSample pic = (DpSdkEng.FPSample)obj;
                pic.PictureOrientation = DpSdkEng.AIOrientation.Or_Portrait;
                pic.PictureWidth = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsX(pictureBox1.Width) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelX();
                pic.PictureHeight = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsY(pictureBox1.Height) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelY();
                pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox1.Refresh();
            }
            catch (Exception ex)
            {
                log.regBitacora("frmMatchFingerPrint.SampleReady:        Message: " + ex.Message.ToString());
            }
        }

        private void RegDone(object vt)
        {
            try
            {
                log.regBitacora("frmMatchFingerPrint.RegDone:        Entra al RegDone");
                int numHuellas = coleccionHuellas.Length;
                int incrementoProgresBar = 100 / numHuellas;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                labelMuestra.Text = "Comparando...";
                labelMuestra.Refresh();
                while (contador < numHuellas)
                {
                    // Pasa la cadena a Byte[] 
                    _FingerPrintTemplate = StringToByte(ColeccionHuellas[contador].CadenaHuellaHexadecimal);
                    bool verifyOK = false;
                    object score = 0;
                    object theshold = 0;
                    bool tture = true;
                    DpSdkEng.FPTemplate sensorTemplate = (DpSdkEng.FPTemplate)vt;
                    //armar el RegTemplate a partir de la cadena de strings
                    DpSdkEng.FPTemplateClass RegTemplate = new DpSdkEng.FPTemplateClass();
                    RegTemplate.Import(_FingerPrintTemplate);
                    DpSdkEng.FPVerifyClass verify = new DpSdkEng.FPVerifyClass();
                    verify.Compare(RegTemplate, sensorTemplate, ref verifyOK, ref score, ref theshold, ref tture, ref secureMode);
                    if (verifyOK == true)
                    {
                        Coincidencias++;
                    }
                    labelMuestra.Text = "Concidencias: " + Coincidencias.ToString();
                    labelMuestra.Refresh();
                    contador++;
                    progressBar1.Value += incrementoProgresBar;
                }

                Thread.Sleep(1500);
                this.Close();
            }
            catch (Exception ex)
            {
                log.regBitacora("frmMatchFingerPrint.RegDone:        Message: " + ex.Message.ToString());
            }
        }
        
        #endregion


        #region Metodos Generales

        /// <summary>
        /// Pasa la Huella capturada del Sensor a Hexadecimal 
        /// </summary>
        /// <param name="obj">Huella obtenida del Sensor</param>
        /// <returns>Cadena de la huella en Hexadecimal</returns>
        private string TemplateToHexadecimal(object objHuella)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder(1100);
            byte[] huellita = (byte[])objHuella;
            foreach (var aux in huellita)
            {
                string Data = string.Format("{0:X}", aux);
                if (Data.Length == 1)
                {
                    Data = "0" + Data;
                }
                s.Append(Data);
            }
            return s.ToString();
        }


        /// <summary>
        /// Pasa a Bytes[] una huella
        /// </summary>
        /// <param name="data">Huella en formato Hexadecimal</param>
        /// <returns>Huella en Bytes[]</returns>
        private byte[] StringToByte(string hexDataFinger)
        {
            try
            {
                int Counter;
                Counter = hexDataFinger.Length / 2;
                byte[] Finger = new byte[Counter];
                for (int x = 0; x < Counter; x++)
                {
                    string Part = hexDataFinger.Substring(x * 2, 2);
                    int Number = Int32.Parse(Part, System.Globalization.NumberStyles.AllowHexSpecifier);
                    Finger[x] = (byte)Number;
                }
                return Finger;
            }
            catch (Exception hdEx)
            {
                throw (hdEx);
            }

        }

        private bool StatusSensor()
        {
            bool sensorConectado = true;
            try
            {
                DpSdkEng.FPDevicesClass FPDevice = new DpSdkEng.FPDevicesClass();
                if (FPDevice.Count == 0)
                {
                    sensorConectado = false;
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmMatchFingerPrint.StatusSensor:        Message: " + ex.Message.ToString());
            }

           return sensorConectado;
        }


   



        #endregion


    }
}
