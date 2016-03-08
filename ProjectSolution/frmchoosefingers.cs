using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32; 

namespace FingerColaborator
{
    internal partial class frmChooseFingers : Form
    {

        #region Variables globales
        
        private int contadorCapturas;
        private Bitacora log= new Bitacora();
        private HuellaDigital[] dedosACapturar;
        private int resultadoCodigo;

        #endregion
             


        #region Estructuras




        #endregion


        #region propiedades

        public HuellaDigital[] DedosACapturar
        {
            get
            {
                return dedosACapturar;
            }
            set
            {
                dedosACapturar = value;
            }
        }

        /// <summary>     
        /// 0: Dedos seleccionados correctamente
        /// 2: Existe un problema con la configuracion del sensor
        /// 3: No hay sensor conectado       
        /// 5: La captura de huellas fue cancelada     
        /// </summary>
        public int ResultadoCodigo
        {
            get
            {
                return resultadoCodigo;
            }
            set
            {
                resultadoCodigo = value;
            }
        } 




        #endregion

        #region Eventos del Form

        private void frmChooseFingers_Load(object sender, EventArgs e)
        {
            contadorCapturas = 0;

            //JMDJM 4/02/2014
            if (dedosACapturar == null)
            {
                DefineFingers();
            }
            else 
            {
                SetFingers();                    
            }
        }

        public frmChooseFingers()
        {
            InitializeComponent();
        }


        private void DefineFingers() 
        {
            //Hace que todos los dedos se selecionen
            chkAllFingers.Checked = true;


        }


        private void SetFingers() 
        {
            //Primero ponemos los CheckBox en False
            chkAllFingers.Checked = false;
            mIzq_Menique.Checked = false;
            mIzq_Anular.Checked = false;
            mIzq_Medio.Checked = false;
            mIzq_Indice.Checked = false;
            mIzq_Pulgar.Checked = false;       
            mDer_Pulgar.Checked = false;
            mDer_Indice.Checked = false;
            mDer_Medio.Checked = false;
            mDer_Anular.Checked = false;
            mDer_Menique.Checked = false;

            //Deshabilitamos los CheckBox
            chkAllFingers.Enabled = false;
            mIzq_Menique.Enabled = false;
            mIzq_Anular.Enabled = false;
            mIzq_Medio.Enabled = false;
            mIzq_Indice.Enabled = false;
            mIzq_Pulgar.Enabled = false;
            mDer_Pulgar.Enabled = false;
            mDer_Indice.Enabled = false;
            mDer_Medio.Enabled = false;
            mDer_Anular.Enabled = false;
            mDer_Menique.Enabled = false;



            //Solo Habilitamos los dedos que nos solicitan capturar
            for (int i = 0; i < dedosACapturar.Length; i++)
            {
                if (dedosACapturar[i].NumeroMano == 1) 
                {
                    switch (dedosACapturar[i].NumeroDedo)
                    {
                        case 1:
                            mIzq_Pulgar.Enabled = true;
                            mIzq_Pulgar.Checked = true;
                            break;
                        case 2:
                            mIzq_Indice.Enabled = true;
                            mIzq_Indice.Checked = true;
                            break;
                        case 3:
                            mIzq_Medio.Enabled = true;
                            mIzq_Medio.Checked = true;
                            break;
                        case 4:
                            mIzq_Anular.Enabled = true;
                            mIzq_Anular.Checked = true;
                            break;
                        case 5:
                            mIzq_Menique.Enabled = true;
                            mIzq_Menique.Checked = true;
                            break;
                        default:
                            break;
                    }
                }

                if (dedosACapturar[i].NumeroMano == 2)
                {
                    switch (dedosACapturar[i].NumeroDedo)
                    {
                        case 1:
                            mDer_Pulgar.Enabled = true;
                            mDer_Pulgar.Checked = true;
                            break;
                        case 2:
                            mDer_Indice.Enabled = true;
                            mDer_Indice.Checked = true;
                            break;
                        case 3:
                            mDer_Medio.Enabled = true;
                            mDer_Medio.Checked = true;
                            break;
                        case 4:
                            mDer_Anular.Enabled = true;
                            mDer_Anular.Checked = true;
                            break;
                        case 5:
                            mDer_Menique.Enabled = true;
                            mDer_Menique.Checked = true;
                            break;
                        default:
                            break;
                    }
                }
                
                
               

            }

        }




