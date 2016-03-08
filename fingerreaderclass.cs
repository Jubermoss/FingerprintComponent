using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32; 

namespace FingerColaborator
{

    #region delegados

    internal delegate void FingerReaderClassEventHandler_RegistraHuellasDone();
    internal delegate void FingerReaderClassEventHandler_BuscaHuellasFormSimpleDone();
    internal delegate void FingerReaderClassEventHandler_BuscaHuellas();
    #endregion

    internal class FingerReaderClass
    {

       #region Eventos de la clase
        internal event FingerReaderClassEventHandler_RegistraHuellasDone RegistraHuellasDone;
        internal event FingerReaderClassEventHandler_BuscaHuellasFormSimpleDone BuscaHuellasFormSimpleDone;
        internal event FingerReaderClassEventHandler_BuscaHuellas BuscaHuellasDone;
        #endregion
        

       #region Variables globales

        internal frmChooseFingers frmSelectFingers;
        internal frmFingerReader frmCapture;
        internal frmIdentifyFinger frmIdentify;
        internal frmSearchEmployeeFinger frmSearchEmploye;
        private Bitacora log;  
        private int contadorCapturas;
        private int totalDedos;
        private  HuellaDigital[] coleccionHuellas;
        private int coincidenciasHuella;
        private  Resultado fingerReaderResult = new Resultado();
        private HuellaDigital[] dedosAcapturar; //JMDJM 4/02/2014


        #endregion

        #region Propiedades
        /// <summary>
        /// Devuelve la coleccion de objetos de tipo HuellaDigital que contiene toda la informacion de las Huellas.
        /// </summary>
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

        public Resultado FingerReaderResult 
        {
            get
            {
                return fingerReaderResult;
            }
            set
            {
                fingerReaderResult = value;
            }
        }

        //JMDJM 4/02/2014
        public HuellaDigital[] DedosAcapturar
        {
            get
            {
                return dedosAcapturar;
            }
            set
            {
                dedosAcapturar = value;
            }
        }

        #endregion



        #region Estructuras

        /// <summary>
        /// 0: Coincide la Huella
        /// 1: No Coincide la Huella
        /// 2: Existe un problema con la configuracion del sensor
        /// 3: No hay sensor conectado
        /// 4: El formato de entrada de la(s) huellas es incorrecto 
        /// 5: La captura de huellas fue cancelada
        /// 6: Finalizo la  captura de huellas
        /// </summary>
        internal struct Resultado
        {
          public  int codigo;
          public  string descripcion;
        }

        #endregion



        #region "Constructor"
        public FingerReaderClass()
        {
            log = new Bitacora();
        }
        #endregion


       #region Registro de Huellas

        /// <summary>
        /// Inicia el proceso de Registro y verificacion de Huellas
        /// Lanza el form "frmChooseFingers" que selecciona las huellas a capturar.
        /// Una vez finalizado el proceso, la coleccion de huellas se obtiene de la propiedad "ColeccionHuellas"
        /// </summary>
        public void RegistraHuellas() 
        {
            try
            {
                log.regBitacora("FingerReaderClass.RegistraHuellas:        frmChooseFingers.Show()");
                frmSelectFingers = new frmChooseFingers();
                frmSelectFingers.DedosACapturar = dedosAcapturar; //JMDJM 4/02/2014
                frmSelectFingers.FormClosed += new FormClosedEventHandler(frmChooseFingers_Closed);
                frmSelectFingers.ShowDialog(); 
                
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderClass.RegistraHuellas:        Message: " + ex.Message.ToString());
            }
        }



