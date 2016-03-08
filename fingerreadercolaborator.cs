using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using MsgCitrix;
using System.Threading;
using System.IO;
using System.Text;


namespace FingerColaborator
{

    #region delegados
    public delegate void FingerReaderColaboratorEventHandler_RegistroHuellasCompleto();
    public delegate void FingerReaderColaboratorEventHandler_BusquedaHuellaCompleto();
    #endregion

    public  class FingerReaderColaborator: EventArgs 
    {
        #region Eventos de la clase
        
        /// <summary>
        /// Evento que se dispara justo despues de que termina la Captura y Verificacion de las Huellas
        /// En este momento  es posible obtener la informacion las Huellas Capturadas.
        /// </summary>
        public event FingerReaderColaboratorEventHandler_RegistroHuellasCompleto RegistroHuellasCompleto;
        
        /// <summary>
        /// Evento que se dispara justo despues de que termina la busqueda de una huella
        /// En este momento  es posible obtener el numero de coincidencias encontradas.
        /// </summary>
        public event FingerReaderColaboratorEventHandler_BusquedaHuellaCompleto BusquedaHuellaCompleto;

        #endregion


        #region Variables Globales

        FingerReaderClass frc = new FingerReaderClass();
        private MensajesCitrixClass MsgCitrix = new MensajesCitrixClass();
        private Bitacora log = new Bitacora();
        private HuellaDigital[] coleccionHuellas;
        private int coincidenciasHuella;
        private int _tipoMaquina; // 1: Virtual  0: Fisica
        private string _rutaAppVirtual;
        private Resultado fingerColaboratorResult = new Resultado();
        private string _tipoEvento;
        private HuellaDigital[] dedosAcapturar;

        #endregion


        #region Constructor

        public FingerReaderColaborator()
        {
            frc.RegistraHuellasDone += new FingerReaderClassEventHandler_RegistraHuellasDone(RegistraHuellasDone);
            frc.BuscaHuellasDone += new FingerReaderClassEventHandler_BuscaHuellas(frc_BuscaHuellasDone);
            MsgCitrix.recibioMensaje += new MsgCitrix.__MensajesCitrix_recibioMensajeEventHandler(MsgCitrix_recibioMensaje);
        }

        #endregion 



        #region Estructuras
        public struct Mensaje
        {
            internal string AccionMensaje;
            internal string ContenidoMensaje;
        }

        /// <summary>
        /// 0: Coincide la Huella
        /// 1: No Coincide la Huella
        /// 2: Existe un problema con la configuracion del sensor
        /// 3: No hay sensor conectado
        /// 4: El formato de entrada de la(s) huellas es incorrecto 
        /// 5: La captura de huellas fue cancelada
        /// 6: Finalizo la  captura de huellas
        /// </summary>
        public struct Resultado
        {
            public int codigo;
            public string descripcion;
        }

        #endregion

        #region Propiedades

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

        /// <summary>
        /// 0: Coincide la Huella
        /// 1: No Coincide la Huella
        /// 2: Existe un problema con la configuracion
        /// 3: No Hay lector
        /// </summary>
        public Resultado FingerColaboratorResult
        {
            get
            {
                return fingerColaboratorResult;
            }
            set
            {
                fingerColaboratorResult = value;
            }
        }


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


        

        #region Metodos y eventos para Registrar Huellas

