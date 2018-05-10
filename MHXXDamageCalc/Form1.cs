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
        string[] originalSharpness = new string[] { "(No Sharpness)", "Purple *", "White", "Blue", "Green", "Yellow", "Orange", "Red" };

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
            sharpnessMods.Add("Blue", new Tuple<double, double>(1.20, 1.06));
            sharpnessMods.Add("Green", new Tuple<double, double>(1.05, 1.0));
            sharpnessMods.Add("Yellow", new Tuple<double, double>(1.00, 0.75));
            sharpnessMods.Add("Orange", new Tuple<double, double>(0.75, 0.50));
            sharpnessMods.Add("Red", new Tuple<double, double>(0.50, 0.25));


        }

        public void setSelected()
        {
            weapEle.SelectedIndex = 0;
            weapSharpness.SelectedIndex = 0;
            weapSec.SelectedIndex = 0;

            paraAltType.SelectedIndex = 0;
            paraSharp.SelectedIndex = 0;
            paraEleCrit.SelectedIndex = 0;
            paraSecEle.SelectedIndex = 0;
            paraMonStatus.SelectedIndex = 0;

            calcAverage.Select();
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

                paraEleSharp.Text = "1.0";
                paraEleSharp.Enabled = false;

                paraEleCrit.SelectedIndex = 0;
                paraEleCrit.Enabled = false;

                paraSecEle.SelectedIndex = 0;
                paraSecEle.Enabled = false;

                paraStatusCrit.Checked = false;
                paraStatusCrit.Enabled = false;

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

            if(paraAltType.SelectedIndex >= 1 && paraAltType.SelectedIndex <= 5)
            {
                paraStatusCrit.Enabled = false;
            }
            else
            {
                paraStatusCrit.Enabled = true;
            }

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

            if (paraSecEle.SelectedIndex >= 1 && paraSecEle.SelectedIndex <= 5)
            {
                paraStatusCrit.Enabled = false;
            }
            else
            {
                paraStatusCrit.Enabled = true;
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

        private void paraFixed_CheckedChanged(object sender, EventArgs e)
        {
            if (paraFixed.Checked)
            {
                paraRaw.Text = "100";
                paraRaw.Enabled = false;

                paraSharp.SelectedIndex = 0;
                paraSharp.Enabled = false;

                paraRawSharp.Text = "1.0";
                paraRawSharp.Enabled = false;

                paraChaotic.Checked = false;
                paraChaotic.Enabled = false;

                paraAffinity.Text = "0";
                paraAffinity.Enabled = false;

                paraPosAff.Text = "0";
                paraPosAff.Enabled = false;

                paraNegAff.Text = "0";
                paraNegAff.Enabled = false;

                paraEleCrit.SelectedIndex = 0;
                paraEleCrit.Enabled = false;

                paraHitzone.Text = "0";
                paraHitzone.Enabled = false;

                paraCritBoost.Checked = false;
                paraCritBoost.Enabled = false;

                paraStatusCrit.Checked = false;
                paraStatusCrit.Enabled = false;

                paraMadAff.Checked = false;
                paraMadAff.Enabled = false;
            }

            else
            {
                paraRaw.Enabled = true;
                paraSharp.Enabled = true;
                paraRawSharp.Enabled = true;
                paraChaotic.Enabled = true;
                paraAffinity.Enabled = true;
                paraPosAff.Enabled = true;
                paraNegAff.Enabled = true;
                paraEleCrit.Enabled = true;
                paraHitzone.Enabled = true;
                if (paraAltType.SelectedIndex >= 1 && paraAltType.SelectedIndex <= 5)
                {
                    paraStatusCrit.Enabled = false;
                }
                else
                {
                    if (paraSecEle.SelectedIndex >= 1 && paraSecEle.SelectedIndex <= 5)
                    {
                        paraStatusCrit.Enabled = false;
                    }
                    else
                    {
                        paraStatusCrit.Enabled = true;
                    }
                }
                paraCritBoost.Enabled = true;
                paraStatusCrit.Enabled = true;
                paraMadAff.Enabled = true;
            }
        }

        private void calcOutput_Click(object sender, EventArgs e)
        {
            calcDetails.ResetText();
            Tuple<double, double, double, double> calcOutput = CalculateDamage();

            calcRawWeap.Text = calcOutput.Item1.ToString("N2");
            calcRawOut.Text = (calcOutput.Item2 * int.Parse(paraHitCount.Text)).ToString("N2");
            calcEleOut.Text = (calcOutput.Item3 * int.Parse(paraHitCount.Text)).ToString("N2");
            calcSecOut.Text = (calcOutput.Item4 * int.Parse(paraHitCount.Text)).ToString("N2");

            EffectiveRawCalc(calcOutput);
        }

        private void calcAll_Click(object sender, EventArgs e)
        {
            calcDetails.ResetText();
            Tuple<double, double, double, double> calcOutput = CalculateDamage();

            calcRawWeap.Text = calcOutput.Item1.ToString("N2");
            calcRawOut.Text = (calcOutput.Item2 * int.Parse(paraHitCount.Text)).ToString("N2");
            calcEleOut.Text = (calcOutput.Item3 * int.Parse(paraHitCount.Text)).ToString("N2");
            calcSecOut.Text = (calcOutput.Item4 * int.Parse(paraHitCount.Text)).ToString("N2");

            EffectiveRawCalc(calcOutput);

            Tuple<double, bool, double, double, double, double, double> allTuple = CalculateAllDamage(calcOutput);
            calcFinal.Text = allTuple.Item1.ToString();

            if(allTuple.Item2)
            {
                calcBounce.Text = "No";
            }
            else
            {
                calcBounce.Text = "Yes";
            }

            calcRawAll.Text = allTuple.Item3.ToString("N2"); //But with use of the outputted tuple from the moreDamage function.

            calcEleAll.Text = allTuple.Item4.ToString("N2");
            calcSecAll.Text = allTuple.Item5.ToString("N2");

            calcKOAll.Text = allTuple.Item6.ToString("N2");
            calcExhAll.Text = allTuple.Item7.ToString("N2");
            HealthCalc(allTuple); //Estimate how many hits it would take to kill the monster.
        }

        private void EffectiveRawCalc(Tuple<double, double, double, double> calcOutput)
        {
            int hitCount = int.Parse(paraHitCount.Text);
            string rawWeap = calcOutput.Item1.ToString("N2");
            string rawOut = (calcOutput.Item2 * hitCount).ToString("N2");
            string eleOut = calcOutput.Item3.ToString("N2");
            string eleCombo = (calcOutput.Item3 * hitCount).ToString("N2");
            string secOut = calcOutput.Item4.ToString("N2");
            string secCombo = (calcOutput.Item4 * hitCount).ToString("N2");

            if (calcOutput.Item3 == 0)
            {
                string[] formatArray = new string[] { rawWeap, rawOut, paraHitCount.Text};
                calcDetails.Text = String.Format("This weapon deals {0} damage. After MV, this weapon deals {1} damage over {2} hit(s).\n", formatArray);
            }

            else if (calcOutput.Item4 == 0)
            {
                string[] formatArray = new string[] { rawWeap, eleOut, (string)paraAltType.SelectedItem, rawOut, eleCombo, paraHitCount.Text };
                calcDetails.Text = String.Format("This weapon deals {0}/{1} {2} damage. After MV, this weapon deals {3}/{4} damage over {5} hit(s).\n", formatArray);
            }

            else
            {
                string[] formatArray = new string[] { rawWeap, eleOut, (string)paraAltType.SelectedItem, secOut, (string)paraSecEle.SelectedItem, rawOut, eleCombo, secCombo, paraHitCount.Text };
                calcDetails.Text = String.Format("This weapon deals {0}/{1} {2} damage and an additional {3} {4} damage. After MV, this weapon deals {5}/{6}/{7} damage over {8} hit(s).\n", formatArray);
            }
        }

        /// <summary>
        /// Calculates the amount of health that the monster can have, and applies the variance.
        /// Furthermore, lists how many hits it would take to slay the monster.
        /// </summary>
        private void HealthCalc(Tuple<double, bool, double, double, double, double, double> allTuple)
        {
            double finalDamage = allTuple.Item1;
            if (finalDamage == 0)
            {
                calcDetails.AppendText("No damage was dealt in this situation.");
                return;
            }

            double health = double.Parse(paraHealth.Text);
            if (health == 0)
            {
                calcDetails.AppendText("It is impossible to determine how many hits the monster can take before dying, as the health listed is 0.");
                return;
            }

            double lowHealth = Math.Ceiling(health * 0.975);
            double highHealth = Math.Ceiling(health * 1.025);

            string minHits = Math.Ceiling(lowHealth / finalDamage).ToString();
            string minDamage = (finalDamage / lowHealth * 100).ToString("N2");
            string avgHits = Math.Ceiling(health / finalDamage).ToString();
            string avgDamage = (finalDamage / health * 100).ToString("N2");
            string maxHits = Math.Ceiling(highHealth / finalDamage).ToString();
            string maxDamage = (finalDamage / highHealth * 100).ToString("N2");

            string[] formatArray = new string[] { paraHitCount.Text, finalDamage.ToString("N2"), avgHits, avgDamage, health.ToString(), minHits, minDamage, maxHits, maxDamage };
            string formatString = String.Format("With {0} hit(s) that deals {1} damage total, it will, on average, take {2} of these attacks ({3}%) to kill a monster with {4} health. At minimum health, it will take {5} attacks ({6}%), and at maximum health, it will take {7} attacks ({8}%).", formatArray);
            calcDetails.AppendText(formatString);
        }

        private Tuple<double, double, double, double> CalculateDamage()
        {
            //Collecting parameter info
            double raw = double.Parse(paraRaw.Text);
            double rawSharp = double.Parse(paraRawSharp.Text);

            string altType = paraAltType.Text;
            double element = double.Parse(paraElePower.Text);
            double elementSharp = double.Parse(paraEleSharp.Text);

            Tuple<double, double> affinity;
            if(!paraChaotic.Checked)
            {
                double incAffinity = double.Parse(paraAffinity.Text);
                if(incAffinity >= 0)
                {
                    affinity = new Tuple<double, double>(incAffinity * 0.01, 0);
                }
                else
                {
                    affinity = new Tuple<double, double>(0, incAffinity * -0.01);
                }
            }
            else
            {
                affinity = new Tuple<double, double>(double.Parse(paraPosAff.Text) * 0.01, double.Parse(paraNegAff.Text) * 0.01);
            }

            string secType = paraSecEle.Text;
            double secElement = double.Parse(paraSecPower.Text);

            double motionValue = double.Parse(paraAvgMV.Text) * 0.01;
            int hitCount = int.Parse(paraHitCount.Text);

            double KO = double.Parse(paraKO.Text);
            double exhaust = double.Parse(paraExh.Text);

            bool fixedType = paraFixed.Checked;
            //bool critBoost = paraCritBoost.Checked;
            bool mindsEye = paraMinds.Checked;
            //bool statusCrit = paraStatusCrit.Checked;
            bool madAffinity = paraMadAff.Checked;

            double critBoost = 0.25;
            double statusCrit = 0;
            double eleCrit = 0;

            double rawWeap = 0;
            double rawTotal = 0;
            double eleTotal = 0;
            double secTotal = 0;

            if(secElement != 0)
            {
                element /= 2;
                secElement /= 2;
            }

            Tuple<double, double> subAffinity = affinity;

            if(fixedType)
            {
                return new Tuple<double, double, double, double>(motionValue * raw, motionValue * raw, element, secElement);
            }

            if(calcNeutral.Checked)
            {
                subAffinity = new Tuple<double, double>(0, 0);
            }

            if(calcPositive.Checked)
            {
                subAffinity = new Tuple<double, double>(1, 0);
            }

            if(calcNegative.Checked)
            {
                subAffinity = new Tuple<double, double>(0, 1);
            }

            if(calcAverage.Checked || calcPositive.Checked)
            {
                if(paraCritBoost.Checked)
                {
                    critBoost = 0.40;
                }

                if(paraStatusCrit.Checked)
                {
                    statusCrit = 0.2;
                }

                if(paraEleCrit.SelectedIndex == 1)
                {
                    eleCrit = 0.35;
                }

                if(paraEleCrit.SelectedIndex == 2)
                {
                    eleCrit = 0.3;
                }

                if(paraEleCrit.SelectedIndex == 3)
                {
                    eleCrit = 0.25;
                }

                if(paraEleCrit.SelectedIndex == 4)
                {
                    eleCrit = 0.2;
                }
            }

            if(madAffinity)
            {
                subAffinity = new Tuple<double, double>(subAffinity.Item1 + subAffinity.Item2 * 0.25, subAffinity.Item2 - subAffinity.Item2 * 0.25);
            }

            rawWeap = raw * (1 + subAffinity.Item1 * critBoost) * (1 - subAffinity.Item2 * 0.25) * rawSharp;
            rawTotal = rawWeap * motionValue;

            if(isElement(altType))
            {
                eleTotal = element * elementSharp * (1 + subAffinity.Item1 * eleCrit);
            }

            else if(isStatus(altType))
            {
                eleTotal = element * elementSharp * (1 + subAffinity.Item1 * statusCrit);
            }

            else
            {
                eleTotal = element * elementSharp;
            }

            if (isElement(secType))
            {
                secTotal = secElement * elementSharp * (1 + subAffinity.Item1 * eleCrit);
            }

            else if (isStatus(secType))
            {
                secTotal = secElement * elementSharp * (1 + subAffinity.Item1 * statusCrit);
            }

            else
            {
                secTotal = secElement * elementSharp;
            }

            return new Tuple<double, double, double, double>(rawWeap, rawTotal, eleTotal, secTotal);
        }

        private Tuple<double, bool, double, double, double, double, double> CalculateAllDamage(Tuple<double, double, double, double> calcOutput)
        {
            double rawZone = double.Parse(paraHitzone.Text) * 0.01;
            double eleZone = double.Parse(paraEleHit.Text) * 0.01;
            double secZone = double.Parse(paraSecZone.Text) * 0.01;
            double hitCount = double.Parse(paraHitCount.Text);
            double KODam = double.Parse(paraKO.Text);
            double ExhDam = double.Parse(paraExh.Text);
            double KOZone = double.Parse(paraKOZone.Text) * 0.01;
            double ExhaustZone = double.Parse(paraExhZone.Text) * 0.01;
            double questMod = double.Parse(paraQuestMod.Text);

            double rawDamage = calcOutput.Item2;

            double totaldamage = 0;
            double KODamage = KODam * KOZone;
            double ExhDamage = ExhDam * ExhaustZone;
            bool BounceBool = false;

            if(!paraFixed.Checked)
            {
                rawDamage *= rawZone * questMod;

                if ((rawZone * double.Parse(paraRawSharp.Text)) > 0.25 || paraMinds.Checked)
                {
                    BounceBool = true;
                }
                else
                {
                    BounceBool = false;
                }
            }

            else
            {
                rawDamage *= questMod;
            }

            totaldamage += rawDamage;

            string element = paraAltType.Text;
            double eleDamage = calcOutput.Item3;

            if(isElement(element))
            {
                eleDamage *= eleZone * questMod;
                totaldamage += eleDamage;
            }

            string second = paraSecEle.Text;
            double secDamage = calcOutput.Item4;

            if(isElement(second))
            {
                secDamage *= eleZone * questMod;
                totaldamage += secDamage;
            }

            totaldamage = Math.Floor(totaldamage);
            totaldamage *= hitCount;

            KODamage = Math.Floor(KODamage);
            KODamage *= hitCount;

            ExhDamage = Math.Floor(ExhDamage);
            ExhDamage *= hitCount;

            return new Tuple<double, bool, double, double, double, double, double>(totaldamage, BounceBool, rawDamage, eleDamage, secDamage, KODamage, ExhDamage);
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

        private void weapEle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(weapEle.SelectedIndex == 0)
            {
                weapEleDamage.Text = "0";
                weapEleDamage.Enabled = false;

                weapElePict.Image = null;
                return;
            }

            weapEleDamage.Enabled = true;
            weapElePict.Load(str2Pict[weapEle.Text]);
        }

        private void weapSharpness_SelectedIndexChanged(object sender, EventArgs e)
        {
            weapSharpOne.SelectedItem = weapSharpness.SelectedItem;
            weapSharpTwo.SelectedItem = weapSharpness.SelectedItem;

            weapSharpOne.Items.Clear();
            
        }
    }
}
