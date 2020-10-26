using Bunifu.Framework.UI;
using MapWinGIS;
using System;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using AxMapWinGIS;
using System.Linq;
using System.Threading;

namespace GIS
{
    public partial class Form1 : Form
    {
        private int _mDrawingHandle = -1;
        int providerid;
        TileProviders providers;
        Shapefile _shapefile;
        int _idshape;
        public string selected_point = "Resource\\Icon\\red-point.png";
        // the handle of the layer with markers
        private int m_layerHandle = -1;
        private ContextMenuStrip menuMap = new ContextMenuStrip();
        public Dictionary<string, string> list_point = new Dictionary<string, string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public string[] _ListPointPath = {"Resource\\Icon\\red-point.png", "Resource\\Icon\\Hospital.png", "Resource\\Icon\\BusStation.png", "Resource\\Icon\\Building.png", "Resource\\Icon\\restaurant.png", "Resource\\Icon\\Park.png",
        "Resource\\Icon\\Black_Flag.png","Resource\\Icon\\Red_Flag.png"
        };
        private void Form1_Load(object sender, EventArgs e)
        {
            // set Vietnam Area
            MainMap.CurrentZoom = 7; // set current zoom 
            MainMap.Latitude = (float)21.074;
            MainMap.Longitude = (float)105.848;
            bunifuFlatButton4.Visible = false;
            bunifuFlatButton5.Visible = false;
            bunifuFlatButton6.Visible = false;
            cmb_typepoint.Text = "Red point";

            //menu map
            menuMap.Items.Add("Open file").Name = "openshapefile";
            menuMap.Items.Add("Save as").Name = "saveas";
            menuMap.ItemClicked += MenuMap_ItemClicked;
            //add and load dictionary
            for (int i = 0; i < cmb_typepoint.Items.Count; i++)
            {
                list_point.Add(cmb_typepoint.Items[i].ToString(), _ListPointPath[i]);
            }
        }