        private void btnIniciaCaptura_Click(object sender, EventArgs e)
        {
            log.regBitacora("frmChooseFingers.btnIniciaCaptura_Click:        -- Seleccionando dedos a capturar --");
            try
            {
                HuellaDigital[] tempArr = new HuellaDigital[10];

                contadorCapturas = 0;

                #region Arma Coleccion de huellas a capturar.
                if (mIzq_Pulgar.Checked)
                {
                    HuellaDigital Izq_Pulgar = new HuellaDigital();
                    Izq_Pulgar.NumeroMano = 1;
                    Izq_Pulgar.NombreMano = "Izquierda";
                    Izq_Pulgar.NumeroDedo = 1;
                    Izq_Pulgar.NombreMano = "Pulgar";
                    tempArr[0] = Izq_Pulgar;
                    contadorCapturas++;
                }

                if (mIzq_Indice.Checked)
                {
                    HuellaDigital Izq_Indice = new HuellaDigital();
                    Izq_Indice.NumeroMano = 1;
                    Izq_Indice.NombreMano = "Izquierda";
                    Izq_Indice.NumeroDedo = 2;
                    Izq_Indice.NombreMano = "Indice";
                    tempArr[1] = Izq_Indice;
                    contadorCapturas++;
                }

                if (mIzq_Medio.Checked)
                {
                    HuellaDigital Izq_Medio = new HuellaDigital();
                    Izq_Medio.NumeroMano = 1;
                    Izq_Medio.NombreMano = "Izquierda";
                    Izq_Medio.NumeroDedo = 3;
                    Izq_Medio.NombreMano = "Medio";
                    tempArr[2] = Izq_Medio;
                    contadorCapturas++;
                }

                if (mIzq_Anular.Checked)
                {
                    HuellaDigital Izq_Anular = new HuellaDigital();
                    Izq_Anular.NumeroMano = 1;
                    Izq_Anular.NombreMano = "Izquierda";
                    Izq_Anular.NumeroDedo = 4;
                    Izq_Anular.NombreMano = "Anular";
                    tempArr[3] = Izq_Anular;
                    contadorCapturas++;
                }

                if (mIzq_Menique.Checked)
                {
                    HuellaDigital Izq_Menique = new HuellaDigital();
                    Izq_Menique.NumeroMano = 1;
                    Izq_Menique.NombreMano = "Izquierda";
                    Izq_Menique.NumeroDedo = 5;
                    Izq_Menique.NombreMano = "Menique";
                    tempArr[4] = Izq_Menique;
                    contadorCapturas++;
                }



                if (mDer_Pulgar.Checked)
                {

                    HuellaDigital Der_Pulgar = new HuellaDigital();
                    Der_Pulgar.NumeroMano = 2;
                    Der_Pulgar.NombreMano = "Derecha";
                    Der_Pulgar.NumeroDedo = 1;
                    Der_Pulgar.NombreMano = "Pulgar";
                    tempArr[5] = Der_Pulgar;
                    contadorCapturas++;
                }

                if (mDer_Indice.Checked)
                {

                    HuellaDigital Der_Indice = new HuellaDigital();
                    Der_Indice.NumeroMano = 2;
                    Der_Indice.NombreMano = "Derecha";
                    Der_Indice.NumeroDedo = 2;
                    Der_Indice.NombreMano = "Indice";
                    tempArr[6] = Der_Indice;
                    contadorCapturas++;
                }

                if (mDer_Medio.Checked)
                {

                    HuellaDigital Der_Medio = new HuellaDigital();
                    Der_Medio.NumeroMano = 2;
                    Der_Medio.NombreMano = "Derecha";
                    Der_Medio.NumeroDedo = 3;
                    Der_Medio.NombreMano = "Medio";
                    tempArr[7] = Der_Medio;
                    contadorCapturas++;
                }


                if (mDer_Anular.Checked)
                {
                    HuellaDigital Der_Menique = new HuellaDigital();
                    Der_Menique.NumeroMano = 2;
                    Der_Menique.NombreMano = "Derecha";
                    Der_Menique.NumeroDedo = 4;
                    Der_Menique.NombreMano = "Anular";
                    tempArr[8] = Der_Menique;
                    contadorCapturas++;
                }

                if (mDer_Menique.Checked)
                {
                    HuellaDigital Der_Anular = new HuellaDigital();
                    Der_Anular.NumeroMano = 2;
                    Der_Anular.NombreMano = "Derecha";
                    Der_Anular.NumeroDedo = 5;
                    Der_Anular.NombreMano = "Menique";
                    tempArr[9] = Der_Anular;
                    contadorCapturas++;
                }


                #endregion


                if (contadorCapturas == 0)
                {
                    MessageBox.Show("Debe seleccionar al menos un dedo","Seleccion incorrecta");
                    
                }
                else
                {
                    if (StatusSensor() == true) //status sensor OK
                    {
                        if (GetVersionDriver())
                        {
                            dedosACapturar = new HuellaDigital[contadorCapturas];
                            int index = 0;
                            foreach (var item in tempArr)
                            {
                                if (item != null)
                                {
                                    DedosACapturar[index] = item;
                                    index++;
                                }                              
                            }
                            log.regBitacora("frmChooseFingers.btnIniciaCaptura_Click:        -- Dedos seleccionados correctamente --");
                            resultadoCodigo = 0; //Dedos seleccionados correctamente
                            this.Close();
                        }
                        else 
                        {
                            //Mensaje 2
                            resultadoCodigo = 2; //Existe un problema con la configuracion del sensor
                            MessageBox.Show("Existe un problema con la configuracion del sensor", "Drivers Sensor");
                            this.Close();
                        }
                    }
                    else
                    {
                        //Mensaje 3
                        resultadoCodigo = 3; // No hay sensor conectado  
                        MessageBox.Show("Conecte el sensor para iniciar la captura", "Lector de huellas no encontrado");
                       // this.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmChooseFingers.btnIniciaCaptura_Click:        Message: " + ex.Message.ToString());
            }
                        
        }

        private void btnCancelaCaptura_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult answer = MessageBox.Show("¿Desea cancelar la captura?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes)
                {
                   log.regBitacora("frmChooseFingers.btnCancelaCaptura_Click:        Captura Cancelada");
                    dedosACapturar = null;
                    resultadoCodigo = 5; // 5: La captura de huellas fue cancelada                       
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmChooseFingers.btnCancelaCaptura_Click:        Message: " + ex.Message.ToString());
            }
                       
        }

        


        private void SelectAllFingers()        
        {
            try
            {
                if (chkAllFingers.Checked)
                {
                    mIzq_Menique.Checked = true;
                    mIzq_Anular.Checked = true;
                    mIzq_Medio.Checked = true;
                    mIzq_Indice.Checked = true;
                    mIzq_Pulgar.Checked = true;
                    mDer_Pulgar.Checked = true;
                    mDer_Indice.Checked = true;
                    mDer_Medio.Checked = true;
                    mDer_Anular.Checked = true;
                    mDer_Menique.Checked = true;
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmChooseFingers.chkAllFingers_CheckedChanged:        Message: " + ex.Message.ToString());
            }
        }







       #endregion

        #region Metodos Generales

        private bool StatusSensor()
        {
            bool sensorConectado = true;           
            try
            {
                DpSdkEng.FPDevicesClass FPDevice = new DpSdkEng.FPDevicesClass();
                if (FPDevice.Count == 0 )
                {
                    sensorConectado = false;
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("frmChooseFingers.StatusSensor:        Message: " + ex.Message.ToString());
            }

            log.regBitacora("frmChooseFingers.StatusSensor:        StatusSensor: " + sensorConectado.ToString());

            return sensorConectado;
        }

        public static bool GetVersionDriver()
        {
            try
            {
                bool result = false;
                string versionDrivers = "";
                RegistryKey key1 = Registry.LocalMachine.OpenSubKey(@"Software\DigitalPersona\Products\Platinum Fingerprint Recognition Software");
                if (key1 == null)
                {
                    key1 = Registry.LocalMachine.OpenSubKey(@"Software\DigitalPersona\Products\UareU Platinum Integrator");
                }
                if (key1 == null)
                {
                    key1 = Registry.LocalMachine.OpenSubKey(@"Software\DigitalPersona\Products\Platinum SDK");
                }
                versionDrivers = key1.GetValue("version").ToString();
                if (versionDrivers != "")
                {
                    result = true;
                }


                return result;
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region Eventos en los CheckBox


        private void chkAllFingers_CheckedChanged(object sender, EventArgs e)
        {
            SelectAllFingers();
        }

        private void mIzq_Menique_CheckedChanged(object sender, EventArgs e)
        {
            if (!mIzq_Menique.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mIzq_Anular_CheckedChanged(object sender, EventArgs e)
        {
            if (!mIzq_Anular.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mIzq_Medio_CheckedChanged(object sender, EventArgs e)
        {
            if (!mIzq_Medio.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mIzq_Indice_CheckedChanged(object sender, EventArgs e)
        {
            if (!mIzq_Indice.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mIzq_Pulgar_CheckedChanged(object sender, EventArgs e)
        {
            if (!mIzq_Pulgar.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mDer_Pulgar_CheckedChanged(object sender, EventArgs e)
        {
            if (!mIzq_Menique.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mDer_Indice_CheckedChanged(object sender, EventArgs e)
        {
            if (!mDer_Indice.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mDer_Medio_CheckedChanged(object sender, EventArgs e)
        {
            if (!mDer_Medio.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mDer_Anular_CheckedChanged(object sender, EventArgs e)
        {
            if (!mDer_Anular.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        private void mDer_Menique_CheckedChanged(object sender, EventArgs e)
        {
            if (!mDer_Menique.Checked)
            {
                chkAllFingers.Checked = false;
            }
        }

        #endregion

    }
}