        /// <summary>
        /// Se ejecuta automaticamente despues  de que se cierra el Form  "frmChooseFingers"
        /// Lanza el form frmFingerReader que captura las cuatro muestras de una huella
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmChooseFingers_Closed(object sender, FormClosedEventArgs e)
        {
            try
            {
                log.regBitacora("FingerReaderClass.frmChooseFingers_Closed:        Se cierra el Form  'frmChooseFingers'");
                switch (frmSelectFingers.ResultadoCodigo)
                {
                    case 2:
                        fingerReaderResult.codigo = 2;
                        fingerReaderResult.descripcion = "Existe un problema con la configuracion del sensor";
                        coleccionHuellas = null;
                        RegistraHuellasDone();
                        break;

                    case 3:
                        fingerReaderResult.codigo = 3;
                        fingerReaderResult.descripcion = "No hay sensor conectado";
                        coleccionHuellas = null;
                        RegistraHuellasDone();
                        break;

                    case 5:
                        fingerReaderResult.codigo = 5;
                        fingerReaderResult.descripcion = "La captura de huellas fue cancelada";
                        coleccionHuellas = null;
                        RegistraHuellasDone();
                        break;

                    default:
                        coleccionHuellas = frmSelectFingers.DedosACapturar;
                        totalDedos = coleccionHuellas.Length;
                        contadorCapturas = 0;
                        log.regBitacora("FingerReaderClass.frmChooseFingers_Closed:        Dedos a capturar: " + totalDedos);
                        IniciaCapturaMuestras();
                        break;
                }

           }
            catch(Exception ex)
            {
                log.regBitacora("FingerReaderClass.frmChooseFingers_Closed:        Message: " + ex.Message.ToString());
            }
        }

        
        /// <summary>
        /// Recolecta 4 muestras, verifica y finalmente Registra una coleccion de huellas.
        /// Antes de ejecutar este metodo se debe pasar el numero de dedos a capturar por propiedad
        /// FingerClass.TotalDedosAcapturar = N
        /// </summary>
        private void IniciaCapturaMuestras()
        {
            try
            {
                log.regBitacora("FingerReaderClass.IniciaCapturaMuestras:        Se lanza el Form --> frmFingerReader.Show()");
                frmCapture = new frmFingerReader();
                frmCapture.FormClosed += new FormClosedEventHandler(VerificaHuellas);
                frmCapture.NumManoActual = coleccionHuellas[contadorCapturas].NumeroMano;
                frmCapture.NumDedoActual = coleccionHuellas[contadorCapturas].NumeroDedo;
                frmCapture.ContadorCaptura = contadorCapturas;
                frmCapture.TotalCapturas = totalDedos;
                frmCapture.ShowDialog();              
            }
            catch (Exception ex) 
            {
                log.regBitacora("FingerReaderClass.IniciaCapturaMuestras:        Message: " + ex.Message.ToString());
            }          
        }

        
        /// <summary>
        /// Se ejecuta automaticamente despues  de que se cierra el Form  "frmFingerReader"
        /// Lanza el form frmIdentifyFinger que captura la quinta Huella
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerificaHuellas(object sender, FormClosedEventArgs e)
        {
            try
            {

                log.regBitacora("FingerReaderClass.VerificaHuellas:        Se cerro el Form --> frmFingerReader.FormClosed()");

                if (frmCapture.CancelaCaptura == true)
                {
                    coleccionHuellas = null;
                    fingerReaderResult.codigo = 5;
                    fingerReaderResult.descripcion = "La captura de huellas fue cancelada";
                    RegistraHuellasDone();
                }
                else 
                {
                    if (frmCapture.ErrorCapture == true) //Se recaptura el dedo actual 
                    {
                        IniciaCapturaMuestras();
                    }
                    else 
                    {
                        if (frmCapture.HuellaMuestra != null) 
                        { 
                            log.regBitacora("FingerReaderClass.VerificaHuellas:        Se recuperan las muestras de la huella");
                            //Recuperamos la muestra de las 4 huellas del sensor entregadas por el  Form frmFingerReader
                            coleccionHuellas[contadorCapturas].BytesHuella = frmCapture.HuellaMuestra.BytesHuella;
                            coleccionHuellas[contadorCapturas].CadenaHuellaHexadecimal = frmCapture.HuellaMuestra.CadenaHuellaHexadecimal;
                            coleccionHuellas[contadorCapturas].TemplateHuella = frmCapture.HuellaMuestra.TemplateHuella;
                            coleccionHuellas[contadorCapturas].HuellaCampo1 = frmCapture.HuellaMuestra.HuellaCampo1;
                            coleccionHuellas[contadorCapturas].HuellaCampo2 = frmCapture.HuellaMuestra.HuellaCampo2;
                            coleccionHuellas[contadorCapturas].HuellaCampo3 = frmCapture.HuellaMuestra.HuellaCampo3;
                            coleccionHuellas[contadorCapturas].HuellaCampo4 = frmCapture.HuellaMuestra.HuellaCampo4;
                            coleccionHuellas[contadorCapturas].HuellaCampo5 = frmCapture.HuellaMuestra.HuellaCampo5;

                            // Empieza la verificacion de la muestra contra la 5ta Huella del sensor.
                            frmIdentify = new frmIdentifyFinger();
                            frmIdentify.FormClosed += new FormClosedEventHandler(AlmacenaHuellas);
                            frmIdentify.NumManoActual = coleccionHuellas[contadorCapturas].NumeroMano;
                            frmIdentify.NumDedoActual = coleccionHuellas[contadorCapturas].NumeroDedo;
                            frmIdentify.CadenaHuellaHexadecimal = coleccionHuellas[contadorCapturas].CadenaHuellaHexadecimal;
                            frmIdentify.HuellasAlmacenadasPreviemente = coleccionHuellas;
                            frmIdentify.ContadorCaptura = contadorCapturas;
                            frmIdentify.TotalCapturas = totalDedos;
                            frmIdentify.ShowDialog();
                        }
                    }
                }

            }
            catch (Exception ex)  
            {
                log.regBitacora("FingerReaderClass.VerificaHuellas:        Message: " + ex.Message.ToString());
            }

        }

