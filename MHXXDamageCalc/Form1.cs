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
        string[] originalSharpness = new string[] { "(No Sharpness)", "Purple", "White", "Blue", "Green", "Yellow", "Orange", "Red" };
        double[] monsterStatus = new double[] { 1, 1.1, 3, 2, 1.1 };
        ImportedStats stats = new ImportedStats();
        Dictionary<string, Func<int, bool>> armorModifiers = new Dictionary<string, Func<int, bool>>();
        Dictionary<string, Func<int, bool>> foodModifiers = new Dictionary<string, Func<int, bool>>();
        Dictionary<string, Func<int, bool>> weaponModifiers = new Dictionary<string, Func<int, bool>>();

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
            str2Pict.Add("KO", "./Images/KO.png");
            str2Pict.Add("Exhaust", "./Images/Exhaust.png");

            sharpnessMods.Add("(No Sharpness)", new Tuple<double, double>(1.00, 1.00));
            sharpnessMods.Add("Purple", new Tuple<double, double>(1.39, 1.20));
            sharpnessMods.Add("White", new Tuple<double, double>(1.32, 1.125));
            sharpnessMods.Add("Blue", new Tuple<double, double>(1.20, 1.06));
            sharpnessMods.Add("Green", new Tuple<double, double>(1.05, 1.0));
            sharpnessMods.Add("Yellow", new Tuple<double, double>(1.00, 0.75));
            sharpnessMods.Add("Orange", new Tuple<double, double>(0.75, 0.50));
            sharpnessMods.Add("Red", new Tuple<double, double>(0.50, 0.25));

            armorModifiers.Add("Elementality", x => Amplify(1));
            armorModifiers.Add("Frosty Protection", x => AntiDaora(1));
            armorModifiers.Add("Metallic Protection", x => AntiTeostra(1));
            armorModifiers.Add("Art. Novice (Fixed Weaps.)", x => Artillery(1));
            armorModifiers.Add("Art. Novice (Exp. Ammo)", x => Artillery(2));
            armorModifiers.Add("Art. Novice (Impact CB)", x => Artillery(3));
            armorModifiers.Add("Art. Novice (GL)", x => Artillery(4));
            armorModifiers.Add("Art. Expert (Fixed Weaps.)", x => Artillery(5));
            armorModifiers.Add("Art. Expert (Exp. Ammo)", x => Artillery(6));
            armorModifiers.Add("Art. Expert (Impact CB)", x => Artillery(7));
            armorModifiers.Add("Art. Expert (GL)", x => Artillery(8));
            armorModifiers.Add("Attack Up (S)", x => Attack(1));
            armorModifiers.Add("Attack Up (M)", x => Attack(2));
            armorModifiers.Add("Attack Up (L)", x => Attack(3));
            armorModifiers.Add("Attack Down (S)", x => Attack(4));
            armorModifiers.Add("Attack Down (M)", x => Attack(5));
            armorModifiers.Add("Attack Down (L)", x => Attack(6));

            armorModifiers.Add("Bloodlust Soul", x => Bloodbath(1));
            armorModifiers.Add("True B.lust Soul", x => TrueBloodbath(1));
            armorModifiers.Add("Boltreaver Soul", x => Boltreaver(1));
            armorModifiers.Add("True B.reaver Soul", x => TrueBoltreaver(1));
            armorModifiers.Add("Bludgeoner (GU)", x => Blunt(1));
            armorModifiers.Add("Bludgoner (Gen)", x => Blunt(2));
            armorModifiers.Add("Bombardier (Blast)", x => BombBoost(1));
            armorModifiers.Add("Bombardier (Bomb)", x => BombBoost(2));
            armorModifiers.Add("Heavy Hitter", x => Brawn(1));
            armorModifiers.Add("Ruthlessness", x => Brutality(1));

            armorModifiers.Add("Repeat Offender (1 hit)", x => ChainCrit(1));
            armorModifiers.Add("Repeat Offender (>=5 hits)", x => ChainCrit(2));
            armorModifiers.Add("Trump Card (Other HAs)", x => Chance(1));
            armorModifiers.Add("Trump Card (Lion's Maw)", x => Chance(2));
            armorModifiers.Add("Trump Card (W. Breath)", x => Chance(3));
            armorModifiers.Add("Trump Card (D. Riot 'Pwr')", x => Chance(4));
            armorModifiers.Add("Trump Card (D. Riot 'Sta')", x => Chance(5));
            armorModifiers.Add("Trump Card (D. Riot 'Ele')", x => Chance(6));
            armorModifiers.Add("Trump Card (S. Blade)", x => Chance(7));
            armorModifiers.Add("Polar Hunter (Cold Area)", x => ColdBlooded(1));
            armorModifiers.Add("Polar Hunter (Cool Drink)", x => ColdBlooded(2));
            armorModifiers.Add("Resuscitate", x => Crisis(1));
            armorModifiers.Add("Critical Draw", x => CritDraw(1));
            armorModifiers.Add("Ele. Crit (SnS/DB/Bow)", x => CritElement(1));
            armorModifiers.Add("Ele. Crit (LBG/HBG)", x => CritElement(2));
            armorModifiers.Add("Ele. Crit (Other)", x => CritElement(3));
            armorModifiers.Add("Ele. Crit (GS)", x => CritElement(4));
            armorModifiers.Add("Status Crit", x => CritStatus(1));
            armorModifiers.Add("Critical Boost", x => CriticalUp(1));

            armorModifiers.Add("Pro D. Fencer (0 Carts)", x => DFencing(1));
            armorModifiers.Add("Pro D. Fencer (One Cart)", x => DFencing(2));
            armorModifiers.Add("Pro D. Fencer (Two Carts)", x => DFencing(3));
            armorModifiers.Add("Deadeye Soul", x => Deadeye(1));
            armorModifiers.Add("True D.eye S. (0 Cs, Not)", x => TrueDeadeye(1));
            armorModifiers.Add("True D.eye S. (0 Cs, Rage)", x => TrueDeadeye(2));
            armorModifiers.Add("True D.eye S. (1 Cs, Not)", x => TrueDeadeye(3));
            armorModifiers.Add("True D.eye S. (1 Cs, Rage)", x => TrueDeadeye(4));
            armorModifiers.Add("True D.eye S. (2 Cs, Not)", x => TrueDeadeye(5));
            armorModifiers.Add("True D.eye S. (2 Cs, Rage)", x => TrueDeadeye(6));
            armorModifiers.Add("Dragon Atk +1", x => DragonAtk(1));
            armorModifiers.Add("Dragon Atk +2", x => DragonAtk(2));
            armorModifiers.Add("Dragon Atk Down", x => DragonAtk(3));
            armorModifiers.Add("Dragon's Spirit", x => DragonAura(1));
            armorModifiers.Add("Dreadking Soul", x => Dreadking(1));
            armorModifiers.Add("True Dreadking Soul", x => TrueDreadking(1));
            armorModifiers.Add("Dreadqueen Soul", x => Dreadqueen(1));
            armorModifiers.Add("True Dreadqueen Soul", x => TrueDreadqueen(1));
            armorModifiers.Add("Drilltusk Soul", x => Drilltusk(1));
            armorModifiers.Add("True D.t. S. (Adren +2)", x => TrueDrilltusk(1));
            armorModifiers.Add("True D.t. S. (Fixed Weaps.)", x => TrueDrilltusk(2));
            armorModifiers.Add("True D.t. S. (Exp. Shots)", x => TrueDrilltusk(3));
            armorModifiers.Add("True D.t. S. (Impact Phial)", x => TrueDrilltusk(4));
            armorModifiers.Add("True D.t. S. (GL Shots)", x => TrueDrilltusk(5));

            armorModifiers.Add("Honed Blade", x => Edgemaster(1));
            armorModifiers.Add("Elderfrost Soul", x => Elderfrost(1));
            armorModifiers.Add("True Elderfrost Soul", x => TrueElderfrost(1));
            armorModifiers.Add("Element Atk Up", x => Elemental(1));
            armorModifiers.Add("Element Atk Down", x => Elemental(2));
            armorModifiers.Add("Critical Eye +1", x => Expert(1));
            armorModifiers.Add("Critical Eye +2", x => Expert(2));
            armorModifiers.Add("Critical Eye +3", x => Expert(3));
            armorModifiers.Add("Critical Eye -1", x => Expert(4));
            armorModifiers.Add("Critical Eye -2", x => Expert(5));
            armorModifiers.Add("Critical Eye -3", x => Expert(6));

            armorModifiers.Add("Mind's Eye", x => Fencing(1));
            armorModifiers.Add("Fire Atk +1", x => FireAtk(1));
            armorModifiers.Add("Fire Atk +2", x => FireAtk(2));
            armorModifiers.Add("Fire Atk Down", x => FireAtk(3));
            armorModifiers.Add("Resentment", x => Furor(1));
            armorModifiers.Add("Wrath Awoken", x => Fury(1));

            armorModifiers.Add("Latent Power +1", x => GlovesOff(1));
            armorModifiers.Add("Latent Power +2", x => GlovesOff(2));

            armorModifiers.Add("Sharpness +1", x => Handicraft(1));
            armorModifiers.Add("Sharpness +2", x => Handicraft(2));
            armorModifiers.Add("TrueShot Up", x => Haphazard(1));
            armorModifiers.Add("Heavy/Heavy Up", x => HeavyUp(1));
            armorModifiers.Add("Hellblade Soul", x => Hellblade(1));
            armorModifiers.Add("True H.blade Soul (Blast)", x => TrueHellblade(1));
            armorModifiers.Add("True H.blade Soul (Bomb)", x => TrueHellblade(2));
            armorModifiers.Add("Tropic Hunter ", x => HotBlooded(1));
            armorModifiers.Add("Soul of the Hunter's Pub", x => HuntersPub(1));

            armorModifiers.Add("Ice Atk +1", x => IceAtk(1));
            armorModifiers.Add("Ice Atk +2", x => IceAtk(2));
            armorModifiers.Add("Ice Atk Down", x => IceAtk(3));

            armorModifiers.Add("KO King", x => KO(1));

            armorModifiers.Add("Exp. Trapper (Blast)", x => Mechanic(1));
            armorModifiers.Add("Exp. Trapper (Bomb)", x => Mechanic(2));

            armorModifiers.Add("Normal/Rapid Up", x => NormalUp(1));

            armorModifiers.Add("Pellet/Spread Up (Pellet S)", x => PelletUp(1));
            armorModifiers.Add("Pellet/Spread Up (Spread)", x => PelletUp(2));
            armorModifiers.Add("Pierce/Pierce Up", x => PierceUp(1));
            armorModifiers.Add("Adrenaline +2", x => Potential(1));
            armorModifiers.Add("Worrywart", x => Potential(2));
            armorModifiers.Add("Fleet Feet", x => Prudence(1));
            armorModifiers.Add("Punishing Draw", x => PunishDraw(1));

            armorModifiers.Add("Sheath Control", x => Readiness(1));
            armorModifiers.Add("Bonus Shot", x => RapidFire(1));
            armorModifiers.Add("Redhelm Soul", x => Redhelm(1));
            armorModifiers.Add("True R.helm Soul", x => TrueRedhelm(1));
            armorModifiers.Add("Rueful Crit", x => ReverseCrit(1));
            armorModifiers.Add("True R.razor Soul", x => TrueRustrazor(1));

            armorModifiers.Add("Shining Blade", x => ScaledSword(1));
            armorModifiers.Add("Silverwind Soul", x => Silverwind(1));
            armorModifiers.Add("True S.wind Soul", x => TrueSilverwind(1));
            armorModifiers.Add("Challenger +1", x => Spirit(1));
            armorModifiers.Add("Challenger +2", x => Spirit(2));
            armorModifiers.Add("Stamina Thief", x => StamDrain(1));
            armorModifiers.Add("Status Atk +1", x => StatusAtk(1));
            armorModifiers.Add("Status Atk +2", x => StatusAtk(2));
            armorModifiers.Add("Status Atk Down", x => StatusAtk(3));
            armorModifiers.Add("Silver Bullet (Normal/Rapid)", x => SteadyHand(1));
            armorModifiers.Add("Silver Bullet (Pellet S)", x => SteadyHand(2));
            armorModifiers.Add("Silver Bullet (Spread)", x => SteadyHand(3));
            armorModifiers.Add("Silver Bullet (Pierce/Pierce)", x => SteadyHand(4));
            armorModifiers.Add("Soulseer Soul (No Enrage)", x => Soulseer(1));
            armorModifiers.Add("Soulseer Soul (Rage)", x => Soulseer(2));
            armorModifiers.Add("True S.seer Soul (Not)", x => TrueSoulseer(1));
            armorModifiers.Add("True S.seer Soul (Rage)", x => TrueSoulseer(2));
            armorModifiers.Add("Fortify (1st Cart)", x => Survivor(1));
            armorModifiers.Add("Fortify (2nd Cart)", x => Survivor(2));

            armorModifiers.Add("Weakness Exploit", x => Tenderizer(1));
            armorModifiers.Add("Thunder Atk +1", x => ThunderAtk(1));
            armorModifiers.Add("Thunder Atk +2", x => ThunderAtk(2));
            armorModifiers.Add("Thunder Atk Down", x => ThunderAtk(3));
            armorModifiers.Add("Thunderlord Soul", x => Thunderlord(1));
            armorModifiers.Add("True T.lord Soul", x => TrueThunderlord(1));

            armorModifiers.Add("Peak Performance", x => Unscathed(1));

            armorModifiers.Add("Airborne", x => Vault(1));

            armorModifiers.Add("Water Atk +1", x => WaterAtk(1));
            armorModifiers.Add("Water Atk +2", x => WaterAtk(2));
            armorModifiers.Add("Water Atk Down", x => WaterAtk(3));

            foreach (KeyValuePair<string, Func<int, bool>> pair in armorModifiers)
            {
                modArmor.Items.Add(pair.Key);
            }

            foodModifiers.Add("F.Bombardier (Fixed Weaps.)", x => FBombardier(1));
            foodModifiers.Add("F.Bombardier (Explosive S)", x => FBombardier(2));
            foodModifiers.Add("F.Bombardier (Impact CB)", x => FBombardier(3));
            foodModifiers.Add("F.Bombardier (GL)", x => FBombardier(4));
            foodModifiers.Add("F.Booster", x => FBooster());
            foodModifiers.Add("F.Bulldozer", x => FBulldozer());
            foodModifiers.Add("F.Heroics", x => FHeroics());
            foodModifiers.Add("F.Pyro (Blast)", x => FPyro());
            foodModifiers.Add("F.Sharpshooter", x => FSharpshooter());
            foodModifiers.Add("F.Slugger", x => FSlugger());
            foodModifiers.Add("F.Specialist", x => FSpecialist());
            foodModifiers.Add("F.Temper", x => FTemper());
            foodModifiers.Add("Cool Cat", x => CoolCat());

            foodModifiers.Add("Powercharm", x => Powercharm());
            foodModifiers.Add("Power Talon", x => PowerTalon());
            foodModifiers.Add("Demon Drug", x => DemonDrug(1));
            foodModifiers.Add("Mega Demon Drug", x => DemonDrug(2));
            foodModifiers.Add("Attack Up (S) Meal", x => AUMeal(1));
            foodModifiers.Add("Attack Up (M) Meal", x => AUMeal(2));
            foodModifiers.Add("Attack Up (L) Meal", x => AUMeal(3));
            foodModifiers.Add("Might Seed", x => MightSeed(1));
            foodModifiers.Add("Might Pill", x => MightSeed(2));
            foodModifiers.Add("Nitroshroom (Mushromancer)", x => Nitroshroom());
            foodModifiers.Add("Demon Horn", x => Demon(1));
            foodModifiers.Add("Demon S", x => Demon(2));
            foodModifiers.Add("Demon Affinity S", x => Demon(3));

            foreach (KeyValuePair<string, Func<int, bool>> pair in foodModifiers)
            {
                modFood.Items.Add(pair.Key);
            }

            weaponModifiers.Add("Lo Sharp. Mod.", x => LSM(1));
            weaponModifiers.Add("GS - Center of Blade", x => GS(1));
            weaponModifiers.Add("GS - Lion's Maw I", x => GS(2));
            weaponModifiers.Add("GS - Lion's Maw II", x => GS(3));
            weaponModifiers.Add("GS - Lion's Maw III", x => GS(4));
        }

        public void setSelected()
        {
            moveType.SelectedIndex = 0;

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
            if (paraChaotic.Checked)
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
            if (paraAltType.SelectedIndex == 0)
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

            if (paraAltType.SelectedIndex >= 1 && paraAltType.SelectedIndex <= 5)
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
            if (paraSecEle.SelectedIndex == 0)
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

            if (allTuple.Item2)
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
                string[] formatArray = new string[] { rawWeap, rawOut, paraHitCount.Text };
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
            if (!paraChaotic.Checked)
            {
                double incAffinity = double.Parse(paraAffinity.Text);
                if (incAffinity >= 0)
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

            if (secElement != 0)
            {
                element /= 2;
                secElement /= 2;
            }

            Tuple<double, double> subAffinity = affinity;

            if (fixedType)
            {
                return new Tuple<double, double, double, double>(motionValue * raw, motionValue * raw, element, secElement);
            }

            if (calcNeutral.Checked)
            {
                subAffinity = new Tuple<double, double>(0, 0);
            }

            if (calcPositive.Checked)
            {
                subAffinity = new Tuple<double, double>(1, 0);
            }

            if (calcNegative.Checked)
            {
                subAffinity = new Tuple<double, double>(0, 1);
            }

            if (calcAverage.Checked || calcPositive.Checked)
            {
                if (paraCritBoost.Checked)
                {
                    critBoost = 0.40;
                }

                if (paraStatusCrit.Checked)
                {
                    statusCrit = 0.2;
                }

                if (paraEleCrit.SelectedIndex == 1)
                {
                    eleCrit = 0.35;
                }

                if (paraEleCrit.SelectedIndex == 2)
                {
                    eleCrit = 0.3;
                }

                if (paraEleCrit.SelectedIndex == 3)
                {
                    eleCrit = 0.25;
                }

                if (paraEleCrit.SelectedIndex == 4)
                {
                    eleCrit = 0.2;
                }
            }

            if (madAffinity)
            {
                subAffinity = new Tuple<double, double>(subAffinity.Item1 + subAffinity.Item2 * 0.25, subAffinity.Item2 - subAffinity.Item2 * 0.25);
            }

            rawWeap = raw * (1 + subAffinity.Item1 * critBoost) * (1 - subAffinity.Item2 * 0.25) * rawSharp;
            rawTotal = rawWeap * motionValue;

            if (isElement(altType))
            {
                eleTotal = element * elementSharp * (1 + subAffinity.Item1 * eleCrit);
            }

            else if (isStatus(altType))
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

            rawDamage *= monsterStatus[paraMonStatus.SelectedIndex];

            if (!paraFixed.Checked)
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

            if (isElement(element))
            {
                eleDamage *= eleZone * questMod;
                totaldamage += eleDamage;
            }

            string second = paraSecEle.Text;
            double secDamage = calcOutput.Item4;

            if (isElement(second))
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
            if (weapEle.SelectedIndex == 0)
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
            weapSharpOne.Items.Clear();
            weapSharpTwo.Items.Clear();
            for (int i = 1; i <= weapSharpness.SelectedIndex; i++)
            {
                weapSharpOne.Items.Add(originalSharpness[i]);
                weapSharpTwo.Items.Add(originalSharpness[i]);
            }

            if (weapSharpness.SelectedIndex == 0)
            {
                weapSharpOne.Items.Add(originalSharpness[0]);
                weapSharpTwo.Items.Add(originalSharpness[0]);
            }

            weapSharpOne.SelectedItem = weapSharpness.SelectedItem;
            weapSharpTwo.SelectedItem = weapSharpness.SelectedItem;
        }

        private void weapSharpOne_SelectedIndexChanged(object sender, EventArgs e)
        {
            weapSharpTwo.Items.Clear();
            for (int i = 0; i <= weapSharpOne.SelectedIndex; i++)
            {
                weapSharpTwo.Items.Add(originalSharpness[i + 1]);
            }

            if (weapSharpness.SelectedIndex == 0)
            {
                weapSharpTwo.Items.Add(originalSharpness[0]);
            }
            weapSharpTwo.SelectedItem = weapSharpOne.SelectedItem;
        }

        private void weapChaotic_CheckedChanged(object sender, EventArgs e)
        {
            if (weapChaotic.Checked)
            {
                weapPositive.Visible = true;
                weapNegative.Visible = true;
                weapPosAff.Visible = true;
                weapNegAff.Visible = true;

                weapAffinity.Text = "0";
                weapAffinity.Visible = false;
            }
            else
            {
                weapPositive.Visible = false;
                weapNegative.Visible = false;

                weapPosAff.Text = "0";
                weapPosAff.Visible = false;

                weapNegAff.Text = "0";
                weapNegAff.Visible = false;

                weapAffinity.Visible = true;
            }
        }

        private void weapOverride_CheckedChanged(object sender, EventArgs e)
        {
            if (weapOverride.Checked)
            {
                weapEle.Enabled = false;
                weapEleDamage.Enabled = false;
            }
            else
            {
                weapEle.Enabled = true;
                weapEleDamage.Enabled = true;
            }
        }

        private void weapSec_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (weapSec.SelectedIndex == 0)
            {
                weapSecPict.Image = null;
                weapSecDamage.Text = "0";
                weapSecDamage.Enabled = false;
                return;
            }
            else if (weapSec.SelectedIndex == 1)
            {
                weapSecPict.Load(str2Pict["Fire"]);
            }

            else if (weapSec.SelectedIndex == 2)
            {
                weapSecPict.Load(str2Pict["Water"]);
            }

            else if (weapSec.SelectedIndex == 3)
            {
                weapSecPict.Load(str2Pict["Thunder"]);
            }

            else if (weapSec.SelectedIndex == 4)
            {
                weapSecPict.Load(str2Pict["Ice"]);
            }

            else if (weapSec.SelectedIndex == 5)
            {
                weapSecPict.Load(str2Pict["Para"]);
            }

            else if (weapSec.SelectedIndex == 6)
            {
                weapSecPict.Load(str2Pict["Poison"]);
            }

            else if (weapSec.SelectedIndex == 7)
            {
                weapSecPict.Load(str2Pict["Blast"]);
            }

            else if (weapSec.SelectedIndex == 8)
            {
                weapSecPict.Load(str2Pict["Dragon"]);
            }

            else if (weapSec.SelectedIndex == 9)
            {
                weapSecPict.Load(str2Pict["Poison"]);
            }

            else if (weapSec.SelectedIndex == 10)
            {
                weapSecPict.Load(str2Pict["Para"]);
            }

            else if (weapSec.SelectedIndex == 11)
            {
                weapSecPict.Load(str2Pict["Exhaust"]);
            }

            else if(weapSec.SelectedIndex == 12)
            {
                weapSecPict.Load(str2Pict["Exhaust"]);
            }

            weapSecDamage.Enabled = true;
        }

        private void weapReset_Click(object sender, EventArgs e)
        {
            weapRaw.Text = "0";
            weapEle.SelectedIndex = 0;
            weapAffinity.Text = "0";
            weapPosAff.Text = "0";
            weapNegAff.Text = "0";
            weapSharpness.SelectedIndex = 0;
            weapChaotic.Checked = false;
            weapSec.SelectedIndex = 0;
            weapOverride.Checked = false;
        }

        private void monReset_Click(object sender, EventArgs e)
        {
            monCut.Text = "0";
            monImpact.Text = "0";
            monShot.Text = "0";
            monKO.Text = "0";
            monExhaust.Text = "0";

            monFire.Text = "0";
            monWater.Text = "0";
            monThunder.Text = "0";
            monIce.Text = "0";
            monDragon.Text = "0";

            monHealth.Text = "0";
            monQuest.Text = "1.0";
            monExhaustMod.Text = "1.0";
        }

        private void moveReset_Click(object sender, EventArgs e)
        {
            moveMV.Text = "0";
            moveHitCount.Text = "0";
            moveAvg.Text = "0";
            moveSharpMod.Text = "1.0";

            moveType.SelectedIndex = 0;
            moveKO.Text = "0";
            moveExhaust.Text = "0";
            moveElement.Text = "1.0";

            moveMinds.Checked = false;
            moveAerial.Checked = false;
            moveDraw.Checked = false;
        }

        private void moveType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (moveType.SelectedIndex == 0)
            {
                moveTypePict.Load("./Images/Cut.png");
            }

            else if (moveType.SelectedIndex == 1)
            {
                moveTypePict.Load("./Images/Impact.png");
            }

            else if (moveType.SelectedIndex == 2)
            {
                moveTypePict.Load("./Images/Shot.png");
            }

            else if (moveType.SelectedIndex == 3)
            {
                moveTypePict.Load("./Images/Fixed.png");
            }
        }


