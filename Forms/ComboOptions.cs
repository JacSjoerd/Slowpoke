//SlowPoke
// Type: Flintstones.ComboOptions
//SlowPoke
//SlowPoke
//SlowPoke

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Flintstones
{
  public class ComboOptions : Form
  {
    public MainForm parent;
    private IContainer components;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;
    private Label label5;
    public ListBox comboslistbox;
    public TextBox comboname;
    public TextBox comboskillslist;
    public NumericUpDown comboicon;
    public NumericUpDown comboslot;
    public Button comboadd;
    public Button combodelete;
    private Label label6;
    private Label label7;

    public Client Client { get; private set; }

    public ComboOptions(Client client)
    {
      this.parent = Program.MainForm;
      this.Client = client;
      this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
      this.InitializeComponent();
    }

    private void ComboOptions_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = true;
      this.Hide();
      this.Client.Tab.opencomboform.Enabled = true;
    }

    private void comboname_TextChanged(object sender, EventArgs e)
    {
      if (this.comboname.Text != "" && this.comboslistbox.Items.Contains((object) this.comboname.Text))
      {
        this.comboadd.Text = "Edit";
      }
      else
      {
        if (!(this.comboname.Text != "") || !(this.comboadd.Text == "Edit"))
          return;
        this.comboadd.Text = "Add";
      }
    }

    private void comboadd_Click(object sender, EventArgs e)
    {
      if (!(this.comboname.Text != "") || !(this.comboicon.Value > 0M) || !(this.comboslot.Value > 0M))
        return;
      if (!this.comboslistbox.Items.Contains((object) this.comboname.Text))
      {
        this.Client.CreateSkill((byte) this.comboslot.Value, (int) this.comboicon.Value, this.comboname.Text);
        this.comboslistbox.Items.Add((object) this.comboname.Text);
      }
      else
      {
        if (this.Client.FakeSkills.ContainsKey(this.comboname.Text))
          this.Client.RemoveSkill((byte) this.comboslot.Value);
        this.Client.CreateSkill((byte) this.comboslot.Value, (int) this.comboicon.Value, this.comboname.Text);
      }
      this.comboslistbox.SelectedIndex = this.comboslistbox.Items.Count - 1;
    }

    private void comboskillslist_TextChanged(object sender, EventArgs e)
    {
      if (this.comboslistbox.SelectedItem == null || !this.Client.FakeSkills.ContainsKey(this.comboslistbox.SelectedItem.ToString()))
        return;
      string str = this.comboskillslist.Text.Replace(Environment.NewLine, "|");
      if (this.Client.Combos.ContainsKey(this.comboslistbox.SelectedItem.ToString()))
        this.Client.Combos[this.comboslistbox.SelectedItem.ToString()] = str;
      else
        this.Client.Combos.Add(this.comboslistbox.SelectedItem.ToString(), str);
    }

    private void comboslistbox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.comboslistbox.SelectedItem != null)
      {
        this.comboskillslist.Enabled = true;
        this.combodelete.Enabled = true;
        var comboSkill = this.comboslistbox.SelectedItem.ToString();
        if (this.Client.FakeSkills.ContainsKey(comboSkill) && this.Client.Combos.ContainsKey(comboSkill))
          this.comboskillslist.Text = this.Client.Combos[comboSkill].Replace("|", Environment.NewLine);
        this.comboslot.Value = (Decimal) this.Client.FakeSkills[comboSkill].SkillSlot;
        this.comboicon.Value = (Decimal) this.Client.FakeSkills[comboSkill].Icon;
        this.comboname.Text = comboSkill;
        this.comboadd.Text = "Edit";
      }
      else
      {
        this.comboskillslist.Enabled = false;
        this.combodelete.Enabled = false;
        this.comboskillslist.Text = "";
        this.comboslot.Value = 0M;
        this.comboicon.Value = 0M;
        this.comboname.Text = "";
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.comboslistbox = new ListBox();
      this.comboname = new TextBox();
      this.label1 = new Label();
      this.comboskillslist = new TextBox();
      this.label2 = new Label();
      this.comboicon = new NumericUpDown();
      this.label3 = new Label();
      this.comboslot = new NumericUpDown();
      this.label4 = new Label();
      this.comboadd = new Button();
      this.combodelete = new Button();
      this.label5 = new Label();
      this.label6 = new Label();
      this.label7 = new Label();
      this.comboicon.BeginInit();
      this.comboslot.BeginInit();
      this.SuspendLayout();
      this.comboslistbox.FormattingEnabled = true;
      this.comboslistbox.Location = new System.Drawing.Point(25, 38);
      this.comboslistbox.Name = "comboslistbox";
      this.comboslistbox.Size = new Size(103, 199);
      this.comboslistbox.TabIndex = 0;
      this.comboslistbox.SelectedIndexChanged += new EventHandler(this.comboslistbox_SelectedIndexChanged);
      this.comboname.Location = new System.Drawing.Point(366, 66);
      this.comboname.Name = "comboname";
      this.comboname.Size = new Size(100, 20);
      this.comboname.TabIndex = 2;
      this.comboname.TextChanged += new EventHandler(this.comboname_TextChanged);
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(363, 50);
      this.label1.Name = "label1";
      this.label1.Size = new Size(71, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Combo Name";
      this.comboskillslist.Enabled = false;
      this.comboskillslist.Location = new System.Drawing.Point(147, 38);
      this.comboskillslist.Multiline = true;
      this.comboskillslist.Name = "comboskillslist";
      this.comboskillslist.Size = new Size(151, 263);
      this.comboskillslist.TabIndex = 4;
      this.comboskillslist.TextChanged += new EventHandler(this.comboskillslist_TextChanged);
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(363, 108);
      this.label2.Name = "label2";
      this.label2.Size = new Size(38, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Icon #";
      this.comboicon.Location = new System.Drawing.Point(407, 106);
      this.comboicon.Name = "comboicon";
      this.comboicon.Size = new Size(59, 20);
      this.comboicon.TabIndex = 6;
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(363, 147);
      this.label3.Name = "label3";
      this.label3.Size = new Size(35, 13);
      this.label3.TabIndex = 7;
      this.label3.Text = "Slot #";
      this.comboslot.Location = new System.Drawing.Point(407, 145);
      this.comboslot.Name = "comboslot";
      this.comboslot.Size = new Size(59, 20);
      this.comboslot.TabIndex = 8;
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(376, 177);
      this.label4.Name = "label4";
      this.label4.Size = new Size(78, 39);
      this.label4.TabIndex = 9;
      this.label4.Text = "1-35 Temuair\r\n37-71 Medenia\r\n73-88 h menu";
      this.comboadd.Location = new System.Drawing.Point(375, 244);
      this.comboadd.Name = "comboadd";
      this.comboadd.Size = new Size(75, 23);
      this.comboadd.TabIndex = 10;
      this.comboadd.Text = "Add";
      this.comboadd.UseVisualStyleBackColor = true;
      this.comboadd.Click += new EventHandler(this.comboadd_Click);
      this.combodelete.Enabled = false;
      this.combodelete.Location = new System.Drawing.Point(38, 260);
      this.combodelete.Name = "combodelete";
      this.combodelete.Size = new Size(75, 23);
      this.combodelete.TabIndex = 11;
      this.combodelete.Text = "Delete";
      this.combodelete.UseVisualStyleBackColor = true;
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(337, 22);
      this.label5.Name = "label5";
      this.label5.Size = new Size(169, 13);
      this.label5.TabIndex = 12;
      this.label5.Text = "Create a skill to trigger your combo";
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(161, 22);
      this.label6.Name = "label6";
      this.label6.Size = new Size(121, 13);
      this.label6.TabIndex = 13;
      this.label6.Text = "Skills in selected Combo";
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(35, 22);
      this.label7.Name = "label7";
      this.label7.Size = new Size(59, 13);
      this.label7.TabIndex = 14;
      this.label7.Text = "Combo List";
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(536, 338);
      this.Controls.Add((Control) this.label7);
      this.Controls.Add((Control) this.label6);
      this.Controls.Add((Control) this.label5);
      this.Controls.Add((Control) this.combodelete);
      this.Controls.Add((Control) this.comboadd);
      this.Controls.Add((Control) this.label4);
      this.Controls.Add((Control) this.comboslot);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.comboicon);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.comboskillslist);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.comboname);
      this.Controls.Add((Control) this.comboslistbox);
      this.Name =  "ComboOptions";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text =  "ComboOptions";
      this.FormClosing += new FormClosingEventHandler(this.ComboOptions_FormClosing);
      this.comboicon.EndInit();
      this.comboslot.EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