         /// <summary>
        /// Se ejecuta automaticamente despues  de que se cierra el Form --> frmIdentifyFinger.FormClosed()
        /// Compara una 5ta Huella contra la muestra de 4 huellas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlmacenaHuellas(object sender, FormClosedEventArgs e)
        {
            try
            {
                log.regBitacora("FingerReaderClass.AlmacenaHuellas:        Inicia Proceso de Verificacion");

                if (frmIdentify.ResultadoVerificacion == true)// Verificacion exitosa
                {
                    if (frmIdentify.HuellaDuplicada == false) //valida que no se halla capturado antes
                    {
                        contadorCapturas++;
                    }
                    else
                    {
                        log.regBitacora("FingerReaderClass.AlmacenaHuellas:        + + + Huella duplicada, Recapturar + + +");
                        MessageBox.Show("Esa huella ya fue capturada, coloca el dedo indicado en la imagen","Huella repetida");
                    }
                }
                else
                {
                    log.regBitacora("FingerReaderClass.AlmacenaHuellas:        + + + No se pudo Verificar el dedo, Recapturar + + +");
                    MessageBox.Show("No se pudo verificar el  Dedo:  " + coleccionHuellas[contadorCapturas].NumeroDedo + " Mano: " + coleccionHuellas[contadorCapturas].NumeroMano , "Recapturar dedo");
                }
                                     
                              
                
                log.regBitacora("FingerReaderClass.AlmacenaHuellas:        Finaliza Proceso de Verificacion");

                if (contadorCapturas < totalDedos)
                {
                    IniciaCapturaMuestras();
                }
                else 
                {
                  //  MessageBox.Show("Finalizo la captura de los " +totalDedos +" dedos");           
                    fingerReaderResult.codigo = 6;
                    fingerReaderResult.descripcion = "Finalizo la  captura de huellas";
                    log.regBitacora("FingerReaderClass.AlmacenaHuellas:        ** Finaliza el proceso de captura de los " + totalDedos + " dedos **");
                    RegistraHuellasDone();
                }                  
               
            }
            catch (Exception ex)  
            {
                log.regBitacora("FingerReaderClass.AlmacenaHuellas:        Message: " + ex.Message.ToString());
            }
        }
        
        #endregion
             
      
        #region Busqueda de Huellas

        public void BuscaHuellas(HuellaDigital[] ColHuellas) 
        {
            log.regBitacora("FingerReaderClass.BuscaHuellas:        Inicia busqueda de huellas " );
           try
           {             
               if (SensorConectado())
               {
                   string seccionHuella;

                   for (int j = 0; j < ColHuellas.Length; j++)
                   {
                       if (ColHuellas[j].CadenaHuellaHexadecimal != null && ColHuellas[j].CadenaHuellaHexadecimal != "")
                       {
                           ColHuellas[j].CadenaHuellaHexadecimal = ColHuellas[j].CadenaHuellaHexadecimal.Trim();
                           ColHuellas[j] = DivideHuella(ColHuellas[j]);
                       }
                       else
                       {
                           if (ColHuellas[j].HuellaCampo1 != null && ColHuellas[j].HuellaCampo2 != null && ColHuellas[j].HuellaCampo3 != null && ColHuellas[j].HuellaCampo4 != null && ColHuellas[j].HuellaCampo5 != null)
                           {
                               seccionHuella = "";
                               seccionHuella += ColHuellas[j].HuellaCampo1;
                               seccionHuella += ColHuellas[j].HuellaCampo2;
                               seccionHuella += ColHuellas[j].HuellaCampo3;
                               seccionHuella += ColHuellas[j].HuellaCampo4;
                               seccionHuella += ColHuellas[j].HuellaCampo5;
                               ColHuellas[j].CadenaHuellaHexadecimal = seccionHuella.Trim();
                           }
                           else
                           {
                               coincidenciasHuella = 0;
                               fingerReaderResult.codigo = 4;
                               fingerReaderResult.descripcion = "El formato de entrada de la(s) huellas es incorrecto";
                               BuscaHuellasDone();
                           }
                       }

                   }

                   // Lanzamos el Form para la busqueda de Huellas.
                   Load_frmSearchEmploye(ColHuellas);
               }
               else
               {                 
                   coincidenciasHuella = 0;
                   BuscaHuellasDone();
               }
               log.regBitacora("FingerReaderClass.BuscaHuellas:        Finaliza busqueda de huellas");
           }
           catch (Exception ex)
           {
               coincidenciasHuella = 0;
               fingerReaderResult.codigo = 4;
               fingerReaderResult.descripcion = "El formato de entrada de la(s) huellas es incorrecto";
               BuscaHuellasDone(); 
               log.regBitacora("FingerReaderClass.BuscaHuellas:        Message: " + ex.Message.ToString());
           }
        }