        /// <summary>
        /// Metodo para Capturar y Verificar huellas. Captura de 1 a 10 dedos 
        /// Devuelve la informacion de las huellas a travez  de su propropiedad "ColeccionHuellas"
        /// </summary>
        public void RegistraHuellas()
        {
            log.regBitacora("FingerReaderColaborator.RegistraHuellas    |   Inicia Registro de Huellas");
            try
            {               
                _tipoEvento = "captura";
                _tipoMaquina = GetTipoMaquina();
                if (_tipoMaquina == 1) ConfiguraAplicacionVirtual();
                switch (_tipoMaquina)
                {
                    //Fisica
                    case 0:
                        log.regBitacora("FingerReaderColaborator.RegistraHuellas    |   Tipo Maquina: Es Fisica");
                        frc.DedosAcapturar = dedosAcapturar;
                        frc.RegistraHuellas();
                        break;

                    //Virtual
                    case 1:
                        log.regBitacora("FingerReaderColaborator.RegistraHuellas    |   Tipo Maquina: Es Virtual");
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex) 
            {
                log.regBitacora("FingerReaderColaborator.RegistraHuellas    |   Exeption: "+ex.Message);
            }
        }


        /// <summary>
        /// Evento que se dispara justo despues de que termina la Captura y Verificacion de las Huellas
        /// En este momento  es posible obtener la informacion de las Huellas Capturadas.
        /// </summary>
        private void RegistraHuellasDone()
        {
            log.regBitacora("FingerReaderColaborator.RegistraHuellasDone    |   Termina Registro de Huellas");
            try
            {
                ColeccionHuellas = frc.ColeccionHuellas;
                fingerColaboratorResult.codigo = frc.FingerReaderResult.codigo;
                fingerColaboratorResult.descripcion = frc.FingerReaderResult.descripcion;
                RegistroHuellasCompleto();
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.RegistraHuellasDone    |   Exeption: " + ex.Message);
            }
        }

        #endregion 


        #region  Metodos y eventos para Buscar Huellas

        /// <summary>
        /// Metodo para buscar Huellas, toma una huella con el sensor y las compara contra la coleccion de Huellas recibida.
        ///  Devuelve las coincidencias de la huella a travez  de su piedad "CoincidenciasHuella"
        /// </summary>
        /// <param name="ColHuellas">Coleccion de objetos de la clase "HuellaDigital". 
        /// Los campos de las huellas no pueden estar vacios</param>
        public void BuscaHuellas(HuellaDigital[] ColHuellas) 
        {

            coleccionHuellas = ColHuellas;

            log.regBitacora("FingerReaderColaborator.BuscaHuellas    |   **** " + ColHuellas.Length + " Huellas Recibidas****");

            //for (int x = 0; x < ColHuellas.Length; x++) 
            //{
            //    log.regBitacora("FingerReaderColaborator.BuscaHuellas    |   HUELLA: " + x);
            //    log.regBitacora("FingerReaderColaborator.BuscaHuellas    |      MANO: " + ColHuellas[x].NumeroMano.ToString());
            //    log.regBitacora("FingerReaderColaborator.BuscaHuellas    |      DEDO: " + ColHuellas[x].NumeroDedo.ToString());
            //    log.regBitacora("FingerReaderColaborator.BuscaHuellas    |      CADENA: " + ColHuellas[x].CadenaHuellaHexadecimal);
            //}


             log.regBitacora("FingerReaderColaborator.BuscaHuellas    |   Inicia Busqueda de Huellas");
           
             try
             {
                 _tipoEvento = "busqueda";         
                 _tipoMaquina = GetTipoMaquina();
                 if (_tipoMaquina == 1) ConfiguraAplicacionVirtual();                
                 switch (_tipoMaquina)
                 {
                     //Fisica
                     case 0:
                         log.regBitacora("FingerReaderColaborator.BuscaHuellas    |   Tipo Maquina: Es Fisica" );                      
                         frc.BuscaHuellas(ColHuellas);
                         break;

                     //Virtual
                     case 1:
                         log.regBitacora("FingerReaderColaborator.BuscaHuellas    |   Tipo Maquina: Es Virtual");
                         break;

                     default:
                         break;
                 }
             }
             catch (Exception ex)
             {
                 log.regBitacora("FingerReaderColaborator.BuscaHuellas    |   Exeption: " + ex.Message);
             }

        }


        /// <summary>
        /// Evento que se dispara justo despues de que termina la busqueda de una huella
        /// En este momento  es posible obtener el numero de coincidencias encontradas.
        /// </summary>
        private void frc_BuscaHuellasDone()
        {
            log.regBitacora("FingerReaderColaborator.frc_BuscaHuellasDone    |   Termina Busqueda de Huellas");
            try
            {
                CoincidenciasHuella = frc.CoincidenciasHuella;
                fingerColaboratorResult.codigo = frc.FingerReaderResult.codigo;
                fingerColaboratorResult.descripcion = frc.FingerReaderResult.descripcion;
                BusquedaHuellaCompleto();
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.frc_BuscaHuellasDone    |   Exeption: " + ex.Message);
            }
        }

        #endregion




        #region Gestion de Mensajes Citrix

        /// <summary>
        /// Metodo que gestiona la comunicacion con Citrix y las acciones a realizar.
        /// </summary>
        /// <param name="pstrMensaje">Es la</param>
        private void MsgCitrix_recibioMensaje(ref string pstrMensaje)
        {
            
            try
            {
                // pstrMensaje es la cadena que recibe
                log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   CITRIX -- Recibio Mensaje de FingerCollVirtual.exe");
              //  log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   pstrMensaje: " + pstrMensaje);
                //Con un split separo la Accion de los Parametros del mensaje
                string[] msjCitrix = pstrMensaje.Split(new Char[] { '|' });
                Mensaje mensaje = new Mensaje();
                mensaje.AccionMensaje = msjCitrix[0];
                mensaje.ContenidoMensaje = msjCitrix[1];            
                log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   AccionMensaje: " + mensaje.AccionMensaje);
                //log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   ContenidoMensaje: " + mensaje.ContenidoMensaje);

                // hago un flujo para cada accion
                switch (mensaje.AccionMensaje)
                {
                    case "FinalizaCapturaHuellas":  
                        //ahora recupero los parametros con otro split
                        string[] parametrosCaptura = mensaje.ContenidoMensaje.Split(new Char[] { '/' });
                        ColeccionHuellas = StringToObjHuellas(parametrosCaptura[0]);
                        fingerColaboratorResult.codigo = int.Parse(parametrosCaptura[1]);
                        fingerColaboratorResult.descripcion = parametrosCaptura[2];
                        RegistroHuellasCompleto();
                        break;

                    case "ResultadoVerificacion":
                        //ahora recupero los parametros con otro split
                        string[] parametrosVerificacion = mensaje.ContenidoMensaje.Split(new Char[] { '/' });
                        coincidenciasHuella = int.Parse(parametrosVerificacion[0]);
                        fingerColaboratorResult.codigo = int.Parse(parametrosVerificacion[1]);
                        fingerColaboratorResult.descripcion = parametrosVerificacion[2];
                        BusquedaHuellaCompleto();
                        break;

                    case "CancelaCapturaHuellas":
                        //ahora recupero los parametros con otro split
                        string[] parametrosCancelacion = mensaje.ContenidoMensaje.Split(new Char[] { '/' });
                        ColeccionHuellas = null;
                        fingerColaboratorResult.codigo = int.Parse(parametrosCancelacion[0]);
                        fingerColaboratorResult.descripcion = parametrosCancelacion[1];
                        RegistroHuellasCompleto();
                        break;

                    case "CargaFingerCollCompleta":

                        MsgCitrix.set_EnviarA("FingerCollVirtual");

                        switch  (_tipoEvento)
                        {
                            case "busqueda":                            
                                MsgCitrix.EnviaMensaje("IniciaVerificacionHuella|" + ObjHuellasToString(coleccionHuellas));
                                log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   Manda Mensaje a FingerCollVirtual.exe -> IniciaVerificacionHuella|" + "ObjHuellasToString(coleccionHuellas)");
                               // log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   Manda Mensaje a FingerCollVirtual.exe -> IniciaVerificacionHuella|" + ObjHuellasToString(coleccionHuellas));
                                break;
                            case "captura":

                                //JMDJM 04/03/2014
                                if (dedosAcapturar == null)
                                {
                                    MsgCitrix.EnviaMensaje("IniciaCapturaHuella|");
                                    log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   Manda Mensaje a FingerCollVirtual.exe -> IniciaCapturaHuella|");
                                }
                                else 
                                { 
                                    string strDedosAcapturar =  ObtieneDedosAcapturar(dedosAcapturar);
                                    MsgCitrix.EnviaMensaje("IniciaCapturaHuellaDefinidas|" + strDedosAcapturar);
                                    log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   Manda Mensaje a FingerCollVirtual.exe -> IniciaCapturaHuellaDefinidas|");
                                }
                                break;
                            default:
                                break;
                        }
                        break;

                    case "StatusSensor":
                        //ahora recupero los parametros con otro split
                        string[] parametrosSensor = mensaje.ContenidoMensaje.Split(new Char[] { '/' });
                        ColeccionHuellas = null;
                        coincidenciasHuella = 0;
                        fingerColaboratorResult.codigo = int.Parse(parametrosSensor[0]);
                        fingerColaboratorResult.descripcion = parametrosSensor[1];
                        RegistroHuellasCompleto();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.MsgCitrix_recibioMensaje    |   Exeption: " + ex.Message);
            }
        }

        #endregion 


        


        #region Metodos Generales

        /// <summary>
        /// Obtiene el tipo de maquina en la cual se ejecuta la aplicacion
        /// </summary>
        /// <returns>entero 1:Virtual  0:Fisica</returns>
        private int GetTipoMaquina()
        {
            int tipoWs = 0; // 1: Virtual  0: Fisica
            try
            {   
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ctx\", false);
                if (key != null)
                {
                    string value = key.GetValue("Virtual").ToString();
                    key.Close();
                    if (value == "1")
                    {
                        tipoWs = 1;
                    }
                }
                return tipoWs;
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.GetTipoMaquina    |   Exeption: " + ex.Message);
                return tipoWs;
            }

        }

        /// <summary>
        /// Activa la aplicacion remota en la maquina fisica 
        /// </summary>
        private void ConfiguraAplicacionVirtual()
        {
            try
            {
                
                _rutaAppVirtual = "APP:C::\\ADN\\fingercollvirtual.exe:0:1";
                MsgCitrix.set_Nombre("FingerColaborator");
                MsgCitrix.set_EnviarA("StatusCitrix");
                MsgCitrix.EnviaMensaje(_rutaAppVirtual);
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.ConfiguraAplicacion    |   Exeption: " + ex.Message);
            }

        }


        /// <summary>
        /// Recibe una coleccion de Huellas y las pasa a una cadena de strings
        /// </summary>
        /// <param name="colHuellas"></param>
        /// <returns>Arma la cadena --> Mano,Dedo,HuellaCampo1,HuellaCampo2,HuellaCampo3,HuellaCampo4,HuellaCampo5;...</returns>
        private string ObjHuellasToString(HuellaDigital[] colHuellas)
        {
            string CadenaHuellas = "";
            try
            {
                log.regBitacora("FingerReaderColaborator.ObjHuellasToString    |   Inicia conversion de coleccion de Huellas a strings");
                log.regBitacora("FingerReaderColaborator.ObjHuellasToString    |   Huellas a procesar: "+ colHuellas.Length.ToString());
                for (int j = 0; j < colHuellas.Length; j++)
                {
                   // log.regBitacora("FingerReaderColaborator.ObjHuellasToString    |   Huella: " + j);
                    if(colHuellas[j].CadenaHuellaHexadecimal != null && colHuellas[j].CadenaHuellaHexadecimal !="")
                    {
                        
                        colHuellas[j].CadenaHuellaHexadecimal = colHuellas[j].CadenaHuellaHexadecimal.Trim();
                      //  log.regBitacora("FingerReaderColaborator.ObjHuellasToString    |   CadenaHuellaHexadecimal: " + colHuellas[j].CadenaHuellaHexadecimal);
                        colHuellas[j] = DivideHuella(colHuellas[j]);

                        //Arma la cadena --> Mano,Dedo,HuellaCampo1,HuellaCampo2,HuellaCampo3,HuellaCampo4,HuellaCampo5;...
                        CadenaHuellas += colHuellas[j].NumeroMano.ToString() + ",";
                        CadenaHuellas += colHuellas[j].NumeroDedo.ToString() + ",";
                        CadenaHuellas += colHuellas[j].HuellaCampo1 + ",";
                        CadenaHuellas += colHuellas[j].HuellaCampo2 + ",";
                        CadenaHuellas += colHuellas[j].HuellaCampo3 + ",";
                        CadenaHuellas += colHuellas[j].HuellaCampo4 + ",";
                        CadenaHuellas += colHuellas[j].HuellaCampo5 + (j + 1 < colHuellas.Length ? ";" : "");
                    }

                }


                return CadenaHuellas;
            }
            catch(Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.ObjHuellasToString    |   Exeption: " + ex.Message);
                return CadenaHuellas;
            }
        }

        /// <summary>
        /// Recibe una cadena de strings y las convierte en una coleccion de objetos de tipo Huella digital
        /// </summary>
        /// <param name="cadenaHuellas">string con un formato especifico: 
        /// Recibe la cadena --> Mano,Dedo,HuellaCampo1,HuellaCampo2,HuellaCampo3,HuellaCampo4,HuellaCampo5;...</param>
        /// <returns></returns>
        private HuellaDigital[] StringToObjHuellas(string cadenaHuellas)
        {
            try
            {
                string[] HuellasCapturadas = cadenaHuellas.Split(new Char[] { ';' });
                HuellaDigital[] objHuellas = new HuellaDigital[HuellasCapturadas.Length];
                int indice = 0;
                foreach (var huella in HuellasCapturadas)
                {
                    string[] CamposHuella = huella.Split(new Char[] { ',' });
                    HuellaDigital HuellaTemp = new HuellaDigital();
                    HuellaTemp.NumeroMano = int.Parse(CamposHuella[0]);
                    HuellaTemp.NumeroDedo = int.Parse(CamposHuella[1]);
                    HuellaTemp.HuellaCampo1 = CamposHuella[2];
                    HuellaTemp.HuellaCampo2 = CamposHuella[3];
                    HuellaTemp.HuellaCampo3 = CamposHuella[4];
                    HuellaTemp.HuellaCampo4 = CamposHuella[5];
                    HuellaTemp.HuellaCampo5 = CamposHuella[6];
                    HuellaTemp.CadenaHuellaHexadecimal = HuellaTemp.HuellaCampo1 +
                                                         HuellaTemp.HuellaCampo2 +
                                                         HuellaTemp.HuellaCampo3 +
                                                         HuellaTemp.HuellaCampo4 +
                                                         HuellaTemp.HuellaCampo5;
                    objHuellas[indice] = HuellaTemp;
                    indice++;

                }
                return objHuellas;
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.StringToObjHuellas    |   Exeption: " + ex.Message);
                HuellaDigital[] _huella = new HuellaDigital[1]; _huella = null;
                return _huella;
            }

        }

        private HuellaDigital DivideHuella(HuellaDigital HuellaActual)
        {
            try
            {
                log.regBitacora("FingerReaderColaborator.DivideHuella    |   Inicia division de la huella");
                int longitudHuella = HuellaActual.CadenaHuellaHexadecimal.Length;
                HuellaActual.HuellaCampo1 = HuellaActual.CadenaHuellaHexadecimal.Substring(0, 255);
                HuellaActual.HuellaCampo2 = HuellaActual.CadenaHuellaHexadecimal.Substring(255, 255);
                HuellaActual.HuellaCampo3 = HuellaActual.CadenaHuellaHexadecimal.Substring(510, 255);
                HuellaActual.HuellaCampo4 = HuellaActual.CadenaHuellaHexadecimal.Substring(765, 255);
                HuellaActual.HuellaCampo5 = HuellaActual.CadenaHuellaHexadecimal.Substring(1020);
                log.regBitacora("FingerReaderColaborator.DivideHuella    |   Finaliza division de la huella");
                return HuellaActual;
                    
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.DivideHuella    |   Exeption: " + ex.Message);
                return HuellaActual;
            }

        }

        /// <summary>
        /// Recibe una coleccion de Huellas que solo tiene la informacion de los dedos que se van a capturar y las pasa a una cadena de strings
        /// </summary>
        /// <param name="DedosAcapturar"></param>
        /// <returns></returns>
        private string ObtieneDedosAcapturar(HuellaDigital[] ColHuellasAcapturar) 
        {
            string strHuellasAcapturar = "";
            try
            {
                log.regBitacora("FingerReaderColaborator.ObtieneDedosAcapturar    |   Inicia conversion de coleccion de Huellas a strings");
                log.regBitacora("FingerReaderColaborator.ObtieneDedosAcapturar    |   Huellas a procesar: " + ColHuellasAcapturar.Length.ToString());
                for (int j = 0; j < ColHuellasAcapturar.Length; j++)
                {
                    //Arma la cadena --> Mano,Dedo;Mano,Dedo;Mano,Dedo...
                    strHuellasAcapturar += ColHuellasAcapturar[j].NumeroMano.ToString() + ",";
                    strHuellasAcapturar += ColHuellasAcapturar[j].NumeroDedo.ToString() + (j + 1 < ColHuellasAcapturar.Length ? ";" : "");
                }


                return strHuellasAcapturar;
            }
            catch (Exception ex)
            {
                log.regBitacora("FingerReaderColaborator.ObtieneDedosAcapturar    |   Exeption: " + ex.Message);
                return strHuellasAcapturar;
            }
        }



        #endregion



    }
}