#if true
        //Skills
        public bool Amplify(int skillVal)
        {
            if (isElement(stats.altDamageType))
            {
                stats.eleMod *= 1.1; //Elementality
            }
            if (isElement(stats.secElement))
            {
                stats.secMod *= 1.1;
            }
            return true;
        }

        public bool AntiDaora(int skillVal)
        {
            if (skillVal == 1) //Polar Hunter's Cool Area effect.
            {
                stats.addRaw += 15;
                return true;
            }

            else if (skillVal == 2) //Polar Hunter's Cool Drink effect.
            {
                stats.addRaw += 5;
                return true;
            }

            else
            {
                return false;
            }
        }

        public bool AntiTeostra(int skillVal)
        {
            stats.addRaw += 15; //Tropic Hunter's Hot Area effect
            return true;
        }

        public bool Artillery(int skillVal)
        {
            //Artillery Novice
            if (skillVal == 1) //Cannon/Ballista
            {
                stats.rawMod *= 1.1;
                return true;
            }

            else if (skillVal == 2) //Explosive Shots
            {
                stats.expMod *= 1.15;
                return true;
            }

            else if (skillVal == 3) //Impact Phial
            {
                stats.expMod *= 1.3;
                stats.CB = true;
                return true;
            }

            else if (skillVal == 4) //GL shots
            {
                stats.eleMod *= 1.1;
                return true;
            }

            //Artillery Expert
            else if (skillVal == 5) //Cannon/Ballista
            {
                stats.rawMod *= 1.2;
                return true;
            }

            else if (skillVal == 6) //Explosive Shots
            {
                stats.expMod *= 1.3;
                return true;
            }

            else if (skillVal == 7) //Impact Phial
            {
                stats.expMod *= 1.35;
                stats.CB = true;
                return true;
            }

            else if (skillVal == 8) //GL shots
            {
                stats.eleMod *= 1.2;
                return true;
            }

            else
            {
                return false;
            }
        }

        public bool Attack(int skillVal)
        {
            if (skillVal < 1 || skillVal > 6)
            {
                return false;
            }

            else if (skillVal == 1) //Attack Up (S)
            {
                stats.addRaw += 10;
            }

            else if (skillVal == 2) //Attack Up (M)
            {
                stats.addRaw += 15;
            }

            else if (skillVal == 3) //Attack Up (L)
            {
                stats.addRaw += 20;
            }

            else if (skillVal == 4) //Attack Down (S)
            {
                stats.addRaw -= 5;
            }

            else if (skillVal == 5) //Attack Down (M)
            {
                stats.addRaw -= 10;
            }

            else if (skillVal == 6) //Attack Down (L)
            {
                stats.addRaw -= 15;
            }

            return true;
        }

        public bool Bloodbath(int skillVal)
        {
            stats.addRaw += 20; //Attack Up (L) portion
            return true;
        }

        public bool TrueBloodbath(int skillVal)
        {
            stats.addRaw += 20; //Attack Up (L) portion
            return true;
        }

        public bool Boltreaver(int skillVal)
        {
            stats.criticalBoost = true; //Crit Boost portion

            stats.UpdateSharpness((string)weapSharpOne.SelectedItem);

            return true;
        }

        public bool TrueBoltreaver(int skillVal)
        {
            stats.criticalBoost = true; //Crit Boost portion

            stats.UpdateSharpness((string)weapSharpOne.SelectedItem);

            return true;
        }

        public bool Blunt(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            if (skillVal == 1)
            {
                if (stats.sharpness == "Green")
                {
                    stats.rawMod *= 1.1;
                }

                else if (stats.sharpness == "Yellow")
                {
                    stats.rawMod *= 1.15;
                }

                else if (stats.sharpness == "Orange" || stats.sharpness == "Red")
                {
                    stats.rawMod *= 1.2;
                }
            }

            if (skillVal == 2)
            {
                if (stats.sharpness == "Green")
                {
                    stats.addRaw += 15;
                }

                else if (stats.sharpness == "Yellow")
                {
                    stats.addRaw += 25;
                }

                else if (stats.sharpness == "Orange" || stats.sharpness == "Red")
                {
                    stats.addRaw += 30;
                }
            }

            return true;
        }

        public bool BombBoost(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //Blast portion
            {
                if (stats.altDamageType == "Blast")
                {
                    stats.staMod *= 1.2;
                }


                if (stats.secElement == "Blast")
                {
                    stats.staSecMod *= 1.2;
                }
            }

            else if (skillVal == 2) //Bomb portion
            {
                stats.expMod *= 1.3;
            }

            return true;
        }

        public bool Brawn(int skillVal)
        {
            stats.KOMod *= 1.1;
            stats.ExhMod *= 1.1;
            return true;
        }

        public bool Brutality(int skillVal)
        {
            stats.positiveAffinity += 20;
            if (stats.hitzone >= 45)
            {
                stats.positiveAffinity += 50;
            }
            return true;
        }

        public bool ChainCrit(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //One hit
            {
                stats.positiveAffinity += 25;
            }

            else if (skillVal == 1)
            {
                stats.positiveAffinity += 30;
            }

            return true;
        }

        public bool Chance(int skillVal)
        {
            if (skillVal < 1 || skillVal > 7)
            {
                return false;
            }

            else if (skillVal == 1) //Other HAs
            {
                stats.rawMod *= 1.2;
            }

            else if (skillVal == 2) //Lion's Maw
            {
                stats.rawMod *= 1.15;
            }

            else if (skillVal == 3) //Wyvern's Breath
            {
                if(stats.damageType == "Fixed")
                {
                    stats.eleMod *= 1.2;
                    stats.expMod *= 1.2;
                }
            }

            else if (skillVal == 4) //Power Phial Demon Riot
            {
                stats.rawMod *= 1.15;
            }

            else if (skillVal == 5) //Status Phial Demon Riot (includes Poison and Para)
            {
                stats.staMod *= 1.15;
            }

            else if (skillVal == 6) //Element Phial Demon Riot (includes Dragon)
            {
                stats.eleMod *= 1.15;
            }

            else if (skillVal == 7)
            {
                stats.rawMod *= 1.1;
            }

            return true;
        }

        public bool ColdBlooded(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //Cold Areas
            {
                stats.rawMod += 15;
            }

            else if (skillVal == 2) //Cool Drinks
            {
                stats.rawMod += 5;
            }

            return true;
        }

        public bool Crisis(int skillVal)
        {
            stats.addRaw += 20; //while status is inflicted
            return true;
        }

        public bool CritDraw(int skillVal)
        {
            if (moveDraw.Checked)
            {
                stats.positiveAffinity += 100;
            }
            return true;
        }

        public bool CritElement(int skillVal)
        {
            if (skillVal < 1 || skillVal > 4)
            {
                return false;
            }

            else if (skillVal == 1)
            {
                stats.eleCrit = "SnS/DB/Bow";
            }

            else if (skillVal == 2)
            {
                stats.eleCrit = "LBG/HBG";
            }

            else if (skillVal == 3)
            {
                stats.eleCrit = "Other";
            }

            else if (skillVal == 4)
            {
                stats.eleCrit = "GS";
            }

            return true;
        }

        public bool CritStatus(int skillVal)
        {
            stats.statusCrit = true;
            return true;
        }

        public bool CriticalUp(int skillVal)
        {
            stats.criticalBoost = true;
            return true;
        }

        public bool DFencing(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //No Carts
            {
                stats.ExhMod *= 1.2;
            }

            else if (skillVal == 2) //One Cart
            {
                stats.ExhMod *= 1.2;
                stats.rawMod *= 1.1;
            }

            else if (skillVal == 3) //Two Carts
            {
                stats.ExhMod *= 1.2;
                stats.rawMod *= 1.2;
            }

            return true;
        }

        public bool Deadeye(int skillVal)
        {
            stats.addRaw += 20;
            stats.positiveAffinity += 15;
            return true;
        }

        public bool TrueDeadeye(int skillVal)
        {
            if (skillVal < 1 || skillVal > 6)
            {
                return false;
            }

            else if (skillVal == 1) //No Carts, Monster Not Enraged
            {

            }

            else if (skillVal == 2) //No Carts, Monster Enraged
            {
                stats.addRaw += 20;
                stats.positiveAffinity += 15;
            }

            else if (skillVal == 3) //One Cart, Monster Not Enraged
            {
                stats.rawMod *= 1.1;
            }

            else if (skillVal == 4) //One Cart, Monster Enraged
            {
                stats.addRaw += 20;
                stats.positiveAffinity += 15;
                stats.rawMod *= 1.1;
            }

            else if (skillVal == 5) //Two Carts, Monster Not Enraged
            {
                stats.rawMod *= 1.2;
            }

            else if (skillVal == 6) //Two Carts, Monster Enraged
            {
                stats.addRaw += 20;
                stats.positiveAffinity += 15;
                stats.rawMod *= 1.2;
            }

            return true;
        }

        public bool DragonAtk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Dragon Atk +1
            {
                if (stats.altDamageType == "Dragon")
                {
                    stats.eleMod *= 1.05;
                    stats.addElement += 4;
                }
                if (stats.secElement == "Dragon")
                {
                    stats.secMod *= 1.05;
                    stats.addSecElement += 4;
                }
            }

            else if (skillVal == 2) //Dragon Atk +2
            {
                if (stats.altDamageType == "Dragon")
                {
                    stats.eleMod *= 1.1;
                    stats.addElement += 6;
                }
                if (stats.secElement == "Dragon")
                {
                    stats.secMod *= 1.1;
                    stats.addSecElement += 6;
                }
            }

            else if (skillVal == 3) //Dragon Atk Down
            {
                if (stats.altDamageType == "Dragon")
                {
                    stats.eleMod *= 0.75;
                }
                if (stats.secElement == "Dragon")
                {
                    stats.secMod *= 0.75;
                }

            }

            return true;
        }

        public bool DragonAura(int skillVal)
        {
            stats.rawMod *= 1.1;
            if (stats.altDamageType != "Dragon")
            {
                stats.eleMod *= 0;
                stats.staMod *= 0;
            }
            if (stats.secElement != "Dragon")
            {
                stats.secMod *= 0;
                stats.staSecMod *= 0;
            }
            return true;
        }

        public bool Dreadking(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool TrueDreadking(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool Dreadqueen(int skillVal)
        {
            if (isStatus(stats.altDamageType))
            {
                stats.staMod *= 1.25;
                stats.addStatus += 1;
            }
            if (isStatus(stats.secElement))
            {
                stats.staSecMod *= 1.25;
                stats.addSecStatus += 1;
            }
            return true;
        }

        public bool TrueDreadqueen(int skillVal)
        {
            if (isStatus(stats.altDamageType))
            {
                stats.staMod *= 1.25;
                stats.addStatus += 1;
            }
            if (isStatus(stats.secElement))
            {
                stats.staSecMod *= 1.25;
                stats.addSecStatus += 1;
            }
            return true;
        }

        public bool Drilltusk(int skillVal)
        {
            stats.rawMod *= 1.3;
            return true;
        }

        public bool TrueDrilltusk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 5)
            {
                return false;
            }

            else if (skillVal == 1) //Adrenaline +2 effects
            {
                stats.rawMod *= 1.35;
            }

            //Artillery Expert
            else if (skillVal == 2) //Cannon/Ballista
            {
                stats.rawMod *= 1.2;
            }

            else if (skillVal == 3) //Explosive Shots
            {
                stats.expMod *= 1.3;
            }

            else if (skillVal == 4) //Impact Phial
            {
                stats.expMod *= 1.35;
                stats.CB = true;
            }

            else if (skillVal == 5) //GL shots
            {
                stats.eleMod *= 1.2;
            }

            return true;
        }

        public bool Edgemaster(int skillVal)
        {
            stats.addRaw += 20;
            stats.UpdateSharpness((string)weapSharpOne.SelectedItem);
            return true;
        }

        public bool Elderfrost(int skillVal)
        {
            if (stats.sharpness == "Green")
            {
                stats.rawMod *= 1.1;
            }

            else if (stats.sharpness == "Yellow")
            {
                stats.rawMod *= 1.15;
            }

            else if (stats.sharpness == "Orange" || stats.sharpness == "Red")
            {
                stats.rawMod *= 1.2;
            }

            else if (stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.1;
            }

            return true;
        }

        public bool TrueElderfrost(int skillVal)
        {
            if (stats.sharpness == "Green")
            {
                stats.rawMod *= 1.1;
            }

            else if (stats.sharpness == "Yellow")
            {
                stats.rawMod *= 1.15;
            }

            else if (stats.sharpness == "Orange" || stats.sharpness == "Red")
            {
                stats.rawMod *= 1.2;
            }

            else if (stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.1;
            }

            stats.addRaw += 15;

            return true;
        }

        public bool Elemental(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //Element Atk Up
            {
                if (isElement(stats.altDamageType))
                {
                    stats.eleMod *= 1.1;
                }

                if (isElement(stats.secElement))
                {
                    stats.secMod *= 1.1;
                }
            }

            else if (skillVal == 2) //Element Atk Down
            {
                if (isElement(stats.altDamageType))
                {
                    stats.eleMod *= 0.9;
                }

                if (isElement(stats.secElement))
                {
                    stats.secMod *= 0.9;
                }
            }

            return true;
        }

        public bool Expert(int skillVal)
        {
            if (skillVal < 1 || skillVal > 6)
            {
                return false;
            }

            else if (skillVal == 1) //+1
            {
                stats.positiveAffinity += 10;
            }

            else if (skillVal == 2) //+2
            {
                stats.positiveAffinity += 20;
            }

            else if (skillVal == 3) //+3
            {
                stats.positiveAffinity += 30;
            }

            else if (skillVal == 4) //-1
            {
                stats.negativeAffinity += 5;
            }

            else if (skillVal == 5) //-2
            {
                stats.negativeAffinity += 10;
            }

            else if (skillVal == 6) //-3
            {
                stats.negativeAffinity += 15;
            }

            return true;
        }

        public bool Fencing(int skillVal)
        {
            stats.mindsEye = true;
            return true;
        }

        public bool FireAtk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Fire Atk +1
            {
                if (stats.altDamageType == "Fire")
                {
                    stats.eleMod *= 1.05;
                    stats.addElement += 4;
                }
                if (stats.secElement == "Fire")
                {
                    stats.secMod *= 1.05;
                    stats.addSecElement += 4;
                }
            }

            else if (skillVal == 2) //Fire Atk +2
            {
                if (stats.altDamageType == "Fire")
                {
                    stats.eleMod *= 1.1;
                    stats.addElement += 6;
                }
                if (stats.secElement == "Fire")
                {
                    stats.secMod *= 1.1;
                    stats.addSecElement += 6;
                }
            }

            else if (skillVal == 3) //Fire Atk Down
            {
                if (stats.altDamageType == "Fire")
                {
                    stats.eleMod *= 0.75;
                }
                if (stats.secElement == "Fire")
                {
                    stats.secMod *= 0.75;
                }

            }

            return true;
        }

        public bool Furor(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool Fury(int skillVal)
        {
            stats.rawMod *= 1.35;
            return true;
        }

        public bool GlovesOff(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //+1
            {
                stats.positiveAffinity += 30;
            }

            else if (skillVal == 2) //+2
            {
                stats.positiveAffinity += 50;
            }

            return true;
        }

        public bool Handicraft(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1)
            {
                stats.UpdateSharpness((string)weapSharpOne.SelectedItem);
            }

            else if (skillVal == 2)
            {
                stats.UpdateSharpness((string)weapSharpTwo.SelectedItem);
            }

            return true;
        }

        public bool Haphazard(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Explosive Shots
            {
                stats.expMod *= 1.2;
                stats.eleMod *= 1.2;
            }

            else if (skillVal == 2) //Elemental shots
            {
                stats.rawMod *= 1.2;
                stats.eleMod *= 1.2;
            }

            else if (skillVal == 3) //Arc/Power Shots
            {
                stats.rawMod *= 1.2;
            }

            return true;
        }

        public bool HeavyUp(int skillVal)
        {
            stats.rawMod *= 1.1;
            return true;
        }

        public bool Hellblade(int skillVal)
        {
            stats.UpdateSharpness((string)weapSharpTwo.SelectedItem);
            return true;
        }

        public bool TrueHellblade(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }
            else if (skillVal == 1) //Blast portion
            {
                stats.UpdateSharpness((string)weapSharpTwo.SelectedItem);
                if (stats.altDamageType == "Blast")
                {
                    stats.staMod *= 1.2;
                }


                if (stats.secElement == "Blast")
                {
                    stats.staSecMod *= 1.2;
                }
            }

            else if (skillVal == 2) //Bomb portion
            {
                stats.expMod *= 1.3;
            }

            return true;
        }

        public bool HotBlooded(int skillVal)
        {
            stats.addRaw += 15;
            return true;
        }

        public bool HuntersPub(int skillVal)
        {
            stats.KOMod *= 1.1;
            return true;
        }

        public bool IceAtk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Ice Atk +1
            {
                if (stats.altDamageType == "Ice")
                {
                    stats.eleMod *= 1.05;
                    stats.addElement += 4;
                }
                if (stats.secElement == "Ice")
                {
                    stats.secMod *= 1.05;
                    stats.addSecElement += 4;
                }
            }

            else if (skillVal == 2) //Ice Atk +2
            {
                if (stats.altDamageType == "Ice")
                {
                    stats.eleMod *= 1.1;
                    stats.addElement += 6;
                }
                if (stats.secElement == "Ice")
                {
                    stats.secMod *= 1.1;
                    stats.addSecElement += 6;
                }
            }

            else if (skillVal == 3) //Ice Atk Down
            {
                if (stats.altDamageType == "Ice")
                {
                    stats.eleMod *= 0.75;
                }
                if (stats.secElement == "Ice")
                {
                    stats.secMod *= 0.75;
                }

            }

            return true;
        }

        public bool KO(int skillVal)
        {
            stats.KOMod *= 1.1;
            return true;
        }

        public bool Mechanic(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //Blast portion
            {
                if (stats.altDamageType == "Blast")
                {
                    stats.staMod *= 1.2;
                }


                if (stats.secElement == "Blast")
                {
                    stats.staSecMod *= 1.2;
                }
            }

            else if (skillVal == 2) //Bomb portion
            {
                stats.expMod *= 1.3;
            }

            return true;
        }

        public bool NormalUp(int skillVal)
        {
            if (stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.1;
            }

            return true;
        }

        public bool PelletUp(int skillVal)
        {
            if (stats.sharpness != "(No Sharpness)")
            {
                return true;
            }

            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            else if (skillVal == 1) //Pellet S
            {
                stats.rawMod *= 1.2;
            }

            else if (skillVal == 2) //Spread
            {
                stats.rawMod *= 1.3;
            }

            return true;
        }

        public bool PierceUp(int skillVal)
        {
            if (stats.sharpness != "(No Sharpness)")
            {
                return true;
            }

            stats.rawMod *= 1.1;
            return true;
        }

        public bool Potential(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            if (skillVal == 1) //Adrenaline +2
            {
                stats.rawMod *= 1.3;
            }

            else if (skillVal == 2) //Worrywart
            {
                stats.rawMod *= 0.7;
            }

            return true;
        }

        public bool Prudence(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool PunishDraw(int skillVal)
        {
            stats.addRaw += 5;
            if (moveDraw.Checked && stats.damageType == "Cut")
            {
                stats.KOPower += 30;
                stats.exhaustPower += 20;
            }

            return true;
        }

        public bool Readiness(int skillVal)
        {
            stats.addRaw += 5;
            if (moveDraw.Checked && stats.damageType == "Cut")
            {
                stats.KOPower += 30;
                stats.exhaustPower += 20;
            }

            return true;
        }

        public bool RapidFire(int skillVal)
        {
            if (stats.sharpness == "(No Sharpness)" && stats.hitCount > 1)
            {
                stats.hitCount++;
            }

            return true;
        }

        public bool Redhelm(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool TrueRedhelm(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool ReverseCrit(int skillVal)
        {
            stats.ruefulCrit = true;
            return true;
        }

        public bool TrueRustrazor(int skillVal)
        {
            if (stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.5;
            }

            return true;
        }

        public bool ScaledSword(int skillVal)
        {
            if (stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.5;
            }

            return true;
        }

        public bool Silverwind(int skillVal)
        {
            stats.positiveAffinity += 30;
            return true;
        }

        public bool TrueSilverwind(int skillVal)
        {
            stats.positiveAffinity += 30;
            return true;
        }

        public bool Spirit(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            if (skillVal == 1)
            {
                stats.positiveAffinity += 10;
                stats.addRaw += 10;
            }

            else if (skillVal == 2)
            {
                stats.positiveAffinity += 15;
                stats.addRaw += 20;
            }

            return true;
        }

        public bool StamDrain(int skillVal)
        {
            stats.ExhMod *= 1.2;
            return true;
        }

        public bool StatusAtk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Status Atk +1
            {
                if (isStatus(stats.altDamageType))
                {
                    stats.staMod *= 1.1;
                    stats.addStatus += 1;
                }
                if (isStatus(stats.secElement))
                {
                    stats.staSecMod *= 1.1;
                    stats.addSecStatus += 1;
                }
            }

            else if (skillVal == 2) //Status Atk +2
            {
                if (isStatus(stats.altDamageType))
                {
                    stats.staMod *= 1.2;
                    stats.addStatus += 1;
                }
                if (isStatus(stats.secElement))
                {
                    stats.staSecMod *= 1.2;
                    stats.addSecStatus += 1;
                }
            }

            else if (skillVal == 3) //Status Atk Down
            {
                if (isStatus(stats.altDamageType))
                {
                    stats.staMod *= 0.9;
                }
                if (isStatus(stats.secElement))
                {
                    stats.staSecMod *= 0.9;
                }
            }

            return true;
        }

        public bool SteadyHand(int skillVal)
        {
            if (stats.sharpness != "(No Sharpness)")
            {
                return true;
            }

            if (skillVal < 1 || skillVal > 4)
            {
                return false;
            }

            if (skillVal == 1) //Normal Rapid Up
            {
                stats.rawMod *= 1.1;
            }

            else if (skillVal == 2) //Pellet 
            {
                stats.rawMod *= 1.2;
            }

            else if (skillVal == 3) //Spread
            {
                stats.rawMod *= 1.3;
            }

            else if (skillVal == 4) //Pierce/Pierce Up
            {
                stats.rawMod *= 1.1;
            }

            return true;
        }

        public bool Soulseer(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            if (skillVal == 1) //Not Enraged
            {
                stats.positiveAffinity += 30;
            }

            else if (skillVal == 2)
            {
                stats.positiveAffinity += 30;
                stats.addRaw += 10;
                stats.positiveAffinity += 10;
            }

            return true;
        }

        public bool TrueSoulseer(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            if (skillVal == 1) //Not Enraged
            {
                stats.positiveAffinity += 30;
            }

            else if (skillVal == 2)
            {
                stats.positiveAffinity += 30;
                stats.addRaw += 20;
                stats.positiveAffinity += 15;
            }

            return true;
        }

        public bool Survivor(int skillVal)
        {
            if (skillVal < 1 || skillVal > 2)
            {
                return false;
            }

            if (skillVal == 1)
            {
                stats.rawMod *= 1.1;
            }

            if (skillVal == 2)
            {
                stats.rawMod *= 1.2;
            }

            return true;
        }

        public bool Tenderizer(int skillVal)
        {
            if (stats.hitzone >= 45)
            {
                stats.positiveAffinity += 50;
            }
            return true;
        }

        public bool ThunderAtk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Thunder Atk +1
            {
                if (stats.altDamageType == "Thunder")
                {
                    stats.eleMod *= 1.05;
                    stats.addElement += 4;
                }
                if (stats.secElement == "Thunder")
                {
                    stats.secMod *= 1.05;
                    stats.addSecElement += 4;
                }
            }

            else if (skillVal == 2) //Thunder Atk +2
            {
                if (stats.altDamageType == "Thunder")
                {
                    stats.eleMod *= 1.1;
                    stats.addElement += 6;
                }
                if (stats.secElement == "Thunder")
                {
                    stats.secMod *= 1.1;
                    stats.addSecElement += 6;
                }
            }

            else if (skillVal == 3) //Thunder Atk Down
            {
                if (stats.altDamageType == "Thunder")
                {
                    stats.eleMod *= 0.75;
                }
                if (stats.secElement == "Thunder")
                {
                    stats.secMod *= 0.75;
                }

            }

            return true;
        }

        public bool Thunderlord(int skillVal)
        {
            stats.positiveAffinity += 50;
            return true;
        }

        public bool TrueThunderlord(int skillVal)
        {
            stats.positiveAffinity += 50;
            return true;
        }

        public bool Unscathed(int skillVal)
        {
            stats.addRaw += 20;
            return true;
        }

        public bool Vault(int skillVal)
        {
            if(moveAerial.Checked)
            {
                stats.rawMod *= 1.1;
            }
            return true;
        }

        public bool WaterAtk(int skillVal)
        {
            if (skillVal < 1 || skillVal > 3)
            {
                return false;
            }

            else if (skillVal == 1) //Water Atk +1
            {
                if (stats.altDamageType == "Water")
                {
                    stats.eleMod *= 1.05;
                    stats.addElement += 4;
                }
                if (stats.secElement == "Water")
                {
                    stats.secMod *= 1.05;
                    stats.addSecElement += 4;
                }
            }

            else if (skillVal == 2) //Water Atk +2
            {
                if (stats.altDamageType == "Water")
                {
                    stats.eleMod *= 1.1;
                    stats.addElement += 6;
                }
                if (stats.secElement == "Water")
                {
                    stats.secMod *= 1.1;
                    stats.addSecElement += 6;
                }
            }

            else if (skillVal == 3) //Water Atk Down
            {
                if (stats.altDamageType == "Water")
                {
                    stats.eleMod *= 0.75;
                }
                if (stats.secElement == "Water")
                {
                    stats.secMod *= 0.75;
                }

            }

            return true;
        }
#endif

#if true
        //Weapon-Specific
        public bool LSM(int skillVal)
        {
            if (stats.sharpness == "Yellow" || stats.sharpness == "Orange" || stats.sharpness == "Red")
            {
                stats.rawMod *= 0.6;
            }
            return true;
        }

        public bool GS(int skillVal)
        {
            if (skillVal < 1 || skillVal > 4)
            {
                return false;
            }

            if (skillVal == 1) //Center of Blade
            {
                stats.rawMod *= 1.05;
            }
            else if (skillVal == 2) //Lion's Maw I
            {
                stats.rawMod *= 1.1;
            }
            else if (skillVal == 3) //Lion's Maw II
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 4) //Lion's Maw III
            {
                stats.rawMod *= 1.33;
            }

            return true;
        }

        public bool LS(int skillVal)
        {
            if (skillVal == 1) //Center of Blade
            {
                stats.rawMod *= 1.05;
            }
            else if (skillVal == 2) //Spirit Gauge active
            {
                stats.rawMod *= 1.13;
            }
            else if (skillVal == 3) //White Gauge
            {
                stats.rawMod *= 1.05;
            }
            else if (skillVal == 4) //Yellow Gauge
            {
                stats.rawMod *= 1.1;
            }
            else if (skillVal == 5) //Red Gauge
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 6) //Blue Gauge
            {
                stats.rawMod *= 1.7;
            }
            else if (skillVal == 7) //Sacrificial Blade I
            {
                stats.rawMod *= 1.1;
            }
            else if (skillVal == 8) //Sacrificial Blade II
            {
                stats.rawMod *= 1.2;
            }

            else if (skillVal == 9) //Sacrificial Blade III
            {
                stats.rawMod *= 1.3;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool SnS(int skillVal)
        {
            if (skillVal == 1) //Affinity Oil (& Chaos Oil III)
            {
                stats.positiveAffinity += 30;
            }

            else if (skillVal == 2) //Affinity Oil w/ Chaos Oil I & II
            {
                stats.positiveAffinity += 15;
            }

            else if (skillVal == 3) //Stamina Oil
            {
                stats.KOPower += 8;
                stats.exhaustPower += 10;
            }

            else if (skillVal == 4) //Stamina Oil w/ Chaos Oil I/II
            {
                stats.KOPower += 4;
                stats.exhaustPower += 5;
            }

            else if (skillVal == 5) //Stamina Oil w/ Chaos Oil III
            {
                stats.KOPower += 5;
                stats.exhaustPower += 6;
            }

            else if (skillVal == 6) //Mind's Eye Oil
            {
                stats.mindsEye = true;
            }

            else if (skillVal == 7) //Chaos Oil I
            {
                stats.positiveAffinity += 15;
                stats.KOPower += 4;
                stats.exhaustPower += 5;
            }

            else if (skillVal == 8) //Chaos Oil II
            {
                stats.positiveAffinity += 15;
                stats.KOPower += 4;
                stats.exhaustPower += 5;
            }

            else if (skillVal == 9) //Chaos Oil III
            {
                stats.positiveAffinity += 30;
                stats.KOPower += 7;
                stats.exhaustPower += 8;
            }

            else
            {
                return false;
            }
            return true;
        }

        public bool HH(int skillVal)
        {
            if (skillVal == 1) //Attack Up (S) Song
            {
                stats.rawMod *= 1.10;
            }
            else if (skillVal == 2) //Attack Up (S) Encore
            {
                stats.rawMod *= 1.15;
            }
            else if (skillVal == 3) //Attack Up (L) Song
            {
                stats.rawMod *= 1.15;
            }
            else if (skillVal == 4) //Attack Up (L) Encore
            {
                stats.rawMod *= 1.20;
            }
            else if (skillVal == 5) //Elem. Attack Boost Song
            {
                stats.eleMod *= 1.08;
            }
            else if (skillVal == 6) //Elem. Attack Boost Encore
            {
                stats.eleMod *= 1.10;
            }
            else if (skillVal == 7) //Abnormal Boost Song
            {
                stats.staMod *= 1.1;
            }
            else if (skillVal == 8) //Abnormal Boost Encore
            {
                stats.staMod *= 1.15;
            }
            else if (skillVal == 9) //Affinity Up Song
            {
                stats.positiveAffinity += 15;
            }
            else if (skillVal == 10) //Affinity Up Encore
            {
                stats.positiveAffinity += 20;
            }
            else if (skillVal == 11) //Self-Improvement Encore
            {
                stats.mindsEye = true;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool Lance(int skillVal)
        {
            if (skillVal == 1) //Enraged Guard (Yellow)
            {
                stats.rawMod *= 1.1;
            }
            else if (skillVal == 2) //Enraged Guard (Orange)
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 3) //Enraged Guard (Red)
            {
                stats.rawMod *= 1.3;
            }
            else if (skillVal == 4) //Impact/Cut Hitzone
            {
                if (double.Parse(monImpact.Text) * 0.72 > double.Parse(monCut.Text))
                {
                    stats.hitzone = double.Parse(monImpact.Text);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool Gunlance(int skillVal)
        {
            if (skillVal == 1) //Dragon Breath
            {
                stats.avgMV += 10;
                stats.secPower += 10;
            }
            else if (skillVal == 2) //Orange Heat
            {
                stats.rawMod *= 1.15;
            }
            else if (skillVal == 3) //Red Heat
            {
                stats.rawMod *= 1.2;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool SA(int skillVal)
        {
            if (skillVal == 1) //Power Phial
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 2) //Element Phial
            {
                stats.eleMod *= 1.25;
            }
            else if (skillVal == 3) //Energy Charge II
            {
                stats.positiveAffinity += 10;
            }
            else if (skillVal == 4) //Energy Charge III
            {
                stats.positiveAffinity += 30;
            }
            else if (skillVal == 5) //Demon Riot I 'Pwr'
            {
                stats.rawMod *= 1.05;
            }
            else if (skillVal == 6) //Demon Riot II 'Pwr'
            {
                stats.rawMod *= 1.10;
            }
            else if (skillVal == 7) //Demon Riot III 'Pwr'
            {
                stats.rawMod *= 1.20;
            }
            else if (skillVal == 8) //Demon Riot I 'Ele'
            {
                stats.eleMod *= 1.05;
                stats.staMod *= 1.05;
                stats.DemonRiot = true;
            }
            else if (skillVal == 9) //Demon Riot II 'Ele'
            {
                stats.eleMod *= 1.10;
                stats.staMod *= 1.10;
                stats.DemonRiot = true;
            }
            else if (skillVal == 10) //Demon Riot III 'Ele'
            {
                stats.eleMod *= 1.20;
                stats.staMod *= 1.20;
                stats.DemonRiot = true;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool CB(int skillVal)
        {
            if (skillVal == 1) //Red Shield (Other Styles)
            {
                if (stats.damageType == "Fixed")
                {
                    stats.rawMod *= 1.35;
                }
                else
                {
                    stats.rawMod *= 1.15;
                }
                stats.KOPower *= 1.35;
                stats.exhaustPower *= 1.35;
            }
            else if (skillVal == 2) //Red Shield (Striker)
            {
                stats.rawMod *= 1.2;
                if (stats.damageType == "Fixed")
                {
                    stats.rawMod *= 1.35;
                }
                stats.KOPower *= 1.35;
                stats.exhaustPower *= 1.35;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool IG(int skillVal)
        {
            if (skillVal == 1) //Red (Balanced)
            {
                stats.addRaw += 5;
            }
            else if (skillVal == 2) //White (Balanced)
            {
                stats.positiveAffinity += 10;
            }
            else if (skillVal == 3) //Red/White
            {
                stats.rawMod *= 1.15;
            }
            else if (skillVal == 4) //Triple Up
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 5) //White (Speed)
            {
                stats.positiveAffinity += 30;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool Gunner(int skillVal)
        {
            if (skillVal == 1) //Normal Distance (1x)
            {
                stats.rawMod *= 1;
            }
            else if (skillVal == 2) //Critical Distance (1.5x)
            {
                stats.rawMod *= 1.5;
            }
            else if (skillVal == 3) //Long Range (0.8x)
            {
                stats.rawMod *= 0.8;
            }
            else if (skillVal == 4) //Ex. Long Rnage (0.5x)
            {
                stats.rawMod *= 0.5;
            }
            else if(skillVal == 5) //Critical Distance + Heavy Grinder (1.75x) OR Demon S
            {
                stats.rawMod *= 1.75;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool LBG(int skillVal)
        {
            if (skillVal == 1) //Raw Multiplier (1.3x)
            {
                stats.rawMod *= 1.3;
            }
            else if (skillVal == 2) //Long Barrel (1.05x)
            {
                stats.totalAttackPower *= 1.05;
            }
            else if (skillVal == 3) //Power Reload
            {
                stats.rawMod *= 1.06;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool HBG(int skillVal)
        {
            if (skillVal == 1) //Raw Multiplier (1.48x)
            {
                stats.rawMod *= 1.48;
            }
            else if (skillVal == 2) //Power Barrel (1.05x)
            {
                stats.totalAttackPower *= 1.05;
            }
            else if (skillVal == 3) //Power Reload
            {
                stats.rawMod *= 1.06;
            }
            else if(skillVal == 5) //Valor Reload
            {
                stats.rawMod *= 1.05;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool Bow(int skillVal)
        {
            if(skillVal == 1) //Charge 1
            {
                stats.rawMod *= 0.4;
                stats.eleMod *= 0.7;
                stats.staMod *= 0.5;
            }

            else if(skillVal == 2) //Charge 2
            {
                stats.rawMod *= 1.0;
                stats.eleMod *= 0.85;
                stats.staMod *= 1.0;
            }
            
            else if(skillVal == 3) //Charge 3 + Poison
            {
                stats.rawMod *= 1.5;
                stats.eleMod *= 1.0;
                stats.staMod *= 1.5;
            }

            else if(skillVal == 4) //Charge 3 + Para
            {
                stats.rawMod *= 1.5;
                stats.eleMod *= 1.0;
                stats.staMod *= 1.3;
            }

            else if(skillVal == 5) //Charge 4 + Poison
            {
                stats.rawMod *= 1.7;
                stats.eleMod *= 1.125;
                stats.staMod *= 1.5;
            }

            else if(skillVal == 6) //Charge 4 + Para
            {
                stats.rawMod *= 1.7;
                stats.eleMod *= 1.125;
                stats.staMod *= 1.3;
            }

            else if(skillVal == 7) //Valor Power Shot
            {
                stats.rawMod *= 1.3;
            }

            else if (skillVal == 8) //Power C. Lvl. 1
            {
                stats.rawMod *= 1.35;
            }
            else if (skillVal == 9) //Power C. Lvl. 2
            {
                stats.rawMod *= 1.5;
            }
            else if (skillVal == 10) //Ele. C. Lvl. 1
            {
                stats.eleMod *= 1.35;
            }
            else if (skillVal == 11) //Ele. C. Lvl. 2
            {
                stats.eleMod *= 1.50;
            }
            else if (skillVal == 12) //Coating Boost 'Pwr'
            {
                stats.rawMod *= 1.20;
            }
            else if (skillVal == 13) //Coating Boost 'Ele'
            {
                stats.eleMod *= 1.20;
            }
            else if (skillVal == 14) //Coating Boost 'C.Range'
            {
                stats.rawMod *= 1.50;
            }
            else if (skillVal == 15) //Coating Boost 'Sta'
            {
                stats.staMod *= 1.20;
            }

            else
            {
                return false;
            }

            return true;
        }
#endif

#if true
        public bool FBombardier(int skillVal)
        {
            //Artillery Novice
            if (skillVal == 1) //Cannon/Ballista
            {
                stats.rawMod *= 1.1;
                return true;
            }

            else if (skillVal == 2) //Explosive Shots
            {
                stats.expMod *= 1.15;
                return true;
            }

            else if (skillVal == 3) //Impact Phial
            {
                stats.expMod *= 1.3;
                stats.CB = true;
                return true;
            }

            else if (skillVal == 4) //GL shots
            {
                stats.eleMod *= 1.1;
                return true;
            }
            return false;
        }

        public bool FBooster()
        {
            stats.addRaw += 3;
            return true;
        }
        
        public bool FBulldozer()
        {
            if (weapSharpness.Text != "(No Sharpness)")
            {
                stats.rawSharpMod *= 1.05;
            }
            return true;
        }
        
        public bool FHeroics()
        {
            stats.rawMod *= 1.35;
            return true;
        }
        
        public bool FPyro()
        {
            if (stats.altDamageType == "Blast")
            {
                stats.staMod *= 1.1;
            }
            if(stats.secElement == "Blast")
            {
                stats.staSecMod *= 1.1;
            }

            return true;
        }
        
        public bool FSharpshooter()
        {
            if(stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.1;
            }
            return true;
        }
        
        public bool FSlugger()
        {
            stats.KOPower *= 1.1;
            return true;
        }
        
        public bool FSpecialist()
        {
            if (isStatus(stats.altDamageType))
            {
                stats.staMod *= 1.125;
            }
            if(isStatus(stats.secElement))
            {
                stats.staSecMod *= 1.125;
            }
            return true;
        }
        
        public bool FTemper()
        {
            if(stats.sharpness == "(No Sharpness)")
            {
                stats.rawMod *= 1.05;
            }
            return true;
        }
        
        public bool CoolCat()
        {
            stats.addRaw += 15;
            return true;
        }
        
        public bool Powercharm()
        {
            stats.addRaw += 6;
            return true;
        }
        
        public bool PowerTalon()
        {
            stats.addRaw += 9;
            return true;
        }
        
        public bool DemonDrug(int skillVal)
        {
            if (skillVal == 1) //Standard Demondrug
            {
                stats.addRaw += 5;
            }
            else if (skillVal == 2) //Mega Demondrug
            {
                stats.addRaw += 7;
            }
            else
            {
                return false;
            }
            return true;
        }
        
        public bool AUMeal(int skillVal)
        {
            if (skillVal == 1) //AuS
            {
                stats.addRaw += 3;
            }
            else if (skillVal == 2) //AuM
            {
                stats.addRaw += 5;
            }
            else if (skillVal == 3) //AuL
            {
                stats.addRaw += 7;
            }
            else
            {
                return false;
            }
            return true;
        }
        
        public bool MightSeed(int skillVal)
        {
            if (skillVal == 1) //Seed
            {
                stats.addRaw += 10;
            }
            else if (skillVal == 2) //Pill
            {
                stats.addRaw += 25;
            }
            else
            {
                return false;
            }
            return true;
        }
        
        public bool Nitroshroom()
        {
            stats.addRaw += 10;
            return true;
        }
        
        public bool Demon(int skillVal)
        {
            if (skillVal == 1) //Horn
            {
                stats.addRaw += 10;
            }
            else if (skillVal == 2) //S
            {
                stats.addRaw += 10;
                if (weapSharpness.Text != "(No Sharpness)")
                {
                    stats.rawSharpMod *= 1.1;
                }
            }
            else if (skillVal == 3) //Affinity S
            {
                stats.addRaw += 15;
                if (weapSharpness.Text != "(No Sharpness)")
                {
                    stats.rawSharpMod *= 1.1;
                }
                stats.positiveAffinity += 10;
            }
            else
            {
                return false;
            }
            return true;
        }
#endif

#if true
        public bool Frenzy(int skillVal)
        {
            if(skillVal == 1) //No Antivirus
            {
                if(stats.chaotic)
                {
                    stats.chaotic = false;
                    stats.positiveAffinity += stats.negativeAffinity + 15;
                }
                else
                {
                    stats.positiveAffinity += 15;
                }
            }

            if (skillVal == 2)
            {
                if (stats.chaotic)
                {
                    stats.chaotic = false;
                    stats.positiveAffinity += stats.negativeAffinity + 30;
                }
                else
                {
                    stats.positiveAffinity += 30;
                }
            }

            return true;
        }
#endif
    }
    /// <summary>
    /// Stores the relevant variables from the database portion of the application to
    /// import to the calculator portion.
    /// No ctor. Will be filled in when the UpdateButt is clicked.
    /// </summary>
    public class ImportedStats
    {
        public string sharpness; //Current Sharpness
        public string altDamageType;
        public double totalAttackPower;
        public double eleAttackPower;
        public double positiveAffinity;
        public double negativeAffinity;
        public double rawSharpMod;
        public double eleSharpMod;
        public double avgMV;
        public int hitCount;
        public double totalMV;
        public double KOPower;
        public double exhaustPower;
        public bool criticalBoost;
        public bool mindsEye;
        public string damageType;
        public string eleCrit;
        public bool statusCrit;

        public string secElement;
        public double secPower;

        public double hitzone;
        public double eleHitzone;
        public double secHitzone;
        public double questMod;
        public double KOHitzone;
        public double exhaustHitzone;
        public double exhaustMod;

        public double rawMod; //Stores the multiplier of the raw damage.
        public double eleMod; //Stores the elemental multiplier. Has a cap of 1.2x, surpassed when used Demon Riot on an Element Phial SA.
        public double secMod; //Stores the elemental multiplier for the second element. Has same restrictions as above.
        public double expMod; //Stores the explosive multiplier. Has a cap of 1.3x, 1.4x when considering Impact Phial CB.
        public double staMod; //Stores the status multiplier. Has a cap of 1.25x, surpassed when using Demon Riot on a Status Phial SA.
        public double staSecMod;
        public double KOMod;
        public double ExhMod;

        public bool CB; //Shows whether or not the explosive multiplier should be increased because Impact Phials are being used. 
        public bool DemonRiot; //Shows whether or not Demon Riot is being used.
        public bool chaotic; //Shows whether or not a Chaotic Gore weapon is being used.
        public bool ruefulCrit;

        public int addRaw; //Stores the additive portion of raw
        public int addElement; //Stores the additive portion of element after Atk +1 or +2
        public int addSecElement; //Stores the additive portion of element after Atk +1 or +2
        public int addStatus; //Stores the additive portion of status after Atk +1 or +2
        public int addSecStatus; //Stores the addition portion of status after Atk+1 or +2

        public double health; //Stores the monster's health.

        public Dictionary<string, Tuple<double, double>> sharpnessValues = new Dictionary<string, Tuple<double, double>>();

        public ImportedStats()
        {
            sharpnessValues.Add("(No Sharpness)", new Tuple<double, double>(1.00, 1.00));
            sharpnessValues.Add("White", new Tuple<double, double>(1.32, 1.12));
            sharpnessValues.Add("Blue", new Tuple<double, double>(1.20, 1.06));
            sharpnessValues.Add("Green", new Tuple<double, double>(1.05, 1.0));
            sharpnessValues.Add("Yellow", new Tuple<double, double>(1.00, 0.75));
            sharpnessValues.Add("Orange", new Tuple<double, double>(0.75, 0.50));
            sharpnessValues.Add("Red", new Tuple<double, double>(0.50, 0.25));
        }


        public void UpdateSharpness(string newSharpness)
        {
            sharpness = newSharpness;
            rawSharpMod = sharpnessValues[newSharpness].Item1;
            eleSharpMod = sharpnessValues[newSharpness].Item2;
        }
    }
}
