using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace FingerColaborator
{
    internal partial class frmSearchEmployeeFinger : Form
    {

        #region Variables Globales
        private DpSdkOps.FPGetTemplateClass verifyTemple = new DpSdkOps.FPGetTemplateClass();
        private DpSdkEng.AISecureModeMask secureMode = new DpSdkEng.AISecureModeMask();
        private object _FingerPrintTemplate;
        private Bitacora log;
        #endregion     

        #region Propiedades

        private int contador = 0;
        private int coincidenciasHuella = 0;
        private HuellaDigital[] coleccionHuellasEmpleado;

        public int CoincidenciasHuella
        {
            get
            {
                return coincidenciasHuella;
            }
            set
            {
                coincidenciasHuella = value;
            }
        }
        public HuellaDigital[] ColeccionHuellasEmpleado
        {
            get
            {
                return coleccionHuellasEmpleado;
            }
            set
            {
                coleccionHuellasEmpleado = value;
            }
        }

        #endregion
        
        #region Metodos del Form

        public frmSearchEmployeeFinger()
        {
            InitializeComponent();
        }

        private void frmSearchEmployeeFinger_Load(object sender, EventArgs e)
        {
            log = new Bitacora();
            DefineImagenDedosActivos();
            ComparaHuellas();
         
        }

        #endregion

        #region Metodos para la busqueda de Huellas

        private void ComparaHuellas()
        {
            try
            {
                log.regBitacora("frmSearchEmployeeFinger.ComparaHuellas:        Asignando EventHandlers");
                verifyTemple.SampleReady += new DpSdkOps._IFPGetTemplateEvents_SampleReadyEventHandler(SampleReady);
                verifyTemple.Done += new DpSdkOps._IFPGetTemplateEvents_DoneEventHandler(RegDone);
                verifyTemple.DevDisconnected +=new DpSdkOps._IFPGetTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
                verifyTemple.Run(0);
            }
            catch (Exception ex)
            {

                log.regBitacora("frmSearchEmployeeFinger.ComparaHuellas:        Message: " + ex.Message.ToString());
            }
        }

        private void DevDisconnected()
        {
             
            MessageBox.Show("Por favor conecte el sensor de huellas","Sensor no conectado");

        }



        private void SampleReady(object obj)
        {
            try
            {
                log.regBitacora("frmSearchEmployeeFinger.SampleReady:        Entra a SampleReady");
                DpSdkEng.FPSample pic = (DpSdkEng.FPSample)obj;
                pic.PictureOrientation = DpSdkEng.AIOrientation.Or_Portrait;
                pic.PictureWidth = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsX(pictureBox1.Width) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelX();
                pic.PictureHeight = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsY(pictureBox1.Height) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelY();
                pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox1.Refresh();
            }
            catch (Exception ex)
            {

                log.regBitacora("frmSearchEmployeeFinger.SampleReady:        Message: " + ex.Message.ToString());
            }
        }

        private void RegDone(object vt)
        {
            try
            {
                log.regBitacora("frmSearchEmployeeFinger.RegDone:        Entra a RegDone");
                int numHuellas = coleccionHuellasEmpleado.Length;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = numHuellas;
                progressBar1.Value = 0;

                labelMuestra.Text = "Comparando...";
                labelMuestra.Refresh();

                DateTime inicio = DateTime.Now;

                while (contador < numHuellas)
                {
                    // Pasa la cadena a Byte[] 
                    _FingerPrintTemplate = StringToByte(coleccionHuellasEmpleado[contador].CadenaHuellaHexadecimal);
                    bool verifyOK = false;
                    object score = 0;
                    object theshold = 0;
                    bool tture = true;
                    DpSdkEng.FPTemplate sensorTemplate = (DpSdkEng.FPTemplate)vt;
                    DpSdkEng.FPTemplateClass RegTemplate = new DpSdkEng.FPTemplateClass();
                    RegTemplate.Import(_FingerPrintTemplate);
                    DpSdkEng.FPVerifyClass verify = new DpSdkEng.FPVerifyClass();
                    verify.Compare(RegTemplate, sensorTemplate, ref verifyOK, ref score, ref theshold, ref tture, ref secureMode);
                    if (verifyOK == true)
                    {
                        PintaDedoVerificado(coleccionHuellasEmpleado[contador]);
                        coincidenciasHuella++;
                    }
                    labelMuestra.Text = "Concidencias: " + coincidenciasHuella.ToString();
                    labelMuestra.Refresh();
                    contador++;
                    progressBar1.Value += 1;
                }
                verifyTemple.Cancel();

                if (coincidenciasHuella > 0)
                {
                    lblResultado.ForeColor = Color.Green;
                    lblResultado.Text = "SI Coincide";
                    lblResultado.Refresh();
                }
                else 
                {                    
                    lblResultado.ForeColor = Color.Red;
                    lblResultado.Text = "NO Coincide";
                    lblResultado.Refresh();
                }

                verifyTemple.DevDisconnected -= new DpSdkOps._IFPGetTemplateEvents_DevDisconnectedEventHandler(DevDisconnected);
                this.Close();
            }
            catch (Exception ex)
            {

                log.regBitacora("frmSearchEmployeeFinger.RegDone:        Message: " + ex.Message.ToString());
            }
        }

        #endregion

        #region Metodos Generales

        private string ObtieneTiempo(DateTime inicio, DateTime fin) 
        {
          
            TimeSpan intervalo = fin.Subtract(inicio);
            string tiempoBusqueda = "";
            if (intervalo.Seconds < 60)
            {
                tiempoBusqueda = intervalo.Seconds + " Segundos";
            }
            else
            {
                tiempoBusqueda = intervalo.Minutes + " Minutos";
            }
            return tiempoBusqueda;
        }

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

        private bool SensorConectado()
        {
            DpSdkEng.FPDevicesClass FPDevice = new DpSdkEng.FPDevicesClass();
            if (FPDevice.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }             
        
        private void DefineImagenDedosActivos()
        {
            M1D1.BackColor = Color.Lime;
            M1D2.BackColor = Color.Lime;
            M1D3.BackColor = Color.Lime;
            M1D4.BackColor = Color.Lime;
            M1D5.BackColor = Color.Lime;
            M1D1.BackColor = Color.Lime;
            M2D2.BackColor = Color.Lime;
            M2D3.BackColor = Color.Lime;
            M2D4.BackColor = Color.Lime;
            M2D5.BackColor = Color.Lime;
            M1D1.Visible = false;
            M1D2.Visible = false;
            M1D3.Visible = false;
            M1D4.Visible = false;
            M1D5.Visible = false;
            M2D1.Visible = false;
            M2D2.Visible = false;
            M2D3.Visible = false;
            M2D4.Visible = false;
            M2D5.Visible = false;

            foreach (var item in coleccionHuellasEmpleado)
            {
                if (item.NumeroMano == 1)
                {
                    switch (item.NumeroDedo)
                    {
                        case 1:
                            M1D1.Visible = true;
                            break;
                        case 2:
                            M1D2.Visible = true;
                            break;
                        case 3:
                            M1D3.Visible = true;
                            break;
                        case 4:
                            M1D4.Visible = true;
                            break;
                        case 5:
                            M1D5.Visible = true;
                            break;
                        default:
                            break;
                    }
                }

                if (item.NumeroMano == 2)
                {
                    switch (item.NumeroDedo)
                    {
                        case 1:
                            M2D1.Visible = true;
                            break;
                        case 2:
                            M2D2.Visible = true;
                            break;
                        case 3:
                            M2D3.Visible = true;
                            break;
                        case 4:
                            M2D4.Visible = true;
                            break;
                        case 5:
                            M2D5.Visible = true;
                            break;
                        default:
                            break;
                    }

                }




            }



        
        }
        
        private void PintaDedoVerificado(HuellaDigital huellaDedoVerificado) 
        {
            if (huellaDedoVerificado.NumeroMano == 1)
            {
                switch (huellaDedoVerificado.NumeroDedo)
                {
                    case 1:
                        M1D1.BackColor = Color.Red;
                        break;
                    case 2:
                        M1D2.BackColor = Color.Red;
                        break;
                    case 3:
                        M1D3.BackColor = Color.Red;
                        break;
                    case 4:
                        M1D4.BackColor = Color.Red;
                        break;
                    case 5:
                        M1D5.BackColor = Color.Red;
                        break;
                    default:
                        break;
                }
            }
            if (huellaDedoVerificado.NumeroMano == 2)
            {
                switch (huellaDedoVerificado.NumeroDedo)
                {
                    case 1:
                        M2D1.BackColor = Color.Red;
                        break;
                    case 2:
                        M2D2.BackColor = Color.Red;
                        break;
                    case 3:
                        M2D3.BackColor = Color.Red;
                        break;
                    case 4:
                        M2D4.BackColor = Color.Red;
                        break;
                    case 5:
                        M2D5.BackColor = Color.Red;
                        break;
                    default:
                        break;
                }
            }
        
        }

        #endregion
                
    }
}
