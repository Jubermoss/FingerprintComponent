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
    public partial class frmDummyMsg : Form
    {
        public frmDummyMsg()
        {
            InitializeComponent();
        }

        private string rutaApp;
        private string mensaje;

        public string RutaApp
        {
            get
            {
                return rutaApp;
            }
            set
            {
                rutaApp = value;
            }
        }

        public string Mensaje
        {
            get
            {
                return mensaje;
            }
            set
            {
                mensaje = value;
            }
        }




        private void frmDummyMsg_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = "Mensaje: "+ rutaApp;
            richTextBox1.Text += "\n";
            richTextBox1.Text += "Mensaje: " + mensaje;

        }
    }
}
