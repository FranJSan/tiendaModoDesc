using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Text.RegularExpressions;

/// <summary>
/// Tarea 2.1 -> ACCESO EN MODO DESCONECTADO
/// 
/// El programa tiene la misma funcionalidad que el anterior, pero en Modo Desconectado. 
/// Además he agregado un DataGrid y unos CheckBoxs para porder configurar qué columnas visualizar y también tiene
/// establecido un controlado para el evento doble_click igual al de los ListBox.
/// 
/// El programa comienza conectandose a la base de datos y cargando en un DataSet la tabla 'products' y 'categories',
/// que serán las que se usarán durante la ejecución. Una vez cargado el dataSet, se cierra la conexión con la base
/// de datos.
/// 
/// Por lo demás, tiene la misma funcionalidad que la primera tarea:
/// Se crea un RadioButton por categoria de Products, asociandole un evento para mostrar todos los productos de esa 
/// categoría a las ListBox. Haciendo click sobre cualquier item de las ListBox se seleccionará el resto de información 
/// de ese producto. Haciendo doble click, se pasarán los datos de ese producto a los TextBox para actualizar el registro.
/// 
/// La parte de actualización de datos comprueba la validez de los mismos antes de actualizar los datos en el dataSet.
/// 
/// Por último, durante el evento Form_Closing, se abre la conexión y se pushean los cambios a la base de datos.
/// </summary>
namespace Tienda
{
    public partial class FrmMain : Form
    {
        private IDbConnection connection;
        private IDbCommand command;

        // Variables para Modo desc        
        DataSet dataSet = new DataSet();
        OleDbDataAdapter dataAdapterCat;
        OleDbDataAdapter dataAdapterProducts;

        public FrmMain()
        {
            InitializeComponent();
            // Establezco el tamaño del Form para asegurarme de que el DataGrid queda oculto
            this.Size = new Size(1044, 316);

            LBProductId.Click += new EventHandler(LB_Click);
            LBProductName.Click += new EventHandler(LB_Click);
            LBUnitPrice.Click += new EventHandler(LB_Click);
            LBUnitStock.Click += new EventHandler(LB_Click);

            LBProductId.MouseDoubleClick += new MouseEventHandler(LB_DoubleClick);  
            LBProductName.MouseDoubleClick += new MouseEventHandler(LB_DoubleClick);
            LBUnitPrice.MouseDoubleClick += new MouseEventHandler(LB_DoubleClick);
            LBUnitStock.MouseDoubleClick += new MouseEventHandler(LB_DoubleClick);
        }

        /// <summary>
        /// Controlador load de FrmMain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Load(object sender, EventArgs e)
        {
            EstablecerConexion();
            CargarDatosCategoriesEnDataSet();            
            CargarDatosProductsEnDataSet();
            CerrarConexion();
            CrearCategoriasModoDesc();

            // CheckBoxs para el DataGrid
            CrearCB();
        }

        /// <summary>
        /// Establece la conexión con la base de datos.
        /// </summary>
        private void EstablecerConexion()
        {
            connection = new OleDbConnection();
            connection.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\\temp\\nwind.mdb";
            connection.Open();
        }

        /// <summary>
        /// Método que carga en el DataSet la tabla 'categories'
        /// </summary>
        private void CargarDatosCategoriesEnDataSet()
        {
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM categories";

            dataAdapterCat = new OleDbDataAdapter();
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapterCat);
            dataAdapterCat.SelectCommand = (OleDbCommand)command;
            dataAdapterCat.Fill(dataSet, "categories");
        }

