using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.InteropServices;
namespace Restart_tetris
{
    public partial class MainForm : Form
    {
        //存储所有的砖块的样式及颜色
        ArrayList bricksarraylist=new ArrayList();
        Random rd = new Random();
        int index;
        //存储当前的panel1需要画的砖块
        standardbrick currentstandardbrick;
        //存储当前的panel2需要画的砖块
        standardbrick nextstandardbrick;
        Graphics panel1g ;
        Graphics panel2g;
        //存储新的砖块在panel1上画出来之前，panel1上已有的图案
        Color[,] preservecolor = new Color[13, 24];
        //xx，yy调整砖块在panel1上的位置
        int xx;
        int yy;
        //存储玩家所得分数
        int score;
        //需要检查的位置的坐标（因为在准备执行旋转或向下等操作时要检查所需的位置是否为空）
        Point[] pout;
        //画布上需要检查的位置的实际的颜色
        ArrayList nextcolor=new ArrayList();
        IntPtr hdc;
        [DllImport("gdi32.dll")]//windows API
        private static extern int GetPixel(IntPtr hDc, int x, int y);
        public MainForm()
        {
            InitializeComponent();
            bricksarraylist.Add(new concretebrick("0000001100001000010000000", Color.Gold));
            bricksarraylist.Add(new concretebrick("0010000100001000010000000", Color.Red));
            bricksarraylist.Add(new concretebrick("0000000100011000100000000", Color.Pink));
            bricksarraylist.Add(new concretebrick("0000000100011000010000000", Color.Purple));
            bricksarraylist.Add(new concretebrick("0000000110011000000000000", Color.Green));
            bricksarraylist.Add(new concretebrick("0000000110001000010000000", Color.DodgerBlue));
            bricksarraylist.Add(new concretebrick("0000000110001100000000000", Color.Yellow));
            panel1g = panel1.CreateGraphics();
            panel2g = panel2.CreateGraphics();
            //hdc = panel1g.GetHdc();
            score=0;
            xx = 6;
            yy = 2;
            for (int i = 0; i < 13; i++)
                for (int j = 0; j < 24; j++)
                    //初始化为panel1的backcolor
                    preservecolor[i, j] = Color.MidnightBlue;
        }

