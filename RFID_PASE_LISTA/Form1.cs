using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using MySql.Data.MySqlClient;

namespace RFID_PASE_LISTA
{
    public partial class Form1 : Form

    {
        string GetEntrada;
        SerialPort puertoAr;
        public Form1()
        {
            InitializeComponent();
            puertoAr = new SerialPort();
            puertoAr.PortName = "COM4";
            puertoAr.BaudRate = 9600;
            puertoAr.DtrEnable = true;

            try
            {
                // puertoAr.Open();
            }
            catch (Exception x)
            {

                MessageBox.Show("Se debe de conectar Arduino " + x);
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            txtTarjeta.Enabled = false;
            txtNombre.Enabled = false;
            txtFechaHora.Enabled = false;
            txtEmpleado.Enabled = false;

            this.CenterToParent();
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string entrada = Leer_Arduino();
            string uid = conexion(entrada);
            txtTarjeta.Text = entrada;

            if (txtTarjeta.Text == uid)
            {
                GetData(uid);
                txtFechaHora.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                MessageBox.Show("Verificado");
                registro();
                borrar();
            }
            else { MessageBox.Show("No estas en la lista"); txtTarjeta.Text = ""; }



        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {

            borrar();

            //puertoAr.Write("b");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string cadenaConexion = @"server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM asistencia", cadenaConexion);
            DataSet ds = new DataSet();
            da.Fill(ds, "usuarios");
            dataGridView1.DataSource = ds.Tables["usuarios"].DefaultView;
        }

        public string conexion(string tarjeta)
        {
            MySqlConnection con;
            string conString;
            string resultado = "";
            conString = "server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";

            try
            {
                con = new MySqlConnection();
                con.ConnectionString = conString;
                con.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = string.Format("SELECT uid FROM usuarios WHERE uid = '{0}'", tarjeta);
                MySqlDataReader leer = cmd.ExecuteReader();

                if (leer.Read())
                {
                    resultado = leer.GetString(0);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return resultado;
        }

        public void GetData(string tarjeta)
        {
            MySqlConnection con;
            string conString;
            string[] resultado = new string[3];
            conString = "server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";

            try
            {
                con = new MySqlConnection();
                con.ConnectionString = conString;
                con.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = string.Format("SELECT Nombre,No_Empleado FROM usuarios WHERE uid = '{0}'", tarjeta);

                MySqlDataReader leer = cmd.ExecuteReader();
                if (leer.Read())
                {
                    txtNombre.Text = leer.GetString(0);
                    txtEmpleado.Text = leer.GetString(1);

                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            txtNombre.Enabled = true;

            txtEmpleado.Enabled = true;
            txtFechaHora.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            Usuario_Nuevo();
        }

        public void Usuario_Nuevo()
        {

            if (txtEmpleado.Text != "" && txtFechaHora.Text != "" && txtNombre.Text != "" && txtTarjeta.Text != "")
            {
                try
                {
                    string consulta = string.Format("INSERT INTO usuarios(`uid`,`nombre`,`entrada`,`no_empleado`)VALUES('{0}','{1}','{2}','{3}');", txtTarjeta.Text, txtNombre.Text, txtFechaHora.Text, txtEmpleado.Text);
                    string cadenaConexion = @"server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";
                    MySqlDataAdapter adaptador = new MySqlDataAdapter(consulta, cadenaConexion);
                    MySqlCommandBuilder comando = new MySqlCommandBuilder(adaptador);
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);
                    MessageBox.Show("Agregado");
                    borrar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error " + ex.Message);
                }
            }
            else { MessageBox.Show("Favor de capturar datos"); }

        }

        public string Leer_Arduino()
        {
            MessageBox.Show("leyendo");
            puertoAr.Open();
            string entrada = puertoAr.ReadLine();
            puertoAr.Close();
            System.Threading.Thread.Sleep(2000);
            txtTarjeta.Text = entrada;
            return entrada;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Leer_Arduino();
        }

        public void borrar()
        {
            txtNombre.Text = "";
            txtEmpleado.Text = "";
            txtTarjeta.Text = "";
            txtFechaHora.Text = "";
        }

        public void registro()
        {
            if (txtEmpleado.Text != "" && txtFechaHora.Text != "" && txtNombre.Text != "" && txtTarjeta.Text != "")
            {
                try
                {
                    bool hay_entrada = Verificar_entrada();
                    bool hay_salida = verificar_salida();
                    MessageBox.Show(""+hay_entrada);
                    MessageBox.Show("" + hay_salida);

                    if (hay_entrada == false && hay_salida == false )
                    {
                        MessageBox.Show("No hay entrada");
                        string consulta = string.Format("INSERT INTO asistencia(`no_usuario`,`entrada`)VALUES('{0}','{1}');", txtEmpleado.Text, txtFechaHora.Text);
                        string cadenaConexion = @"server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";
                        MySqlDataAdapter adaptador = new MySqlDataAdapter(consulta, cadenaConexion);
                        MySqlCommandBuilder comando = new MySqlCommandBuilder(adaptador);
                        DataTable dt = new DataTable();
                        adaptador.Fill(dt);
                        MessageBox.Show("Agregado");
                        borrar();
                        
                    }
                    else if(hay_entrada == true && hay_salida == true)
                    {
                        MessageBox.Show("Nuevo registro");
                        string consulta = string.Format("INSERT INTO asistencia(`no_usuario`,`entrada`)VALUES('{0}','{1}');", txtEmpleado.Text, txtFechaHora.Text);
                        string cadenaConexion = @"server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";
                        MySqlDataAdapter adaptador = new MySqlDataAdapter(consulta, cadenaConexion);
                        MySqlCommandBuilder comando = new MySqlCommandBuilder(adaptador);
                        DataTable dt = new DataTable();
                        adaptador.Fill(dt);
                        MessageBox.Show("Agregado");
                        borrar();
                    }
                    else if(hay_entrada == true && hay_salida == false)
                    {
                        MessageBox.Show("hay entrada");
                        string consulta = string.Format("UPDATE asistencia SET salida = '{0}' WHERE no_usuario = '{1}' AND entrada = '{2}'",txtFechaHora.Text,txtEmpleado.Text, GetEntrada);
                        string cadenaConexion = @"server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";
                        MySqlDataAdapter adaptador = new MySqlDataAdapter(consulta, cadenaConexion);
                        MySqlCommandBuilder comando = new MySqlCommandBuilder(adaptador);
                        DataTable dt = new DataTable();
                        adaptador.Fill(dt);
                        MessageBox.Show("Agregado");
                        borrar();
                    }


                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error " + ex.Message);
                }
            }
            else { MessageBox.Show("Favor de capturar datos"); }
        }

        public bool Verificar_entrada()
        {
            bool variable = false;
            string flag = "";
            MySqlConnection con;
            string conString;
            string[] resultado = new string[3];
            conString = "server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";

            try
            {
                con = new MySqlConnection();
                con.ConnectionString = conString;
                con.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = string.Format("SELECT entrada FROM asistencia WHERE no_usuario = '{0}'", txtEmpleado.Text);
                //si no hay entrada marca entrada
                //si hay entrada marca salida
                MySqlDataReader leer = cmd.ExecuteReader();
                while (leer.Read())
                {
                    flag = leer.GetString(0);
                   
                    

                }
                GetEntrada = flag;
                if (flag == "")
                {
                    variable = false;
                }
                else
                {
                    variable = true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return variable;
        }

        public bool verificar_salida()
        {
            string flag = "";
            bool variable = false;
            MySqlConnection con;
            string conString;
            string[] resultado = new string[3];
            conString = "server=127.0.0.1; database=rfid_usuarios; User id=root; password=;";

            try
            {
                con = new MySqlConnection();
                con.ConnectionString = conString;
                con.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = string.Format("SELECT salida FROM asistencia WHERE no_usuario = '{0}'", txtEmpleado.Text);
                //si no hay entrada marca entrada
                //si hay entrada marca salida
                MySqlDataReader leer = cmd.ExecuteReader();
                while (leer.Read())
                {
                    flag = leer.GetString(0);
                    MessageBox.Show(flag);
                    


                }
                if (flag == "")
                {
                    variable = false;

                }
                else
                {
                    variable = true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return variable;
        }

        public void registro_Actual()
        {
         
        }
    }
}