        /// <summary>
        /// Método que carga en el DataSet la tabla 'products'
        /// </summary>
        private void CargarDatosProductsEnDataSet()
        {
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM products";

            dataAdapterProducts = new OleDbDataAdapter();
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapterProducts);
            dataAdapterProducts.SelectCommand = (OleDbCommand)command;
            dataAdapterProducts.Fill(dataSet, "products");
        }

        /// <summary>
        /// Método que cierra la conexión con la base de datos.
        /// </summary>
        private void CerrarConexion()
        {
            connection.Close();
        }

        /// <summary>
        /// Este método crea los RadioButtons necesarios de categories haciendo uso del DataSet.
        /// </summary>
        private void CrearCategoriasModoDesc()
        {
            DataTable tableCategories = dataSet.Tables["categories"];

            foreach (DataRow row in tableCategories.Rows)
            {
                CrearRadioButton((int)row["CategoryId"], (string)row["CategoryName"]);
            }
        }

        /// <summary>
        /// Método para crear los CheckBox que se usarán para configurar los campos que muestra el 
        /// DataGrid. Después de crearlos se agregan al Panel.
        /// </summary>
        private void CrearCB()
        {
            for (int i = 0; i < dataSet.Tables["products"].Columns.Count; i++)
            {
                CheckBox cb = new CheckBox();
                cb.Text = dataSet.Tables["products"].Columns[i].ColumnName;

                // Establezco por defecto los mismos campos que los ListBox como seleccionados
                if (cb.Text.Equals("ProductID") ||
                    cb.Text.Equals("ProductName") ||
                    cb.Text.Equals("UnitPrice") ||
                    cb.Text.Equals("UnitsInStock")
                    )
                {
                    cb.Checked = true;
                }
                else cb.Checked = false;

                cb.Location = new Point(0, 25 * i);
                PnlCB.Controls.Add(cb);
                cb.CheckedChanged += new EventHandler(Chaged_Cheked_CB);
            }
        }

        /// <summary>
        /// Método que muestra los datos en los ListBox y en el DataGrid, según la categoria
        /// seleccionada.
        /// </summary>
        /// <param name="sender">RabioButton fuente del evento</param>
        /// <param name="e"></param>
        private void MostrarDatosModoDesc(Object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;           

            // establezco los datos de la table que quiero visualizar
            dataSet.Tables["products"].DefaultView.RowFilter ="CategoryId=" + rb.Tag.ToString();
            DataTable table = dataSet.Tables["products"];
           
            LBProductId.DataSource = table;
            LBProductId.DisplayMember = "ProductID";
            LBProductId.ValueMember = "ProductID";
   
            LBProductName.DataSource = table;
            LBProductName.DisplayMember = "ProductName";
            LBProductName.ValueMember = "ProductID";

            LBUnitPrice.DataSource = table;
            LBUnitPrice.DisplayMember = "UnitPrice";
            LBUnitPrice.ValueMember = "ProductID";

            LBUnitStock.DataSource = table;
            LBUnitStock.DisplayMember = "UnitsInStock";
            LBUnitStock.ValueMember = "ProductID";

            // DataGrid
            DGProducts.DataSource = table;
            ConfigurarDataGrid();
        }

        /// <summary>
        /// Método para configurar los campos del DataGrid. 
        /// El método evalúa todos los CheckBox y establece la visibilidad de los 
        /// campos en el DataGrid según estén seleccionados o no.
        /// </summary>
        private void ConfigurarDataGrid()
        {
            for (int i = 0; i < PnlCB.Controls.OfType<CheckBox>().Count(); i++)
            {
                if (PnlCB.Controls.OfType<CheckBox>().ElementAt(i).Checked)
                {
                    DGProducts.Columns[PnlCB.Controls.OfType<CheckBox>().ElementAt(i).Text].Visible = true;
                }
                else DGProducts.Columns[PnlCB.Controls.OfType<CheckBox>().ElementAt(i).Text].Visible = false;
            }
        }

       
        /// <summary>
        /// Método controlador del evento ChekedChanged de los CheckBox. LLama al método 
        /// ConfigurarDataGrid(), para que settear la visualización de las columnas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chaged_Cheked_CB(object sender, EventArgs e)
        {
            // Si no se ha establecido el DataSource del DataGrid, no actualizo sus columnas.
            if (DGProducts.DataSource == null) return;
            
            ConfigurarDataGrid();
        }

        /// <summary>
        /// Crea un RadionButton con el id y la categoría que se pase como argumentos.
        /// </summary>
        /// <param name="id">id de la categoria de la base de datos. Se guarda como Tag</param>
        /// <param name="categoria">Nombre de la categoría. Valor de Text</param>
        private void CrearRadioButton(int id, string categoria)
        {        
            RadioButton rb = new RadioButton();
            rb.Text = categoria;
            rb.Top = ((rb.Height + 3) * (id - 1));
            rb.Tag = id;
            rb.CheckedChanged += new EventHandler(MostrarDatosModoDesc);

            PanelRb.Controls.Add(rb);
        }

        /// <summary>
        /// Controlador del evento doble click sobre los items de las ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Vuelca los datos de la selección en los TextBox de actualización</remarks>
        private void LB_DoubleClick(object sender, MouseEventArgs e)
        {
            LB_Click(sender, e);
            DataRowView registro = (DataRowView)LBProductId.SelectedItem;
            TBProcuctId.Text = registro["ProductID"].ToString();
            TBProductName.Text = registro["ProductName"].ToString();
            TBUnitPrice.Text = registro["UnitPrice"].ToString();
            TBUnitStock.Text = registro["UnitsInStock"].ToString();
        }

        /// <summary>
        /// Controlador del evento Click sobre los items de las ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Selecciona en todas las ListBox los datos del item seleccionado</remarks>
        private void LB_Click(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            LBProductId.SelectedIndex = lb.SelectedIndex;
            LBProductName.SelectedIndex = lb.SelectedIndex;
            LBUnitPrice.SelectedIndex = lb.SelectedIndex;
            LBUnitStock.SelectedIndex = lb.SelectedIndex;
        }

        /// <summary>
        /// Método del evento Click del botón Actualizar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Actualiza el registro correspondiente al 'ProductID' en el DataSet</remarks>
        private void ClickBtnActualizar_ModoDesc(object sender, EventArgs e)
        {
            if (!ComprobarTextBoxs()) return;

            DataRow row = dataSet.Tables["products"].Select("ProductID = " + TBProcuctId.Text)[0];
            row.BeginEdit();
            row["ProductName"] = TBProductName.Text;
            row["UnitPrice"] = TBUnitPrice.Text;
            row["UnitsInStock"] = TBUnitStock.Text;
            row.EndEdit();
        }

        /// <summary>
        /// Método que comprueba los datos de entrada de los TextBox.
        /// </summary>
        /// <returns>true si los datos de entrada son válidos,
        /// false en caso contrario.</returns>
        /// <remarks>
        /// Usa patrones Regex para comprobar la valided de los datos y avisa al usuario sobre los errores.
        /// </remarks>
        private bool ComprobarTextBoxs()
        {
            
            if (!Regex.Match(TBProcuctId.Text, "^\\d+$").Success)
            {
                MessageBox.Show("Error en el Id.");
                return false;
            }
            
            TBUnitPrice.Text = TBUnitPrice.Text.Replace(',', '.');
            
            if (!Regex.Match(TBUnitPrice.Text, "^\\d+(\\.\\d+)?$").Success && TBUnitPrice.Text.Length != 0)
            {
                MessageBox.Show("Error en Unit price.");                               
                return false;
            }

            if (!Regex.Match(TBUnitStock.Text, "^\\d+$").Success && TBUnitStock.Text.Length != 0)
            {
                MessageBox.Show("Error en Unit stock.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Método para borrar los datos y la selección de los RadioButtons, ListBoxs y DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBorrarConsultas_Click(object sender, EventArgs e)
        {
            foreach (RadioButton rb in PanelRb.Controls.OfType<RadioButton>())
            {
                rb.Checked = false;
            }

            LBProductId.DataSource = null;
            LBProductName.DataSource = null;
            LBUnitPrice.DataSource = null;
            LBUnitStock.DataSource = null;
            DGProducts.DataSource = null;
            
        }

        /// <summary>
        /// Método para borrar los datos y la selección de los TextBoxs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBorrarInputs_Click(object sender, EventArgs e)
        {
            TBProcuctId.Text = "";
            TBProductName.Text = "";
            TBUnitPrice.Text = "";
            TBUnitStock.Text = "";
        }

        /// <summary>
        /// Controlador del evento Click sobre el Label '↓ DataGridView ↓'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Establece un nuevo tamaño del Form para mostrar/ocultar en DataGrid</remarks>
        private void LblDataGrid_Click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            if (lbl.Text.Equals("↓ DataGridView ↓"))
            {
                this.Size = new Size(1044, 663);
                LblDataGrid.Text = "↑ Ocultar ↑";
            } else
            {
                this.Size = new Size(1044, 316);
                LblDataGrid.Text = "↓ DataGridView ↓";
            }            
        }

        /// <summary>
        /// Controlador del DoubleClick sobre el DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Setea los datos a los TextBox. 
        /// No he podido utilizar el controlador de los items del ListBox, porque en los items se llama al método
        /// que iguala la selección de items y en este caso no es posible</remarks>
        private void DGProducts_DoubleClick(object sender, EventArgs e)
        {
            DataRowView registro = (DataRowView)LBProductId.SelectedItem;
            TBProcuctId.Text = registro["ProductID"].ToString();
            TBProductName.Text = registro["ProductName"].ToString();
            TBUnitPrice.Text = registro["UnitPrice"].ToString();
            TBUnitStock.Text = registro["UnitsInStock"].ToString();
        }

        /// <summary>
        /// Controlado del evento Closing del Form. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Antes de cerrar la aplicación, este método abre la conexión con la base de datos,
        /// actualiza los datos que se hayan modificado del DataSet y cierra la conexión.</remarks>
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Open();
            dataAdapterProducts.Update(dataSet, "products");
            dataSet.AcceptChanges();
            CerrarConexion();
        }
    }
}
