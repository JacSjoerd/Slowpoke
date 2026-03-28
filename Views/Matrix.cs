

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Flintstones
{
  public class Matrix : UserControl
  {
    private int _MatrixSize = 10;
    private IContainer components;

    public Client Client { get; set; }

    public int MaxtrixSize
    {
      get => this._MatrixSize;
      set
      {
        this._MatrixSize = value;
        this.Invalidate();
      }
    }

    public Graphics G => this._G;

    private Graphics _G
    {
      get
      {
        try
        {
          return Graphics.FromHwnd(this.Handle);
        }
        catch
        {
          return null;
        }
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
      this.SuspendLayout();
      this.AutoScaleMode = AutoScaleMode.Inherit;
      this.Name =  "Matrix";
      this.ResumeLayout(false);
    }
  }
}
