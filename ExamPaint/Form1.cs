using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExamPaint
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        List<Point> points = new List<Point>();
        bool isDraw = false;//режим когда зажата левая кнопка мыши и мы двигаем мышкой
        bool replayMode = false;//режим воспроизведения
        Color brushColor = Color.Red;//цвет кисти
        int countPoints = 0;//количество уже отрисованных точек(он нам понадобиться при воспроиздвении)
        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(panel.ClientSize.Width, panel.ClientSize.Height);
            buttonRun.Enabled = false;//блокируем кнопку
            buttonColor.BackColor = brushColor;//закрышиваем ее
            timer.Interval = 100;
            textBoxSpeed.Text = "1";
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, panel, new object[] { true });
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            bmp = new Bitmap(panel.ClientSize.Width, panel.ClientSize.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            Pen pen = new Pen(brushColor, 5);
            if(points.Count>1 && !replayMode)//когда не включено воспроизведение и количество точек больше 1, чтобы можно было соединить хотя бы 2 точки
            {
                graphics.DrawLines(pen, points.ToArray());//рисуем линию
            }
            else if(replayMode && countPoints>0)//режим воспроизведения. На каждом тике таймера будем отрисовывать какое то количество линий кол-во которых будет увеличиваться по времени
            {
                for(int i =0; i<countPoints-1; i++)
                {
                    graphics.DrawLine(pen, points[i], points[i+1]);//соедением линии
                }
            }
            e.Graphics.DrawImageUnscaled(bmp, 0, 0);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            //кнопка воспроизведения
            timer.Start();
            replayMode = true;
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            isDraw = true;
        }

        private void panel_MouseUp(object sender, MouseEventArgs e)
        {
            isDraw = false;
            buttonRun.Enabled = true;
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            //если зажата левая кнопка мыши значить isDraw=true тогда просто кидаем в список текущие координаты мышки и отрисываем
            if(isDraw)
            {
                points.Add(e.Location);
            }
            panel.Invalidate();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            countPoints++;// на каждом тике добавляем точку для отрисовки
            if(countPoints == points.Count)//завершаем работу таймера если достигли максимального кол-ва точек(т.е все нарисовали)
            {
                timer.Stop();
                countPoints = 0;
                replayMode = false;
            }
            panel.Invalidate();
        }

        private void textBoxSpeed_TextChanged(object sender, EventArgs e)
        {
            int speed;//сюда получим значение лежащее в текс боксе
            if(int.TryParse(textBoxSpeed.Text, out int result))
            {
                speed = result;
                if(speed > 100 || speed == 0)//ограничения, короче у меня 100 это Interval = 1 милисекунда, 1 это Interval = 100 милисек
                {
                    //интервал это как часто срабатыввает это событие то есть гапример интервал 1000 милисекунд это значит срабатывает раз в секунду
                    MessageBox.Show("Максимальная скорость воспроизведения 100. Минимальная 1.");
                    timer.Interval = 100;
                    textBoxSpeed.Text = "1";//просто заносим в текс бокс свой текст
                }
                else if(speed == 100)
                {
                    timer.Interval = 1;
                }
                else timer.Interval = 100 - speed;//устанавливаем интервал в зависимости от скорости

            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();//диалог выбора цвета
            if(colorDialog.ShowDialog() == DialogResult.OK)
            {
                brushColor = colorDialog.Color;//меняем цвет кисти
                buttonColor.BackColor = brushColor;//меняем цвет кнопки
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            //кнопка очищение поверхности рисования
            points.Clear();
            countPoints = 0;
            buttonRun.Enabled = false;
            panel.Invalidate();
        }

        private void textBoxSpeed_KeyPress(object sender, KeyPressEventArgs e)
        {
            //это просто ограничения на ввод, то есть можем вводить только 0-9 и ентер и удалить
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != Convert.ToChar(8))
            {
                e.Handled = true;
            }
        }
    }
}
