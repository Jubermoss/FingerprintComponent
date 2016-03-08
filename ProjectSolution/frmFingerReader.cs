using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;



namespace FingerColaborator
{
    internal partial  class frmFingerReader : Form
    {
        #region Variables Globales

        private DpSdkOps.FPRegisterTemplateClass RegisterTemplate = new DpSdkOps.FPRegisterTemplateClass();
        private object pRegTmplate = 0; 
        private int count = 0;
        private Bitacora log = new Bitacora();
        private bool cancelaCaptura;
            
        #endregion

        #region Propiedades

        public bool CancelaCaptura
        {
            get
            {
                return cancelaCaptura;
            }
            set
            {
                cancelaCaptura = value;
            }
        } 

        private HuellaDigital huellaMuestra = new HuellaDigital();
             
        public HuellaDigital HuellaMuestra
        {
            get
            {
                return huellaMuestra;
            }
            set
            {
                huellaMuestra = value;
            }
        }

        private int numDedoActual;
        private int numManoActual;
        private bool errorCapture;
        private int totalCapturas;
        private int contadorCaptura;
       
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
        public bool ErrorCapture
        {
            get
            {
                return errorCapture;
            }
            set
            {
                errorCapture = value;
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
         
        #endregion

        #region Metodos del Form

        public frmFingerReader()
        {
            InitializeComponent();
        }

        private void frmFingerReader_Load(object sender, EventArgs e)
        {

                log.regBitacora("frmFingerReader.frmFingerReader_Load:        ==>> DEDO: " + (contadorCaptura + 1) + "  Comienza lectura de las muestras");
                cancelaCaptura = false;   
                DefineImagenDedo(numDedoActual, numManoActual);
                DefineLoader();
                errorCapture = false;
                progressBar1.Maximum = 0;
                progressBar1.Maximum = 100;
                IniciaCapuraMuestrasDedo();

        }

        private void btnCancelaCaptura_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult answer = MessageBox.Show("¿Desea cancelar la captura?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes)
                {
                    log.regBitacora("frmFingerReader.btnCancelaCaptura_Click:        Captura Cancelada");
                    huellaMuestra = null;
                    cancelaCaptura = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.btnCancelaCaptura_Click:        Message: " + ex.Message.ToString());
            }
        }

        #endregion

        #region Metodos para captura de huellas

        private void IniciaCapuraMuestrasDedo()
        {
            try
            {
                log.regBitacora("frmFingerReader.IniciaCapuraMuestrasDedo:        Asignando EventHandlers");
                RegisterTemplate.SampleReady += new DpSdkOps._IFPRegisterTemplateEvents_SampleReadyEventHandler(SampleReady);
                RegisterTemplate.Done += new DpSdkOps._IFPRegisterTemplateEvents_DoneEventHandler(RegDone);
                RegisterTemplate.Error += new DpSdkOps._IFPRegisterTemplateEvents_ErrorEventHandler(Error);
                RegisterTemplate.DevDisconnected +=new DpSdkOps._IFPRegisterTemplateEvents_DevDisconnectedEventHandler(DevDisconnected); 
                RegisterTemplate.Run(1);
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.IniciaCapuraMuestrasDedo:        Message: " + ex.Message.ToString());
            }

        }

        private void DevDisconnected()
        {
            MessageBox.Show("Por favor conecte el sensor de huellas", "Sensor no conectado");
            log.regBitacora("frmFingerReader.DevDisconnected:        Captura Interrumpida: Sensor Desconectado.");           
            errorCapture = true;
            huellaMuestra = null;
            RegisterTemplate.DevDisconnected -= new DpSdkOps._IFPRegisterTemplateEvents_DevDisconnectedEventHandler(DevDisconnected); 
            RegisterTemplate.Cancel();    
            this.Close();
            return;

        }

        private void SampleReady(object obj)
        {
            try
            {
                log.regBitacora("frmFingerReader.SampleReady:        Entra a SampleReady, Toma muestra: "+ (count + 1));
                DpSdkEng.FPSample pic = (DpSdkEng.FPSample)obj;
                pic.PictureOrientation = DpSdkEng.AIOrientation.Or_Portrait;
                pic.PictureWidth = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsX(pictureBox1.Width) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelX();
                pic.PictureHeight = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsY(pictureBox1.Height) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelY();

                switch (count)
                {
                    case 0:
                        pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                        pictureBox1.Refresh();
                        labelMuestra.Text = "Muestra 1 de 4";
                        labelMuestra.Refresh();
                        progressBar1.Value = 25;
                        break;
                    case 1:
                        pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                        pictureBox1.Refresh();
                        labelMuestra.Text = "Muestra 2 de 4";
                        labelMuestra.Refresh();
                        progressBar1.Value = 50;
                        break;
                    case 2:
                        pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                        pictureBox1.Refresh();
                        labelMuestra.Text = "Muestra 3 de 4 ";
                        labelMuestra.Refresh();
                        progressBar1.Value = 75;
                        break;
                    case 3:
                        pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                        pictureBox1.Refresh();
                        labelMuestra.Text = "Muestra 4 de 4";
                        labelMuestra.Refresh();
                        progressBar1.Value = 100;
                        break;
                    default:
                        break;
                }
                count++;
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.SampleReady:        Message: " + ex.Message.ToString());
            
            }
        }
          
        private void RegDone(object obj)
        {
            try
            {              
                log.regBitacora("frmFingerReader.RegDone:        Entra al RegDone");
                DpSdkEng.FPTemplate tmplate = (DpSdkEng.FPTemplate)obj;
                tmplate.Export(ref pRegTmplate);
                creaHuella();
                RegisterTemplate.DevDisconnected -= new DpSdkOps._IFPRegisterTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
                RegisterTemplate.Cancel();
                this.Close();
                return;
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.RegDone:        Message: " + ex.Message.ToString());
            
            
            }
        }

        private void Error(DpSdkEngLib.AIErrors errorCode)
        {
            try
            {
                log.regBitacora("frmFingerReader.Error:        Error en la captura de las muestras");
                MessageBox.Show("Las muestras no corresponden al mismo dedo.","Recapturar dedo");
                errorCapture = true;
                huellaMuestra = null;
                RegisterTemplate.DevDisconnected -= new DpSdkOps._IFPRegisterTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
                RegisterTemplate.Cancel();
                this.Close();
                return;
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.Error:        Message: " + ex.Message.ToString());
            }
        }
               
        private void creaHuella() 
        {
            try
            {
                HuellaMuestra.TemplateHuella = pRegTmplate;
                HuellaMuestra.CadenaHuellaHexadecimal = TemplateToHexadecimal(pRegTmplate);
                HuellaMuestra.BytesHuella = StringToByte(HuellaMuestra.CadenaHuellaHexadecimal);
                DivideHuella(HuellaMuestra.CadenaHuellaHexadecimal);
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.creaHuella:        Message: " + ex.Message.ToString());

            }
        }

        #endregion
        
        #region metodos generales
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
               byte[] huellaBytes = (byte[])objHuella;
               foreach (var aux in huellaBytes)
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
                log.regBitacora("frmFingerReader.TemplateToHexadecimal:        Message: " + ex.Message.ToString());
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
                log.regBitacora("frmFingerReader.DefineImagenDedo:        Message: " + ex.Message.ToString());
            }

           
        }

        private  void DivideHuella(string HuellaBase64) 
        {
            try
            {
                int longitudHuella = HuellaBase64.Length;
                HuellaMuestra.HuellaCampo1 = HuellaBase64.Substring(0, 255);
                HuellaMuestra.HuellaCampo2 = HuellaBase64.Substring(255, 255);
                HuellaMuestra.HuellaCampo3 = HuellaBase64.Substring(510, 255);
                HuellaMuestra.HuellaCampo4 = HuellaBase64.Substring(765, 255);
                HuellaMuestra.HuellaCampo5 = HuellaBase64.Substring(1020);
            }
            catch (Exception ex)
            {
                log.regBitacora("frmFingerReader.DivideHuella:        Message: " + ex.Message.ToString());
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
                log.regBitacora("frmFingerReader.DefineLoader:        Message: " + ex.Message.ToString());
            }
        
        }
                
        #endregion

      








    }
}
