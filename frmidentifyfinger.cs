using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;


namespace FingerColaborator
{
    internal partial class frmIdentifyFinger : Form
    {
        
        #region Variables Globales

        private DpSdkOps.FPGetTemplateClass verifyTemple = new DpSdkOps.FPGetTemplateClass();
        private DpSdkEng.AISecureModeMask secureMode = new DpSdkEng.AISecureModeMask();
        private object _FingerPrintTemplate;
        private int intentos = 0;
        private Bitacora log = new Bitacora();     
 
        #endregion


        #region Propiedades
        
        private bool resultadoVerificacion;
        private int numDedoActual;
        private int numManoActual;
        private int contadorCaptura;
        private string cadenaHuellaHexadecimal;
        private bool huellaDuplicada;
        private HuellaDigital[] huellasAlmacenadasPreviemente;
        private int totalCapturas;

        public int NumDedoActual
        {
            get
            {
                return numDedoActual;
            }
            set
            {
                numDedoActual = value;
            }
        }
        public int NumManoActual
        {
            get
            {
                return numManoActual;
            }
            set
            {
                numManoActual = value;
            }
        }
        public bool ResultadoVerificacion
        {
            get
            {
                return resultadoVerificacion;
            }
            set
            {
                resultadoVerificacion = value;
            }
        }
        public string CadenaHuellaHexadecimal 
        {
            get
            {
                return cadenaHuellaHexadecimal;
            }
            set
            {
                cadenaHuellaHexadecimal = value;
                //Pasa la cadena a Byte[] 
                _FingerPrintTemplate = StringToByte(cadenaHuellaHexadecimal);
            }
        }
        public bool HuellaDuplicada
        {
            get
            {
                return huellaDuplicada;
            }
            set
            {
                huellaDuplicada = value;
            }
        }
        public HuellaDigital[] HuellasAlmacenadasPreviemente
        {
            get
            {
                return huellasAlmacenadasPreviemente;
            }
            set
            {
                huellasAlmacenadasPreviemente = value;
            }
        }
        public int ContadorCaptura
        {
            get
            {
                return contadorCaptura;
            }
            set
            {
                contadorCaptura = value;
            }
        }
        public int TotalCapturas
        {
            get
            {
                return totalCapturas;
            }
            set
            {
                totalCapturas = value;
            }
        }
        
        #endregion


        #region Metodos del Form
        
        public frmIdentifyFinger()
        {
            InitializeComponent();
        }

        private void frmIdentifyFinger_Load(object sender, EventArgs e)
        {
                log.regBitacora("frmIdentifyFinger.frmFingerReader_Load:        Comienza verificacion de la huella");
                DefineImagenDedo(numDedoActual, numManoActual);
                DefineLoader();
                verifyTemple.SampleReady += new DpSdkOps._IFPGetTemplateEvents_SampleReadyEventHandler(SampleReady);
                verifyTemple.Done += new DpSdkOps._IFPGetTemplateEvents_DoneEventHandler(RegDone);
                verifyTemple.DevDisconnected += new DpSdkOps._IFPGetTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
                IniciaVerificacion();
        }
        
        #endregion

        #region Metodos para la verificacion de huellas

        private void DevDisconnected()
        {
            verifyTemple.DevDisconnected -= new DpSdkOps._IFPGetTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
            MessageBox.Show("Por favor conecte el sensor de huellas", "Sensor no conectado");
        }


        private void IniciaVerificacion()
        {
            try
            {
                log.regBitacora("frmIdentifyFinger.IniciaVerificacion:        Asignando EventHandlers");               
                verifyTemple.Run(0);
            }
            catch(Exception ex)
            {
                log.regBitacora("frmIdentifyFinger.IniciaVerificacion:        Message: " + ex.Message.ToString());
            }
        }
      
        private void SampleReady(object obj)
        {
            try
            {
                log.regBitacora("frmIdentifyFinger.SampleReady:        Entra a SampleReady");
                DpSdkEng.FPSample pic = (DpSdkEng.FPSample)obj;
                pic.PictureOrientation = DpSdkEng.AIOrientation.Or_Portrait;
                pic.PictureWidth = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsX(pictureBox1.Width) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelX();
                pic.PictureHeight = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsY(pictureBox1.Height) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelY();
                pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox1.Refresh();
            }
            catch (Exception ex)
            {
                log.regBitacora("frmIdentifyFinger.SampleReady:        Message: " + ex.Message.ToString());
            }

        }

