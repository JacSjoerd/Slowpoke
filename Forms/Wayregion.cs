
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Flintstones
{
  public class Wayregion : Form
  {
    private IContainer components;
    private TrackBar trackBar1;
    private GroupBox groupBox1;
    private GroupBox groupBox2;
    private Label label3;
    private Label label2;
    private GroupBox groupBox4;
    private GroupBox groupBox5;
    private GroupBox groupBox6;
    private Matrix matrix1;
    public Label map;
    public CheckBox addwaypoints;
    public CheckBox addblocks;
    public CheckBox adddoor;
    private Button clearwaypoints;
    private Button deletewaypoint;
    private Button deleteblock;
    private Button deletedoor;
    private Label label1;
    private Label label4;
    private Label label5;
    private Label label6;
    private Label label7;
    private Label label8;
    private Label label9;
    private Label label10;
    private Label label11;
    private Label label12;
    private OpenFileDialog openFileDialog1;
    private SaveFileDialog saveFileDialog1;
    public CheckBox waitonplayers;
    private Button removeselectedplayer;
    private TextBox newplayer;
    public Button addplayer;
    public ListBox waitonplayerslistbox;
    public Button savemappack;
    public Button loadmappack;

    public Client Client { get; private set; }

    public Thread DrawThread { get; set; }

    public int DrawMap { get; set; }

    public Wayregion(Client client)
    {
      this.Client = client;
      this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
      InitializeComponent();
    }

    public void StartDrawing()
    {
      this.matrix1.Client = this.Client;
      try
      {
        if (this.DrawThread != null)
        {
          this.DrawThread.Abort();
          this.DrawThread = (Thread)null;
        }
        this.DrawThread = new Thread(new ThreadStart(this.Draw));
        this.DrawThread.Start();
      }
      catch
      {
      }
    }

    public void Draw()
    {
      this.DrawMap = this.Client.MapInfo.Number;
      this.matrix1.MaxtrixSize = this.trackBar1.Value;
      while (true)
      {
        Graphics g = this.matrix1.G;
        Bitmap bitmap = new Bitmap(this.Client.MapInfo.Width * this.matrix1.MaxtrixSize, this.Client.MapInfo.Height * this.matrix1.MaxtrixSize);
        if (this.DrawMap != this.Client.MapInfo.Number)
        {
          this.DrawMap = this.Client.MapInfo.Number;
          this.matrix1.Refresh();
          bitmap.Dispose();
          bitmap = new Bitmap(bitmap.Width * this.matrix1.MaxtrixSize, bitmap.Height * this.matrix1.MaxtrixSize);
        }
        Graphics graphics = Graphics.FromImage((Image) bitmap);
        int minX = Client.MapInfo.BaseMatrix.GetLowerBound(0);
        int maxX = Client.MapInfo.BaseMatrix.GetUpperBound(0);
        int minY = Client.MapInfo.BaseMatrix.GetLowerBound(1);
        int maxY = Client.MapInfo.BaseMatrix.GetUpperBound(1);
        for (int x = minX; x <= maxX; x++)
        {
          for (int y = minY; y <= maxY; y++)
          {
            if (this.Client.MapInfo.BaseMatrix[x, y] == 0)
              graphics.FillRectangle(Brushes.Black, x * matrix1.MaxtrixSize, y * matrix1.MaxtrixSize, matrix1.MaxtrixSize, matrix1.MaxtrixSize);
            else if (Client.MapInfo.BaseMatrix[x, y] == 1 
                  || Client.MapInfo.BaseMatrix[x, y] == 48 
                  || Client.MapInfo.BaseMatrix[x, y] == 6)
              graphics.FillRectangle(Brushes.White,  x * matrix1.MaxtrixSize, y * matrix1.MaxtrixSize, matrix1.MaxtrixSize, matrix1.MaxtrixSize);
          }
        }

        foreach (KeyValuePair<Location, string> region in this.Client.TempRegions[this.Client.MapInfo.Number].Regions)
        {
          if (region.Key is null) continue;

          if (region.Value == "WayPoint")
            graphics.FillRectangle(Brushes.Blue, region.Key.X * this.matrix1.MaxtrixSize, region.Key.Y * this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize);
          if (region.Value == "Block")
            graphics.FillRectangle(Brushes.Gray, region.Key.X * this.matrix1.MaxtrixSize, region.Key.Y * this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize);
          if (region.Value == "Door")
            graphics.FillRectangle(Brushes.Green, region.Key.X * this.matrix1.MaxtrixSize, region.Key.Y * this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize);
        }

        Character[] characterArray = null;
        lock (this.Client.Characters)
        {
          characterArray = Client.Characters.Values.ToArray<Character>();
        }
        foreach (Character character in characterArray)
        {
          if (character is null) continue;

          if (character.IsOnScreen && character.ID != this.Client.PlayerID && character is Npc && ((character as Npc).Type == NpcType.NormalMonster || (character as Npc).Type == NpcType.PassableMonster))
            graphics.FillRectangle(Brushes.Red, character.Location.X * this.matrix1.MaxtrixSize, character.Location.Y * this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize);
        }
        graphics.FillRectangle(Brushes.Goldenrod, this.Client.ServerLocation.X * this.matrix1.MaxtrixSize, this.Client.ServerLocation.Y * this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize, this.matrix1.MaxtrixSize);
        if (g != null)
        {
          g.DrawImage(bitmap, 0, 0);
        }
      }
    }

    private void SpellPriority_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = true;
      this.Hide();
      this.Client.Tab.walkconfig.Enabled = true;
    }

    private void trackBar1_Scroll(object sender, EventArgs e) => this.matrix1.MaxtrixSize = this.trackBar1.Value;

    private void matrix1_MouseClick(object sender, MouseEventArgs e)
    {
      Npc npc = new Npc();
      ushort x = (ushort) (e.X / this.matrix1.MaxtrixSize);
      ushort y = (ushort) (e.Y / this.matrix1.MaxtrixSize);
      Location key = new Location((int) x, (int) y);
      if (this.addblocks.Checked)
        this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Add(key, "Block");
      else if (this.addwaypoints.Checked)
      {
        this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Add(key, "WayPoint");
        npc.Location.X = (int) x;
        npc.Location.Y = (int) y;
        npc.Location.Direction = Direction.West;
        npc.Image = 532;
        npc.Type = NpcType.PassableMonster;
        npc.Map = this.Client.MapInfo.Number;
      }
      else
      {
        if (!this.adddoor.Checked)
          return;
        this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Add(key, "Door");
        npc.Location.X = (int) x;
        npc.Location.Y = (int) y;
        npc.Location.Direction = Direction.West;
        npc.Image = 532;
        npc.Type = NpcType.PassableMonster;
        npc.Map = this.Client.MapInfo.Number;
      }
    }

    private void addwaypoints_Click(object sender, EventArgs e)
    {
      this.addblocks.Checked = false;
      this.adddoor.Checked = false;
    }

    private void addblocks_Click(object sender, EventArgs e)
    {
      this.addwaypoints.Checked = false;
      this.adddoor.Checked = false;
    }

    private void adddoor_Click(object sender, EventArgs e)
    {
      this.addblocks.Checked = false;
      this.addwaypoints.Checked = false;
    }

    private void clearwaypoints_Click(object sender, EventArgs e)
    {
      if (!this.Client.TempRegions.ContainsKey(this.Client.MapInfo.Number))
        return;
      this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Clear();
      this.Client.Previous.Clear();
    }

    private void deletedoor_Click(object sender, EventArgs e)
    {
      if (!this.Client.TempRegions.ContainsKey(this.Client.MapInfo.Number))
        return;
      foreach (KeyValuePair<Location, string> region in this.Client.TempRegions[this.Client.MapInfo.Number].Regions)
      {
        if (region.Key != null && region.Value == "Door")
        {
          this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Remove(region.Key);
          break;
        }
      }
    }

    private void deletewaypoint_Click(object sender, EventArgs e)
    {
      if (!this.Client.TempRegions.ContainsKey(this.Client.MapInfo.Number))
        return;
      foreach (KeyValuePair<Location, string> keyValuePair in this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Reverse<KeyValuePair<Location, string>>())
      {
        if (keyValuePair.Key != null && keyValuePair.Value == "WayPoint")
        {
          this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Remove(keyValuePair.Key);
          break;
        }
      }
    }

    private void deleteblock_Click(object sender, EventArgs e)
    {
      if (!this.Client.TempRegions.ContainsKey(this.Client.MapInfo.Number))
        return;
      foreach (KeyValuePair<Location, string> keyValuePair in this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Reverse<KeyValuePair<Location, string>>())
      {
        if (keyValuePair.Key != null && keyValuePair.Value == "Block")
        {
          this.Client.TempRegions[this.Client.MapInfo.Number].Regions.Remove(keyValuePair.Key);
          break;
        }
      }
    }

    private void savemappack_Click(object sender, EventArgs e)
    {
      this.savemappack.Enabled = false;
      this.loadmappack.Enabled = false;
      if (!Directory.Exists(Program.StartupPath + "\\Settings\\MapPacks"))
        Directory.CreateDirectory(Program.StartupPath + "\\Settings\\MapPacks");
      this.saveFileDialog1.InitialDirectory = Program.StartupPath + "\\Settings\\MapPacks";
      int num = (int) this.saveFileDialog1.ShowDialog();
    }

    private void loadmappack_Click(object sender, EventArgs e)
    {
      this.savemappack.Enabled = false;
      this.loadmappack.Enabled = false;
      if (!Directory.Exists(Program.StartupPath + "\\Settings\\MapPacks"))
        Directory.CreateDirectory(Program.StartupPath + "\\Settings\\MapPacks");
      this.openFileDialog1.InitialDirectory = Program.StartupPath + "\\Settings\\MapPacks";
      int num = (int) this.openFileDialog1.ShowDialog();
      this.savemappack.Enabled = true;
      this.loadmappack.Enabled = true;
    }

    private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) => this.SaveMapPack();

    private void openFileDialog1_FileOk(object sender, CancelEventArgs e) => this.LoadMapPack();

    private void SaveMapPack()
    {
      if (!(this.saveFileDialog1.FileName != string.Empty))
        return;
      XDocument xdocument = new XDocument();
      xdocument.Add((object) new XElement((XName) "WayPoints"));
      int content = 0;
      foreach (KeyValuePair<int, MapNum> tempRegion in this.Client.TempRegions)
      {
        if (tempRegion.Value.Regions.Count > 0)
          ++content;
      }
      xdocument.Element((XName) "WayPoints").Add((object) new XElement((XName) "NumberOfMaps", (object) content));
      int num1 = 0;
      foreach (KeyValuePair<int, MapNum> tempRegion in this.Client.TempRegions)
      {
        if (tempRegion.Value.Regions.Count > 0)
        {
          xdocument.Element((XName) "WayPoints").Add((object) new XElement((XName) ("map" + num1.ToString())));
          xdocument.Element((XName) "WayPoints").Element((XName) ("map" + num1.ToString())).Add((object) new XElement((XName) "Count", (object) tempRegion.Value.Regions.Count.ToString()));
          xdocument.Element((XName) "WayPoints").Element((XName) ("map" + num1.ToString())).Add((object) new XElement((XName) "MapNum", (object) tempRegion.Key.ToString()));
          int num2 = 0;
          foreach (KeyValuePair<Location, string> region in tempRegion.Value.Regions)
          {
            if (region.Key != null)
            {
              xdocument.Element((XName) "WayPoints").Element((XName) ("map" + num1.ToString())).Add((object) new XElement((XName) ("Location" + num2.ToString()), (object) (region.Key.X.ToString() + "," + region.Key.Y.ToString())));
              xdocument.Element((XName) "WayPoints").Element((XName) ("map" + num1.ToString())).Add((object) new XElement((XName) ("Point" + num2.ToString()), (object) region.Value.ToString()));
              ++num2;
            }
          }
          ++num1;
        }
      }
      xdocument.Save(this.saveFileDialog1.FileName);
      this.savemappack.Enabled = true;
      this.loadmappack.Enabled = true;
    }

    public void LoadMapPack(string FileName = "")
    {
      if (FileName == "")
        FileName = this.openFileDialog1.FileName;
      if (!(FileName != "") || !File.Exists(FileName))
        return;
      this.Client.TempRegions.Clear();
      this.Client.Previous.Clear();
      XDocument xdocument = XDocument.Load(FileName);
      for (int index1 = 0; index1 < (int) Convert.ToUInt16(xdocument.Element((XName) "WayPoints").Element((XName) "NumberOfMaps").Value); ++index1)
      {
        MapNum mapNum = new MapNum();
        mapNum.Number = (int) Convert.ToUInt16(xdocument.Element((XName) "WayPoints").Element((XName) ("map" + index1.ToString())).Element((XName) "MapNum").Value);
        this.Client.TempRegions.Add(mapNum.Number, mapNum);
        for (int index2 = 0; index2 < (int) Convert.ToUInt16(xdocument.Element((XName) "WayPoints").Element((XName) ("map" + index1.ToString())).Element((XName) "Count").Value); ++index2)
        {
          Npc npc = new Npc();
          string[] strArray = xdocument.Element((XName) "WayPoints").Element((XName) ("map" + index1.ToString())).Element((XName) ("Location" + index2.ToString())).Value.Split(',');
          int x = int.Parse(strArray[0]);
          int y = int.Parse(strArray[1]);
          Location key = new Location(x, y);
          string str = xdocument.Element((XName) "WayPoints").Element((XName) ("map" + index1.ToString())).Element((XName) ("Point" + index2.ToString())).Value;
          this.Client.TempRegions[mapNum.Number].Regions.Add(key, str);
          if (str == "Door" || str == "WayPoint")
          {
            npc.Location.X = x;
            npc.Location.Y = y;
            npc.Location.Direction = Direction.West;
            npc.Image = 532;
            npc.Type = NpcType.PassableMonster;
            npc.Map = mapNum.Number;
          }
        }
      }
      if (!this.Client.TempRegions.ContainsKey(this.Client.MapInfo.Number))
      {
        MapNum mapNum = new MapNum();
        mapNum.Number = this.Client.MapInfo.Number;
        this.Client.TempRegions.Add(mapNum.Number, mapNum);
      }
      this.savemappack.Enabled = true;
      this.loadmappack.Enabled = true;
    }

    private void newplayer_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar != '\r')
        return;
      e.Handled = true;
      if (!(this.newplayer.Text != string.Empty) || this.waitonplayerslistbox.Items.Contains((object) this.newplayer.Text))
        return;
      this.waitonplayerslistbox.Items.Add((object) this.newplayer.Text);
      this.newplayer.Text = string.Empty;
    }

    private void addplayer_Click(object sender, EventArgs e)
    {
      if (!(this.newplayer.Text != string.Empty) || this.waitonplayerslistbox.Items.Contains((object) this.newplayer.Text))
        return;
      this.waitonplayerslistbox.Items.Add((object) this.newplayer.Text);
      this.newplayer.Text = string.Empty;
    }

    private void removeselectedplayer_Click(object sender, EventArgs e)
    {
      if (this.waitonplayerslistbox.Items.Count <= 0)
        return;
      this.waitonplayerslistbox.Items.Remove(this.waitonplayerslistbox.SelectedItem);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.map = new Label();
      this.trackBar1 = new TrackBar();
      this.groupBox1 = new GroupBox();
      this.groupBox2 = new GroupBox();
      this.label3 = new Label();
      this.label2 = new Label();
      this.groupBox4 = new GroupBox();
      this.deletewaypoint = new Button();
      this.addwaypoints = new CheckBox();
      this.groupBox5 = new GroupBox();
      this.deleteblock = new Button();
      this.addblocks = new CheckBox();
      this.groupBox6 = new GroupBox();
      this.deletedoor = new Button();
      this.adddoor = new CheckBox();
      this.savemappack = new Button();
      this.loadmappack = new Button();
      this.clearwaypoints = new Button();
      this.label1 = new Label();
      this.label4 = new Label();
      this.label5 = new Label();
      this.label6 = new Label();
      this.label7 = new Label();
      this.label8 = new Label();
      this.label9 = new Label();
      this.label10 = new Label();
      this.label11 = new Label();
      this.label12 = new Label();
      this.saveFileDialog1 = new SaveFileDialog();
      this.openFileDialog1 = new OpenFileDialog();
      this.matrix1 = new Matrix();
      this.waitonplayers = new CheckBox();
      this.waitonplayerslistbox = new ListBox();
      this.addplayer = new Button();
      this.removeselectedplayer = new Button();
      this.newplayer = new TextBox();
      this.trackBar1.BeginInit();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox6.SuspendLayout();
      this.SuspendLayout();
      this.map.AutoSize = true;
      this.map.Location = new System.Drawing.Point(6, 17);
      this.map.Name = "map";
      this.map.Size = new Size(0, 15);
      this.map.TabIndex = 0;
      this.trackBar1.LargeChange = 1;
      this.trackBar1.Location = new System.Drawing.Point(6, 47);
      this.trackBar1.Name = "trackBar1";
      this.trackBar1.Size = new Size(195, 45);
      this.trackBar1.TabIndex = 1;
      this.trackBar1.Value = 5;
      this.trackBar1.Scroll += new EventHandler(this.trackBar1_Scroll);
      this.groupBox1.Controls.Add((Control) this.map);
      this.groupBox1.Location = new System.Drawing.Point(22, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(212, 44);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Map";
      this.groupBox2.Controls.Add((Control) this.label3);
      this.groupBox2.Controls.Add((Control) this.label2);
      this.groupBox2.Controls.Add((Control) this.trackBar1);
      this.groupBox2.Location = new System.Drawing.Point(23, 71);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(210, 104);
      this.groupBox2.TabIndex = 4;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Scale";
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(150, 29);
      this.label3.Name = "label3";
      this.label3.Size = new Size(39, 15);
      this.label3.TabIndex = 3;
      this.label3.Text = "Large";
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 29);
      this.label2.Name = "label2";
      this.label2.Size = new Size(39, 15);
      this.label2.TabIndex = 2;
      this.label2.Text = "Small";
      this.groupBox4.Controls.Add((Control) this.deletewaypoint);
      this.groupBox4.Controls.Add((Control) this.addwaypoints);
      this.groupBox4.Location = new System.Drawing.Point(27, 181);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new Size(207, 73);
      this.groupBox4.TabIndex = 6;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Waypoints";
      this.deletewaypoint.Location = new System.Drawing.Point(78, 45);
      this.deletewaypoint.Name = "deletewaypoint";
      this.deletewaypoint.Size = new Size(107, 23);
      this.deletewaypoint.TabIndex = 1;
      this.deletewaypoint.Text = "Delete Last";
      this.deletewaypoint.UseVisualStyleBackColor = true;
      this.deletewaypoint.Click += new EventHandler(this.deletewaypoint_Click);
      this.addwaypoints.AutoSize = true;
      this.addwaypoints.Location = new System.Drawing.Point(6, 20);
      this.addwaypoints.Name = "addwaypoints";
      this.addwaypoints.Size = new Size(98, 17);
      this.addwaypoints.TabIndex = 0;
      this.addwaypoints.Text = "Add Waypoints";
      this.addwaypoints.UseVisualStyleBackColor = true;
      this.addwaypoints.Click += new EventHandler(this.addwaypoints_Click);
      this.groupBox5.Controls.Add((Control) this.deleteblock);
      this.groupBox5.Controls.Add((Control) this.addblocks);
      this.groupBox5.Location = new System.Drawing.Point(27, 260);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new Size(207, 74);
      this.groupBox5.TabIndex = 7;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Blocks";
      this.deleteblock.Location = new System.Drawing.Point(76, 45);
      this.deleteblock.Name = "deleteblock";
      this.deleteblock.Size = new Size(107, 23);
      this.deleteblock.TabIndex = 3;
      this.deleteblock.Text = "Delete Last";
      this.deleteblock.UseVisualStyleBackColor = true;
      this.deleteblock.Click += new EventHandler(this.deleteblock_Click);
      this.addblocks.AutoSize = true;
      this.addblocks.Location = new System.Drawing.Point(6, 20);
      this.addblocks.Name = "addblocks";
      this.addblocks.Size = new Size(80, 17);
      this.addblocks.TabIndex = 0;
      this.addblocks.Text = "Add Blocks";
      this.addblocks.UseVisualStyleBackColor = true;
      this.addblocks.Click += new EventHandler(this.addblocks_Click);
      this.groupBox6.Controls.Add((Control) this.deletedoor);
      this.groupBox6.Controls.Add((Control) this.adddoor);
      this.groupBox6.Location = new System.Drawing.Point(27, 340);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new Size(207, 75);
      this.groupBox6.TabIndex = 8;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Door";
      this.deletedoor.Location = new System.Drawing.Point(78, 45);
      this.deletedoor.Name = "deletedoor";
      this.deletedoor.Size = new Size(107, 23);
      this.deletedoor.TabIndex = 3;
      this.deletedoor.Text = "Delete Last";
      this.deletedoor.UseVisualStyleBackColor = true;
      this.deletedoor.Click += new EventHandler(this.deletedoor_Click);
      this.adddoor.AutoSize = true;
      this.adddoor.Location = new System.Drawing.Point(6, 20);
      this.adddoor.Name = "adddoor";
      this.adddoor.Size = new Size(71, 17);
      this.adddoor.TabIndex = 0;
      this.adddoor.Text = "Add Door";
      this.adddoor.UseVisualStyleBackColor = true;
      this.adddoor.Click += new EventHandler(this.adddoor_Click);
      this.savemappack.Location = new System.Drawing.Point(22, 514);
      this.savemappack.Name = "savemappack";
      this.savemappack.Size = new Size(118, 32);
      this.savemappack.TabIndex = 11;
      this.savemappack.Text = "Save Map Pack";
      this.savemappack.UseVisualStyleBackColor = true;
      this.savemappack.Click += new EventHandler(this.savemappack_Click);
      this.loadmappack.Location = new System.Drawing.Point(176, 514);
      this.loadmappack.Name = "loadmappack";
      this.loadmappack.Size = new Size(110, 32);
      this.loadmappack.TabIndex = 12;
      this.loadmappack.Text = "Load Map Pack";
      this.loadmappack.UseVisualStyleBackColor = true;
      this.loadmappack.Click += new EventHandler(this.loadmappack_Click);
      this.clearwaypoints.Location = new System.Drawing.Point(76, 447);
      this.clearwaypoints.Name = "clearwaypoints";
      this.clearwaypoints.Size = new Size(107, 27);
      this.clearwaypoints.TabIndex = 2;
      this.clearwaypoints.Text = "Clear All";
      this.clearwaypoints.UseVisualStyleBackColor = true;
      this.clearwaypoints.Click += new EventHandler(this.clearwaypoints_Click);
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(329, 489);
      this.label1.Name = "label1";
      this.label1.Size = new Size(34, 15);
      this.label1.TabIndex = 14;
      this.label1.Text = "- You";
      this.label4.AutoSize = true;
      this.label4.BackColor = Color.Goldenrod;
      this.label4.Location = new System.Drawing.Point(307, 489);
      this.label4.Name = "label4";
      this.label4.Size = new Size(16, 15);
      this.label4.TabIndex = 15;
      this.label4.Text = "   ";
      this.label5.AutoSize = true;
      this.label5.BackColor = Color.Blue;
      this.label5.Location = new System.Drawing.Point(399, 489);
      this.label5.Name = "label5";
      this.label5.Size = new Size(16, 15);
      this.label5.TabIndex = 16;
      this.label5.Text = "   ";
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(421, 489);
      this.label6.Name = "label6";
      this.label6.Size = new Size(71, 15);
      this.label6.TabIndex = 17;
      this.label6.Text = "- Waypoints";
      this.label7.AutoSize = true;
      this.label7.BackColor = Color.Red;
      this.label7.Location = new System.Drawing.Point(517, 489);
      this.label7.Name = "label7";
      this.label7.Size = new Size(16, 15);
      this.label7.TabIndex = 18;
      this.label7.Text = "   ";
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(539, 489);
      this.label8.Name = "label8";
      this.label8.Size = new Size(65, 15);
      this.label8.TabIndex = 19;
      this.label8.Text = "- Monsters";
      this.label9.AutoSize = true;
      this.label9.BackColor = Color.Gray;
      this.label9.Location = new System.Drawing.Point(622, 489);
      this.label9.Name = "label9";
      this.label9.Size = new Size(16, 15);
      this.label9.TabIndex = 20;
      this.label9.Text = "   ";
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(644, 489);
      this.label10.Name = "label10";
      this.label10.Size = new Size(51, 15);
      this.label10.TabIndex = 21;
      this.label10.Text = "- Blocks";
      this.label11.AutoSize = true;
      this.label11.BackColor = Color.Green;
      this.label11.Location = new System.Drawing.Point(722, 489);
      this.label11.Name = "label11";
      this.label11.Size = new Size(16, 15);
      this.label11.TabIndex = 22;
      this.label11.Text = "   ";
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(744, 489);
      this.label12.Name = "label12";
      this.label12.Size = new Size(41, 15);
      this.label12.TabIndex = 23;
      this.label12.Text = "- Door";
      this.saveFileDialog1.DefaultExt = "xml";
      this.saveFileDialog1.Filter = "XML|*.xml";
      this.saveFileDialog1.FileOk += new CancelEventHandler(this.saveFileDialog1_FileOk);
      this.openFileDialog1.DefaultExt = "xml";
      this.openFileDialog1.Filter = "XML|*.xml";
      this.openFileDialog1.FileOk += new CancelEventHandler(this.openFileDialog1_FileOk);
      this.matrix1.BackColor = Color.Black;
      this.matrix1.Client = (Client) null;
      this.matrix1.Location = new System.Drawing.Point(270, 13);
      this.matrix1.MaxtrixSize = 10;
      this.matrix1.Name = "matrix1";
      this.matrix1.Size = new Size(518, 461);
      this.matrix1.TabIndex = 13;
      this.matrix1.MouseClick += new MouseEventHandler(this.matrix1_MouseClick);
      this.waitonplayers.AutoSize = true;
      this.waitonplayers.Checked = true;
      this.waitonplayers.CheckState = CheckState.Checked;
      this.waitonplayers.Location = new System.Drawing.Point(815, 37);
      this.waitonplayers.Name = "waitonplayers";
      this.waitonplayers.Size = new Size(113, 19);
      this.waitonplayers.TabIndex = 24;
      this.waitonplayers.Text = "Wait On Players";
      this.waitonplayers.UseVisualStyleBackColor = true;
      this.waitonplayerslistbox.FormattingEnabled = true;
      this.waitonplayerslistbox.ItemHeight = 15;
      this.waitonplayerslistbox.Location = new System.Drawing.Point(802, 93);
      this.waitonplayerslistbox.Name = "waitonplayerslistbox";
      this.waitonplayerslistbox.Size = new Size(150, 244);
      this.waitonplayerslistbox.TabIndex = 25;
      this.addplayer.Location = new System.Drawing.Point(839, 387);
      this.addplayer.Name = "addplayer";
      this.addplayer.Size = new Size(75, 40);
      this.addplayer.TabIndex = 26;
      this.addplayer.Text = "Add";
      this.addplayer.UseVisualStyleBackColor = true;
      this.addplayer.Click += new EventHandler(this.addplayer_Click);
      this.removeselectedplayer.Location = new System.Drawing.Point(815, 460);
      this.removeselectedplayer.Name = "removeselectedplayer";
      this.removeselectedplayer.Size = new Size(123, 27);
      this.removeselectedplayer.TabIndex = 27;
      this.removeselectedplayer.Text = "Remove Selected";
      this.removeselectedplayer.UseVisualStyleBackColor = true;
      this.removeselectedplayer.Click += new EventHandler(this.removeselectedplayer_Click);
      this.newplayer.Location = new System.Drawing.Point(815, 360);
      this.newplayer.Name = "newplayer";
      this.newplayer.Size = new Size(123, 21);
      this.newplayer.TabIndex = 28;
      this.newplayer.KeyPress += new KeyPressEventHandler(this.newplayer_KeyPress);
      this.AutoScaleDimensions = new SizeF(7f, 15f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = SystemColors.ButtonHighlight;
      this.ClientSize = new Size(966, 557);
      this.Controls.Add((Control) this.newplayer);
      this.Controls.Add((Control) this.removeselectedplayer);
      this.Controls.Add((Control) this.addplayer);
      this.Controls.Add((Control) this.waitonplayerslistbox);
      this.Controls.Add((Control) this.waitonplayers);
      this.Controls.Add((Control) this.clearwaypoints);
      this.Controls.Add((Control) this.label12);
      this.Controls.Add((Control) this.label11);
      this.Controls.Add((Control) this.label10);
      this.Controls.Add((Control) this.label9);
      this.Controls.Add((Control) this.label8);
      this.Controls.Add((Control) this.label7);
      this.Controls.Add((Control) this.label6);
      this.Controls.Add((Control) this.label5);
      this.Controls.Add((Control) this.label4);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.matrix1);
      this.Controls.Add((Control) this.loadmappack);
      this.Controls.Add((Control) this.savemappack);
      this.Controls.Add((Control) this.groupBox6);
      this.Controls.Add((Control) this.groupBox5);
      this.Controls.Add((Control) this.groupBox4);
      this.Controls.Add((Control) this.groupBox2);
      this.Controls.Add((Control) this.groupBox1);
      this.Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.Name =  "Wayregion";
      this.Text = "Set Wayregions";
      this.FormClosing += new FormClosingEventHandler(this.SpellPriority_FormClosing);
      this.trackBar1.EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupBox6.ResumeLayout(false);
      this.groupBox6.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
