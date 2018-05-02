using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MHXXDamageCalc
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> str2Pict = new Dictionary<string, string>(); //Stores conversion of strings to relative paths.
        Dictionary<string, Tuple<double, double>> sharpnessMods = new Dictionary<string, Tuple<double, double>>(); //Stores conversion of sharpness to sharpness modifiers.
        //The first number in the Tuple is the raw sharpness modifier, the second is the elemental sharpness modifier.

        public Form1()
        {
            InitializeComponent();
            setUp();
            setSelected();
        }

        public void setUp()
        {
            str2Pict.Add("Fire", "./Images/Fire.png");
            str2Pict.Add("Water", "./Images/Water.png");
            str2Pict.Add("Thunder", "./Images/Thunder.png");
            str2Pict.Add("Ice", "./Images/Ice.png");
            str2Pict.Add("Dragon", "./Images/Dragon.png");
            str2Pict.Add("Poison", "./Images/Poison.png");
            str2Pict.Add("Para", "./Images/Para.png");
            str2Pict.Add("Sleep", "./Images/Sleep.png");
            str2Pict.Add("Blast", "./Images/Blast.png");

            sharpnessMods.Add("(No Sharpness)", new Tuple<double, double>(1.00, 1.00));
            sharpnessMods.Add("Purple *", new Tuple<double, double>(1.39, 1.20));
            sharpnessMods.Add("White", new Tuple<double, double>(1.32, 1.125));
            sharpnessMods.Add("Blue", new Tuple<double, double>(1.39, 1.20));
            sharpnessMods.Add("Green", new Tuple<double, double>(1.39, 1.20));
            sharpnessMods.Add("Yellow", new Tuple<double, double>(1.00, 0.75));
            sharpnessMods.Add("Orange", new Tuple<double, double>(0.75, 0.50));
            sharpnessMods.Add("Red", new Tuple<double, double>(0.50, 0.25));
        }

        public void setSelected()
        {
            paraAltType.SelectedIndex = 0;
            paraSharp.SelectedIndex = 0;
            paraEleCrit.SelectedIndex = 0;
            paraSecEle.SelectedIndex = 0;
            paraMonStatus.SelectedIndex = 0;
        }
        
        //EVENT FUNCTIONS
        /// <summary>
        /// Checks if whatever's put into the field can be converted into a double.
        /// If no, then throws an error.
        /// If yes, then allows the user to continue.
        /// </summary>
        /// <param name="sender">Any field in which the user can input numbers.</param>
        /// <param name="e"></param>
        private void GenericField_Validating(object sender, CancelEventArgs e)
        {
            if (!CalcFieldValidation(((TextBox)sender).Text, out string errorMsg))
            {
                e.Cancel = true;
                ((TextBox)sender).Select(0, ((TextBox)sender).Text.Length);

                this.ErrorPreventer.SetError((TextBox)sender, errorMsg);
            }
        }

        /// <summary>
        /// Resets the ErrorPreventer if the input is correct.
        /// </summary>
        /// <param name="sender">Any field in which the user can input numbers.</param>
        /// <param name="e"></param>
        private void GenericField_Validated(object sender, System.EventArgs e)
        {
            ErrorPreventer.SetError((TextBox)sender, "");
        }

        private void IntField_Validating(object sender, CancelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!int.TryParse(textBox.Text, out int result))
            {
                e.Cancel = true;
                ((TextBox)sender).Select(0, ((TextBox)sender).Text.Length);

                this.ErrorPreventer.SetError((TextBox)sender, "Enter in a valid integer.");
            }
        }

        /// <summary>
        /// Special version of Generic Validating method for Affinity fields.
        /// Limits the input to between 100 and -100.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AffinityField_Validating(object sender, CancelEventArgs e)
        {
            if (!CalcFieldValidation(((TextBox)sender).Text, out string errorMsg))
            {
                e.Cancel = true;
                ((TextBox)sender).Select(0, ((TextBox)sender).Text.Length);

                this.ErrorPreventer.SetError((TextBox)sender, errorMsg);
            }
            else if (double.Parse(((TextBox)sender).Text) > 100)
            {
                ((TextBox)sender).Text = 100.ToString();
            }
            else if (double.Parse(((TextBox)sender).Text) < -100)
            {
                ((TextBox)sender).Text = "-100";
            }
        }

        private void PositiveAffinityField_Validating(object sender, CancelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!CalcFieldValidation(((TextBox)sender).Text, out string errorMsg))
            {
                e.Cancel = true;
                textBox.Select(0, (textBox.Text.Length));

                this.ErrorPreventer.SetError((TextBox)sender, errorMsg);
            }

            else if (double.Parse(textBox.Text) > 100)
            {
                ((TextBox)sender).Text = "100";
            }

            else if (double.Parse(textBox.Text) < 0)
            {
                ((TextBox)sender).Text = "0";
            }
        }

        /// <summary>
        /// Validates whatever's put into the field to doubles.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns></returns>
        public bool CalcFieldValidation(string input, out string ErrorMessage)
        {
            if (!double.TryParse(input, out double result))
            {
                ErrorMessage = "Enter in a valid number.";
                return false;
            }
            else
            {
                ErrorMessage = "";
                return true;
            }
        }

        private void paraChaotic_CheckedChanged(object sender, EventArgs e)
        {
            if(paraChaotic.Checked)
            {
                paraAffinity.Visible = false;

                paraPositive.Visible = true;
                paraPosAff.Visible = true;
                paraNega.Visible = true;
                paraNegAff.Visible = true;
            }
            else
            {
                paraAffinity.Visible = true;

                paraPositive.Visible = false;
                paraPosAff.Visible = false;
                paraNega.Visible = false;
                paraNegAff.Visible = false;
            }
        }

        private void paraAltType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(paraAltType.SelectedIndex == 0)
            {
                paraElePower.Text = "0";
                paraElePower.Enabled = false;

                paraEleHit.Text = "0";
                paraEleHit.Enabled = false;

                paraEleSharp.Text = "1";
                paraEleSharp.Enabled = false;

                paraEleCrit.SelectedIndex = 0;
                paraEleCrit.Enabled = false;

                paraSecEle.SelectedIndex = 0;
                paraSecEle.Enabled = false;

                calcEleOut.Text = "0";
                calcEleOut.Enabled = false;

                calcEleAll.Text = "0";
                calcEleAll.Enabled = false;

                paraElePict.Image = null;
                calcElePict.Image = null;
                calcAllElePict.Image = null;
                return;
            }

            paraElePower.Enabled = true;
            paraEleHit.Enabled = true;
            paraEleSharp.Enabled = true;
            paraEleCrit.Enabled = true;
            paraSecEle.Enabled = true;
            calcEleOut.Enabled = true;
            calcEleAll.Enabled = true;

            string path = str2Pict[(string)paraAltType.SelectedItem];
            paraElePict.Load(path);
            calcElePict.Load(path);
            calcAllElePict.Load(path);
            return;
        }

        private void paraSecEle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(paraSecEle.SelectedIndex == 0)
            {
                paraSecPower.Text = "0";
                paraSecPower.Enabled = false;

                paraSecZone.Text = "0";
                paraSecZone.Enabled = false;

                calcSecOut.Text = "0";
                calcSecOut.Enabled = false;

                calcSecAll.Text = "0";
                calcSecAll.Enabled = false;

                paraSecPict.Image = null;
                calcSecPict.Image = null;
                calcAllSecPict.Image = null;
                return;
            }

            paraSecPower.Enabled = true;
            paraSecZone.Enabled = true;
            calcSecOut.Enabled = true;
            calcSecAll.Enabled = true;

            string path = str2Pict[(string)paraSecEle.SelectedItem];
            paraSecPict.Load(path);
            calcSecPict.Load(path);
            calcAllSecPict.Load(path);
            return;
        }

        private void paraSharp_SelectedIndexChanged(object sender, EventArgs e)
        {
            string currSharp = (string)paraSharp.SelectedItem;
            Tuple<double, double> sharpnessVals = sharpnessMods[currSharp];

            paraRawSharp.Text = sharpnessVals.Item1.ToString();
            paraEleSharp.Text = sharpnessVals.Item2.ToString();
        }


        private void calcOutput_Click(object sender, EventArgs e)
        {
            calcDetails.ResetText();
            Tuple<double, double, double, double> calcOutput = CalculateDamage();
        }

        private Tuple<double, double, double, double> CalculateDamage()
        {
            //Collecting parameter info
            double raw = double.Parse(paraRaw.Text);
            double rawSharp = double.Parse(paraRawSharp.Text);

            string altType = paraAltType.Text;
            double element = double.Parse(paraElePower.Text);
            double elementSharp = double.Parse(paraEleSharp.Text);

            string eleCrit = paraEleCrit.Text;

            Tuple<double, double> affinity;
            if(!paraChaotic.Checked)
            {
                affinity = new Tuple<double, double>(double.Parse(paraAffinity.Text), 0);
            }
            else
            {
                affinity = new Tuple<double, double>(double.Parse(paraPosAff.Text), double.Parse(paraNegAff.Text));
            }

            string secType = paraSecEle.Text;
            double secElement = double.Parse(paraSecPower.Text);

            double motionValue = double.Parse(paraAvgMV.Text);
            int hitCount = int.Parse(paraHitCount.Text);

            double KO = double.Parse(paraKO.Text);
            double exhaust = double.Parse(paraExh.Text);

            bool fixedType = paraFixed.Checked;
            bool critBoost = paraCritBoost.Checked;
            bool mindsEye = paraMinds.Checked;
            bool statusCrit = paraStatusCrit.Checked;
            bool madAffinity = paraMadAff.Checked;



            throw new NotImplementedException();
            //string sharpness
        }

        private bool isElement(string element)
        {
            if (element == "Fire" || element == "Water" || element == "Thunder" || element == "Ice" || element == "Dragon")
            {
                return true;
            }
            return false;
        }

        private bool isStatus(string element)
        {
            if (element == "Poison" || element == "Para" || element == "Sleep")
            {
                return true;
            }
            return false;
        }
    }
}