        private void RegDone(object vt)
        {
            try
            {
                log.regBitacora("frmIdentifyFinger.RegDone:        Entra al RegDone");
                bool verifyOK = false;
                object score = 0;
                object theshold = 0;
                bool tture = true;

                verifyTemple.DevDisconnected -= new DpSdkOps._IFPGetTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
                DpSdkEng.FPTemplate sensorTemplate = (DpSdkEng.FPTemplate)vt;

                //Armar el RegTemplate a partir de la cadena de strings
                DpSdkEng.FPTemplateClass RegTemplate = new DpSdkEng.FPTemplateClass();
                RegTemplate.Import(_FingerPrintTemplate);

                DpSdkEng.FPVerifyClass verify = new DpSdkEng.FPVerifyClass();
                verify.Compare(RegTemplate, sensorTemplate, ref verifyOK, ref score, ref theshold, ref tture, ref secureMode);

                labelVerificacion.Text = "";
                labelMuestra.Text="";
                if (verifyOK == true)
                {
                    labelVerificacion.Text = "SI Coincide";
                    labelVerificacion.ForeColor = Color.Green;
                    labelVerificacion.Refresh();
                    verifyTemple.Cancel();
                    ResultadoVerificacion = true;
                    VerificaHuellaDuplicada(vt);
                    this.Close();
                    return;
                }
                else
                {
                    intentos++;
                    if (intentos <= 2) //se permiten solo 3 intentos de verificacion
                    {
                        labelVerificacion.Text = "NO Coincide";
                        labelVerificacion.ForeColor = Color.Red;
                        labelVerificacion.Refresh();
                        labelMuestra.Text = "Intenta otra vez";
                        verifyTemple.Cancel();
                        IniciaVerificacion();
                    }
                    else
                    {
                        labelVerificacion.Text = "Recapturar";
                        labelVerificacion.ForeColor = Color.Red;
                        labelVerificacion.Refresh();
                        labelMuestra.Text = "Recaptura dedo";
                        verifyTemple.Cancel();
                        resultadoVerificacion = false;
                        this.Close();
                        return;
                    }
                }
            }
            catch (Exception ex) 
            {
                log.regBitacora("frmIdentifyFinger.RegDone:        Message: " + ex.Message.ToString());
            }

        }