        private void Load_frmSearchEmploye(HuellaDigital[] ColHuellas) 
        {
            try
            {
                log.regBitacora("FingerReaderClass.Load_frmSearchEmploye:        Carga Form 'frmSearchEmployeeFinger'");
                frmSearchEmploye = new frmSearchEmployeeFinger();
                frmSearchEmploye.FormClosed += new FormClosedEventHandler(frmSearchEmploye_FormClosed);
                frmSearchEmploye.ColeccionHuellasEmpleado = ColHuellas;
                frmSearchEmploye.ShowDialog();
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderClass.Load_frmSearchEmploye:        Message: " + ex.Message.ToString());
            }
        } 
          

        private void frmSearchEmploye_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                log.regBitacora("FingerReaderClass.frmSearchEmploye_FormClosed:        Cierra Form 'frmSearchEmployeeFinger'");
                coincidenciasHuella = frmSearchEmploye.CoincidenciasHuella;
                if (coincidenciasHuella > 0)
                {
                    fingerReaderResult.codigo = 0;
                    fingerReaderResult.descripcion = "Coincide la Huella";
                }
                if (coincidenciasHuella == 0)
                {
                    fingerReaderResult.codigo = 1; 
                    fingerReaderResult.descripcion = "No Coincide la Huella";
                }
                BuscaHuellasDone();
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderClass.frmSearchEmploye_FormClosed:        Message: " + ex.Message.ToString());
            }
        }

        

        #endregion


        #region Metodos de la clase 

        private bool SensorConectado()
        {

            try
            {
                DpSdkEng.FPDevicesClass FPDevice = new DpSdkEng.FPDevicesClass();
                if (FPDevice.Count != 0)
                {
                    if (GetVersionDriver())
                    {
                        return true;
                    }
                    else 
                    {
                        fingerReaderResult.codigo = 2;
                        fingerReaderResult.descripcion = "Existe un problema con la configuracion del sensor";
                        MessageBox.Show("Existe un problema con la configuracion del sensor", "Drivers Sensor");
                        return false;
                    }
                }
                else
                {
                    fingerReaderResult.codigo = 3;
                    fingerReaderResult.descripcion = "No hay sensor conectado";
                    MessageBox.Show("Conecte el sensor para iniciar la verificacion", "Lector de huellas no encontrado");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderClass.SensorConectado:        Message: " + ex.Message.ToString());
                return false;
            }

        }

        private  bool GetVersionDriver()
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
            catch(Exception ex)
            {
                log.regBitacora("FingerReaderClass.GetVersionDriver:        Message: " + ex.Message.ToString());
                return false;
            }
        }

        private HuellaDigital DivideHuella(HuellaDigital HuellaActual)
        {
            try
            {
                log.regBitacora("FingerReaderClass.DivideHuella    |   Inicia division de la huella");
                int longitudHuella = HuellaActual.CadenaHuellaHexadecimal.Length;
                HuellaActual.HuellaCampo1 = HuellaActual.CadenaHuellaHexadecimal.Substring(0, 255);
                HuellaActual.HuellaCampo2 = HuellaActual.CadenaHuellaHexadecimal.Substring(255, 255);
                HuellaActual.HuellaCampo3 = HuellaActual.CadenaHuellaHexadecimal.Substring(510, 255);
                HuellaActual.HuellaCampo4 = HuellaActual.CadenaHuellaHexadecimal.Substring(765, 255);
                HuellaActual.HuellaCampo5 = HuellaActual.CadenaHuellaHexadecimal.Substring(1020);
                log.regBitacora("FingerReaderClass.DivideHuella    |   Finaliza division de la huella");
                return HuellaActual;

            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderClass.DivideHuella    |   Exeption: " + ex.Message);
                return HuellaActual;
            }

        }

        #endregion

    }
}
