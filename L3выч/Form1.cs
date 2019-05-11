using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ZedGraph;

namespace L3выч
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PaneSettings();
        }
        LineItem curve1;
        GraphPane pane;
        //Настраиваем график
        private void PaneSettings()
        {
            GraphPane pane = zedGraphControl1.GraphPane;
            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.Scale.MaxAuto = true;
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.Scale.MaxAuto = true;
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            pane.XAxis.Title.Text = "Ось X";
            pane.YAxis.Title.Text = "Ось Y";
            pane.Title.Text = "Графики функций";
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = false;
            pane.XAxis.MajorGrid.IsZeroLine = false;
            pane.YAxis.MajorGrid.IsZeroLine = false;
            pane.XAxis.Scale.FontSpec.Size = 12;
            pane.YAxis.Scale.FontSpec.Size = 12;
        }
        //Исходна система уравнений для решения методом Эйлера
        double dy1_dx(double y1, double y2, double y3)
        {
            return 2 * (y1) + y2 - 2 * (y3);
        }
        double dy2_dx(double y1, double y2, double y3)
        {
            return y1 - 2 * (y2);
        }
        double dy3_dx(double y1, double y2, double y3)
        {
            return y2 - y3;
        }
        //Собственные числа матрицы A
        double a1 = 2.08613019765150, a2 = -2.51413692933529, a3 = -0.571993268316203;
        //Матрицы М1, М2, М3
        double[,] M2 = new double[3, 3] { { 0.0871324848481126,	-0.393327706979884,	0.115091885878961 }, { -0.169473197619615,	0.765025275326572,	-0.223854509360269 }, { 0.111927254680135,	-0.505254961660019,	0.147843058986419} };
        double[,] M3 = new double[3, 3] { { -0.118392793539760,	0.304505125337512,	0.553227488161811 }, { -0.0829074542747664,	0.213237023559306,	0.387412579612278}, { -0.193706289806139,	0.498211415143651,	0.905154728509096} };
        double[,] M1 = new double[3, 3] { { 1.03126030869165, 0.0888225816423723, -0.668319374040772 }, { 0.252380651894381, 0.0217377011141224, -0.163558070252009 }, { 0.0817790351260045, 0.00704354651636767, -0.0529977874955145 } };
        //Решение методом Лагранжа-Сильвестра
        double FUN1(double x, double y10, double y20, double y30)
        {
            return (M1[0, 0] * y10 + M1[0, 1] * y20 + M1[0, 2] * y30) * Math.Pow(Math.E, (a1 * x)) +
                   (M2[0, 0] * y10 + M2[0, 1] * y20 + M2[0, 2] * y30) * Math.Pow(Math.E, (a2 * x)) +
                   (M3[0, 0] * y10 + M3[0, 1] * y20 + M3[0, 2] * y30) * Math.Pow(Math.E, (a3 * x));
        }
        double FUN2(double x, double y10, double y20, double y30)
        {
            return (M1[1, 0] * y10 + M1[1, 1] * y20 + M1[1, 2] * y30) * Math.Pow(Math.E, (a1 * x)) +
                   (M2[1, 0] * y10 + M2[1, 1] * y20 + M2[1, 2] * y30) * Math.Pow(Math.E, (a2 * x)) +
                   (M3[1, 0] * y10 + M3[1, 1] * y20 + M3[1, 2] * y30) * Math.Pow(Math.E, (a3 * x));
        }
        double FUN3(double x, double y10, double y20, double y30)
        {
            return (M1[2, 0] * y10 + M1[2, 1] * y20 + M1[2, 2] * y30) * Math.Pow(Math.E, (a1 * x)) +
                   (M2[2, 0] * y10 + M2[2, 1] * y20 + M2[2, 2] * y30) * Math.Pow(Math.E, (a2 * x)) +
                   (M3[2, 0] * y10 + M3[2, 1] * y20 + M3[2, 2] * y30) * Math.Pow(Math.E, (a3 * x));
        }
        //Функция обработки события нажатия на кнопку "Решить систему и постоить график"
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //Проверка введенных значений на корректность
                if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "")
                    throw new Exception("Введите число.");
                //Считываем и проверяем введенные данные
               double left = 0;
               double right = double.Parse(textBox2.Text);
                if (left > right || left == right) throw new ApplicationException("Правая граница должна быть больше левой.");
                if ((left > 300)||(right > 300)) { throw new Exception("Значение функции выходит за границу типа double."); }
                //Списки точек для построения графиков
                PointPairList F1 = new PointPairList();
                PointPairList F2 = new PointPairList();
                PointPairList F3 = new PointPairList();
                //Начальные условия
                double y10 = double.Parse(textBox1.Text);
                double y20 = double.Parse(textBox4.Text);
                double y30 = double.Parse(textBox3.Text);
                if ((y10 == 0) && (y20 == 0)&&(y30==0)) { throw new Exception("Значения функций равны 0."); }
                //Задаем шаг
                double step = (right - left) / 200;
                
                //Метод Лагранжа-Сильвестра
               
                   //Очищаем списки точек
                    F1.Clear();
                    F2.Clear();
                    F3.Clear();
                    double y11 = 0.0, y21 = 0.0, y31 = 0.0;
                    int i = 0;
                    for (double x = left; x <= right; x += step)
                    {
                        //Вычисляем значения функций
                        y11 = FUN1(x, y10, y20, y30);  
                        y21 = FUN2(x, y10, y20, y30);
                        y31 = FUN3(x, y10, y20, y30);
                        //Добавляем точки в списки
                        F1.Add(x, y11);
                        F2.Add(x, y21);
                        F3.Add(x, y31);
                        i++;
                    }
                
                //Настраиваем график
                pane = zedGraphControl1.GraphPane;
                pane.CurveList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                //Настройка панели для построения графика
                pane = zedGraphControl1.GraphPane;
                zedGraphControl1.AutoSize = false; pane.XAxis.Scale.MaxAuto = true;
                pane.XAxis.Scale.MinAuto = true;
                pane.YAxis.Scale.MaxAuto = true;
                pane.YAxis.Scale.MinAuto = true;
                //Установка масштаба 
                pane.XAxis.Scale.Min = left;
                pane.XAxis.Scale.Max = right;
                pane.XAxis.Scale.Format = "F2";
                pane.XAxis.Scale.FontSpec.Size = 12;
                pane.YAxis.Scale.FontSpec.Size = 12;
                //Очищаем список кривых
                pane.CurveList.Clear();
                //Определяем заголовки
                pane.Title.Text = "Графики функций";
                pane.XAxis.Title.Text = pane.XAxis.Title.Text = "Ось X";
                pane.YAxis.Title.Text = pane.YAxis.Title.Text = "Ось Y";
                //Строим графики функций
                Draw1(F1);
                Draw2(F2);
                Draw3(F3);
            }
            catch (Exception ea)
            {
                MessageBox.Show(ea.Message);
            }
        }
        //Построение графика первой функции
        private void Draw1(PointPairList list)
        {
            try
            {
                //Очищение панели
                pane.CurveList.Clear();
                //Построение графика функции
                curve1 = pane.AddCurve("функция 1", list, Color.Gray, SymbolType.None);
                //Задаем ширину линии
                curve1.Line.Width = 2;
                //Обновляем данные
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
            }
            //Обработка исключений
            catch (Exception m)
            {
                MessageBox.Show(m.Message);
            }
        }
        //Построение графика второй функции
        private void Draw2(PointPairList list)
        {
            //Сортировка массива точек
            list.TrimExcess();
            list.Sort();
            try
            {
                //Построение графика функции
                curve1 = pane.AddCurve("функция 2", list, Color.Blue, SymbolType.None);
                //Задаем ширину линии
                curve1.Line.Width = 2;
                //Обновляем данные
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
            }
            //Обработка исключений
            catch (Exception m)
            {
                MessageBox.Show(m.Message);
            }
        }
        //Построение графика второй функции
        private void Draw3(PointPairList list)
        {
            //Сортировка массива точек
            list.TrimExcess();
            list.Sort();
            try
            {
                //Построение графика функции
                curve1 = pane.AddCurve("функция 3", list, Color.DarkCyan, SymbolType.None);
                //Задаем ширину линии
                curve1.Line.Width = 2;
                //Обновляем данные
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
            }
            //Обработка исключений
            catch (Exception m)
            {
                MessageBox.Show(m.Message);
            }
        }
        //Проверка
        private void Ch(TextBox textBox1, KeyPressEventArgs e)
        {
            //Проверяем, не является ли запятая первым введенным символом
            if (e.KeyChar == ',' && textBox1.Text.Length == 0)
            //Выводим окно, сообщающее об ошибке ввода.
            {
                MessageBox.Show(this, "Введите число.", "Ошибка");
                e.Handled = true;
            }
            //Проверяем, что минус является первым символом
            if (e.KeyChar == '-' && textBox1.Text.Length != 0)
            //Выводим окно, сообщающее об ошибке ввода.
            {
                MessageBox.Show(this, "Минус должен быть первым символом.", "Ошибка");
                e.Handled = true;
            }
            //Проверяем, не ввел ли пользователь запятую второй раз.
            if (e.KeyChar == ',' && textBox1.Text.IndexOf(",") != -1)
            {
                //Выводим окно, сообщающее об ошибке ввода.
                { MessageBox.Show(this, "Запятая уже введена.", "Ошибка"); }
                //Неправильно введенный символ не обрабатываем и не выводим его в TextBox.
                e.Handled = true;
            }
            else
            {
                //Проверяем, не ввел ли пользователь минус второй раз.
                if (e.KeyChar == ',' && textBox1.Text.IndexOf(",") != -1)
                {
                    //Выводим окно, сообщающее об ошибке ввода.
                    { MessageBox.Show(this, "Минус уже введен.", "Ошибка"); }
                    //Неправильно введенный символ не обрабатываем и не выводим его в TextBox.
                    e.Handled = true;
                }
                //Проверяем введенный символ: если он не равен Backspase, запятой, минусу или цифре от 0 до 9.
                if ((e.KeyChar != (char)45) && (e.KeyChar != (char)44) && (e.KeyChar != (char)8) && (e.KeyChar < (char)48 || e.KeyChar > (char)57))
                {
                    //Выводим окно, сообщающее об ошибке ввода.
                    { MessageBox.Show(this, "Введен недопустимый символ. Введите число.", "Ошибка"); }
                    //Неправильно введенный символ не обрабатываем и не выводим его в TextBox.
                    e.Handled = true;
                }
            }
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            Ch(textBox1, e);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            Ch(textBox2, e);
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            Ch(textBox4, e);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            Ch(textBox3, e);
        }

       
    }

}

