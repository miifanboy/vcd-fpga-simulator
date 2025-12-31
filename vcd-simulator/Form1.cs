using CustomControls.RJControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace vcd_simulator
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        struct reg
        {
            public char id;
            public string name;
            public int size;
            public string value;
        }
        string codes = "gfedcba";
        bool isLoaded = false;
        Dictionary<int, Dictionary<char,reg>> vcd;
        Dictionary<char, reg> registersTemp;
        Dictionary<int,int> timeLine;

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        
        
        void LoadToDictionary(string path)
        {
            string[] lines = File.ReadAllLines(path);

            int linenum = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("$var"))
                {
                    reg temp = new reg();

                    string[] s_line = line.Split(' ');
                    temp.name = s_line[4];
                    temp.size = Convert.ToInt32(s_line[2]);
                    temp.id = Convert.ToChar(s_line[3]);
                    if (registersTemp.ContainsKey(temp.id))
                        registersTemp[temp.id] = temp;
                    else
                        registersTemp.Add(temp.id, temp);

                }
                if (line.StartsWith("#"))
                {
                    int time = Convert.ToInt32(line.Substring(1));
                    timeLine.Add(time, linenum);
                }

                linenum++;
            }

            List<int> times = timeLine.Keys.ToList();

            for(int i = 0; i < times.Count; i++)
            {
                Dictionary<char, reg> registers = new Dictionary<char, reg>();
                if (i - 1 >= 0)
                {
                    vcd[times[i - 1]].ToList().ForEach(v =>
                    {
                        registers.Add(v.Key,v.Value);

                    });
                }
                
                int lineorder = timeLine[times[i]];
                if (i + 1 < times.Count)
                {
                    for (int j = lineorder; j < timeLine[times[i + 1]]; j++)
                    {
                        string line = lines[j];
                        if (line.StartsWith("b"))
                        {
                            string newline = line.Substring(1);
                            string[] s_reg = newline.Split(' ');
                            char id = Convert.ToChar(s_reg[1]);
                            if (s_reg[0] != "x")
                            {

                                reg newreg = registersTemp[id];

                                string val = s_reg[0].PadLeft(newreg.size, '0');
                                newreg.value = val;
                                if (registers.ContainsKey(newreg.id))
                                    registers[newreg.id] = newreg;
                                else
                                    registers.Add(newreg.id, newreg);
                            }
                        }
                        if (line.StartsWith("0") || line.StartsWith("1"))
                        {
                            reg newreg = registersTemp[line[1]];

                            string val = Convert.ToString(line[0]);
                            newreg.value = val;

                            if (registers.ContainsKey(newreg.id))
                                registers[newreg.id] = newreg;
                            else
                                registers.Add(newreg.id, newreg);
                        }
                    }
                }
                else
                {
                    for (int j = lineorder; j < lines.Length; j++)
                    {
                        string line = lines[j];
                        if (line.StartsWith("b"))
                        {
                            string newline = line.Substring(1);
                            string[] s_reg = newline.Split(' ');
                            char id = Convert.ToChar(s_reg[1]);
                            if (s_reg[0] != "x")
                            {

                                reg newreg = registersTemp[id];

                                string val = s_reg[0].PadLeft(newreg.size, '0');
                                newreg.value = val;
                                if (registers.ContainsKey(newreg.id))
                                    registers[newreg.id] = newreg;
                                else
                                    registers.Add(newreg.id, newreg);
                            }
                        }
                        if (line.StartsWith("0") || line.StartsWith("1"))
                        {
                            reg newreg = registersTemp[line[1]];

                            string val = Convert.ToString(line[0]);
                            newreg.value = val;

                            if (registers.ContainsKey(newreg.id))
                                registers[newreg.id] = newreg;
                            else
                                registers.Add(newreg.id, newreg);
                        }
                    }
                }
                vcd.Add(times[i], registers);
            }
            isLoaded = true;
        }
        void SetupSlider()
        {    
            trackBar1.SetRange(0,vcd.Keys.Count-1);

        }


        void setTime()
        {
            List<int> times = timeLine.Keys.ToList<int>();

            timeLabel.Text = Convert.ToString(times[trackBar1.Value]);
        }
        void FlushSSDS()
        {

            for (int i = 0; i < 4; i++)
            {
                foreach (var c in codes)
                {
                    Control ssd = Controls.Find("SSD" + Convert.ToString(i) + c, false)[0];
                    if (ssd is PictureBox)
                    {
                        ssd.BackColor = Color.DimGray;
                    }

                }
            }
        }
        void FlushLEDS()
        {
            for (int i = 0; i < 8; i++)
            {
                Control led = Controls.Find("LED" + Convert.ToString(i), false)[0];
                if (led is PictureBox)
                {
                    led.BackColor = Color.DimGray;
                }
            }
        }
        void FlushSwitches()
        {
            for (int i = 0; i < 4; i++)
            {
                Control sw = Controls.Find("SW" + Convert.ToString(i), false)[0];
                if (sw is RJToggleButton cb)
                {
                    cb.Checked = false;
                }
            }
        }
        void FlushButtons()
        {
            BTN0.Checked = false;
            BTN1.Checked = false;
            BTN2.Checked = false;
            BTN3.Checked = false;
        }
        void RenderSSD(string name,string val)
        {
            int idx = 0;
            foreach(var c in codes)
            {
                Control pictureBox = Controls.Find(name + c,false)[0];
                if(pictureBox is PictureBox)
                {
                    if (val[idx] == '1')
                    {
                        pictureBox.BackColor = Color.Red;

                    }
                    else
                    {
                        pictureBox.BackColor = Color.DimGray;
                    }
                }
                idx++;
            }
            
        }
        void RenderLEDS(string name, string val)
        {
            for(int i = 0; i < val.Length; i++)
            {
                Control pictureBox = Controls.Find("LED" + Convert.ToString(val.Length - i - 1), false)[0];
                if (pictureBox is PictureBox)
                {
                    if (val[i] == '1')
                    {
                        pictureBox.BackColor = Color.Red;

                    }
                    else
                    {
                        pictureBox.BackColor = Color.DimGray;
                    }
                }
            }
        }

        void RenderSwitches(string name, string val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                Control switches = Controls.Find("SW" + Convert.ToString(val.Length - i - 1), false)[0];
                if (switches is RJToggleButton cb)
                {
                    if (val[i] == '1')
                    {
                        cb.Checked = true;

                    }
                    else
                    {
                        cb.Checked = false;
                    }
                }
            }
        }

        void RenderButtons(string name, string val)
        {
            if(name == "enterA")
            {
                if(val == "1")BTN3.Checked = true;
                else BTN3.Checked = false;

            }
            else if (name == "enterB")
            {
                if (val == "1") BTN0.Checked = true;
                else BTN0.Checked = false;

            }
            else if (name == "rst")
            {
                if (val == "1") BTN2.Checked = true;
                else BTN2.Checked = false;
            }

        }

        void SetState(string name, string val)
        {
            stateName.Text = Convert.ToString(Convert.ToInt32(val, 2));
        }
        void SetClk(string name, string val)
        {
            clkText.Text = val;
        }
        void Render()
        {
            List<int> timeline = vcd.Keys.ToList();
            timeline.Sort();
            FlushSSDS();
            FlushLEDS();
            FlushSwitches();
            FlushButtons();
            vcd[timeline[trackBar1.Value]].Values.ToList().ForEach(v => { 
                if(v.name.StartsWith("SSD") && v.value != null)
                {
                    RenderSSD(v.name, v.value);
                }
                else if (v.name.StartsWith("LEDX") && v.value != null)
                {
                    RenderLEDS(v.name, v.value);
                }
                else if (v.name.StartsWith("letterIn") && v.value != null)
                {
                    RenderSwitches(v.name, v.value);
                }
                else if (v.name.StartsWith("enter") && v.value != null)
                {
                    RenderButtons(v.name, v.value);
                }
                else if (v.name.StartsWith("rst") && v.value != null)
                {
                    RenderButtons(v.name, v.value);
                }
                else if (v.name.StartsWith("cState") && v.value != null)
                {
                    SetState(v.name, v.value);
                }
                else if (v.name.StartsWith("clk") && v.value != null)
                {
                    SetClk(v.name, v.value);
                }


            });
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                vcd = new Dictionary<int, Dictionary<char, reg>>();
                registersTemp = new Dictionary<char, reg>();
                timeLine = new Dictionary<int, int>();

                LoadToDictionary(ofd.FileName);
                SetupSlider();
                Render();
                setTime();
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (isLoaded)
            {
                Render();
                setTime();
            }
        }
    }
}