        private void MenuMap_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "openshapefile")
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Open Shape File";
                dialog.Filter = "SHP files|*.shp";
                dialog.Multiselect = true;
                try
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        foreach (var item in dialog.FileNames)
                        {
                            FileInfo fi = new FileInfo(item);
                            Myfile.Add(fi.Name, item);
                            CreateNewLayer(fi.Name);
                        }
                        //pathToFile = dialog.FileName;
                        //FileInfo fi = new FileInfo(pathToFile);
                        //Myfile.Add(fi.Name, pathToFile);
                        //CreateNewLayer(fi.Name);
                    }
                    checkedListBox1.Visible = true;
                    bunifuFlatButton5.Visible = true;
                    bunifuFlatButton4.Visible = true;
                    bunifuFlatButton6.Visible = true;
                }
                catch
                {
                    MessageBox.Show("Layer đã tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if (e.ClickedItem.Name == "saveas")
            {
                menuMap.Hide();
                SaveFileDialog sf = new SaveFileDialog();
                // Feed the dummy name to the save dialog

                try
                {
                    sf.Filter = "SHP files|*.shp";
                    if (sf.ShowDialog() == DialogResult.OK)
                    {

                        // Now here's our save folder
                        Shapefile shapefile = MainMap.get_Shapefile(m_layerHandle);

                        //string savePath = Path.GetDirectoryName(sf.FileName);
                        // Do whatever
                        shapefile.SaveAs(sf.FileName, null);
                        MessageBox.Show("Create new shapefile successfully" + Environment.NewLine + "File name:" + sf.FileName, "System", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Error 404 Not Found!");
                }
            }
        }

        int count = 0;
        private void bunifuFlatButton3_Click(object sender, EventArgs e)
        {
            count = 0;
            if (count == 0)
            {
                this.bunifuFlatButton3.Normalcolor = System.Drawing.Color.Red;
                this.btn_select_area.Normalcolor = System.Drawing.Color.Black;
                this.btn_Mesure_Area.Normalcolor = System.Drawing.Color.Black;
                this.bunifuFlatButton7.Normalcolor = System.Drawing.Color.Black;
                MainMap.CursorMode = MapWinGIS.tkCursorMode.cmPan;
                count = 1;
            }
           else if(count == 1)
            {
                this.bunifuFlatButton3.Normalcolor = System.Drawing.Color.Black;
                MainMap.CursorMode = MapWinGIS.tkCursorMode.cmZoomIn;
                count = 0;
            }
            
        }
        private void MenuLayer_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        private void lvDevice_MouseClick(object sender, EventArgs e)
        {
            
        }
        private void btn_Click(object sender, EventArgs e)
        {
            MainMap.CurrentZoom = 7; // set current zoom 
            MainMap.Latitude = (float)21.074;
            MainMap.Longitude = (float)105.848;
            FrmCheckNet frm = new FrmCheckNet();
            frm.Show();
            if (CheckForInternetConnection() == false)
            {
                MessageBox.Show("Không có kết nối internet, vui lòng kiểm tra lại", "Hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                frm.Close();
                return;
            }
            else
            {
                frm.Close();
                MainMap.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
                MainMap.TileProvider = MapWinGIS.tkTileProvider.OpenStreetMap;
            }
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            try
            {
                providers = MainMap.Tiles.Providers;
                providerid = (int)tkTileProvider.ProviderCustom + 101;
                providers.Add(providerid, "mapmaker", "http://127.0.0.1/mapmaker/z{zoom}/{y}/{x}.png", tkTileProjection.SphericalMercator, 0, 14);
                MainMap.Tiles.ProviderId = providerid;
            }
            catch
            {
                MessageBox.Show("The server was not found");
                return;
            }
        }

        private void btn_select_area_Click(object sender, EventArgs e)
        {
            count = 0;
            if (count == 0)
            {
                this.bunifuFlatButton7.Normalcolor = System.Drawing.Color.Black;
                this.btn_select_area.Normalcolor = System.Drawing.Color.Red;
                this.bunifuFlatButton3.Normalcolor = System.Drawing.Color.Black;
               
                if(btn_Mesure_Area.Normalcolor == System.Drawing.Color.Red)
                {
                    MainMap.Measuring.MeasuringType = tkMeasuringType.MeasureArea;
                }
                else
                {
                    MainMap.Measuring.MeasuringType = tkMeasuringType.MeasureDistance;
                }
                MainMap.CursorMode = MapWinGIS.tkCursorMode.cmMeasure;
                count = 1;
            }
            else if (count == 1)
            {
                this.btn_select_area.Normalcolor = System.Drawing.Color.Black;
                MainMap.CursorMode = MapWinGIS.tkCursorMode.cmZoomIn;
                count = 0;
            }

        }
        public void ResetButton(BunifuFlatButton button)
        {
            button.Normalcolor = System.Drawing.Color.Black;
            MainMap.CursorMode = MapWinGIS.tkCursorMode.cmZoomIn;
            count = 0;

        }
        int count2 = 0;
        private void btn_Mesure_Area_Click(object sender, EventArgs e)
        {
            
            if (count2 == 0)
            {
                this.bunifuFlatButton7.Normalcolor = System.Drawing.Color.Black;
                this.btn_Mesure_Area.Normalcolor = System.Drawing.Color.Red;
                this.bunifuFlatButton3.Normalcolor = System.Drawing.Color.Black;
                MainMap.Measuring.MeasuringType = tkMeasuringType.MeasureArea;
                count2 = 1;
            }
            else if (count2 == 1)
            {
                this.btn_Mesure_Area.Normalcolor = System.Drawing.Color.Black;
                MainMap.CursorMode = MapWinGIS.tkCursorMode.cmZoomIn;
                count2 = 0;
            }
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {
            try
            {
                providers = MainMap.Tiles.Providers;
                providerid = (int)tkTileProvider.ProviderCustom + 102;
                providers.Add(providerid, "GoogleTransit", "http://127.0.0.1/GoogleTransit/z{zoom}/{y}/{x}.png", tkTileProjection.SphericalMercator, 0, 20);
                MainMap.Tiles.ProviderId = providerid;
            }
            catch
            {
                MessageBox.Show("The server was not found");
                return;
            }
        }

        public void MarkPoints(AxMap axMap1, string dataPath)
        {
            //axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            string filename = @"C:\Users\Admin\source\repos\GIS\GIS\bin\Debug\Shapefiles_Data\Hanoi\planet_105.141,20.676_106.164,21.273.osm.shp\shape\buildings.shp";
            if (!File.Exists(filename))
            {
                MessageBox.Show("Couldn't file the file: " + filename);
                return;
            }
            var sf = new Shapefile();
            //sf.Open(filename, null);
            //m_layerHandle = axMap1.AddLayer(sf, true);
            sf = axMap1.get_Shapefile(m_layerHandle);     // in case a copy of shapefile was created by GlobalSettings.ReprojectLayersOnAdding
            sf = new Shapefile();
            if (!sf.CreateNewWithShapeID("", ShpfileType.SHP_POINT))
            {
                MessageBox.Show("Failed to create shapefile: " + sf.ErrorMsg[sf.LastErrorCode]);
                return;
            }
            m_layerHandle = axMap1.AddLayer(sf, true);
            ShapeDrawingOptions options = sf.DefaultDrawingOptions;
            options.PointType = tkPointSymbolType.ptSymbolPicture;
            options.Picture = this.OpenMarker(dataPath);
            sf.CollisionMode = tkCollisionMode.AllowCollisions;
            axMap1.SendMouseDown = true;
            axMap1.CursorMode = tkCursorMode.cmNone;
            axMap1.MouseDownEvent += MainMap_MouseDownEvent;   // change MapEvents to axMap1
            
           
        }

        private MapWinGIS.Image OpenMarker(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("Can't find the file: " + path);
            }
            else
            {
                MapWinGIS.Image img = new MapWinGIS.Image();
                if (!img.Open(path, ImageType.USE_FILE_EXTENSION, true, null))
                {
                    MessageBox.Show(img.ErrorMsg[img.LastErrorCode]);
                    img.Close();
                }
                else
                    return img;
            }
            return null;
        }

        private void MainMap_MouseDownEvent(object sender, AxMapWinGIS._DMapEvents_MouseDownEvent e)
        {
            if (e.button == 1)          // left button
            {
                Shapefile sf = MainMap.get_Shapefile(m_layerHandle);
                Shape shp = new Shape();
                shp.Create(ShpfileType.SHP_POINT);
                MapWinGIS.Point pnt = new MapWinGIS.Point();
                double x = 0.0;
                double y = 0.0;
                MainMap.PixelToProj(e.x, e.y, ref x, ref y);
                pnt.x = x;
                pnt.y = y;
                int index = shp.numPoints;
                shp.InsertPoint(pnt, ref index);
                index = sf.NumShapes;
                if (!sf.EditInsertShape(shp, ref index))
                {
                    MessageBox.Show("Failed to insert shape: " + sf.ErrorMsg[sf.LastErrorCode]);
                    return;
                }
                MainMap.Redraw();
            }

        }
        List<string> ListName = new List<string>();
        Dictionary<string, string> Myfile = new Dictionary<string, string>();
        private void bunifuFlatButton2_Click_1(object sender, EventArgs e)
        {
            
        }
        public void CreateNewLayer(string name)
        {
            checkedListBox1.Items.Add(name);
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            
        }

        private void bunifuFlatButton4_Click(object sender, EventArgs e)
        {
            MainMap.RemoveAllLayers();
            MainMap.Projection = tkMapProjection.PROJECTION_NONE;
            foreach (var item in checkedListBox1.CheckedItems)
            {

                //string filename = item.ToString();
                string filename = Myfile[item.ToString()];
                _l.Add(filename);
                if (filename == "")
                {
                    MessageBox.Show("Vui lòng chọn layer");
                    return;
                }
                int layerHandle = MainMap.AddLayerFromFilename(filename, tkFileOpenStrategy.fosAutoDetect, true);
                if (layerHandle == -1)
                {
                    MessageBox.Show("Failed to open datasource: " + MainMap.FileManager.get_ErrorMsg(MainMap.FileManager.LastErrorCode));
                }
            }
           // MainMap.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;

        }

        private void bunifuFlatButton5_Click(object sender, EventArgs e)
        {
            MainMap.RemoveAllLayers();
            for (int i = 0; i < _l.Count; i++)
            {
                _l.RemoveAt(i);
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnLanscape_DoubleClick(object sender, EventArgs e)
        {

        }

        private void bunifuFlatButton6_Click(object sender, EventArgs e)
        {
            List<string> _list = new List<string>();
            try
            {
                this.Invoke(new Action(() =>
                {
                    foreach (var item in checkedListBox1.CheckedItems)
                    {

                        _list.Add(item.ToString());
    
                       
                    }
                    foreach (var item in _list)
                    {
                        checkedListBox1.Items.Remove(item);
                        Myfile.Remove(item);
                    }
                    MainMap.RemoveAllLayers();
                }));
            }
            catch
            {

            }
            
            }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        int on_layer = 0;
        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open Shape File";
            dialog.Filter = "SHP files|*.shp";
            dialog.Multiselect = true;
            try
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var item in dialog.FileNames)
                    {
                        FileInfo fi = new FileInfo(item);
                        Myfile.Add(fi.Name, item);
                        CreateNewLayer(fi.Name);
                    }
                    //pathToFile = dialog.FileName;
                    //FileInfo fi = new FileInfo(pathToFile);
                    //Myfile.Add(fi.Name, pathToFile);
                    //CreateNewLayer(fi.Name);
                }
                checkedListBox1.Visible = true;
                bunifuFlatButton5.Visible = true;
                bunifuFlatButton4.Visible = true;
                bunifuFlatButton6.Visible = true;
            }
            catch
            {
                MessageBox.Show("Layer đã tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        int count3 = 0;

        private void bunifuFlatButton7_Click(object sender, EventArgs e)
        {
            if (count3 == 0)
            {
                this.btn_select_area.Normalcolor = System.Drawing.Color.Black;
                this.bunifuFlatButton3.Normalcolor = System.Drawing.Color.Black;
                this.btn_Mesure_Area.Normalcolor = System.Drawing.Color.Black;
                this.bunifuFlatButton7.Normalcolor = System.Drawing.Color.Red;
                //CreatePointShapefile(MainMap);
                MarkPoints(MainMap, selected_point);
                count3 = 1;
            }
            else
            {
                this.bunifuFlatButton7.Normalcolor = System.Drawing.Color.Black;
                Shapefile sf = MainMap.get_Shapefile(m_layerHandle);
                sf.DefaultDrawingOptions.Visible = false;   // hide all the unclassified points
                MainMap.CursorMode = MapWinGIS.tkCursorMode.cmPan;
                count3 = 0;
            }

        }
       

        private void cmb_typepoint_SelectedIndexChanged(object sender, EventArgs e)
        {
            //selected_point = list_point[cmb_typepoint.SelectedItem.ToString()];
           // MessageBox.Show(cmb_typepoint.SelectedItem.ToString());
        }

        private void cmb_typepoint_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                selected_point = list_point[cmb_typepoint.SelectedItem.ToString()];
                this.bunifuFlatButton7.Normalcolor = System.Drawing.Color.Red;
                MarkPoints(MainMap, selected_point);
                count3 = 1;

            }
            catch
            {

            }
        }

        int count4 = 0;
        private void btn_remove_Click(object sender, EventArgs e)
        {
            if (count4 == 0)
            {

                this.btn_remove.Normalcolor = System.Drawing.Color.Red;
                count4 = 1;
                ToolTip(MainMap);
                MainMap.CurrentZoom = 14; // set current zoom 
                MainMap.Latitude = (float)21.074;
                MainMap.Longitude = (float)105.848;
            }
            else
            {
                count4 = 0;
                this.btn_remove.Normalcolor = System.Drawing.Color.Black;
                TunOffLb(MainMap);
            }
           
        }
        List<string> _l = new List<string>();

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }
        public void GeoProjection(AxMap axMap1)
        {
            GeoProjection proj = new GeoProjection();

            // EPSG code
            proj.ImportFromEPSG(4326);  // WGS84

            // proj 4 string
            proj.ImportFromProj4("+proj=longlat +datum=WGS84 +no_defs");  // WGS84
            // autodetect the format
            string unknown_format = "4326";
            proj.ImportFromAutoDetect(unknown_format);
            // from file
            string filename = "some_name";
            proj.ReadFromFile(filename);
            // show the name of the loaded projection
            MessageBox.Show("Projection loaded: " + proj.Name);
            // show proj 4 representation
            MessageBox.Show("Proj4 representation: " + proj.ExportToProj4());
            // let's show the properties of the geographic projection
            string s = "";
            double[] arr = new double[5];
            for (int i = 0; i < 5; i++)
            {
                // extract the parameter in element of val arr
                proj.get_GeogCSParam((tkGeogCSParameter)i, ref arr[i]);

                // append the name of parameter and the value to the string
                s += (tkGeogCSParameter)i + ": " + arr[i] + Environment.NewLine;
            }
            MessageBox.Show("Parameters of geographic coordinate system: " + Environment.NewLine + s);
            MessageBox.Show("Author:" + Environment.NewLine + "Dinh Quoc Tuan" + Environment.NewLine + "Nguyen Duc Kien" + Environment.NewLine + "Ngo Trung Kien" + Environment.NewLine + "Pham Hoang Duong");
        }
        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {

            
        }
        public void CreateSf(AxMap axMap)
        {
            Shapefile sf = new Shapefile();

        }
        // <summary>
        // Opens a shapefile, registers event handler
        // </summary>
        public void ToolTip(AxMap axMap1)
        {
            axMap1.Projection = tkMapProjection.PROJECTION_NONE;
            foreach (var filename in _l)
            {
                if (!File.Exists(filename))
                {
                    MessageBox.Show("Couldn't file the file: " + filename);
                    return;
                }
                Shapefile sf = new Shapefile();
                sf.Open(filename, null);
                if (!sf.StartEditingShapes(true, null))
                {
                    MessageBox.Show("Failed to start edit mode: " + sf.Table.ErrorMsg[sf.LastErrorCode]);
                }
                else
                {
                    sf.UseQTree = true;
                    sf.Labels.Generate("[Name]", tkLabelPositioning.lpCentroid, false);
                    axMap1.AddLayer(sf, true);
                    axMap1.SendMouseMove = true;
                    axMap1.ShowRedrawTime = true;
                    axMap1.MapUnits = tkUnitsOfMeasure.umMeters;
                    axMap1.CurrentScale = 50000;
                    axMap1.CursorMode = tkCursorMode.cmNone;
                    axMap1.MouseMoveEvent += AxMap1MouseMoveEvent;  // change MapEvents to axMap1
                    _mDrawingHandle = axMap1.NewDrawing(tkDrawReferenceList.dlScreenReferencedList);
                    Labels labels = axMap1.get_DrawingLabels(_mDrawingHandle);
                    labels.FrameVisible = true;
                    labels.FrameType = tkLabelFrameType.lfRectangle;
                }
            }
            //string filename = @"C:\Users\Admin\source\repos\GIS\GIS\bin\Debug\Shapefiles_Data\Hanoi\planet_105.141,20.676_106.164,21.273.osm.shp\shape\roads.shp";
            
        }
        public void TunOffLb(AxMap axMap1)
        {
           
        }
        void AxMap1MouseMoveEvent2(object sender, _DMapEvents_MouseMoveEvent e)
        {

        }
        void AxMap1MouseMoveEvent(object sender, _DMapEvents_MouseMoveEvent e)
        {
            Labels labels = MainMap.get_DrawingLabels(0);
            labels.Clear();
            // it's assumed here that the layer we want to edit is the first 1 (with 0 index)
            int layerHandle = MainMap.get_LayerHandle(0);
            var sf = MainMap.get_Shapefile(layerHandle);
            if (sf != null)
            {
                double projX = 0.0;
                double projY = 0.0;
                MainMap.PixelToProj(e.x, e.y, ref projX, ref projY);
                object result = null;
                var ext = new Extents();
                ext.SetBounds(projX, projY, 0.0, projX, projY, 0.0);
                if (sf.SelectShapes(ext, 0.0, SelectMode.INTERSECTION, ref result))
                {
                    int[] shapes = result as int[];
                    if (shapes != null && shapes.Length == 1)
                    {
                        string s = "";
                        for (int i = 0; i < sf.NumFields; i++)
                        {
                            s += sf.Field[i].Name + ": " + sf.CellValue[i, shapes[0]] + "\n";
                        }
                        labels.AddLabel(s, e.x + 80, e.y);
                    }
                }
            }
            MainMap.Redraw2(tkRedrawType.RedrawSkipDataLayers);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GeoProjection(MainMap);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            menuMap.Show(linkLabel2,new System.Drawing.Point(0,20));
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("This function will be coming soon");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("This function will be coming soon");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.mapwindow.org/documentation/mapwingis4.9/examples.html");
        }
    }
}
