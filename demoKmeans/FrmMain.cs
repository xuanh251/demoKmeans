using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace demoKmeans
{
    public partial class FrmMain : DevExpress.XtraEditors.XtraForm
    {
        private Point myPoint;
        List<PointModel> listPoint = new List<PointModel>();
        private Graphics grs;
        private Bitmap bm;
        private Color currentColor;
        int tt = 1;
        private List<Point> listCentral;
        private List<PhanCum> ListPhanCum;
        public FrmMain()
        {
            InitializeComponent();
            layoutControl1.Controls.Add(PnlMain);
            PnlMain.Dock = DockStyle.Fill;
            PnlMain.Paint += panelControl1_Paint;
            PnlMain.MouseDown += panelControl1_MouseDown;
            PnlMain.MouseUp += panelControl1_MouseUp;
            currentColor = Color.Black;
            bm = new Bitmap(PnlMain.Width, PnlMain.Height);
            grs = Graphics.FromImage(bm);
        }

        private void panelControl1_MouseDown(object sender, MouseEventArgs e)
        {
            myPoint = new Point(e.X, e.Y);
            Pen pen = new Pen(currentColor, 3);
            grs.DrawEllipse(pen, myPoint.X, myPoint.Y, 3, 3);
            PnlMain.BackgroundImage = (Bitmap)bm.Clone();
            listPoint.Add(new PointModel()
            {
                Name = Number2String(tt++, true),
                X = myPoint.X,
                Y = myPoint.Y
            });
            txtDSDiem.ResetText();
            foreach (var item in listPoint)
            {
                txtDSDiem.Text += item.Name + "(" + item.X + "," + item.Y + ")  ";
            }
        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(currentColor, 3);
            e.Graphics.DrawEllipse(pen, myPoint.X, myPoint.Y, 3, 3);
        }

        private void panelControl1_MouseUp(object sender, MouseEventArgs e)
        {
           
        }
        private String Number2String(int number, bool isCaps)
        {
            Char c = (Char)((isCaps ? 65 : 97) + (number - 1));
            return c.ToString();
        }

        private void btnPhanCum_Click(object sender, EventArgs e)
        {
            var end = false;
            try
            {
                var k = int.Parse(txtSoCum.Text);
                int solanlap = 1;
                TaoPTTT(k);//khởi tạo ngẫu nhiên danh sách phần tử trung tâm
                while (!end && solanlap < 100)
                {
                    txtKetQua.Text = solanlap.ToString();
                    ListPhanCum = new List<PhanCum>();
                    //if (ListPhanCum.Count != 0) ListPhanCum.Clear();
                    foreach (var p in listPoint)//duyệt qua từng điểm để tính khoảng cách xem gần tâm nào nhất
                    {
                        float dMin = float.MaxValue;
                        Point thuocPttt = new Point();
                        foreach (var c in listCentral)
                        {
                            var d = TinhKhoangCach(new Point(p.X, p.Y), c);
                            if (dMin > d)//chỗ này lấy ra đc pttt và khoảng cách gần nhất
                            {
                                dMin = d;
                                thuocPttt = c;
                            }
                        }//chia thêm các điểm vào các cụm tương ứng
                        ListPhanCum.Add(new PhanCum()
                        {
                            KhoangCach = dMin,
                            Name = p.Name,
                            ThuocCum = thuocPttt
                        });
                    }
                    //cập nhật lại pttt
                    List<Point> newListCentral = new List<Point>();
                    foreach (var central in listCentral.ToList())
                    {
                        int sumX = 0;
                        int sumY = 0;
                        var mList = ListPhanCum.Where(s => s.ThuocCum == central).ToList();
                        foreach (var item in mList)
                        {
                            sumX += listPoint.FirstOrDefault(s => s.Name == item.Name).X;
                            sumY += listPoint.FirstOrDefault(s => s.Name == item.Name).Y;
                        }

                        var newCentral = new Point(sumX / mList.Count, sumY / mList.Count);
                        newListCentral.Add(newCentral);//tạo danh sách pttt mới
                    }
                    if (newListCentral.Intersect(listCentral).Count() == newListCentral.Count())
                    {//nếu tất cả pt giống nhau thì dừng
                        end = true;
                    }
                    else
                    {
                        listCentral = newListCentral;
                        solanlap++;
                    }
                }
                //in kết quả và chia màu
                RePaint();
                txtKetQua.Text = "Số lần lặp:" + solanlap.ToString() + Environment.NewLine;
                foreach (var c in listCentral)
                {
                    Random rnd = new Random();
                    currentColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    Pen Bpen = new Pen(currentColor, 5);
                    grs.DrawEllipse(Bpen, c.X, c.Y, 5, 5);
                    txtKetQua.Text += "Cụm " + (int)(listCentral.IndexOf(c) + 1) + "(" + c.X + "," + c.Y + "): ";
                    foreach (var p in ListPhanCum.Where(s => s.ThuocCum == c))
                    {
                        Pen pen = new Pen(currentColor, 3);
                        grs.DrawEllipse(pen, listPoint.FirstOrDefault(s => s.Name == p.Name).X, listPoint.FirstOrDefault(s => s.Name == p.Name).Y, 3, 3);
                        PnlMain.BackgroundImage = (Bitmap)bm.Clone();
                        PnlMain.Refresh();
                        txtKetQua.Text += p.Name + "(" + listPoint.FirstOrDefault(s => s.Name == p.Name).X + ", " + listPoint.FirstOrDefault(s => s.Name == p.Name).Y + "), ";
                    }
                    txtKetQua.Text += Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Đã xảy ra lỗi" + Environment.NewLine + ex.ToString(), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void TaoPTTT(int k)
        {
            var rd = new Random();
            listCentral = new List<Point>();
            for (int i = 0; i < k; i++)
            {
                var rdPoint = rd.Next(listPoint.Count());
                var p = new Point(listPoint.ElementAt(rdPoint).X, listPoint.ElementAt(rdPoint).Y);
                listCentral.Add(p);
            }
        }
        private float TinhKhoangCach(Point p1, Point p2)
        {
            float d = (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return d;
        }

        private void btnLamLai_Click(object sender, EventArgs e)
        {
            RePaint();
            txtDSDiem.ResetText();
            txtSoCum.ResetText();
            txtKetQua.ResetText();
            listPoint = new List<PointModel>();
            tt = 1;
        }
        private void RePaint()
        {
            bm = new Bitmap(PnlMain.Width, PnlMain.Height);
            grs = Graphics.FromImage(bm);
            PnlMain.Refresh();
            PnlMain.BackgroundImage = (Bitmap)bm.Clone();
        }
    }
}