        private void 播放音乐ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //添加了一个openfiledialog控件，名字叫openmusic
            openmusic.CheckFileExists = true;
            openmusic.CheckPathExists = true;
            openmusic.InitialDirectory=Application.StartupPath+"\\歌曲";
            if (openmusic.ShowDialog() == DialogResult.OK)
            {
                //openmusic.showdialog()的返回值是ok和cancel两种
                this.axWindowsMediaPlayer1.URL = openmusic.FileName;
            }
        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //setnotice是另一个form窗体，用来设置玩家需要的等级
            set_notice setnotice = new set_notice();
            setnotice.Show();
            setnotice.Owner = this;
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //helpdocument helpd = new helpdocument();
            //helpd.Show();
            //打开帮助文档
            System.Diagnostics.Process.Start(Application.StartupPath + "\\GPIB连接仪器.pdf");
        }
        //“暂停”或“继续”按钮的触发事件
        private void pause_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "暂停")
            {
                this.pause.Text = "继续";
                this.timer1.Stop();
            }
            else
            {
                this.pause.Text = "暂停";
                this.timer1.Start();
            } 
        }
        //“开始”按钮的触发事件
        private void start_Click(object sender, EventArgs e)
        {
            //label2.text是setnotice中表示等级的
            switch (this.label2.Text)
            {
                case "1":
                    this.timer1.Interval = 1000;
                    break;
                case "2":
                    this.timer1.Interval = 800;
                    break;
                case "3":
                    this.timer1.Interval = 600;
                    break;
                case "4":
                    this.timer1.Interval = 400;
                    break;
                case "5":
                    this.timer1.Interval = 200;
                    break;
            }
            this.transformation.Enabled = true;
            this.left.Enabled = true;
            this.right.Enabled = true;
            this.down.Enabled = true;
            this.start.Enabled = false;
            this.pause.Enabled = true;
            //产生一个砖块，用于在主画布上显示
            currentstandardbrick = createbrick();
            //产生下一个砖块，显示在小画布上，用于提示
            nextstandardbrick = createbrick();
            //在主画布上显示currentstandardbrick
            paint(currentstandardbrick.brickpointsduplicate, currentstandardbrick.brickcolorduplicate,panel1g,preservecolor);
            //在小上画布上显示下一个砖块
            paint2(nextstandardbrick.brickpointsduplicate, nextstandardbrick.brickcolorduplicate, panel2g);
            this.timer1.Start();
        }
        //产生砖块
        private standardbrick createbrick()
        {
            //每当产生新砖块时，都说明currentstandardbrick已经更新，所以画到画布上应时应调整位置
            xx = 6;
            yy = 2;
            //随机从bricksarraylist中取出一个砖块
            index = rd.Next(bricksarraylist.Count);
            concretebrick tempconcretebrick=(concretebrick)bricksarraylist[index];
            string tempcode = tempconcretebrick.cbcodeduplicate;
            Color tempcolor = tempconcretebrick.cbcolorduplicate;
            //下面主要是将那些01的代码转换成实际的坐标，只有有坐标才可以在panel1上绘出
            List<Point> list=new List<Point>();
            for (int i = 0; i < tempcode.Length;i++ )
            {
                if (tempcode[i] == '1')
                {
                    Point p = new Point(i % 5, i / 5);
                    //使砖块的坐标以坐标原点为中心，便于后面旋转等操作的坐标变换
                    p.Offset(-2, -2);
                    list.Add(p);
                }

            }
            standardbrick tempstandardbrick=new standardbrick(list.ToArray(),tempcolor);
            //随机选择是否旋转一下
            if (rd.Next(2) == 1)
                clockwise(tempstandardbrick);
            return tempstandardbrick;
        }
        //在panel1上画出格子，便于游戏时调整砖块的位置
        private void paintgrid(Graphics g)
        {
            try
            {
                lock (g)
                {
                    using (Pen p = new Pen(Color.Blue, 1))
                    {
                        for (int i = 1; i < 13; i++)
                            g.DrawLine(p, i * 20, 0, i * 20, 480);
                        for (int j=1; j< 24;j++ )
                            g.DrawLine(p, 0, j * 20, 260, j * 20);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //在panel1上画出currentstandardbrick、
        private void paint(Point[] ps,Color c,Graphics g,Color[,] preservec)
        {
            foreach (Point p in ps)
            {
                lock (g)
                {
                    try
                    {
                        g.FillRectangle(new SolidBrush(c), pointstorects(p));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            //画出网格
            paintgrid(g);
            //画出以前的砖块
            for (int i = 0; i < 13; i++)
                for (int j = 0; j < 24;j++ )
                    if(preservecolor[i,j]!=Color.MidnightBlue)
                    panel1g.FillRectangle(new SolidBrush(preservec[i, j]), i * 20 + 1, j * 20 + 1, 20 - 1, 20 - 1);
        }
        //将坐标画成格子
        private Rectangle pointstorects(Point p)
        {
            Rectangle rect = new Rectangle((p.X+xx) * 20+1, (p.Y+yy) * 20+1, 20 - 1, 20 - 1);
            return rect;
        }
        //在panel2上画出nextstandardbrick
        private void paint2(Point[] ps, Color c, Graphics g)
        {
            g.Clear(Color.MidnightBlue);
            foreach (Point p in ps)
            {
                lock (g)
                {
                    try
                    {
                        g.FillRectangle(new SolidBrush(c), pointstorects2(p));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        //将坐标画成格子
        private Rectangle pointstorects2(Point p)
        {
            Rectangle rect = new Rectangle((p.X +2) * 20 + 1, (p.Y + yy) * 20 + 1, 20 - 1, 20 - 1);
            return rect;
        }
        //panel1的重绘函数
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            this.SuspendLayout();
            if (currentstandardbrick != null)
                paint(currentstandardbrick.brickpointsduplicate, currentstandardbrick.brickcolorduplicate, e.Graphics, preservecolor);
            this.ResumeLayout();
        }
        //控制砖块的下降
        private void timer1_Tick(object sender, EventArgs e)
        {
            //判断是否能下降
            if (moveorstop(currentstandardbrick, 3))
            {
                yy++;
                this.panel1.Refresh();

            }
            //不能
            else
            {
                //保存的panel上已画出的砖块，即更新preservecolor
                preservebricks();
                //更新是否有满格，若有则需要更新preservecolor
                checkandout();
                //更新当前砖块
                currentstandardbrick = nextstandardbrick;
                //检测是否可以显示当前砖块，若显示当前砖块需要的位置都已填满则不能显示
                int showornot=0;
                for (int i = 0; i < currentstandardbrick.brickpointsduplicate.Length;i++ )
                    if (preservecolor[currentstandardbrick.brickpointsduplicate[i].X + 6, currentstandardbrick.brickpointsduplicate[i].Y+2]==Color.MidnightBlue)
                     showornot++;
                if (showornot==currentstandardbrick.brickpointsduplicate.Length)
                {
                    //若能显示
                    nextstandardbrick = createbrick();
                    paint(currentstandardbrick.brickpointsduplicate, currentstandardbrick.brickcolorduplicate, panel1g,preservecolor);
                    paint2(nextstandardbrick.brickpointsduplicate, nextstandardbrick.brickcolorduplicate, panel2g);
                }
                else
                {
                    //若不能，游戏结束
                    this.timer1.Stop();
                    panel2g.Clear(Color.MidnightBlue);
                    xx = 6; yy = 2;
                    paint(currentstandardbrick.brickpointsduplicate, currentstandardbrick.brickcolorduplicate, panel1g,preservecolor);
                    MessageBox.Show("game over!");
                    panel1g.DrawString("GAME OVER!", new Font("Arial BLACK",20f), new SolidBrush(Color.DarkTurquoise), new RectangleF(30, 140, 300, 100));
                    //游戏结束时各个控件都不能用
                    stopfunction();
                    currentstandardbrick = null;
                    nextstandardbrick = null;
                    //panel1.Refresh();//这里刷新会把刚写上去的game over也刷掉（可能refresh是要再重绘一次控件表面，即调用paint函数但这后一次的重绘没有包含game over重绘在内）
                    //panel2.Refresh();
                }
            }
        }
        //顺时针旋转改变currentstandardbrick的坐标
        private void clockwise(standardbrick sb)
        {
           if(sb!=null)
           {
                int temp2;
                for (int i = 0; i <sb.brickpointsduplicate.Length; i++)
                {
                    temp2 = sb.brickpointsduplicate[i].Y;
                    sb.brickpointsduplicate[i].Y = -sb.brickpointsduplicate[i].X;
                    sb.brickpointsduplicate[i].X = temp2;
                }
            }
            else MessageBox.Show("程序出现异常");
        }
        //左移函数
        private void leftfunction(standardbrick sb)
        {
            if (sb != null)
            {
                int temp2 = sb.brickpointsduplicate[0].X;
                for (int i = 1; i < sb.brickpointsduplicate.Length; i++)
                {
                    if (temp2> sb.brickpointsduplicate[i].X)
                        temp2 = sb.brickpointsduplicate[i].X;
                }
                if (temp2 +xx>0)
                    xx--;
            }
            else MessageBox.Show("程序出现异常");
        }
        //右移函数
        private void rightfunction(standardbrick sb)
        {
            if (sb != null)
            {
                int temp2 = sb.brickpointsduplicate[0].X;
                for (int i = 1; i < sb.brickpointsduplicate.Length; i++)
                {
                    if (temp2<sb.brickpointsduplicate[i].X)
                        temp2 = sb.brickpointsduplicate[i].X;
                }
                if (temp2 + xx<12)
                    xx++;
            }
            else MessageBox.Show("程序出现异常");
        }
        //检查是否有满格，若有则需要更新preservecolor
        private Color[,] checkandout()
        {
            int position=0;
            int length=0;
            for (int  j=23; j>=0; j--)
            {
                int sign = 0;
                for (int i=12; i>=0; i--)
                {
                    if (preservecolor[i, j] != Color.MidnightBlue)
                        sign++;
                }
                if (sign == 13)
                {
                    length++;
                    if (position == 0)
                        position = j;
                }
            }
            for (int j = position; j >=length; j--)
                for (int i = 12; i >= 0; i--)
                    preservecolor[i, j] = preservecolor[i, j - length];
            for (int j = length - 1; j >= 0; j--)
                for (int i = 12; i >= 0; i--)
                    preservecolor[i, j] = Color.MidnightBlue;
            //有满格时，增加分数
            score+=10*length;
            this.label4.Text = score.ToString();
            //this.Invoke(new setdelegate(setfunction), new object[] { score.ToString() });
            return preservecolor;
        }
        //记录当前panel1上的每个格子的颜色，保存在preservecolor内
        private Color[,] preservebricks()
        {
            try
            {
                hdc = panel1g.GetHdc();
                for (int i = 0; i < 13; i++)
                    for (int j = 0; j < 24; j++)
                    {
                        //getpixel返回的是颜色的B、G、R的值
                        switch (GetPixel(hdc, i * 20 + 10, j * 20 + 10))
                        {
                            case 0x00d7ff:
                                preservecolor[i, j] = Color.Gold;
                                break;
                            case 0xff:
                                preservecolor[i, j] = Color.Red;
                                break;
                            case 0xcbc0ff:
                                preservecolor[i, j] = Color.Pink;
                                break;
                            case 0x800080:
                                preservecolor[i, j] = Color.Purple;
                                break;
                            case 0x08000:
                                preservecolor[i, j] = Color.Green;
                                break;
                            case 0xff901e:
                                preservecolor[i, j] = Color.DodgerBlue;
                                break;
                            case 0xffff:
                                preservecolor[i, j] = Color.Yellow;
                                break;
                            //case 0xd1ce00:
                            //    preservecolor[i, j] = Color.DarkTurquoise;
                            //    break;
                            case 0x701919:
                                preservecolor[i, j] = Color.MidnightBlue;
                                break;
                        }
                    }
                //每次用完需要释放
                panel1g.ReleaseHdc();
                return preservecolor;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        //快速下降的函数
        private void downfunction(standardbrick sb)
        {
            if (sb != null)
            {
                //若检测能下降则yy一直加
                while (moveorstop(currentstandardbrick,3))
                 yy++;
                //若不能下降
                 this.panel1.Refresh();   
                //保存当前panel1上的图案,保存在preservecolor内
                 preservebricks();
                //检查是否有满格，若有需要更新preservecolor
                 checkandout();
                //更新当前砖块
                 currentstandardbrick = nextstandardbrick;
                 //检测是否可以显示当前砖块，若显示当前砖块需要的位置都已填满则不能显示
                 int showornot = 0;
                 for (int i = 0; i < currentstandardbrick.brickpointsduplicate.Length; i++)
                     if (preservecolor[currentstandardbrick.brickpointsduplicate[i].X + 6, currentstandardbrick.brickpointsduplicate[i].Y + 2] == Color.MidnightBlue)
                         showornot++;
                 if (showornot == currentstandardbrick.brickpointsduplicate.Length)
                 {
                    //若可以显示
                     nextstandardbrick = createbrick();
                     paint(currentstandardbrick.brickpointsduplicate, currentstandardbrick.brickcolorduplicate, panel1g,preservecolor);
                     paint2(nextstandardbrick.brickpointsduplicate, nextstandardbrick.brickcolorduplicate, panel2g);
                 }
                 else
                 { 
                     //若不能，游戏结束
                     this.timer1.Stop();
                     panel2g.Clear(Color.MidnightBlue);
                     xx = 6; yy = 2;
                     paint(currentstandardbrick.brickpointsduplicate, currentstandardbrick.brickcolorduplicate, panel1g,preservecolor);
                     MessageBox.Show("game over!");
                     panel1g.DrawString("GAME OVER!", new Font("Arial BLACK", 20f), new SolidBrush(Color.DarkTurquoise), new RectangleF(30,140, 300, 100));
                    //游戏结束时，所有控件都不能用
                     stopfunction();
                     currentstandardbrick = null;
                     nextstandardbrick = null;
                     
                 }
            }
            else MessageBox.Show("game over!");
        }
        //游戏结束时，设置各个控件不能用
        private void stopfunction()
        {
            this.start.Enabled=false;
            this.pause.Enabled=false;
            this.transformation.Enabled=false;
            this.left.Enabled=false;
            this.right.Enabled=false;
            this.down.Enabled=false;
        }
        //这里是检测是否可以移动
        private bool moveorstop(standardbrick sb,int flag)
        {           
            if (sb != null)
            {
                //这里返回需要检查的位置的坐标
                Point[] checkpoint = inandout(sb.brickpointsduplicate,flag);
                //检查是否在panel1的区域里
                int numberin=0;
                for(int i=0;i<checkpoint.Length;i++)
                {
                    if((checkpoint[i].X+xx)*20>=0&&(checkpoint[i].X+xx)*20<260&&(checkpoint[i].Y+yy)*20>=0&&(checkpoint[i].Y+yy)*20<480)
                        numberin++;
                }
                if(numberin==checkpoint.Length)
                {
                    //若都在panel的区域里，且都未覆盖，返回true
                    hdc = panel1g.GetHdc();
                    //nextcolor存储这些坐标的颜色
                    nextcolor.Clear();
                    for (int i = 0; i < checkpoint.Length; i++)
                    {
                        nextcolor.Add(GetPixel(hdc, (checkpoint[i].X + xx) * 20 + 5, (checkpoint[i].Y + yy) * 20 + 5));
                    }
                    panel1g.ReleaseHdc();
                    int nextcolorcount = 0;
                    for (int i = 0; i < nextcolor.Count; i++)
                    {
                        if ((int)nextcolor[i] ==0x701919)
                            nextcolorcount++;
                    }
                    if (nextcolorcount == nextcolor.Count)
                        return true;
                    else return false;
                }
                else return false;
            }
            else
            {
                MessageBox.Show("程序出现异常");
                return false;
            }
        }
        //这里的功能是输出需要检查的位置的坐标
        private Point[] inandout(Point[] pin,int tag)
        {
            List<Point> plisttemp=new List<Point>();
            if (tag == 3)//若下移，则需要检查的坐标
            {
                for (int i = 0; i < pin.Length; i++)
                {
                    plisttemp.Add(new Point(pin[i].X, pin[i].Y + 1));
                }
                List<Point> plisttemp1 =new List<Point>(plisttemp);
              for (int i = 0; i <pin.Length; i++)
                {
                    for (int j = 0; j < pin.Length; j++)
                        if (plisttemp[i] == pin[j])
                            plisttemp1.Remove(plisttemp[i]);
               }
                pout=plisttemp1.ToArray();
           }
            if (tag == 2)//若变换，则需要检查的坐标
            {
                for (int i = 0; i < pin.Length; i++)
                {
                    plisttemp.Add(new Point(pin[i].Y,-pin[i].X));
                }
                List<Point> plisttemp1 = new List<Point>(plisttemp);
                for (int i = 0; i < pin.Length; i++)
                {
                    for (int j = 0; j < pin.Length; j++)
                        if (plisttemp[i] == pin[j])
                            plisttemp1.Remove(plisttemp[i]);
                }
                pout = plisttemp1.ToArray();
            }
            if (tag == 1)//若左移，则需要检查的坐标
            {
                for (int i = 0; i < pin.Length; i++)
                {
                    plisttemp.Add(new Point(pin[i].X-1, pin[i].Y ));
                }
                List<Point> plisttemp1 = new List<Point>(plisttemp);
                for (int i = 0; i < pin.Length; i++)
                {
                    for (int j = 0; j < pin.Length; j++)
                        if (plisttemp[i] == pin[j])
                            plisttemp1.Remove(plisttemp[i]);
                }
                pout = plisttemp1.ToArray();
            }
            if (tag == 0)//若右移，则需要检查的坐标
            {
                for (int i = 0; i < pin.Length; i++)
                {
                    plisttemp.Add(new Point(pin[i].X+1, pin[i].Y));
                }
                List<Point> plisttemp1 = new List<Point>(plisttemp);
                for (int i = 0; i < pin.Length; i++)
                {
                    for (int j = 0; j < pin.Length; j++)
                        if (plisttemp[i] == pin[j])
                            plisttemp1.Remove(plisttemp[i]);
                }
                pout = plisttemp1.ToArray();
            }
            return pout;
        }
        //↑键的触发函数
        private void transformation_Click(object sender, EventArgs e)
        {
            if (moveorstop(currentstandardbrick, 2))
                clockwise(currentstandardbrick);
            this.panel1.Refresh();
        }
        //←键的触发函数
        private void left_Click(object sender, EventArgs e)
        {
            if (moveorstop(currentstandardbrick, 1))
                leftfunction(currentstandardbrick);
            this.panel1.Refresh();
        }
        //→键的触发函数
        private void right_Click(object sender, EventArgs e)
        {
            if (moveorstop(currentstandardbrick,0))
            rightfunction(currentstandardbrick);
            this.panel1.Refresh();
        }
        //↓键的触发函数
        private void down_Click(object sender, EventArgs e)
        {
            downfunction(currentstandardbrick);
        }
        //用W、A、D、S四个键分与上、左、右、下四个按钮绑定
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (moveorstop(currentstandardbrick, 2))
                    clockwise(currentstandardbrick);
                    this.panel1.Refresh();
                    break;
                case Keys.A:
                    if (moveorstop(currentstandardbrick, 1))
                    leftfunction(currentstandardbrick);
                    this.panel1.Refresh();
                    break;
                case Keys.D:
                    if (moveorstop(currentstandardbrick,0))
                    rightfunction(currentstandardbrick);
                    this.panel1.Refresh();
                    break;
                case Keys.S:
                    downfunction(currentstandardbrick);
                    break;
            }
        }
    }
    //创建这个类是用于描述一个标准样式的砖块（包含坐标和颜色）
    class standardbrick
    {
        Point[] brickpoints;
        Color brickcolor;
        public standardbrick(Point[] p, Color c)
        {
            brickpoints = p;
            brickcolor = c;
        }
        public Point[] brickpointsduplicate
        {
            get { return brickpoints; }
        }
        public Color brickcolorduplicate
        {
            get { return brickcolor; }
        }
    }
    //创建这个类也是用于描述一个具体的砖块（包含样式和颜色）
    class concretebrick
    {
        private string cbcode;
        private Color cbcolor;
        public concretebrick(string str, Color c)
        {
            cbcode = str;
            cbcolor = c;
        }
        public string cbcodeduplicate
        {
            get { return cbcode; }
        }
        public Color cbcolorduplicate
        {
            get { return cbcolor; }
        }
    }
}
