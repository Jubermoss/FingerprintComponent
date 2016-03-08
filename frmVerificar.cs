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
    public partial  class frmVerificar : Form
    {
        HuellaDigital Huella;
        


        //**********Verify ****************  
        //Paso1
        private DPSDKOPSLib.FPRegisterTemplateClass RegisterTemplate = new DPSDKOPSLib.FPRegisterTemplateClass();
        public object pRegTmplate = 0; //Se usa para exportar los bytes de la huella (es la muestra que genera.)    
        int count = 0;
        //Paso2
        private DPSDKOPSLib.FPGetTemplateClass verifyTemple3 = new DPSDKOPSLib.FPGetTemplateClass();
        private object auxpRegTmplate = 0; 
        private DpSdkEngLib.AISecureModeMask sm2 = new DpSdkEngLib.AISecureModeMask();




        public frmVerificar()
        {
            InitializeComponent();
        }

        private void frmVerificar_Load(object sender, EventArgs e)
        {
            btnCapturaTestigo.Enabled = false;
            Huella = new HuellaDigital();
        }


        private void btnCapturaMuestras_Click(object sender, EventArgs e)
        {
            labelMuestra1.Text = "Muestra 1";
            labelMuestra2.Text = "Muestra 2";
            labelMuestra3.Text = "Muestra 3";
            labelMuestra4.Text = "Muestra 4";


            if (SensorConectado())
            {
               InicioCapuraMuestras();
                btnCapturaMuestras.Enabled = false;
                labelstatusCaptura.Text = "Coloca 4 veces el dedo en el sensor";
            }
            else 
            {
                MessageBox.Show("Sensor Desconectado");
            }
   
        }

        private void btnCapturaTestigo_Click(object sender, EventArgs e)
        {
            labelResultado.Text="-";
            if (SensorConectado())
            {
                InicioComparacion();
                btnCapturaTestigo.Enabled = false;
                labelStatusHuellaTestigo.Text = "Coloca el dedo en el sensor";
            }
            else
            {
                MessageBox.Show("Sensor Desconectado");
            }
        }


        public void InicioCapuraMuestras()
        {
            RegisterTemplate.SampleReady += new DPSDKOPSLib._IFPRegisterTemplateEvents_SampleReadyEventHandler(SampleReady);
            RegisterTemplate.Done += new DPSDKOPSLib._IFPRegisterTemplateEvents_DoneEventHandler(RegDone);
            RegisterTemplate.Run(1);


        }
        
        private void SampleReady(object obj)
        {
            DpSdkEngLib.FPSample pic = (DpSdkEngLib.FPSample)obj;
            pic.PictureOrientation = DpSdkEngLib.AIOrientation.Or_Portrait;
            pic.PictureWidth = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsX(pictureBox1.Width) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelX();
            pic.PictureHeight = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsY(pictureBox1.Height) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelY();

            if (count == 0)
            {
                pictureBox1.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox1.Refresh();
                labelMuestra1.Text += " OK";
                labelMuestra1.Refresh();
     
            }
            if (count == 1)
            {
                pictureBox2.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox2.Refresh();
                labelMuestra2.Text += " OK";
                labelMuestra2.Refresh();
            }
            if (count == 2)
            {
                pictureBox3.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox3.Refresh();
                labelMuestra3.Text += " OK";
                labelMuestra3.Refresh();
            }
            if (count == 3)
            {
                pictureBox4.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
                pictureBox4.Refresh();
                labelMuestra4.Text += " OK";
                labelMuestra4.Refresh();
            }
            count++;
            Console.Write("Sample Ready");
        }
        
        private void RegDone(object obj)
        {
            DpSdkEngLib.FPTemplate tmplate = (DpSdkEngLib.FPTemplate)obj;
            tmplate.Export(ref pRegTmplate);
            textBox1.Text= TemplateToHexadecimal(pRegTmplate);
            btnCapturaMuestras.Enabled = false;
            btnCapturaTestigo.Enabled = true;
        }


        public void InicioComparacion()
        {
            verifyTemple3.Done += new DPSDKOPSLib._IFPGetTemplateEvents_DoneEventHandler(VerifyDoneComparacion);
            verifyTemple3.SampleReady += new DPSDKOPSLib._IFPGetTemplateEvents_SampleReadyEventHandler(SampleReadyComparacion);
            verifyTemple3.Run(0);
        }
        

        private void SampleReadyComparacion(object obj)
        {
            DpSdkEngLib.FPSample pic = (DpSdkEngLib.FPSample)obj;
            pic.PictureOrientation = DpSdkEngLib.AIOrientation.Or_Portrait;
            pic.PictureWidth = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsX(pictureBox5.Width) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelX();
            pic.PictureHeight = Microsoft.VisualBasic.Compatibility.VB6.Support.PixelsToTwipsY(pictureBox5.Height) / Microsoft.VisualBasic.Compatibility.VB6.Support.TwipsPerPixelY();
            pictureBox5.Image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(pic.Picture);
            pictureBox5.Refresh();
        }



        private void VerifyDoneComparacion(object vt)
        {
            bool verifyOK = false;
            object score = 0;
            object theshold = 0;
            bool tture = true;
            DpSdkEngLib.FPTemplate verifyTemplate = (DpSdkEngLib.FPTemplate)vt;
            DpSdkEngLib.FPTemplateClass RegTemplate = new DpSdkEngLib.FPTemplateClass();
            RegTemplate.Import(pRegTmplate); //AQUI CARGA EL TEMPLATE ENVIADO DESDE FORM 1
            DpSdkEngLib.FPVerifyClass verify = new DpSdkEngLib.FPVerifyClass();
            verify.Compare(RegTemplate, verifyTemplate, ref verifyOK, ref score, ref theshold, ref tture, ref sm2);
            if (verifyOK == true)
            {
                labelResultado.Text="La Huella SI coincide con las muestras !!";
                labelStatusHuellaTestigo.Text = "-";
                labelstatusCaptura.Text="-";
                btnCapturaTestigo.Enabled = false;
                btnCapturaMuestras.Enabled = false;
            }

            else
            {
                labelResultado.Text = "La Huella NO coincide con las muestras";
                labelStatusHuellaTestigo.Text = "-";
                labelstatusCaptura.Text = "-";
                btnCapturaTestigo.Enabled = true;
                btnCapturaMuestras.Enabled = false;
            }

            labelResultado.Refresh();


        }



        #region Metodos privados

        /// <summary>
        /// Pasa la Huella capturada del Sensor a Hexadecimal 
        /// </summary>
        /// <param name="obj">Huella obtenida del Sensor</param>
        /// <returns>Cadena de la huella en Hexadecimal</returns>
        public string TemplateToHexadecimal(object objHuella)
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

        public bool SensorConectado()
        {
            DpSdkEngLib.FPDevicesClass FPDevice = new DpSdkEngLib.FPDevicesClass();
            if (FPDevice.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

    }
}