        private void VerificaHuellaDuplicada(object vt)
        {
            try
            {
                log.regBitacora("frmIdentifyFinger.RegDone:        VerificaHuellaDuplicada");
                int numHuellasAlmacenadas = huellasAlmacenadasPreviemente.Length;
                //int incrementoProgresBar = 100 / numHuellas;
                int Coincidencias = 0;
                huellaDuplicada = false;
                int contador = 0;
                int aux = contadorCaptura;
                while (contador < numHuellasAlmacenadas)
                {
                    if (contador != contadorCaptura && huellasAlmacenadasPreviemente[contador] != null)
                    {
                        // carga el template de la huella almacenada anteriormente 
                        _FingerPrintTemplate = huellasAlmacenadasPreviemente[contador].TemplateHuella;
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
                            huellaDuplicada = true;
                        }
                    }
                    contador++;
                }
            }
            catch (Exception ex)  
            {
                log.regBitacora("frmIdentifyFinger.VerificaHuellaDuplicada:        Message: " + ex.Message.ToString());
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
            try
            {
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
            }
            catch (Exception ex) 
            {
                log.regBitacora("frmIdentifyFinger.TemplateToHexadecimal:        Message: " + ex.Message.ToString());
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
                log.regBitacora("frmIdentifyFinger.StringToByte:        Inicia conversion a bytes[]");
                int Counter;
                Counter = hexDataFinger.Length / 2;
                byte[] Finger = new byte[Counter];
                for (int x = 0; x < Counter; x++)
                {
                    string Part = hexDataFinger.Substring(x * 2, 2);
                    int Number = Int32.Parse(Part, System.Globalization.NumberStyles.AllowHexSpecifier);
                    Finger[x] = (byte)Number;
                }
                log.regBitacora("frmIdentifyFinger.StringToByte:        Fin conversion a bytes[]");
                return Finger;
            }
            catch (Exception hdEx)
            {
                throw (hdEx);
            }

        }

 
         private void DefineImagenDedo(int numDedoActual, int numManoActual)
        {
            try
            {     
                labelDedo.Text = "Mano: " + numManoActual + "   Dedo: " + numDedoActual;

                Assembly myAssembly = Assembly.GetExecutingAssembly();

                //Mano Izquierda
                if (numManoActual == 1)
                {
                    Stream myStreamIzq = myAssembly.GetManifestResourceStream("FingerColaborator.Resources.i" + numDedoActual + ".png");
                    Stream myStreamDer = myAssembly.GetManifestResourceStream("FingerColaborator.Resources.md.png");
                    Bitmap manoIzq = new Bitmap(myStreamIzq);
                    Bitmap manoDer = new Bitmap(myStreamDer);

                    pictureBoxIzq.Image = manoIzq;
                    pictureBoxIzq.Refresh();
                    pictureBoxDer.Image = manoDer;
                    pictureBoxDer.Refresh();
                }
                //Mano Derecha
                if (numManoActual == 2)
                {
                    Stream myStreamIzq = myAssembly.GetManifestResourceStream("FingerColaborator.Resources.mi.png");
                    Stream myStreamDer = myAssembly.GetManifestResourceStream("FingerColaborator.Resources.d" + numDedoActual + ".png");
                    Bitmap manoIzq = new Bitmap(myStreamIzq);
                    Bitmap manoDer = new Bitmap(myStreamDer);

                    pictureBoxIzq.Image = manoIzq;
                    pictureBoxIzq.Refresh();
                    pictureBoxDer.Image = manoDer;
                    pictureBoxDer.Refresh();
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmIdentifyFinger.DefineImagenDedo:        Message: " + ex.Message.ToString());
            }
        }

         private void DefineLoader()
         {
             try
             {
                 switch (contadorCaptura)
                 {

                     case 0:
                         pnlCapture1.BackColor = System.Drawing.Color.Yellow;
                         break;

                     case 1:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.Yellow;

                         break;

                     case 2:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 3:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 4:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture5.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 5:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture5.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture6.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 6:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture5.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture6.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture7.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 7:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture5.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture6.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture7.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture8.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 8:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture5.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture6.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture7.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture8.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture9.BackColor = System.Drawing.Color.Yellow;
                         break;
                     case 9:
                         pnlCapture1.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture2.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture3.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture4.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture5.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture6.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture7.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture8.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture9.BackColor = System.Drawing.Color.LawnGreen;
                         pnlCapture10.BackColor = System.Drawing.Color.Yellow;
                         break;

                     default:
                         break;
                 }

                 switch (totalCapturas)
                 {
                     case 1:
                         pnlCapture2.Visible = false;
                         pnlCapture3.Visible = false;
                         pnlCapture4.Visible = false;
                         pnlCapture5.Visible = false;
                         pnlCapture6.Visible = false;
                         pnlCapture7.Visible = false;
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 2:
                         pnlCapture3.Visible = false;
                         pnlCapture4.Visible = false;
                         pnlCapture5.Visible = false;
                         pnlCapture6.Visible = false;
                         pnlCapture7.Visible = false;
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 3:
                         pnlCapture4.Visible = false;
                         pnlCapture5.Visible = false;
                         pnlCapture6.Visible = false;
                         pnlCapture7.Visible = false;
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 4:
                         pnlCapture5.Visible = false;
                         pnlCapture6.Visible = false;
                         pnlCapture7.Visible = false;
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 5:
                         pnlCapture6.Visible = false;
                         pnlCapture7.Visible = false;
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 6:
                         pnlCapture7.Visible = false;
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 7:
                         pnlCapture8.Visible = false;
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 8:
                         pnlCapture9.Visible = false;
                         pnlCapture10.Visible = false;
                         break;
                     case 9:
                         pnlCapture10.Visible = false;
                         break;
                     default:
                         break;
                 }
             }
             catch (Exception ex)
             {
                 log.regBitacora("frmIdentifyFinger.DefineLoader:        Message: " + ex.Message.ToString());
             }


         }

        #endregion

        
    }
}
