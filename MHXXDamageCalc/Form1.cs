using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.IO;

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
        Dictionary<string, Func<int, bool>> miscModifiers = new Dictionary<string, Func<int, bool>>();
        Dictionary<string, string> secondElements = new Dictionary<string, string>();

        weaponStorage currentWeapons = new weaponStorage();
        List<string> filteredWeapons = new List<string>();
        List<ListViewItem> allMonsters = new List<ListViewItem>();
        List<ListViewItem> staMonsters = new List<ListViewItem>();

        ListViewColumnSorter weaponColumnSorter;
        ListViewColumnSorter moveColumnSorter;
        ListViewColumnSorter monsterColumnSorter;
        ListViewColumnSorter hitzoneColumnSorter;
        ListViewColumnSorter questColumnSorter;

        string[] quips = new string[] { "Try saying Soulseer Soul 10 times fast.'", "Elderfrost makes some good snowcones.'", "Also try Ping's Dex!'", "Also try Athena's Armor Set Search!'", "If you squint really hard, you can see damage values.'",
        "Is Boltreaver the Monado?'", "Lavasioth got a few trophies for his fabulous dance moves.'", "Tigrex is really hungry, like all the time.'",
        "Akantor could be in Mario Kart with how well he drifts.'", "You need to stop bullying Ukanlos. He's just trying to chill.'", "NEOPTERON GEAR SOLID: Tactical Hunting Action'",
        "If Rustrazor figures out how to triple-wield, we're all screwed.'", "PWAAAAAAAAAH'", "Nightcloak is a nerd. He actually carries around a D20.'",
        "I still prefer calling Bloodbath Diablos 'Bloodlust Diablos'.'", "Generations Unite > Generations Ultimate. Fite me.'", "Do Hellblade eat spicy Doritoes?'",
        "Redhelm? You mean... Super Bear?'", "Welcome... TO THE ELECTRO-DROME!'", "What's a 'World'? ...Can I eat it?'", "Is Silverwind an edgelord?'", "DK! DREADKING-KONG'", "It's all fun and games until a Savage Jho invades.'",
        "Hypers: Twice the Health, Half the Fun'", "Bet you didn't Ceadeus coming.'", "The Last Jhodi: The Hunger Awakens'", "Dedicated to Cha-Cha, best companion.'", "Hunter-tar, Master of all 4 Generations.'",
        "Now finally out of Beta!'", "Are *you* the monster in 'Monster Hunter'?'", "I'M GONNA PUNCH THIS HUNTER SO HARD HE EXPLODES'", "Definitely *not* dedicated to Kayamba.'",
        "4U Best Game! ...Wait'", "Is that Soulseer as the icon or just a Mizutsune?'", "Jaggia? Never even met her!'", "Teostra needs to get his paws out of the Doritos.'",
        "Na Na na na Na Na na na Nargacuga!'", "Honts?'", "MH3U: 7.8/10 Too Much Water.'", "Chameleos is just the cutest Elder Dragon.'", "Mizumi desu~'", "Yukari here. >_<'", "Yes? It's Ashlynn here.'", "Made by Awesomeosity!'", "Mmm? What business do you have with me, Fiore?'"};

        

        public Form1()
        {
            InitializeComponent();
            weaponColumnSorter = new ListViewColumnSorter();
            weaponDetails.ListViewItemSorter = weaponColumnSorter;
            moveColumnSorter = new ListViewColumnSorter();
            moveDetails.ListViewItemSorter = moveColumnSorter;
            monsterColumnSorter = new ListViewColumnSorter();
            monsterList.ListViewItemSorter = monsterColumnSorter;
            hitzoneColumnSorter = new ListViewColumnSorter();
            hitzoneDetails.ListViewItemSorter = hitzoneColumnSorter;
            questColumnSorter = new ListViewColumnSorter();
            questDetails.ListViewItemSorter = questColumnSorter;
            setUp();
            setSelected();
        }

        public void setUp()
        {
            Random rng = new Random();
            this.Text += quips[rng.Next(quips.Length)];

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

            secondElements.Add("(No Element)", "(No Element)");
            secondElements.Add("DB - Fire", "Fire");
            secondElements.Add("DB - Water", "Water");
            secondElements.Add("DB - Thunder", "Thunder");
            secondElements.Add("DB - Ice", "Ice");
            secondElements.Add("DB - Para", "Para");
            secondElements.Add("DB - Poison", "Poison");
            secondElements.Add("DB - Blast", "Blast");
            secondElements.Add("SA - Dragon", "Dragon");
            secondElements.Add("SA - Poison", "Poison");
            secondElements.Add("SA - Para", "Para");
            secondElements.Add("SA - Exhaust", "Exhaust");

            armorModifiers.Add("Elementality", x => Amplify(1));
            armorModifiers.Add("Frosty Protection (Cold Area)", x => AntiDaora(1));
            armorModifiers.Add("Frosty Protection (Cool Drink)", x => AntiDaora(2));
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

            armorModifiers.Add("Bloodbath Soul", x => Bloodbath(1));
            armorModifiers.Add("Bloodbath SoulX ", x => TrueBloodbath(1));
            armorModifiers.Add("Boltreaver Soul", x => Boltreaver(1));
            armorModifiers.Add("Boltreaver Soul X", x => TrueBoltreaver(1));
            armorModifiers.Add("Bludgeoner (GU)", x => Blunt(1));
            armorModifiers.Add("Bludgeoner (Gen)", x => Blunt(2));
            armorModifiers.Add("Bombardier (Blast)", x => BombBoost(1));
            armorModifiers.Add("Bombardier (Bomb)", x => BombBoost(2));
            armorModifiers.Add("Heavy Hitter", x => Brawn(1));
            armorModifiers.Add("Ruthlessness", x => Brutality(1));

            armorModifiers.Add("Repeat Offender (1 hit)", x => ChainCrit(1));
            armorModifiers.Add("Repeat Offender (>=5 hits)", x => ChainCrit(2));
            armorModifiers.Add("Trump Card (Other HAs)", x => Chance(1));
            armorModifiers.Add("Trump Card (Lion's Maw)", x => Chance(2));
            armorModifiers.Add("Trump Card (Wyvern's Breath)", x => Chance(3));
            armorModifiers.Add("Trump Card (Demon Riot 'Pwr')", x => Chance(4));
            armorModifiers.Add("Trump Card (Demon Riot 'Sta')", x => Chance(5));
            armorModifiers.Add("Trump Card (Demon Riot 'Ele')", x => Chance(6));
            armorModifiers.Add("Trump Card (Devouring Demon)", x => Chance(7));
            armorModifiers.Add("Polar Hunter (Cold Area)", x => ColdBlooded(1));
            armorModifiers.Add("Polar Hunter (Cool Drink)", x => ColdBlooded(2));
            armorModifiers.Add("Resuscitate", x => Crisis(1));
            armorModifiers.Add("Critical Draw", x => CritDraw(1));
            armorModifiers.Add("Elemental Crit (SnS/DB/Bow)", x => CritElement(1));
            armorModifiers.Add("Elemental Crit (LBG/HBG)", x => CritElement(2));
            armorModifiers.Add("Elemental Crit (Other)", x => CritElement(3));
            armorModifiers.Add("Elemental Crit (GS)", x => CritElement(4));
            armorModifiers.Add("Status Crit", x => CritStatus(1));
            armorModifiers.Add("Critical Boost", x => CriticalUp(1));

            armorModifiers.Add("Pro D. Fencer (0 Carts)", x => DFencing(1));
            armorModifiers.Add("Pro D. Fencer (One Cart)", x => DFencing(2));
            armorModifiers.Add("Pro D. Fencer (Two Carts)", x => DFencing(3));
            armorModifiers.Add("D.eye Soul", x => Deadeye(1));
            armorModifiers.Add("D.eye Soul X (0 Carts)", x => TrueDeadeye(1));
            armorModifiers.Add("D.eye Soul X (0 Carts, Enraged)", x => TrueDeadeye(2));
            armorModifiers.Add("D.eye Soul X (1 Carts)", x => TrueDeadeye(3));
            armorModifiers.Add("D.eye Soul X (1 Carts, Enraged)", x => TrueDeadeye(4));
            armorModifiers.Add("D.eye Soul X (2 Carts)", x => TrueDeadeye(5));
            armorModifiers.Add("D.eye Soul X (2 Carts, Enraged)", x => TrueDeadeye(6));
            armorModifiers.Add("Dragon Atk +1", x => DragonAtk(1));
            armorModifiers.Add("Dragon Atk +2", x => DragonAtk(2));
            armorModifiers.Add("Dragon Atk Down", x => DragonAtk(3));
            armorModifiers.Add("Dragon's Spirit", x => DragonAura(1));
            armorModifiers.Add("Dreadking Soul", x => Dreadking(1));
            armorModifiers.Add("Dreadking Soul X", x => TrueDreadking(1));
            armorModifiers.Add("Dreadqueen Soul", x => Dreadqueen(1));
            armorModifiers.Add("Dreadqueen Soul X", x => TrueDreadqueen(1));
            armorModifiers.Add("Drilltusk Soul", x => Drilltusk(1));
            armorModifiers.Add("D.tusk Soul X (Adren +2)", x => TrueDrilltusk(1));
            armorModifiers.Add("D.tusk Soul X (Fixed Weaps.)", x => TrueDrilltusk(2));
            armorModifiers.Add("D.tusk Soul X (Exp. Shots)", x => TrueDrilltusk(3));
            armorModifiers.Add("D.tusk Soul X (Impact Phial)", x => TrueDrilltusk(4));
            armorModifiers.Add("D.tusk Soul X (GL Shots)", x => TrueDrilltusk(5));

            armorModifiers.Add("Honed Blade", x => Edgemaster(1));
            armorModifiers.Add("Elderfrost Soul", x => Elderfrost(1));
            armorModifiers.Add("Elderfrost Soul X", x => TrueElderfrost(1));
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
            armorModifiers.Add("Hellblade Soul X (Blast)", x => TrueHellblade(1));
            armorModifiers.Add("Hellblade Soul X (Bomb)", x => TrueHellblade(2));
            armorModifiers.Add("Tropic Hunter", x => HotBlooded(1));
            armorModifiers.Add("Flying Pub Soul", x => HuntersPub(1));

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
            armorModifiers.Add("Redhelm Soul X", x => TrueRedhelm(1));
            armorModifiers.Add("Rueful Crit", x => ReverseCrit(1));
            armorModifiers.Add("Rustrazor Soul X", x => TrueRustrazor(1));

            armorModifiers.Add("Shining Blade", x => ScaledSword(1));
            armorModifiers.Add("Silverwind Soul", x => Silverwind(1));
            armorModifiers.Add("Silverwind Soul X", x => TrueSilverwind(1));
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
            armorModifiers.Add("Soulseer Soul", x => Soulseer(1));
            armorModifiers.Add("Soulseer Soul (Enraged)", x => Soulseer(2));
            armorModifiers.Add("Soulseer Soul X", x => TrueSoulseer(1));
            armorModifiers.Add("Soulseer Soul X (Enraged)", x => TrueSoulseer(2));
            armorModifiers.Add("Fortify (1st Cart)", x => Survivor(1));
            armorModifiers.Add("Fortify (2nd Cart)", x => Survivor(2));

            armorModifiers.Add("Weakness Exploit", x => Tenderizer(1));
            armorModifiers.Add("Thunder Atk +1", x => ThunderAtk(1));
            armorModifiers.Add("Thunder Atk +2", x => ThunderAtk(2));
            armorModifiers.Add("Thunder Atk Down", x => ThunderAtk(3));
            armorModifiers.Add("Thunderlord Soul", x => Thunderlord(1));
            armorModifiers.Add("Thunderlord Soul X", x => TrueThunderlord(1));

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
            weaponModifiers.Add("GS Center of Blade", x => GS(1));
            weaponModifiers.Add("GS Lion's Maw I", x => GS(2));
            weaponModifiers.Add("GS Lion's Maw II", x => GS(3));
            weaponModifiers.Add("GS Lion's Maw III", x => GS(4));
            weaponModifiers.Add("LS Center of Blade", x => LS(1));
            weaponModifiers.Add("LS Spirit Gauge Active", x => LS(2));
            weaponModifiers.Add("LS White Gauge", x => LS(3));
            weaponModifiers.Add("LS Yellow Gauge", x => LS(4));
            weaponModifiers.Add("LS Red Gauge", x => LS(5));
            weaponModifiers.Add("LS Blue Gauge", x => LS(6));
            weaponModifiers.Add("LS Devouring Demon I", x => LS(7));
            weaponModifiers.Add("LS Devouring Demon II", x => LS(8));
            weaponModifiers.Add("LS Devouring Demon III", x => LS(9));
            weaponModifiers.Add("SnS Aff. Oil (+ C. Oil III)", x => SnS(1));
            weaponModifiers.Add("SnS Aff. Oil + C. Oil I/II", x => SnS(2));
            weaponModifiers.Add("SnS Stamina Oil", x => SnS(3));
            weaponModifiers.Add("SnS Stam. Oil + C. Oil I/II", x => SnS(4));
            weaponModifiers.Add("SnS Stam. Oil + C. Oil III", x => SnS(5));
            weaponModifiers.Add("SnS Mind's Eye Oil", x => SnS(6));
            weaponModifiers.Add("SnS Chaos Oil I/II", x => SnS(7));
            weaponModifiers.Add("SnS Chaos Oil III", x => SnS(8));
            weaponModifiers.Add("Hammer Provoke III Active", x => Hammer(1));
            weaponModifiers.Add("HH Attack Up (S) Song", x => HH(1));
            weaponModifiers.Add("HH Attack Up (S) Encore", x => HH(2));
            weaponModifiers.Add("HH Attack Up (L) Song", x => HH(3));
            weaponModifiers.Add("HH Attack Up (L) Encore", x => HH(4));
            weaponModifiers.Add("HH Ele. Attack Boost Song", x => HH(5));
            weaponModifiers.Add("HH Ele. Atk. Boost Encore", x => HH(6));
            weaponModifiers.Add("HH Abnormal Boost Song", x => HH(7));
            weaponModifiers.Add("HH Abnormal Encore", x => HH(8));
            weaponModifiers.Add("HH Affinity Up Song", x => HH(9));
            weaponModifiers.Add("HH Affinity Up Encore", x => HH(10));
            weaponModifiers.Add("HH Self-Improvement Enc.", x => HH(11));
            weaponModifiers.Add("Lance E. Guard (Yellow)", x => Lance(1));
            weaponModifiers.Add("Lance E. Guard (Orange)", x => Lance(2));
            weaponModifiers.Add("Lance Enraged Guard (Red)", x => Lance(3));
            weaponModifiers.Add("Lance Impact/Cut Zones", x => Lance(4));
            weaponModifiers.Add("GL Dragon Breath", x => Gunlance(1));
            weaponModifiers.Add("GL Orange Heat", x => Gunlance(2));
            weaponModifiers.Add("GL Red Heat", x => Gunlance(3));
            weaponModifiers.Add("GL Valor Rapid Shells #2", x => Gunlance(4));
            weaponModifiers.Add("GL Valor Rapid Shells #3", x => Gunlance(5));
            weaponModifiers.Add("GL Valor Rapid Shells #4", x => Gunlance(6));
            weaponModifiers.Add("GL Valor Rapid Shells #5", x => Gunlance(7));
            weaponModifiers.Add("GL Valor Full Burst (1 Loaded)", x => Gunlance(8));
            weaponModifiers.Add("GL Valor Full Burst (2 Loaded)", x => Gunlance(9));
            weaponModifiers.Add("GL Valor Full Burst (3 Loaded)", x => Gunlance(10));
            weaponModifiers.Add("GL Valor Full Burst (4 Loaded)", x => Gunlance(11));
            weaponModifiers.Add("GL Valor Full Burst (5 Loaded)", x => Gunlance(12));
            weaponModifiers.Add("GL Valor Full Burst (6 Loaded)", x => Gunlance(13));
            weaponModifiers.Add("GL Anti-Air Flares I", x => Gunlance(15));
            weaponModifiers.Add("GL Anti-Air Flares II", x => Gunlance(16));
            weaponModifiers.Add("GL Anti-Air Flares III", x => Gunlance(17));
            weaponModifiers.Add("GL Normal/Long Charged Shot", x => Gunlance(18));
            weaponModifiers.Add("GL Wide Charged Shot", x => Gunlance(19));
            weaponModifiers.Add("GL Long Wyvernsfire", x => Gunlance(20));
            weaponModifiers.Add("GL Normal Full Burst", x => Gunlance(21));
            weaponModifiers.Add("GL Wide Full Burst", x => Gunlance(22));
            weaponModifiers.Add("SA Power Phial", x => SA(1));
            weaponModifiers.Add("SA Element Phial", x => SA(2));
            weaponModifiers.Add("SA Energy Charge II", x => SA(3));
            weaponModifiers.Add("SA Energy Charge III", x => SA(4));
            weaponModifiers.Add("SA Demon Riot I 'Pwr'", x => SA(5));
            weaponModifiers.Add("SA Demon Riot II 'Pwr'", x => SA(6));
            weaponModifiers.Add("SA Demon Riot III 'Pwr'", x => SA(7));
            weaponModifiers.Add("SA Demon Riot I 'Ele/Sta'", x => SA(8));
            weaponModifiers.Add("SA Demon Riot II 'Ele/Sta'", x => SA(9));
            weaponModifiers.Add("SA Demon Riot III 'Ele/Sta'", x => SA(10));
            weaponModifiers.Add("CB Red Shield (Other Styles)", x => CB(1));
            weaponModifiers.Add("CB Red Shield (Striker)", x => CB(2));
            weaponModifiers.Add("IG Red Extract (Balanced)", x => IG(1));
            weaponModifiers.Add("IG White Extract (Balanced)", x => IG(2));
            weaponModifiers.Add("IG Red/White Extract", x => IG(3));
            weaponModifiers.Add("IG Triple Up", x => IG(4));
            weaponModifiers.Add("IG White (Speed)", x => IG(5));
            weaponModifiers.Add("Gunner Normal Distance", x => Gunner(1));
            weaponModifiers.Add("Gunner Critical Distance", x => Gunner(2));
            weaponModifiers.Add("Gunner Long Range", x => Gunner(3));
            weaponModifiers.Add("Gunner Ex. Long Range", x => Gunner(4));
            weaponModifiers.Add("Gun. C. D. + H. Polish/Demon S", x => Gunner(5));
            weaponModifiers.Add("LBG Raw Multiplier", x => LBG(1));
            weaponModifiers.Add("LBG Long Barrel Attach.", x => LBG(2));
            weaponModifiers.Add("LBG Power Reload", x => LBG(3));
            weaponModifiers.Add("HBG Raw Multiplier", x => HBG(1));
            weaponModifiers.Add("HBG Power Barrel Attach.", x => HBG(2));
            weaponModifiers.Add("HBG Power Reload", x => HBG(3));
            weaponModifiers.Add("HBG Valor Reload", x => HBG(4));
            weaponModifiers.Add("HBG Gunpowder Infusion", x => HBG(5));
            weaponModifiers.Add("Bow Charge Lv1", x => Bow(1));
            weaponModifiers.Add("Bow Charge Lv2", x => Bow(2));
            weaponModifiers.Add("Bow Charge Lv3 + Pois.", x => Bow(3));
            weaponModifiers.Add("Bow Charge Lv3 + Other", x => Bow(4));
            weaponModifiers.Add("Bow Charge Lv4 + Pois.", x => Bow(5));
            weaponModifiers.Add("Bow Charge Lv4 + Other", x => Bow(6));
            weaponModifiers.Add("Bow Valor Power Shot", x => Bow(7));
            weaponModifiers.Add("Bow Power C. Lv1", x => Bow(8));
            weaponModifiers.Add("Bow Power C. Lv2", x => Bow(9));
            weaponModifiers.Add("Bow Elem. C. Lv1", x => Bow(10));
            weaponModifiers.Add("Bow Elem. C. Lv2", x => Bow(11));
            weaponModifiers.Add("Bow Exhaust C.", x => Bow(16));
            weaponModifiers.Add("Bow Coating Boost 'Pwr'", x => Bow(12));
            weaponModifiers.Add("Bow Coating Boost 'Ele'", x => Bow(13));
            weaponModifiers.Add("Bow Coating Boost 'C.Range'", x => Bow(14));
            weaponModifiers.Add("Bow Coating Boost 'Sta'", x => Bow(15));

            foreach (KeyValuePair<string, Func<int, bool>> pair in weaponModifiers)
            {
                modWeapon.Items.Add(pair.Key);
            }

            miscModifiers.Add("Frenzy Affinity Boost", x => Frenzy(1));
            miscModifiers.Add("Frenzy (+Antivirus)", x => Frenzy(2));
            miscModifiers.Add("Red Bubble Boost", x => Mizu(1));

            foreach (KeyValuePair<string, Func<int, bool>> pair in miscModifiers)
            {
                modMisc.Items.Add(pair.Key);
            }

            using (StreamReader sr = new StreamReader("./Monsters/MonID.csv"))
            {
                if (sr.ReadLine() != "ID,name")
                {
                    return;
                }

                while (sr.Peek() >= 0)
                {
                    string[] inputs = sr.ReadLine().Split(new char[] { ',' });
                    ListViewItem newItem = new ListViewItem(inputs[0]);
                    newItem.SubItems.Add(inputs[1]);
                    ListViewItem otherItem = new ListViewItem(inputs[0]);
                    otherItem.SubItems.Add(inputs[1]);
                    allMonsters.Add(newItem);
                    staMonsters.Add(otherItem);
                    monsterList.Items.Add(newItem);
                    staMonsterList.Items.Add(otherItem);

                }
            }
        }

        public void setSelected()
        {
            moveType.SelectedIndex = 0;
            moveInherit.SelectedIndex = 0;

            monStatus.SelectedIndex = 0;

            weapEle.SelectedIndex = 0;
            weapSharpness.SelectedIndex = 0;
            weapSec.SelectedIndex = 0;
            weapFire.Enabled = false;
            weapWater.Enabled = false;
            weapThunder.Enabled = false;
            weapIce.Enabled = false;
            weapDra.Enabled = false;
            weapPoi.Enabled = false;
            weapPara.Enabled = false;
            weapSleep.Enabled = false;
            weapBlast.Enabled = false;
            weapNoEle.Enabled = false;

            paraAltType.SelectedIndex = 0;
            paraSharp.SelectedIndex = 0;
            paraEleCrit.SelectedIndex = 0;
            paraSecEle.SelectedIndex = 0;
            paraMonStatus.SelectedIndex = 0;

            calcAverage.Select();

            staType.SelectedIndex = 0;
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
                paraAffinity.Text = "0";
                paraAffinity.Visible = false;

                paraPositive.Visible = true;
                paraPosAff.Visible = true;
                paraNega.Visible = true;
                paraNegAff.Visible = true;
            }
            else
            {
                paraAffinity.Visible = true;

                paraPosAff.Text = "0";
                paraNegAff.Text = "0";
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
            int hitCount;
            if ((hitCount = int.Parse(paraHitCount.Text)) == 0)
            {
                hitCount = 1;
            }
            calcRawOut.Text = (calcOutput.Item2 * hitCount).ToString("N2");
            calcEleOut.Text = (calcOutput.Item3 * hitCount).ToString("N2");
            calcSecOut.Text = (calcOutput.Item4 * hitCount).ToString("N2");

            EffectiveRawCalc(calcOutput);
            addDamageHistory(1);
        }

        private void calcAll_Click(object sender, EventArgs e)
        {
            calcDetails.ResetText();
            Tuple<double, double, double, double> calcOutput = CalculateDamage();

            int hitCount = int.Parse(paraHitCount.Text);

            calcRawWeap.Text = calcOutput.Item1.ToString("N2");
            calcRawOut.Text = (calcOutput.Item2 * hitCount).ToString("N2");
            calcEleOut.Text = (calcOutput.Item3).ToString("N2");
            calcSecOut.Text = (calcOutput.Item4).ToString("N2");

            EffectiveRawCalc(calcOutput);

            Tuple<double, bool, double, double, double, double, double> allTuple = CalculateAllDamage(calcOutput);
            calcFinal.Text = allTuple.Item1.ToString();
            if(hitCount == 0)
            {
                calcPerHit.Text = "0";
            }
            else
            {
                calcPerHit.Text = (allTuple.Item1 / hitCount).ToString();
            }

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

            calcKOAll.Text = allTuple.Item6.ToString();
            calcExhAll.Text = allTuple.Item7.ToString();
            HealthCalc(allTuple); //Estimate how many hits it would take to kill the monster.
            addDamageHistory(2);
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
                weapSharpTwo.Items.Remove("Purple");
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
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
                return;
            }
            else if (weapSec.SelectedIndex == 1)
            {
                weapSecPict.Load(str2Pict["Fire"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 2)
            {
                weapSecPict.Load(str2Pict["Water"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 3)
            {
                weapSecPict.Load(str2Pict["Thunder"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 4)
            {
                weapSecPict.Load(str2Pict["Ice"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 5)
            {
                weapSecPict.Load(str2Pict["Para"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 6)
            {
                weapSecPict.Load(str2Pict["Poison"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 7)
            {
                weapSecPict.Load(str2Pict["Blast"]);
                weapOverride.Checked = false;
                weapOverride.Enabled = false;
            }

            else if (weapSec.SelectedIndex == 8)
            {
                weapSecPict.Load(str2Pict["Dragon"]);
                weapOverride.Enabled = true;
            }

            else if (weapSec.SelectedIndex == 9)
            {
                weapSecPict.Load(str2Pict["Poison"]);
                weapOverride.Enabled = true;
            }

            else if (weapSec.SelectedIndex == 10)
            {
                weapSecPict.Load(str2Pict["Para"]);
                weapOverride.Enabled = true;
            }

            else if (weapSec.SelectedIndex == 11)
            {
                weapSecPict.Load(str2Pict["Exhaust"]);
                weapOverride.Enabled = true;
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

            monStatus.SelectedIndex = 0;
        }

        private void moveReset_Click(object sender, EventArgs e)
        {
            moveMV.Text = "0";
            moveHitCount.Text = "0";
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

        private void modArmorButton_Click(object sender, EventArgs e)
        {
            if(modArmor.Text == "")
            {
                return;
            }
            foreach (ListViewItem listItem in modList.Items)
            {
                if (modArmor.Text == listItem.Text)
                {
                    return;
                }
            }
            ListViewItem item = new ListViewItem(modArmor.Text)
            {
                Group = modList.Groups[0]
            };
            modList.Items.Add(item);
        }

        private void modFoodButton_Click(object sender, EventArgs e)
        {
            if (modFood.Text == "")
            {
                return;
            }
            foreach (ListViewItem listItem in modList.Items)
            {
                if (modFood.Text == listItem.Text)
                {
                    return;
                }
            }

            ListViewItem item = new ListViewItem(modFood.Text)
            {
                Group = modList.Groups[1]
            };
            modList.Items.Add(item);
        }

        private void modWeaponButton_Click(object sender, EventArgs e)
        {
            if (modWeapon.Text == "")
            {
                return;
            }
            foreach (ListViewItem listItem in modList.Items)
            {
                if (modWeapon.Text == listItem.Text)
                {
                    return;
                }
            }
            ListViewItem item = new ListViewItem(modWeapon.Text)
            {
                Group = modList.Groups[2]
            };
            modList.Items.Add(item);
        }

        private void modMiscButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listItem in modList.Items)
            {
                if (modMisc.Text == listItem.Text)
                {
                    return;
                }
            }
            ListViewItem item = new ListViewItem(modMisc.Text)
            {
                Group = modList.Groups[3]
            };
            modList.Items.Add(item);
        }

        private void modAllButton_Click(object sender, EventArgs e)
        {
            modList.Items.Clear();
            foreach (ListViewGroup group in modList.Groups)
            {
                group.Items.Clear();
            }
        }

        private void modSelectedButton_Click(object sender, EventArgs e)
        {
            List<ListViewItem> selectedList = new List<ListViewItem>();
            foreach (ListViewItem item in modList.SelectedItems)
            {
                selectedList.Add(item);
            }

            foreach (ListViewItem item in selectedList)
            {
                item.Group.Items.Remove(item);
                modList.Items.Remove(item);
            }
        }

        private void paraUpdate_Click(object sender, EventArgs e)
        {
            ImportSetUp();
            ImportModifiers();
            Export();
        }

        private void moveInherit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (moveInherit.SelectedIndex == 0)
            {
                moveInheritValue.Text = "0";
                moveInheritValue.Enabled = false;

                moveInheritPict.Image = null;
                return;
            }

            moveInheritValue.Enabled = true;
            moveInheritPict.Load(str2Pict[moveInherit.Text]);
        }

        private void weapGS_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveGS.Checked = true;
        }

        private void weapLS_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveLS.Checked = true;
        }

        private void weapSnS_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveSnS.Checked = true;
        }

        private void waepDB_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveDB.Checked = true;
        }

        private void weapHam_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveHammer.Checked = true;
        }

        private void weapHH_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveHH.Checked = true;
        }

        private void weapLan_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveLance.Checked = true;
        }

        private void weapGL_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveGL.Checked = true;
        }

        private void weapSA_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveSA.Checked = true;
        }

        private void weapCB_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveCB.Checked = true;
        }

        private void weapIG_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveIG.Checked = true;
        }

        private void weapLBG_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveShot.Checked = true;
        }

        private void weapHBG_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveShot.Checked = true;
        }

        private void weapBow_CheckedChanged(object sender, EventArgs e)
        {
            ResetWeaponDetails();
            moveBow.Checked = true;
        }

        private void weaponDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (weaponDetails.SelectedItems.Count == 1)
            {
                fillWeapon(weaponDetails.SelectedItems[0].SubItems[0].Name);

                TreeNode[] nodes = weaponTree.Nodes.Find(weaponDetails.SelectedItems[0].SubItems[0].Name, true);
                if (weaponTree.SelectedNode != null)
                {
                    if (weaponTree.SelectedNode.Name == nodes[0].Name)
                    {
                        return;
                    }
                }

                weaponTree.CollapseAll();
                nodes[0].Parent.Expand();
            }

        }

        private void weaponTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

            if (weaponTree.SelectedNode.Nodes.Count == 0)
            {
                if (weaponDetails.SelectedItems.Count == 1)
                {
                    if (weaponDetails.SelectedItems[0].Name != weaponTree.SelectedNode.Name && weaponDetails.Items.ContainsKey(weaponTree.SelectedNode.Name))
                    {
                        weaponDetails.Items[weaponTree.SelectedNode.Name].Selected = true;
                        weaponDetails.TopItem = weaponDetails.Items[weaponTree.SelectedNode.Text];
                    }
                }
                else
                {
                    if (weaponDetails.Items.ContainsKey(weaponTree.SelectedNode.Name))
                    {
                        weaponDetails.Items[weaponTree.SelectedNode.Name].Selected = true;
                        weaponDetails.TopItem = weaponDetails.Items[weaponTree.SelectedNode.Text];
                    }
                }

                fillWeapon(weaponTree.SelectedNode.Name);
            }
        }

        private void weapFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (weapFilter.Checked)
            {
                weapFire.Enabled = true;
                weapWater.Enabled = true;
                weapThunder.Enabled = true;
                weapIce.Enabled = true;
                weapDra.Enabled = true;
                weapPoi.Enabled = true;
                weapPara.Enabled = true;
                weapSleep.Enabled = true;
                weapBlast.Enabled = true;
                weapNoEle.Enabled = true;
            }

            else
            {
                weapFire.Checked = false;
                weapFire.Enabled = false;
                weapWater.Checked = false;
                weapWater.Enabled = false;
                weapThunder.Checked = false;
                weapThunder.Enabled = false;
                weapIce.Checked = false;
                weapIce.Enabled = false;
                weapDra.Checked = false;
                weapDra.Enabled = false;
                weapPoi.Checked = false;
                weapPoi.Enabled = false;
                weapPara.Checked = false;
                weapPara.Enabled = false;
                weapSleep.Checked = false;
                weapSleep.Enabled = false;
                weapBlast.Checked = false;
                weapBlast.Enabled = false;
                weapNoEle.Checked = false;
                weapNoEle.Enabled = false;
            }
        }

        private void weapSearch_TextChanged(object sender, EventArgs e)
        {
            searchWeapons();
            if(weaponDetails.Items.Count > 0)
            {
                weaponDetails.Items[0].Selected = true;
            }
        }

        private void weapFinalUpgrade_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapNoEle_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapSleep_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapPara_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapPoi_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapDra_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapIce_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapThunder_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapWater_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapFire_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weapBlast_CheckedChanged(object sender, EventArgs e)
        {
            filterWeapons();
        }

        private void weaponDetails_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == weaponColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (weaponColumnSorter.Order == SortOrder.Ascending)
                {
                    weaponColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    weaponColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                weaponColumnSorter.SortColumn = e.Column;
                weaponColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.weaponDetails.Sort();
        }

        private void moveGS_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveLS_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveSnS_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveDB_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveHammer_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveHH_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveLance_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveGL_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveSA_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveCB_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveIG_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveShot_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveLBG_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveHBG_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveBow_CheckedChanged(object sender, EventArgs e)
        {
            changeMoveList();
        }

        private void moveDetails_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 2)
            {
                return;
            }
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == moveColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (moveColumnSorter.Order == SortOrder.Ascending)
                {
                    moveColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    moveColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                moveColumnSorter.SortColumn = e.Column;
                moveColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.moveDetails.Sort();
        }

        private void monsterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            questColumnSorter.SortColumn = 0;
            hitzoneColumnSorter.SortColumn = 0;

            if (monsterList.SelectedItems.Count == 0)
            {
                return;
            }

            if(staMonsterList.SelectedIndices.Count == 0)
            {
                staMonsterList.Items[monsterList.SelectedIndices[0]].Selected = true;
            }
            else if (monsterList.SelectedIndices[0] != staMonsterList.SelectedIndices[0])
            {
                staMonsterList.Items[monsterList.SelectedIndices[0]].Selected = true;
            }

            string monster = monsterList.SelectedItems[0].SubItems[1].Text;
            hitzoneDetails.Items.Clear();
            //Access the respective file for monster hitzones and quests, add their entries to the list views.
            using (StreamReader sr = new StreamReader("./Monsters/" + monster + ".csv"))
            {
                if (sr.ReadLine() != "ID,part,cut,impact,shot,fire,water,thunder,ice,dragon,KO,exhaust")
                {
                    return;
                }
                while (sr.Peek() >= 0)
                {
                    string[] inputs = sr.ReadLine().Split(new char[] { ',' });
                    ListViewItem newItem = new ListViewItem(inputs[0]);
                    newItem.SubItems.Add(inputs[1]);
                    newItem.SubItems.Add(inputs[2]);
                    newItem.SubItems.Add(inputs[3]);
                    newItem.SubItems.Add(inputs[4]);
                    newItem.SubItems.Add(inputs[5]);
                    newItem.SubItems.Add(inputs[6]);
                    newItem.SubItems.Add(inputs[7]);
                    newItem.SubItems.Add(inputs[8]);
                    newItem.SubItems.Add(inputs[9]);
                    newItem.SubItems.Add(inputs[10]);
                    newItem.SubItems.Add(inputs[11]);

                    hitzoneDetails.Items.Add(newItem);
                }
            }

            questDetails.Items.Clear();
            using (StreamReader sr = new StreamReader("./Quests/" + monster + ".csv"))
            {
                if (sr.ReadLine() != "ID,name,health,defense,KO,exhaust,GRank")
                {
                    return;
                }
                while (sr.Peek() >= 0)
                {
                    string[] inputs = sr.ReadLine().Split(new char[] { ',' });
                    ListViewItem newItem = new ListViewItem(inputs[0]);
                    newItem.SubItems.Add(inputs[1]);
                    newItem.SubItems.Add(inputs[2]);
                    newItem.SubItems.Add(inputs[3]);
                    newItem.SubItems.Add(inputs[4]);
                    newItem.SubItems.Add(inputs[5]);
                    newItem.SubItems.Add(inputs[6]);

                    questDetails.Items.Add(newItem);
                }
            }
        }

        private void hitzoneDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hitzoneDetails.SelectedItems.Count == 0)
            {
                return;
            }
            ListViewItem hitzone = hitzoneDetails.SelectedItems[0];
            monCut.Text = hitzone.SubItems[2].Text;
            monImpact.Text = hitzone.SubItems[3].Text;
            monShot.Text = hitzone.SubItems[4].Text;
            monFire.Text = hitzone.SubItems[5].Text;
            monWater.Text = hitzone.SubItems[6].Text;
            monThunder.Text = hitzone.SubItems[7].Text;
            monIce.Text = hitzone.SubItems[8].Text;
            monDragon.Text = hitzone.SubItems[9].Text;
            monKO.Text = hitzone.SubItems[10].Text;
            monExhaust.Text = hitzone.SubItems[11].Text;

        }

        private void questDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (questDetails.SelectedItems.Count == 0)
            {
                return;
            }

            ListViewItem quest = questDetails.SelectedItems[0];
            monHealth.Text = quest.SubItems[2].Text;
            monQuest.Text = quest.SubItems[3].Text;
            monKOMod.Text = quest.SubItems[4].Text;
            monExhaustMod.Text = quest.SubItems[5].Text;
            monGRank.Checked = (quest.SubItems[6].Text != "");
        }

        private void monsterList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == monsterColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (monsterColumnSorter.Order == SortOrder.Ascending)
                {
                    monsterColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    monsterColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                monsterColumnSorter.SortColumn = e.Column;
                monsterColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.monsterList.Sort();
        }

        private void hitzoneDetails_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == hitzoneColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (hitzoneColumnSorter.Order == SortOrder.Ascending)
                {
                    hitzoneColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    hitzoneColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                hitzoneColumnSorter.SortColumn = e.Column;
                hitzoneColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.hitzoneDetails.Sort();
        }

        private void questDetails_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == questColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (questColumnSorter.Order == SortOrder.Ascending)
                {
                    questColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    questColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                questColumnSorter.SortColumn = e.Column;
                questColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.questDetails.Sort();
        }

        private void moveDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (moveDetails.SelectedItems.Count == 1)
            {
                fillMove(moveDetails.SelectedItems[0]);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (modGlossary.SelectedNode.Tag != null)
            {
                modDetails.Text = ((string)modGlossary.SelectedNode.Tag).Replace("\\n", Environment.NewLine);
            }
        }

        private void staCritCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (staCritCheck.Checked)
            {
                staAffinity.Enabled = true;
            }
            else
            {
                staAffinity.Text = "0";
                staAffinity.Enabled = false;
            }
        }

        private void staType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (staType.SelectedIndex == 0)
            {
                staPictOne.Image = null;
            }
            if (staType.SelectedIndex == 1)
            {
                staPictOne.Load(str2Pict["Poison"]);

                if (staMonsterList.SelectedItems.Count == 1)
                {
                    staStatusTable.Items[0].Selected = true;
                }
            }
            if (staType.SelectedIndex == 2)
            {
                staPictOne.Load(str2Pict["Para"]);

                if (staMonsterList.SelectedItems.Count == 1)
                {
                    staStatusTable.Items[1].Selected = true;
                }
            }
            if (staType.SelectedIndex == 3)
            {
                staPictOne.Load(str2Pict["Sleep"]);

                if (staMonsterList.SelectedItems.Count == 1)
                {
                    staStatusTable.Items[2].Selected = true;
                }
            }
            if (staType.SelectedIndex == 4)
            {
                staPictOne.Load(str2Pict["Blast"]);

                if (staMonsterList.SelectedItems.Count == 1)
                {
                    staStatusTable.Items[3].Selected = true;
                }
            }
            if (staType.SelectedIndex == 5)
            {
                staKOZone.Enabled = true;
                staKOMod.Enabled = true;
                staExhZone.Text = "0";
                staExhZone.Enabled = false;
                staExhMod.Text = "1.0";
                staExhMod.Enabled = false;


                staPictOne.Load(str2Pict["KO"]);

                if (staMonsterList.SelectedItems.Count == 1)
                {
                    staStatusTable.Items[4].Selected = true;
                }
            }
            if (staType.SelectedIndex == 6)
            {
                staExhZone.Enabled = true;
                staExhMod.Enabled = true;
                staKOZone.Text = "0";
                staKOZone.Enabled = false;
                staKOMod.Text = "1.0";
                staKOMod.Enabled = false;

                staPictOne.Load(str2Pict["Exhaust"]);

                if (staMonsterList.SelectedItems.Count == 1)
                {
                    staStatusTable.Items[5].Selected = true;
                }
            }


            if (staType.SelectedIndex < 5)
            {
                staKOZone.Text = "0";
                staKOZone.Enabled = false;

                staExhZone.Text = "0";
                staExhZone.Enabled = false;

                staKOMod.Text = "1.0";
                staKOMod.Enabled = false;

                staExhMod.Text = "1.0";
                staExhMod.Enabled = false;
            }
        }

        private void staMonsterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (staMonsterList.SelectedItems.Count == 0)
            {
                return;
            }

            if (monsterList.SelectedIndices.Count == 0)
            {
                monsterList.Items[staMonsterList.SelectedIndices[0]].Selected = true;
            }
            else if (monsterList.SelectedIndices[0] != staMonsterList.SelectedIndices[0])
            {
                monsterList.Items[staMonsterList.SelectedIndices[0]].Selected = true;
            }

            string monster = staMonsterList.SelectedItems[0].SubItems[1].Text;
            staStatusTable.Items.Clear();
            //Access the respective file for monster hitzones and quests, add their entries to the list views.
            using (StreamReader sr = new StreamReader("./Statuses/" + monster + ".csv"))
            {
                if (sr.ReadLine() != "status,init,inc,max")
                {
                    return;
                }
                while (sr.Peek() >= 0)
                {
                    string[] inputs = sr.ReadLine().Split(new char[] { ',' });
                    ListViewItem newItem = new ListViewItem(inputs[0]);
                    newItem.SubItems.Add(inputs[1]);
                    newItem.SubItems.Add(inputs[2]);
                    newItem.SubItems.Add(inputs[3]);

                    staStatusTable.Items.Add(newItem);
                }
            }
        }

        private void staStatusTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (staStatusTable.SelectedItems.Count == 1)
            {
                staInit.Text = staStatusTable.SelectedItems[0].SubItems[1].Text;
                staInc.Text = staStatusTable.SelectedItems[0].SubItems[2].Text;
                staMax.Text = staStatusTable.SelectedItems[0].SubItems[3].Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CalculateStatus();
            addStatusHistory();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            staCritCheck.Checked = false;
            staHitCount.Text = "0";
            staType.SelectedIndex = 0;
            staInit.Text = "0";
            staInc.Text = "0";
            staMax.Text = "0";
        }

        private void staUpdate_Click(object sender, EventArgs e)
        {
            ImportSetUp();
            ImportModifiers();
            ExportStatus();
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

            if (stats.sharpness == "(No Sharpness)" && stats.hitCount > 1 && stats.damageType == "Shot")
            {
                stats.hitCount++;
            }

            return true;
        }

        public bool TrueBoltreaver(int skillVal)
        {
            stats.criticalBoost = true; //Crit Boost portion

            stats.UpdateSharpness((string)weapSharpOne.SelectedItem);

            if (stats.sharpness == "(No Sharpness)" && stats.hitCount > 1 && stats.damageType == "Shot")
            {
                stats.hitCount++;
            }

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

            else if (skillVal == 2)
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
                if (stats.damageType == "Fixed")
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
                stats.addRaw += 15;
            }

            else if (skillVal == 2) //Cool Drinks
            {
                stats.addRaw += 5;
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
                stats.eleCrit = 1;
            }

            else if (skillVal == 2)
            {
                stats.eleCrit = 2;
            }

            else if (skillVal == 3)
            {
                stats.eleCrit = 3;
            }

            else if (skillVal == 4)
            {
                stats.eleCrit = 4;
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
                stats.rawMod *= 1.3;
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

            else if (stats.sharpness == "(No Sharpness)" && stats.damageType == "Shot")
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

            else if (stats.sharpness == "(No Sharpness)" && stats.damageType == "Shot")
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
            if (stats.sharpness == "(No Sharpness)" && stats.damageType == "Shot")
            {
                stats.rawMod *= 1.1;
            }
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
            if (stats.sharpness == "(No Sharpness)" && stats.damageType == "Shot")
            {
                stats.rawMod *= 1.1;
            }

            return true;
        }

        public bool PelletUp(int skillVal)
        {
            if (stats.sharpness != "(No Sharpness)" || stats.damageType != "Shot")
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
            if (stats.sharpness == "(No Sharpness)" && stats.damageType == "Shot")
            {
                stats.rawMod *= 1.1;
            }
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
            if (stats.sharpness == "(No Sharpness)" && stats.hitCount > 1 && stats.damageType == "Shot")
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
            if (stats.sharpness == "(No Sharpness)" && stats.damageType == "Shot")
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
            if (moveAerial.Checked)
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

            else if (skillVal == 8) //Chaos Oil III
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

        public bool Hammer(int skillVal)
        {
            stats.addRaw += 15;
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
                stats.rawMod *= 1.3;
            }
            else if (skillVal == 2) //Enraged Guard (Orange)
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 3) //Enraged Guard (Red)
            {
                stats.rawMod *= 1.1;
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
                if (stats.damageType == "Fixed" && stats.altDamageType == "Fire")
                {
                    stats.avgMV += 10;
                    stats.addElement += 10;
                }
            }
            else if (skillVal == 2) //Orange Heat
            {
                stats.rawMod *= 1.15;
            }
            else if (skillVal == 3) //Red Heat
            {
                stats.rawMod *= 1.2;
            }
            else if (skillVal == 4) //Valor Rapid Shell #2
            {
                stats.expMod *= 1.2;
            }
            else if (skillVal == 5) //Valor Rapid Shell #3
            {
                stats.expMod *= 1.5;
            }
            else if (skillVal == 6) //Valor Rapid Shell #4
            {
                stats.expMod *= 2;
            }
            else if (skillVal == 7) //Valor Rapid Shell #5
            {
                stats.expMod *= 2.6;
            }
            else if (skillVal == 8) //Valor Full Burst (1 loaded)
            {
                stats.expMod *= 0.8;
            }
            else if (skillVal == 9) //Valor Full Burst (2 loaded)
            {
                stats.expMod *= 0.9;
            }
            else if (skillVal == 10) //Valor Full Burst (3 loaded)
            {
                stats.expMod *= 1;
            }
            else if (skillVal == 11) //Valor Full Burst (4 loaded)
            {
                stats.expMod *= 1.4;
            }
            else if (skillVal == 12) //Valor Full Burst (5 loaded)
            {
                stats.expMod *= 1.5;
            }
            else if (skillVal == 13) //Valor Full Burst (6 loaded)
            {
                stats.expMod *= 1.6;
            }
            else if (skillVal == 14) //Dragon Breath
            {

            }
            else if (skillVal == 15) //Anti-Air Flares I
            {
                stats.expMod *= 1;
            }
            else if (skillVal == 16) //Anti-Air Flares II
            {
                stats.expMod *= 1.05;
            }
            else if (skillVal == 17) //Anti-Air Flares III
            {
                stats.expMod *= 1.1;
            }
            else if (skillVal == 18) //Normal/Long Charged Shot
            {
                stats.expMod *= 1.2;
            }
            else if (skillVal == 19) //Wide Charged Shot
            {
                stats.expMod *= 1.45;
            }
            else if (skillVal == 20) //Long Wyvernsfire
            {
                stats.expMod *= 1.2;
            }
            else if (skillVal == 21) //Normal Full Burst
            {
                stats.expMod *= 1.1;
            }
            else if (skillVal == 22) //Wide Full Burst
            {
                stats.expMod *= 0.85;
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
            else if (skillVal == 5) //Critical Distance + Heavy Grinder (1.75x) OR Demon S
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
            else if (skillVal == 4) //Valor Reload
            {
                stats.rawMod *= 1.05;
            }
            else if (skillVal == 5) //Gunpowder Infusion
            {
                stats.rawMod *= 1.15;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool Bow(int skillVal)
        {
            if (skillVal == 1) //Charge 1
            {
                stats.rawMod *= 0.4;
                stats.eleMod *= 0.7;
                stats.staMod *= 0.5;
            }

            else if (skillVal == 2) //Charge 2
            {
                stats.rawMod *= 1.0;
                stats.eleMod *= 0.85;
                stats.staMod *= 1.0;
            }

            else if (skillVal == 3) //Charge 3 + Poison
            {
                stats.rawMod *= 1.5;
                stats.eleMod *= 1.0;
                stats.staMod *= 1.5;
            }

            else if (skillVal == 4) //Charge 3 + Para
            {
                stats.rawMod *= 1.5;
                stats.eleMod *= 1.0;
                stats.staMod *= 1.3;
            }

            else if (skillVal == 5) //Charge 4 + Poison
            {
                stats.rawMod *= 1.7;
                stats.eleMod *= 1.125;
                stats.staMod *= 1.5;
            }

            else if (skillVal == 6) //Charge 4 + Para
            {
                stats.rawMod *= 1.7;
                stats.eleMod *= 1.125;
                stats.staMod *= 1.3;
            }

            else if (skillVal == 7) //Valor Power Shot
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
            else if (skillVal == 16)
            {
                stats.KOPower += 4;
                stats.exhaustPower += 8;
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
            if (stats.secElement == "Blast")
            {
                stats.staSecMod *= 1.1;
            }

            return true;
        }

        public bool FSharpshooter()
        {
            if (stats.sharpness == "(No Sharpness)")
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
            if (isStatus(stats.secElement))
            {
                stats.staSecMod *= 1.125;
            }
            return true;
        }

        public bool FTemper()
        {
            if (stats.sharpness == "(No Sharpness)")
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
            if (skillVal == 1) //No Antivirus
            {
                if (stats.chaotic)
                {
                    stats.chaotic = false;
                    stats.positiveAffinity += double.Parse(weapNegAff.Text) + 15;
                    stats.negativeAffinity = stats.negativeAffinity - double.Parse(weapNegAff.Text);
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
                    stats.positiveAffinity += double.Parse(weapNegAff.Text) + 30;
                    stats.negativeAffinity = stats.negativeAffinity - double.Parse(weapNegAff.Text);
                }
                else
                {
                    stats.positiveAffinity += 30;
                }
            }

            return true;
        }

        public bool Mizu(int skillVal)
        {
            stats.addRaw += 10;
            return true;
        }

#endif

        private void EffectiveRawCalc(Tuple<double, double, double, double> calcOutput)
        {
            int hitCount = int.Parse(paraHitCount.Text);
            if (hitCount == 0)
            {
                hitCount = 1;
            }
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

            string[] formatArray = new string[] { paraHitCount.Text, finalDamage.ToString(), avgHits, avgDamage, health.ToString(), minHits, minDamage, maxHits, maxDamage };
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

            Tuple<double, double, double> affinity;
            if (!paraChaotic.Checked)
            {
                double incAffinity = double.Parse(paraAffinity.Text);
                if (incAffinity >= 0)
                {
                    affinity = new Tuple<double, double, double>(incAffinity * 0.01, 0, 0);
                }
                else
                {
                    affinity = new Tuple<double, double, double>(0, incAffinity * -0.01, 0);
                }
            }
            else
            {
                affinity = new Tuple<double, double, double>(double.Parse(paraPosAff.Text) * 0.01, double.Parse(paraNegAff.Text) * 0.01, 0);
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

            Tuple<double, double, double> subAffinity = affinity;

            if (fixedType)
            {
                return new Tuple<double, double, double, double>(motionValue * raw, motionValue * raw, element, secElement);
            }

            if (calcNeutral.Checked)
            {
                subAffinity = new Tuple<double, double, double>(0, 0, 0);
            }

            if (calcPositive.Checked)
            {
                subAffinity = new Tuple<double, double, double>(1, 0, 0);
            }

            if (calcNegative.Checked)
            {
                subAffinity = new Tuple<double, double, double>(0, 1, 0);
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
                subAffinity = new Tuple<double, double, double>(subAffinity.Item1, subAffinity.Item2 - subAffinity.Item2 * 0.25, subAffinity.Item2 * 0.25);
            }

            rawWeap = raw * (1 + subAffinity.Item1 * critBoost) * (1 - subAffinity.Item2 * 0.25) * (1 + subAffinity.Item3 * 1) * rawSharp;
            rawTotal = rawWeap * motionValue;

            if (isElement(altType))
            {
                eleTotal = element * elementSharp * (1 + subAffinity.Item1 * eleCrit);
            }

            else if (isStatus(altType))
            {
                eleTotal = element * (1 + subAffinity.Item1 * statusCrit);
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
                secTotal = secElement * (1 + subAffinity.Item1 * statusCrit);
            }

            else
            {
                secTotal = secElement * elementSharp;
            }

            if(hitCount == 0)
            {
                rawTotal = 0;
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

            double eleDamage = 0;
            double secDamage = 0;

            double bounceTolerance = 0.25;

            if (paraGRank.Checked)
            {
                bounceTolerance = 0.27;
            }

            rawDamage *= monsterStatus[paraMonStatus.SelectedIndex];

            //Quest modifier applies before hitzone consideration. Rounded down.
            rawDamage *= questMod;
            rawDamage = Math.Floor(rawDamage);
            string element = paraAltType.Text;
            string second = paraSecEle.Text;

            if (isElement(element))
            {
                eleDamage = Math.Floor(calcOutput.Item3 * questMod);
            }
            else
            {
                eleDamage = Math.Floor(calcOutput.Item3);
            }

            if (isElement(second))
            {
                secDamage = Math.Floor(calcOutput.Item4 * questMod);
            }
            else
            {
                secDamage = Math.Floor(calcOutput.Item4);
            }

            if (!paraFixed.Checked)
            {
                rawDamage *= rawZone;

                if ((rawZone * double.Parse(paraRawSharp.Text)) > bounceTolerance || paraMinds.Checked)
                {
                    BounceBool = true;
                }
                else
                {
                    BounceBool = false;
                }
            }

            totaldamage += rawDamage;

            if (isElement(element))
            {
                eleDamage *= eleZone;
                totaldamage += eleDamage;
            }

            if (isElement(second))
            {
                secDamage *= secZone;
                totaldamage += secDamage;
            }

            totaldamage = Math.Floor(totaldamage) * hitCount;
            
            if(hitCount == 0)
            {
                eleDamage = 0;
                secDamage = 0;
            }

            KODamage = Math.Floor(KODamage);
            KODamage *= hitCount;
            KODamage = Math.Floor(KODamage);


            ExhDamage = Math.Floor(ExhDamage);
            ExhDamage *= hitCount;
            ExhDamage = Math.Floor(ExhDamage);

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

        private void ImportSetUp()
        {
            stats = new ImportedStats
            {
                sharpness = (string)weapSharpness.SelectedItem,
                totalAttackPower = double.Parse(weapRaw.Text),
            };

            if (weapChaotic.Checked)
            {
                stats.positiveAffinity = double.Parse(weapPosAff.Text);
                stats.negativeAffinity = double.Parse(weapNegAff.Text);
                stats.chaotic = true;
            }
            else
            {
                stats.chaotic = false;
                double affinity = double.Parse(weapAffinity.Text);
                if (affinity > 0)
                {
                    stats.positiveAffinity = affinity;
                    stats.negativeAffinity = 0;
                }
                else
                {
                    stats.positiveAffinity = 0;
                    stats.negativeAffinity = -1 * affinity;
                }
            }

            stats.rawSharpMod = sharpnessMods[stats.sharpness].Item1;
            stats.eleSharpMod = sharpnessMods[stats.sharpness].Item2;

            if (weapOverride.Checked) //Overwrites the default element if checked.
            {
                stats.altDamageType = secondElements[weapSec.Text];
                stats.eleAttackPower = double.Parse(weapSecDamage.Text);
                stats.secElement = "(No Element)";
                stats.secPower = 0;
            }
            else if (moveInherit.SelectedIndex != 0) //If the move has an inherant elemental value:
            {
                stats.altDamageType = moveInherit.Text;
                stats.eleAttackPower = double.Parse(moveInheritValue.Text);
                stats.secElement = "(No Element)";
                stats.secPower = 0;
            }
            else
            {
                stats.altDamageType = weapEle.Text;
                stats.eleAttackPower = double.Parse(weapEleDamage.Text);
                if(stats.altDamageType != "(No Element)")
                {
                    stats.secElement = secondElements[weapSec.Text];
                    stats.secPower = double.Parse(weapSecDamage.Text);
                }
                else
                {
                    stats.secElement = "(No Element)";
                    stats.secPower = 0;
                }
            }

            stats.hitCount = int.Parse(moveHitCount.Text);
            if (stats.hitCount == 0)
            {
                stats.avgMV = 0;
                stats.KOPower = 0;
                stats.exhaustPower = 0;
            }
            else
            {
                stats.avgMV = double.Parse(moveMV.Text) / stats.hitCount;
                stats.KOPower = double.Parse(moveKO.Text) / stats.hitCount;
                stats.exhaustPower = double.Parse(moveExhaust.Text) / stats.hitCount;
            }

            stats.mindsEye = moveMinds.Checked;
            stats.damageType = moveType.Text;

            stats.health = double.Parse(monHealth.Text);

            if (stats.damageType == "Cut")
            {
                stats.hitzone = double.Parse(monCut.Text);
            }
            else if (stats.damageType == "Impact")
            {
                stats.hitzone = double.Parse(monImpact.Text);
            }
            else if (stats.damageType == "Shot")
            {
                stats.hitzone = double.Parse(monShot.Text);
            }
            else if (stats.damageType == "Fixed")
            {
                stats.hitzone = 0;
            }

            if (stats.altDamageType == "Fire")
            {
                stats.eleHitzone = double.Parse(monFire.Text);
            }
            else if (stats.altDamageType == "Water")
            {
                stats.eleHitzone = double.Parse(monWater.Text);
            }
            else if (stats.altDamageType == "Thunder")
            {
                stats.eleHitzone = double.Parse(monThunder.Text);
            }
            else if (stats.altDamageType == "Ice")
            {
                stats.eleHitzone = double.Parse(monIce.Text);
            }
            else if (stats.altDamageType == "Dragon")
            {
                stats.eleHitzone = double.Parse(monDragon.Text);
            }
            else
            {
                stats.eleHitzone = 0;
            }

            if (stats.secElement == "Fire")
            {
                stats.secHitzone = double.Parse(monFire.Text);
            }
            else if (stats.secElement == "Water")
            {
                stats.secHitzone = double.Parse(monWater.Text);
            }
            else if (stats.secElement == "Thunder")
            {
                stats.secHitzone = double.Parse(monThunder.Text);
            }
            else if (stats.secElement == "Ice")
            {
                stats.secHitzone = double.Parse(monIce.Text);
            }
            else if (stats.secElement == "Dragon")
            {
                stats.secHitzone = double.Parse(monDragon.Text);
            }
            else
            {
                stats.secHitzone = 0;
            }

            stats.questMod = double.Parse(monQuest.Text);
            stats.exhaustMod = double.Parse(monExhaustMod.Text);
            stats.KOQuestMod = double.Parse(monKOMod.Text);
            stats.KOHitzone = double.Parse(monKO.Text);
            stats.exhaustHitzone = double.Parse(monExhaust.Text);
            stats.health = double.Parse(monHealth.Text);

            stats.monsterStatus = monStatus.Text;

            stats.GRank = monGRank.Checked;
        }

        private void ImportModifiers()
        {
            foreach (ListViewItem item in modList.Groups[0].Items)
            {
                armorModifiers[item.Text](0);
            }
            foreach (ListViewItem item in modList.Groups[1].Items)
            {
                foodModifiers[item.Text](0);
            }
            foreach (ListViewItem item in modList.Groups[2].Items)
            {
                weaponModifiers[item.Text](0);
            }
            foreach (ListViewItem item in modList.Groups[3].Items)
            {
                miscModifiers[item.Text](0);
            }

            if (stats.damageType == "Fixed")
            {
                if (stats.expMod > 1.3 && !stats.CB && !stats.GL)
                {
                    stats.expMod = 1.3;
                }
                else if (stats.CB && stats.expMod > 1.4)
                {
                    stats.expMod = 1.4;
                }
                else if (stats.GL)
                {

                }
                stats.totalAttackPower = 100;
                stats.totalAttackPower *= stats.expMod;
            }

            else
            {
                stats.totalAttackPower += stats.addRaw;
                stats.totalAttackPower *= stats.rawMod;
            }

            if (isElement(stats.altDamageType))
            {
                stats.eleAttackPower *= stats.eleMod;
                if (stats.eleAttackPower != 0)
                {
                    stats.eleAttackPower += stats.addElement;
                }

            }

            else if (isStatus(stats.altDamageType) || stats.altDamageType == "Blast")
            {
                stats.eleAttackPower *= stats.staMod;
                if (stats.eleAttackPower != 0)
                {
                    stats.eleAttackPower += stats.addStatus;
                }
            }

            if (isElement(stats.secElement))
            {
                stats.secPower *= stats.secMod;
                if (stats.secPower != 0)
                {
                    stats.secPower += stats.addSecElement;
                }
            }

            else if (isStatus(stats.secElement) || stats.secElement == "Blast")
            {
                stats.secPower *= stats.staSecMod;
                if (stats.secPower != 0)
                {
                    stats.secPower += stats.addSecStatus;
                }
            }

            if (stats.positiveAffinity > 100)
            {
                stats.positiveAffinity = 100;
                stats.negativeAffinity = 0;
            }

            if (stats.negativeAffinity > 100)
            {
                stats.negativeAffinity = 100;
                stats.positiveAffinity = 0;
            }

            if (-1 * (stats.negativeAffinity) + stats.positiveAffinity > 100)
            {
                stats.positiveAffinity -= stats.negativeAffinity;
            }

            stats.KOPower *= stats.KOMod;
            stats.exhaustPower *= stats.ExhMod;

            stats.rawSharpMod *= double.Parse(moveSharpMod.Text);
            stats.eleSharpMod *= double.Parse(moveElement.Text);
        }

        private void Export()
        {
            paraFixed.Checked = (stats.damageType == "Fixed");
            paraCritBoost.Checked = stats.criticalBoost;
            paraMinds.Checked = stats.mindsEye;
            paraStatusCrit.Checked = stats.statusCrit;
            paraMadAff.Checked = stats.ruefulCrit;
            paraSharp.SelectedItem = stats.sharpness;
            paraRaw.Text = stats.totalAttackPower.ToString();
            paraRawSharp.Text = stats.rawSharpMod.ToString();
            paraKO.Text = stats.KOPower.ToString();
            paraAltType.SelectedItem = stats.altDamageType;
            paraElePower.Text = stats.eleAttackPower.ToString();
            paraEleSharp.Text = stats.eleSharpMod.ToString();
            paraExh.Text = stats.exhaustPower.ToString();
            paraEleCrit.SelectedIndex = stats.eleCrit;

            if (stats.chaotic)
            {
                paraChaotic.Checked = true;
                paraPosAff.Text = stats.positiveAffinity.ToString();
                paraNegAff.Text = stats.negativeAffinity.ToString();
            }
            else
            {
                paraChaotic.Checked = false;
                double affinity = stats.positiveAffinity - stats.negativeAffinity;
                if (affinity > 0)
                {
                    paraAffinity.Text = affinity.ToString();
                }
                else
                {
                    paraAffinity.Text = affinity.ToString();
                }
            }

            paraAvgMV.Text = stats.avgMV.ToString();
            paraHitCount.Text = stats.hitCount.ToString();
            paraSecEle.SelectedItem = stats.secElement;
            paraSecPower.Text = stats.secPower.ToString();

            paraHitzone.Text = stats.hitzone.ToString();
            paraKOZone.Text = stats.KOHitzone.ToString();
            paraExhZone.Text = stats.exhaustHitzone.ToString();
            paraEleHit.Text = stats.eleHitzone.ToString();
            paraSecZone.Text = stats.secHitzone.ToString();
            paraMonStatus.SelectedItem = stats.monsterStatus;
            paraHealth.Text = stats.health.ToString();
            paraQuestMod.Text = stats.questMod.ToString();

            paraGRank.Checked = stats.GRank;
        }

        private void ResetWeaponDetails()
        {
            weapFinalUpgrade.Checked = false;
            string path = null;

            currentWeapons.Clear();
            filteredWeapons.Clear();
            weapFilter.Checked = false;
            weapSearch.Text = "";

            weaponColumnSorter.SortColumn = 0;
            weaponColumnSorter.Order = SortOrder.Ascending;

            weaponTree.BeginUpdate();
            //Read the appropriate .csv file...
            weaponTree.Nodes.Clear();
            string input;
            string[] inputs;
            char[] splitter = new char[] { ',' };
            char[] affinitySplit = new char[] { '/' };
            string[] levelSplit = new string[] { " Lv. " };
            List<weapon> newWeaponLevels = new List<weapon>();
            int expectedLevel = 1;
            int currCount = 0;

            if (weapGS.Checked)
            {
                path = "./Weapons/GS.csv";
            }

            if (weapLS.Checked)
            {
                path = "./Weapons/LS.csv";
            }

            if (weapSnS.Checked)
            {
                path = "./Weapons/SnS.csv";
            }

            if (weapDB.Checked)
            {
                using (StreamReader sr = new StreamReader("./Weapons/DB.csv"))
                {
                    //Verify
                    input = sr.ReadLine();
                    inputs = input.Split(splitter);
                    if (input != "ID,name,raw,element,eleValue,elementTwo,eleValueTwo,affinity,sharpness,sharpOne,sharpTwo")
                    {
                        return;
                    }

                    while (sr.Peek() >= 0)
                    {
                        inputs = (sr.ReadLine()).Split(splitter);
                        string[] name = inputs[1].Split(levelSplit, StringSplitOptions.None);

                        if (name[1] != expectedLevel.ToString())
                        {
                            currentWeapons.addFamily(newWeaponLevels);
                            newWeaponLevels = new List<weapon>();
                            expectedLevel = 1;
                        }
                        expectedLevel++;

                        string[] affinities = inputs[7].Split(affinitySplit);
                        weapon newWeap = new weapon
                        {
                            ID = int.Parse(inputs[0]),
                            name = inputs[1],
                            raw = int.Parse(inputs[2]),
                            element = inputs[3],
                            eleValue = int.Parse(inputs[4]),
                            elementTwo = inputs[5],
                            eleValueTwo = int.Parse(inputs[6]),
                            displayAffinity = inputs[7],
                            sharpness = inputs[8],
                            sharpnessOne = inputs[9],
                            sharpnessTwo = inputs[10]
                        };

                        if (affinities.Length == 2)
                        {
                            newWeap.negativeAffinity = int.Parse(affinities[0]) * -1;
                            newWeap.positiveAffinity = int.Parse(affinities[1]);
                        }
                        else
                        {
                            int newAff = int.Parse(affinities[0]);
                            if (newAff >= 0)
                            {
                                newWeap.positiveAffinity = newAff;
                                newWeap.negativeAffinity = 0;
                            }
                            else
                            {
                                newWeap.positiveAffinity = 0;
                                newWeap.negativeAffinity = newAff * -1;
                            }
                        }

                        newWeaponLevels.Add(newWeap);
                    }
                    currentWeapons.addFamily(newWeaponLevels);
                }

                //Populate the TreeView

                foreach (string weaponFamily in currentWeapons.families)
                {
                    weaponTree.Nodes.Add(weaponFamily);
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    foreach (weapon weap in currFamily)
                    {
                        TreeNode newNode = new TreeNode(weap.name);
                        newNode.Name = weap.name;
                        weaponTree.Nodes[currCount].Nodes.Add(newNode);
                    }
                    currCount++;
                }

                weaponTree.EndUpdate();

                //Populate the ListView
                weaponDetails.Items.Clear();
                weaponDetails.Columns.Clear();
                weaponDetails.Columns.Add("ID", 30);
                weaponDetails.Columns.Add("Name", 150);
                weaponDetails.Columns.Add("Raw", 50);
                weaponDetails.Columns.Add("Element Type", 100);
                weaponDetails.Columns.Add("Value", 50);
                weaponDetails.Columns.Add("Sec. Element", 100);
                weaponDetails.Columns.Add("Sec. Value", 50);
                weaponDetails.Columns.Add("Affinity", 50);
                weaponDetails.Columns.Add("Sharpness", 80);
                weaponDetails.Columns.Add("Sharpness + 1", 80);
                weaponDetails.Columns.Add("Sharpness + 2", 80);

                foreach (string weaponFamily in currentWeapons.families)
                {
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);

                    addWeapons(currFamily);
                }
                return;
            }

            if (weapHam.Checked)
            {
                path = "./Weapons/Hammer.csv";
            }

            if (weapHH.Checked)
            {
                path = "./Weapons/HH.csv";
            }

            if (weapLan.Checked)
            {
                path = "./Weapons/Lance.csv";
            }

            if (weapGL.Checked)
            {
                path = "./Weapons/GL.csv";
            }

            if (weapSA.Checked)
            {
                using (StreamReader sr = new StreamReader("./Weapons/SA.csv"))
                {
                    //Verify
                    input = sr.ReadLine();
                    inputs = input.Split(splitter);
                    if (input != "ID,name,raw,element,eleValue,elementTwo,eleValueTwo,affinity,sharpness,sharpOne,sharpTwo")
                    {
                        return;
                    }

                    while (sr.Peek() >= 0)
                    {
                        inputs = (sr.ReadLine()).Split(splitter);
                        string[] name = inputs[1].Split(levelSplit, StringSplitOptions.None);

                        if (name[1] != expectedLevel.ToString())
                        {
                            currentWeapons.addFamily(newWeaponLevels);
                            newWeaponLevels = new List<weapon>();
                            expectedLevel = 1;
                        }
                        expectedLevel++;

                        string[] affinities = inputs[7].Split(affinitySplit);
                        weapon newWeap = new weapon
                        {
                            ID = int.Parse(inputs[0]),
                            name = inputs[1],
                            raw = int.Parse(inputs[2]),
                            element = inputs[3],
                            eleValue = int.Parse(inputs[4]),
                            elementTwo = inputs[5],
                            eleValueTwo = int.Parse(inputs[6]),
                            displayAffinity = inputs[7],
                            sharpness = inputs[8],
                            sharpnessOne = inputs[9],
                            sharpnessTwo = inputs[10]
                        };

                        if (affinities.Length == 2)
                        {
                            newWeap.negativeAffinity = int.Parse(affinities[0]) * -1;
                            newWeap.positiveAffinity = int.Parse(affinities[1]);
                        }
                        else
                        {
                            int newAff = int.Parse(affinities[0]);
                            if (newAff >= 0)
                            {
                                newWeap.positiveAffinity = newAff;
                                newWeap.negativeAffinity = 0;
                            }
                            else
                            {
                                newWeap.positiveAffinity = 0;
                                newWeap.negativeAffinity = newAff * -1;
                            }
                        }

                        newWeaponLevels.Add(newWeap);
                    }
                    currentWeapons.addFamily(newWeaponLevels);
                }

                //Populate the TreeView
                foreach (string weaponFamily in currentWeapons.families)
                {
                    weaponTree.Nodes.Add(weaponFamily);
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    foreach (weapon weap in currFamily)
                    {
                        TreeNode newNode = new TreeNode(weap.name);
                        newNode.Name = weap.name;
                        weaponTree.Nodes[currCount].Nodes.Add(newNode);
                    }
                    currCount++;
                }

                weaponTree.EndUpdate();

                //Populate the ListView
                weaponDetails.Items.Clear();
                weaponDetails.Columns.Clear();
                weaponDetails.Columns.Add("ID", 30);
                weaponDetails.Columns.Add("Name", 150);
                weaponDetails.Columns.Add("Raw", 50);
                weaponDetails.Columns.Add("Element Type", 100);
                weaponDetails.Columns.Add("Value", 50);
                weaponDetails.Columns.Add("Phial Element", 100);
                weaponDetails.Columns.Add("Phial Value", 50);
                weaponDetails.Columns.Add("Affinity", 50);
                weaponDetails.Columns.Add("Sharpness", 80);
                weaponDetails.Columns.Add("Sharpness + 1", 80);
                weaponDetails.Columns.Add("Sharpness + 2", 80);

                foreach (string weaponFamily in currentWeapons.families)
                {
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    addWeapons(currFamily);
                }
                return;
            }

            if (weapCB.Checked)
            {
                path = "./Weapons/CB.csv";
            }

            if (weapIG.Checked)
            {
                path = "./Weapons/IG.csv";
            }

            if (weapLBG.Checked)
            {
                using (StreamReader sr = new StreamReader("./Weapons/LBG.csv"))
                {
                    //Verify
                    input = sr.ReadLine();
                    inputs = input.Split(splitter);
                    if (input != "ID,name,raw,affinity")
                    {
                        return;
                    }

                    while (sr.Peek() >= 0)
                    {
                        inputs = (sr.ReadLine()).Split(splitter);
                        string[] name = inputs[1].Split(levelSplit, StringSplitOptions.None);

                        if (name[1] != expectedLevel.ToString())
                        {
                            currentWeapons.addFamily(newWeaponLevels);
                            newWeaponLevels = new List<weapon>();
                            expectedLevel = 1;
                        }
                        expectedLevel++;

                        string[] affinities = inputs[3].Split(affinitySplit);
                        weapon newWeap = new weapon
                        {
                            ID = int.Parse(inputs[0]),
                            name = inputs[1],
                            raw = int.Parse(inputs[2]),
                            displayAffinity = inputs[3]
                        };

                        if (affinities.Length == 2)
                        {
                            newWeap.negativeAffinity = int.Parse(affinities[0]) * -1;
                            newWeap.positiveAffinity = int.Parse(affinities[1]);
                        }
                        else
                        {
                            int newAff = int.Parse(affinities[0]);
                            if (newAff >= 0)
                            {
                                newWeap.positiveAffinity = newAff;
                                newWeap.negativeAffinity = 0;
                            }
                            else
                            {
                                newWeap.positiveAffinity = 0;
                                newWeap.negativeAffinity = newAff * -1;
                            }
                        }

                        newWeaponLevels.Add(newWeap);
                    }
                    currentWeapons.addFamily(newWeaponLevels);
                }

                //Populate the TreeView
                foreach (string weaponFamily in currentWeapons.families)
                {
                    weaponTree.Nodes.Add(weaponFamily);
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    foreach (weapon weap in currFamily)
                    {
                        TreeNode newNode = new TreeNode(weap.name);
                        newNode.Name = weap.name;
                        weaponTree.Nodes[currCount].Nodes.Add(newNode);
                    }
                    currCount++;
                }

                weaponTree.EndUpdate();

                //Populate the ListView
                weaponDetails.Items.Clear();
                weaponDetails.Columns.Clear();
                weaponDetails.Columns.Add("ID", 30);
                weaponDetails.Columns.Add("Name", 150);
                weaponDetails.Columns.Add("Raw", 50);
                weaponDetails.Columns.Add("Affinity", 50);

                foreach (string weaponFamily in currentWeapons.families)
                {
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    addWeapons(currFamily);
                }
                return;
            }

            if (weapHBG.Checked)
            {
                using (StreamReader sr = new StreamReader("./Weapons/HBG.csv"))
                {
                    //Verify
                    input = sr.ReadLine();
                    inputs = input.Split(splitter);
                    if (input != "ID,name,raw,affinity")
                    {
                        return;
                    }

                    while (sr.Peek() >= 0)
                    {
                        inputs = (sr.ReadLine()).Split(splitter);
                        string[] name = inputs[1].Split(levelSplit, StringSplitOptions.None);

                        if (name[1] != expectedLevel.ToString())
                        {
                            currentWeapons.addFamily(newWeaponLevels);
                            newWeaponLevels = new List<weapon>();
                            expectedLevel = 1;
                        }
                        expectedLevel++;

                        string[] affinities = inputs[3].Split(affinitySplit);
                        weapon newWeap = new weapon
                        {
                            ID = int.Parse(inputs[0]),
                            name = inputs[1],
                            raw = int.Parse(inputs[2]),
                            displayAffinity = inputs[3],
                        };

                        if (affinities.Length == 2)
                        {
                            newWeap.negativeAffinity = int.Parse(affinities[0]) * -1;
                            newWeap.positiveAffinity = int.Parse(affinities[1]);
                        }
                        else
                        {
                            int newAff = int.Parse(affinities[0]);
                            if (newAff >= 0)
                            {
                                newWeap.positiveAffinity = newAff;
                                newWeap.negativeAffinity = 0;
                            }
                            else
                            {
                                newWeap.positiveAffinity = 0;
                                newWeap.negativeAffinity = newAff * -1;
                            }
                        }

                        newWeaponLevels.Add(newWeap);
                    }
                    currentWeapons.addFamily(newWeaponLevels);
                }

                //Populate the TreeView
                foreach (string weaponFamily in currentWeapons.families)
                {
                    weaponTree.Nodes.Add(weaponFamily);
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    foreach (weapon weap in currFamily)
                    {
                        TreeNode newNode = new TreeNode(weap.name);
                        newNode.Name = weap.name;
                        weaponTree.Nodes[currCount].Nodes.Add(newNode);
                    }
                    currCount++;
                }

                weaponTree.EndUpdate();

                //Populate the ListView
                weaponDetails.Items.Clear();
                weaponDetails.Columns.Clear();
                weaponDetails.Columns.Add("ID", 30);
                weaponDetails.Columns.Add("Name", 150);
                weaponDetails.Columns.Add("Raw", 50);
                weaponDetails.Columns.Add("Affinity", 50);

                foreach (string weaponFamily in currentWeapons.families)
                {
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    addWeapons(currFamily);
                }
                return;
            }

            if (weapBow.Checked)
            {
                using (StreamReader sr = new StreamReader("./Weapons/Bow.csv"))
                {
                    //Verify
                    input = sr.ReadLine();
                    inputs = input.Split(splitter);
                    if (input != "ID,name,raw,element,eleValue,affinity")
                    {
                        return;
                    }

                    while (sr.Peek() >= 0)
                    {
                        inputs = (sr.ReadLine()).Split(splitter);
                        string[] name = inputs[1].Split(levelSplit, StringSplitOptions.None);

                        if (name[1] != expectedLevel.ToString())
                        {
                            currentWeapons.addFamily(newWeaponLevels);
                            newWeaponLevels = new List<weapon>();
                            expectedLevel = 1;
                        }
                        expectedLevel++;

                        string[] affinities = inputs[5].Split(affinitySplit);
                        weapon newWeap = new weapon
                        {
                            ID = int.Parse(inputs[0]),
                            name = inputs[1],
                            raw = int.Parse(inputs[2]),
                            element = inputs[3],
                            eleValue = int.Parse(inputs[4]),
                            displayAffinity = inputs[5]
                        };

                        if (affinities.Length == 2)
                        {
                            newWeap.negativeAffinity = int.Parse(affinities[0]) * -1;
                            newWeap.positiveAffinity = int.Parse(affinities[1]);
                        }
                        else
                        {
                            int newAff = int.Parse(affinities[0]);
                            if (newAff >= 0)
                            {
                                newWeap.positiveAffinity = newAff;
                                newWeap.negativeAffinity = 0;
                            }
                            else
                            {
                                newWeap.positiveAffinity = 0;
                                newWeap.negativeAffinity = newAff * -1;
                            }
                        }

                        newWeaponLevels.Add(newWeap);
                    }
                    currentWeapons.addFamily(newWeaponLevels);
                }

                //Populate the TreeView
                foreach (string weaponFamily in currentWeapons.families)
                {
                    weaponTree.Nodes.Add(weaponFamily);
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                    foreach (weapon weap in currFamily)
                    {
                        TreeNode newNode = new TreeNode(weap.name);
                        newNode.Name = weap.name;
                        weaponTree.Nodes[currCount].Nodes.Add(newNode);
                    }
                    currCount++;
                }

                weaponTree.EndUpdate();

                //Populate the ListView
                weaponDetails.Items.Clear();
                weaponDetails.Columns.Clear();
                weaponDetails.Columns.Add("ID", 30);
                weaponDetails.Columns.Add("Name", 150);
                weaponDetails.Columns.Add("Raw", 50);
                weaponDetails.Columns.Add("Element Type", 100);
                weaponDetails.Columns.Add("Value", 50);
                weaponDetails.Columns.Add("Affinity", 50);

                foreach (string weaponFamily in currentWeapons.families)
                {
                    List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);

                    addWeapons(currFamily);

                }
                return;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                //Verify
                input = sr.ReadLine();
                inputs = input.Split(splitter);
                if (input != "ID,name,raw,element,eleValue,affinity,sharpness,sharpOne,sharpTwo")
                {
                    return;
                }

                while (sr.Peek() >= 0)
                {

                    inputs = (sr.ReadLine()).Split(splitter);
                    string[] name = inputs[1].Split(levelSplit, StringSplitOptions.None);

                    if (name[1] != expectedLevel.ToString())
                    {
                        currentWeapons.addFamily(newWeaponLevels);
                        newWeaponLevels = new List<weapon>();
                        expectedLevel = 1;
                    }
                    expectedLevel++;

                    string[] affinities = inputs[5].Split(affinitySplit);
                    weapon newWeap = new weapon
                    {
                        ID = int.Parse(inputs[0]),
                        name = inputs[1],
                        raw = int.Parse(inputs[2]),
                        element = inputs[3],
                        eleValue = int.Parse(inputs[4]),
                        displayAffinity = inputs[5],
                        sharpness = inputs[6],
                        sharpnessOne = inputs[7],
                        sharpnessTwo = inputs[8]
                    };

                    if (affinities.Length == 2)
                    {
                        newWeap.negativeAffinity = int.Parse(affinities[0]) * -1;
                        newWeap.positiveAffinity = int.Parse(affinities[1]);
                    }
                    else
                    {
                        int newAff = int.Parse(affinities[0]);
                        if (newAff >= 0)
                        {
                            newWeap.positiveAffinity = newAff;
                            newWeap.negativeAffinity = 0;
                        }
                        else
                        {
                            newWeap.positiveAffinity = 0;
                            newWeap.negativeAffinity = newAff * -1;
                        }
                    }

                    newWeaponLevels.Add(newWeap);
                }
                currentWeapons.addFamily(newWeaponLevels);
            }

            //Populate the TreeView
            currCount = 0;
            foreach (string weaponFamily in currentWeapons.families)
            {
                weaponTree.Nodes.Add(weaponFamily);
                List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);
                foreach (weapon weap in currFamily)
                {
                    TreeNode newNode = new TreeNode(weap.name);
                    newNode.Name = weap.name;
                    weaponTree.Nodes[currCount].Nodes.Add(newNode);
                }
                currCount++;
            }

            weaponTree.EndUpdate();

            //Populate the ListView
            weaponDetails.Items.Clear();
            weaponDetails.Columns.Clear();
            weaponDetails.Columns.Add("ID", 30);
            weaponDetails.Columns.Add("Name", 150);
            weaponDetails.Columns.Add("Raw", 50);
            weaponDetails.Columns.Add("Element Type", 100);
            weaponDetails.Columns.Add("Value", 50);
            weaponDetails.Columns.Add("Affinity", 50);
            weaponDetails.Columns.Add("Sharpness", 80);
            weaponDetails.Columns.Add("Sharpness + 1", 80);
            weaponDetails.Columns.Add("Sharpness + 2", 80);

            foreach (string weaponFamily in currentWeapons.families)
            {
                List<weapon> currFamily = currentWeapons.getFamilyDetails(weaponFamily);

                addWeapons(currFamily);

            }
        }

        private void filterWeapons()
        {
            //Populate the Filter List with items, depending on the chosen items in the filters.
            weaponDetails.Items.Clear();
            List<string> tempFiltered;
            bool[] filters = new bool[]  {weapFire.Checked, weapWater.Checked, weapThunder.Checked, weapIce.Checked, weapDra.Checked,
                weapPoi.Checked, weapPara.Checked, weapSleep.Checked, weapBlast.Checked, weapNoEle.Checked };

            bool allFalse = true;
            foreach (bool filter in filters)
            {
                if (filter)
                {
                    allFalse = false;
                    break;
                }
            }

            if (allFalse)
            {
                tempFiltered = currentWeapons.families;
            }
            else
            {
                tempFiltered = currentWeapons.filterWeapons(filters);
            }

            if (weapFinalUpgrade.Checked)
            {
                filteredWeapons = currentWeapons.getFinal(tempFiltered);
            }
            else
            {
                filteredWeapons = currentWeapons.getFamilies(tempFiltered);
            }

            searchWeapons();
        }

        private void searchWeapons()
        {
            weaponDetails.Items.Clear();
            List<string> finalWeapons = new List<string>();

            if (filteredWeapons.Count == 0)
            {
                filteredWeapons = currentWeapons.weapons;
            }

            string searchString = weapSearch.Text.Trim();
            if (weapSearch.Text != "")
            {
                foreach (string weapon in filteredWeapons)
                {
                    if (weapon.Contains(weapSearch.Text) || (weapon.ToLower()).Contains(weapSearch.Text))
                    {
                        finalWeapons.Add(weapon);
                    }
                }
            }
            else
            {
                finalWeapons.AddRange(filteredWeapons);
            }

            List<weapon> displayWeapons = currentWeapons.getWeapons(finalWeapons);
            addWeapons(displayWeapons);
        }

        private void addWeapons(List<weapon> weapons)
        {
            if (weapDB.Checked)
            {
                foreach (weapon weap in weapons)
                {
                    ListViewItem newItem = new ListViewItem(weap.ID.ToString());
                    newItem.SubItems.Add(weap.name);
                    newItem.SubItems.Add(weap.raw.ToString());
                    newItem.SubItems.Add(weap.element);
                    newItem.SubItems.Add(weap.eleValue.ToString());
                    newItem.SubItems.Add(weap.elementTwo);
                    newItem.SubItems.Add(weap.eleValueTwo.ToString());
                    newItem.SubItems.Add(weap.displayAffinity);
                    newItem.SubItems.Add(weap.sharpness);
                    newItem.SubItems.Add(weap.sharpnessOne);
                    newItem.SubItems.Add(weap.sharpnessTwo);

                    newItem.Name = weap.name;

                    weaponDetails.Items.Add(newItem);

                }
                return;
            }

            if (weapSA.Checked)
            {
                foreach (weapon weap in weapons)
                {
                    ListViewItem newItem = new ListViewItem(weap.ID.ToString());
                    newItem.SubItems.Add(weap.name);
                    newItem.SubItems.Add(weap.raw.ToString());
                    newItem.SubItems.Add(weap.element);
                    newItem.SubItems.Add(weap.eleValue.ToString());
                    newItem.SubItems.Add(weap.elementTwo);
                    newItem.SubItems.Add(weap.eleValueTwo.ToString());
                    newItem.SubItems.Add(weap.displayAffinity);
                    newItem.SubItems.Add(weap.sharpness);
                    newItem.SubItems.Add(weap.sharpnessOne);
                    newItem.SubItems.Add(weap.sharpnessTwo);

                    newItem.Name = weap.name;

                    weaponDetails.Items.Add(newItem);
                }

                return;
            }

            if (weapLBG.Checked || weapHBG.Checked)
            {
                foreach (weapon weap in weapons)
                {
                    ListViewItem newItem = new ListViewItem(weap.ID.ToString());
                    newItem.SubItems.Add(weap.name);
                    newItem.SubItems.Add(weap.raw.ToString());
                    newItem.SubItems.Add(weap.displayAffinity);

                    newItem.Name = weap.name;

                    weaponDetails.Items.Add(newItem);
                }

                return;
            }

            if (weapBow.Checked)
            {
                foreach (weapon weap in weapons)
                {
                    ListViewItem newItem = new ListViewItem(weap.ID.ToString());
                    newItem.SubItems.Add(weap.name);
                    newItem.SubItems.Add(weap.raw.ToString());
                    newItem.SubItems.Add(weap.element);
                    newItem.SubItems.Add(weap.eleValue.ToString());
                    newItem.SubItems.Add(weap.displayAffinity);

                    newItem.Name = weap.name;

                    weaponDetails.Items.Add(newItem);
                }

                return;
            }

            foreach (weapon weap in weapons)
            {
                ListViewItem newItem = new ListViewItem(weap.ID.ToString());
                newItem.SubItems.Add(weap.name);
                newItem.SubItems.Add(weap.raw.ToString());
                newItem.SubItems.Add(weap.element);
                newItem.SubItems.Add(weap.eleValue.ToString());
                newItem.SubItems.Add(weap.displayAffinity);
                newItem.SubItems.Add(weap.sharpness);
                newItem.SubItems.Add(weap.sharpnessOne);
                newItem.SubItems.Add(weap.sharpnessTwo);

                newItem.Name = weap.name;

                weaponDetails.Items.Add(newItem);
            }

            return;
        }

        private void changeMoveList()
        {
            moveColumnSorter.SortColumn = 0;
            moveColumnSorter.Order = SortOrder.Ascending;
            moveDetails.Items.Clear();
            string path;
            if (moveGS.Checked)
            {
                path = "./Moves/GS.csv";
            }
            else if (moveLS.Checked)
            {
                path = "./Moves/LS.csv";
            }
            else if (moveSnS.Checked)
            {
                path = "./Moves/SnS.csv";
            }
            else if (moveDB.Checked)
            {
                path = "./Moves/DB.csv";
            }
            else if (moveHammer.Checked)
            {
                path = "./Moves/Hammer.csv";
            }
            else if (moveHH.Checked)
            {
                path = "./Moves/HH.csv";
            }
            else if (moveLance.Checked)
            {
                path = "./Moves/Lance.csv";
            }
            else if (moveGL.Checked)
            {
                path = "./Moves/GL.csv";
            }
            else if (moveSA.Checked)
            {
                path = "./Moves/SA.csv";
            }
            else if (moveCB.Checked)
            {
                path = "./Moves/CB.csv";
            }
            else if (moveIG.Checked)
            {
                path = "./Moves/IG.csv";
            }
            else if (moveShot.Checked)
            {
                path = "./Moves/Shots.csv";
            }
            else if (moveLBG.Checked)
            {
                path = "./Moves/LBG.csv";
            }
            else if (moveHBG.Checked)
            {
                path = "./Moves/HBG.csv";
            }
            else if (moveBow.Checked)
            {
                path = "./Moves/Bow.csv";
            }
            else
            {
                path = null;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                //Verify
                string input = sr.ReadLine();
                string[] inputs = input.Split(new char[] { ',' });
                if (input != "ID,name,combo,MV,hitCount,damageType,sharpnessMod,elementMod,KO,exhaust,moveElement,mindsEye,aerial,miscNotes")
                {
                    return;
                }

                while (sr.Peek() >= 0)
                {
                    inputs = (sr.ReadLine()).Split(new char[] { ',' });

                    string[] extraElement = inputs[12].Split(new char[] { ' ' });
                    ListViewItem newItem = new ListViewItem(inputs[0]);
                    newItem.SubItems.Add(inputs[1]);
                    newItem.SubItems.Add(inputs[2]);
                    newItem.SubItems.Add(inputs[3]);
                    newItem.SubItems.Add(inputs[4]);
                    newItem.SubItems.Add(inputs[5]);
                    newItem.SubItems.Add(inputs[6]);
                    newItem.SubItems.Add(inputs[7]);
                    newItem.SubItems.Add(inputs[8]);
                    newItem.SubItems.Add(inputs[9]);
                    newItem.SubItems.Add(inputs[10]);
                    newItem.SubItems.Add(inputs[11]);
                    newItem.SubItems.Add(inputs[12]);

                    moveDetails.Items.Add(newItem);
                }
            }
        }

        private void fillWeapon(string name)
        {
            weapon selWeap = currentWeapons.weaponDetails[name];
            weapRaw.Text = selWeap.raw.ToString();
            weapEle.SelectedItem = selWeap.element;
            weapEleDamage.Text = selWeap.eleValue.ToString();
            weapSec.SelectedItem = selWeap.elementTwo;
            weapSecDamage.Text = selWeap.eleValueTwo.ToString();
            if (selWeap.positiveAffinity != 0 && selWeap.negativeAffinity != 0)
            {
                weapChaotic.Checked = true;
                weapPosAff.Text = selWeap.positiveAffinity.ToString();
                weapNegAff.Text = selWeap.negativeAffinity.ToString();
            }
            else
            {
                weapChaotic.Checked = false;
                if (selWeap.positiveAffinity != 0)
                {
                    weapAffinity.Text = selWeap.positiveAffinity.ToString();
                }
                else if (selWeap.negativeAffinity != 0)
                {
                    weapAffinity.Text = (-1 * selWeap.negativeAffinity).ToString();
                }
                else
                {
                    weapAffinity.Text = "0";
                }
            }

            weapSharpness.SelectedItem = selWeap.sharpness;
            weapSharpOne.SelectedItem = selWeap.sharpnessOne;
            weapSharpTwo.SelectedItem = selWeap.sharpnessTwo;
        }

        private void fillMove(ListViewItem selectedMove)
        {
            if (selectedMove.SubItems[3].Text == "*")
            {
                specialMoves(selectedMove.SubItems[1].Text);
            }
            else
            {
                moveMV.Text = selectedMove.SubItems[3].Text;
                string[] inherantElements = selectedMove.SubItems[10].Text.Split(new char[] { ' ' });
                if (inherantElements.Length == 2)
                {
                    moveInherit.SelectedItem = inherantElements[0];
                    moveInheritValue.Text = inherantElements[1];
                }
                else
                {
                    moveInherit.SelectedIndex = 0;
                    moveInheritValue.Text = "0";
                }
            }

            moveHitCount.Text = selectedMove.SubItems[4].Text;
            moveType.SelectedItem = selectedMove.SubItems[5].Text;
            moveSharpMod.Text = selectedMove.SubItems[6].Text;
            moveElement.Text = selectedMove.SubItems[7].Text;
            moveKO.Text = selectedMove.SubItems[8].Text;
            moveExhaust.Text = selectedMove.SubItems[9].Text;

            moveMinds.Checked = (selectedMove.SubItems[11].Text == "Yes");
            moveAerial.Checked = (selectedMove.SubItems[12].Text == "Yes");
        }

        private void specialMoves(string text)
        {
            if (text == "Sonic Smash I (Finisher)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (30 * (1 + (weaponRaw * 1.6 / 100))).ToString();
            }
            else if (text == "Sonic Smash II (Finisher)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (35 * (1 + (weaponRaw * 1.6 / 100))).ToString();
            }
            else if (text == "Sonic Smash III (Finisher)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (38 * (1 + (weaponRaw * 1.6 / 100))).ToString();
            }
            else if (text == "Dragon Blast I")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                double totalDamage = 0;
                totalDamage += 48 * (1 + (weaponRaw * 0.7 / 100));
                totalDamage += 3 * (10 * (1 + (weaponRaw * 0.2 / 100)));
                moveMV.Text = totalDamage.ToString();
            }
            else if (text == "Dragon Blast II")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                double totalDamage = 0;
                totalDamage += 49 * (1 + (weaponRaw * 0.7 / 100));
                totalDamage += 6 * (10 * (1 + (weaponRaw * 0.2 / 100)));
                moveMV.Text = totalDamage.ToString();
            }
            else if (text == "Dragon Blast III")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                double totalDamage = 0;
                totalDamage += 49 * (1 + (weaponRaw * 0.7 / 100));
                totalDamage += 9 * (10 * (1 + (weaponRaw * 0.2 / 100)));
                moveMV.Text = totalDamage.ToString();
            }
            else if (text == "Dragon Blast I (Opener)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (48 * (1 + (weaponRaw * 0.7 / 100))).ToString();
            }
            else if (text == "Dragon Blast II (Opener)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (49 * (1 + (weaponRaw * 0.7 / 100))).ToString();
            }
            else if (text == "Dragon Blast III (Opener)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (49 * (1 + (weaponRaw * 0.7 / 100))).ToString();
            }
            else if (text == "Dragon Blast (Following Hits)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (10 * (1 + (weaponRaw * 0.2 / 100))).ToString();
            }
            else if (text == "Impact Phial Explosion")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (weaponRaw * 0.05).ToString();
            }
            else if (text == "Mini Impact Explosion")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (weaponRaw * 0.025).ToString();
            }
            else if (text == "Super Impact Explosion")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (weaponRaw * 0.1).ToString();
            }
            else if (text == "Super Nova I (Center)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (40 + 40 * ((weaponRaw * 0.75) / 100)).ToString();
            }
            else if (text == "Super Nova I (Edge)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (36 + 36 * ((weaponRaw * 0.75) / 100)).ToString();
            }
            else if (text == "Super Nova II (Center)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (40 + 40 * ((weaponRaw * 1.5) / 100)).ToString();
            }
            else if (text == "Super Nova II (Edge)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (15 + 15 * ((weaponRaw * 1.5) / 100)).ToString();
            }
            else if (text == "Super Nova III (Center)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (45 + 45 * ((weaponRaw * 2) / 100)).ToString();
            }
            else if (text == "Super Nova III (Edge)")
            {
                double weaponRaw = double.Parse(weapRaw.Text);
                moveMV.Text = (5 + 5 * ((weaponRaw * 2) / 100)).ToString();
            }

            moveInherit.SelectedIndex = 0;

            if (text == "Element Lv 1")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.42).ToString();
            }
            else if (text == "Dragon Lv 1")
            {
                moveMV.Text = "5";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.38).ToString();
            }
            else if (text == "Dragon Lv 1 (Individual Hits)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.38).ToString();
            }
            else if (text == "Element Lv 2")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.55).ToString();
            }
            else if (text == "Dragon Lv 2")
            {
                moveMV.Text = "5";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "Dragon Lv 2 (Individual Hit)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "P. Element Lv 1")
            {
                moveMV.Text = "6";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.19).ToString();
            }
            else if (text == "P. Element Lv 1 (Individual Hit)")
            {
                moveMV.Text = "2";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.19).ToString();
            }
            else if (text == "P. Element Lv 2")
            {
                moveMV.Text = "15";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.21).ToString();
            }
            else if (text == "P. Element Lv 2 (Individual Hit)")
            {
                moveMV.Text = "3";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.21).ToString();
            }
            else if (text == "RF Element Lv 1 (x3)")
            {
                moveMV.Text = "21";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.42).ToString();
            }
            else if (text == "RF Element Lv 1 (x3 1 Hit)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.42).ToString();
            }
            else if (text == "RF Element Lv 1 (x4)")
            {
                moveMV.Text = "28";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.42).ToString();
            }
            else if (text == "RF Element Lv 1 (x4 1 Hit)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.42).ToString();
            }
            else if (text == "RF Dragon Lv 1")
            {
                moveMV.Text = "10";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.38).ToString();
            }
            else if (text == "RF Dragon Lv 1 (1 Hit)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.38).ToString();
            }
            else if (text == "RF Element Lv 2")
            {
                moveMV.Text = "21";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.55).ToString();
            }
            else if (text == "RF Element Lv 2 (1 Hit)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.55).ToString();
            }
            else if (text == "RF Dragon Lv 2")
            {
                moveMV.Text = "10";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "RF Dragon Lv 2 (1 Hit)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "RF P.Element Lv 1")
            {
                moveMV.Text = "18";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.19).ToString();
            }
            else if (text == "RF P.Element Lv 1 (1 Hit)")
            {
                moveMV.Text = "2";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.19).ToString();
            }
            else if (text == "RF P.Element Lv 2")
            {
                moveMV.Text = "45";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.21).ToString();
            }
            else if (text == "RF P.Element Lv 2 (1 Hit)")
            {
                moveMV.Text = "3";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.21).ToString();
            }

            else if (text == "Element Lv 1 (Gen)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "Dragon Lv 1 (Gen)")
            {
                moveMV.Text = "5";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.40).ToString();
            }
            else if (text == "Dragon Lv 1 (Individual Gen)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.40).ToString();
            }
            else if (text == "Element Lv 2 (Gen)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.58).ToString();
            }
            else if (text == "Dragon Lv 2 (Gen)")
            {
                moveMV.Text = "5";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.48).ToString();
            }
            else if (text == "Dragon Lv 2 (Individual Gen)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.48).ToString();
            }
            else if (text == "P. Element Lv 1 (Gen)")
            {
                moveMV.Text = "6";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.20).ToString();
            }
            else if (text == "P. Element Lv 1 (Individual Gen)")
            {
                moveMV.Text = "2";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.20).ToString();
            }
            else if (text == "P. Element Lv 2 (Gen)")
            {
                moveMV.Text = "15";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.23).ToString();
            }
            else if (text == "P. Element Lv 2 (Individual Gen)")
            {
                moveMV.Text = "3";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.23).ToString();
            }
            else if (text == "RF Element Lv 1 (x3 Gen)")
            {
                moveMV.Text = "21";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "RF Element Lv 1 (x3 1 Hit Gen)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "RF Element Lv 1 (x4 Gen)")
            {
                moveMV.Text = "28";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "RF Element Lv 1 (x4 1 Hit Gen)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.45).ToString();
            }
            else if (text == "RF Dragon Lv 1 (Gen)")
            {
                moveMV.Text = "10";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.40).ToString();
            }
            else if (text == "RF Dragon Lv 1 (1 Hit Gen)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.40).ToString();
            }
            else if (text == "RF Element Lv 2 (Gen)")
            {
                moveMV.Text = "21";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.58).ToString();
            }
            else if (text == "RF Element Lv 2 (1 Hit Gen)")
            {
                moveMV.Text = "7";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.58).ToString();
            }
            else if (text == "RF Dragon Lv 2 (Gen)")
            {
                moveMV.Text = "10";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.48).ToString();
            }
            else if (text == "RF Dragon Lv 2 (1 Hit Gen)")
            {
                moveMV.Text = "1";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 5;
                moveInheritValue.Text = (weaponRaw * 0.48).ToString();
            }
            else if (text == "RF P.Element Lv 1 (Gen)")
            {
                moveMV.Text = "18";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.20).ToString();
            }
            else if (text == "RF P.Element Lv 1 (1 Hit Gen)")
            {
                moveMV.Text = "2";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.20).ToString();
            }
            else if (text == "RF P.Element Lv 2 (Gen)")
            {
                moveMV.Text = "45";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.23).ToString();
            }
            else if (text == "RF P.Element Lv 2 (1 Hit Gen)")
            {
                moveMV.Text = "3";
                double weaponRaw = double.Parse(weapRaw.Text);
                moveInherit.SelectedIndex = 1;
                moveInheritValue.Text = (weaponRaw * 0.23).ToString();
            }
        }

        private void CalculateStatus()
        {
            int hitCount = int.Parse(staHitCount.Text);
            if (hitCount == 0)
            {
                hitCount = 1;
            }
            int status = int.Parse(staPower.Text);
            if (status == 0)
            {
                staPrint.Text = "Status damage dealt is 0.";
                return;
            }
            double initThreshold = int.Parse(staInit.Text);
            double incThreshold = int.Parse(staInc.Text);
            double maxThreshold = int.Parse(staMax.Text);
            double statusPerHit;
            double statusTotal;
            int hitsToInitThreshold;
            int hitsToIncThreshold;
            int hitsToMaxThreshold;

            string statusType = staType.Text;
            if (statusType == "KO")
            {
                double KOZone = double.Parse(staKOZone.Text) / 100;
                statusPerHit = Math.Floor(status * KOZone);
                statusTotal = statusPerHit * hitCount;

                initThreshold *= double.Parse(staKOMod.Text);
                incThreshold *= double.Parse(staKOMod.Text);
                maxThreshold *= double.Parse(staKOMod.Text);
            }
            else if (statusType == "Exhaust")
            {
                double exhZone = double.Parse(staExhZone.Text) / 100;
                statusPerHit = Math.Floor(status * exhZone);
                statusTotal = statusPerHit * hitCount;

                initThreshold *= double.Parse(staExhMod.Text);
                incThreshold *= double.Parse(staExhMod.Text);
                maxThreshold *= double.Parse(staExhMod.Text);
            }
            else
            {
                double affinity = double.Parse(staAffinity.Text) / 100;
                statusPerHit = Math.Floor(status * (1 + affinity * 0.2));
                statusTotal = statusPerHit * hitCount;
            }

            hitsToInitThreshold = (int)Math.Ceiling(initThreshold / statusTotal);
            hitsToIncThreshold = (int)Math.Ceiling(incThreshold / statusTotal);
            hitsToMaxThreshold = (int)Math.Ceiling(maxThreshold / statusTotal);

            statusPrintOut(statusPerHit, statusTotal, hitsToInitThreshold, hitsToIncThreshold, hitsToMaxThreshold);
        }

        private void statusPrintOut(double statusPerHit, double statusTotal, int hitsToInitThreshold, int hitsToIncThreshold, int hitsToMaxThreshold)
        {
            staPrint.Clear();
            if (statusPerHit == 0)
            {
                staPrint.AppendText("No status damage was dealt in this situation.");
                return;
            }

            string[] formatArray = new string[] { statusPerHit.ToString(), statusTotal.ToString() };
            string formatString = String.Format("This attack will deal {1} status damage ({0} per hit)." + Environment.NewLine, formatArray);
            staPrint.AppendText(formatString);

            if (hitsToIncThreshold == 0 && hitsToInitThreshold == 0 && hitsToMaxThreshold == 0)
            {
                staPrint.AppendText("This monster cannot be affected by this status.");
                return;
            }

            formatArray = new string[] { hitsToInitThreshold.ToString(), hitsToIncThreshold.ToString(), hitsToMaxThreshold.ToString() };
            formatString = String.Format("It will take {0} attacks(s) to get to the initial threshold, taking {1} more attacks(s) each time the threshold is increased. It will take {2} attacks(s) to reach the threshold at maximum.", formatArray);
            staPrint.AppendText(formatString);
        }

        private void ExportStatus()
        {
            if (stats.statusCrit)
            {
                staCritCheck.Checked = true;
                staAffinity.Text = stats.positiveAffinity.ToString();
            }
            else
            {
                staCritCheck.Checked = false;
                staAffinity.Text = "0";
            }
            staHitCount.Text = stats.hitCount.ToString();

            if (isStatus(stats.altDamageType) || stats.altDamageType == "Blast")
            {
                staType.SelectedItem = stats.altDamageType;
                staPower.Text = stats.eleAttackPower.ToString();
            }
            else
            {
                staType.SelectedIndex = 0;
                staPower.Text = "0";
            }

            staKOZone.Text = stats.KOHitzone.ToString();
            staExhZone.Text = stats.exhaustHitzone.ToString();

            staKOMod.Text = stats.KOQuestMod.ToString();
            staExhMod.Text = stats.exhaustMod.ToString();
        }

        private void monsterSearch_TextChanged(object sender, EventArgs e)
        {
            if (staMonSearch.Text != monsterSearch.Text)
            {
                staMonSearch.Text = monsterSearch.Text;
            }
            monsterList.Items.Clear();
            if (monsterSearch.Text != "")
            {
                foreach (ListViewItem item in allMonsters)
                {
                    if ((item.SubItems[1].Text).Contains(monsterSearch.Text) || (item.SubItems[1].Text.ToLower()).Contains(monsterSearch.Text))
                    {
                        monsterList.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (ListViewItem item in allMonsters)
                {
                    monsterList.Items.Add(item);
                }
            }

            if(monsterList.Items.Count > 0)
            {
                monsterList.Items[0].Selected = true;
            }
            
        }

        private void staMonSearch_TextChanged(object sender, EventArgs e)
        {
            if (staMonSearch.Text != monsterSearch.Text)
            {
                monsterSearch.Text = staMonSearch.Text;
            }
            staMonsterList.Items.Clear();
            if (staMonSearch.Text != "")
            {
                foreach (ListViewItem item in staMonsters)
                {
                    if ((item.SubItems[1].Text).Contains(staMonSearch.Text) || (item.SubItems[1].Text.ToLower()).Contains(staMonSearch.Text))
                    {
                        staMonsterList.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (ListViewItem item in staMonsters)
                {
                    staMonsterList.Items.Add(item);
                }
            }

            if(staMonsterList.Items.Count > 0)
            {
                staMonsterList.Items[0].Selected = true;
            }
        }

        private void addDamageHistory(int mode)
        {
            int index = 0;
            ListViewItem damageHistoryEntry = new ListViewItem("0");

            damageHistoryEntry.Tag = formatDamageHistory(mode);
            damageHistory.Items.Insert(0, damageHistoryEntry);
            foreach(ListViewItem item in damageHistory.Items)
            {
                item.Text = index.ToString();
                index++;
            }
            while (damageHistory.Items.Count > 25)
            {
                damageHistory.Items.RemoveAt(25);
            }
        }

        private void addStatusHistory()
        {
            int index = 0;
            ListViewItem statusHistoryEntry = new ListViewItem("0");

            statusHistoryEntry.Tag = formatStatusHistory();
            statusHistory.Items.Insert(0, statusHistoryEntry);
            foreach (ListViewItem item in statusHistory.Items)
            {
                item.Text = index.ToString();
                index++;
            }
            while (statusHistory.Items.Count > 25)
            {
                statusHistory.Items.RemoveAt(25);
            }
        }

        private string formatDamageHistory(int mode)
        {
            if (mode == 1)
            {
                string[] formatArray = grabParameters(mode);
                string formatString = String.Format("{24}\\n\\nCalculation type: {0}\\n\\nFixed Damage? {1}" +
                    "\\nCritical Boost? {2}\\nMind’s Eye? {3}\\nStatus Crit? {4}\\nRueful Crit? {5}" +
                    "\\n\\nWeapon Raw: {6}\\nSharpness Modifier (Raw): {7}\\nAlternate Damage: {8} {9}" +
                    "\\nSharpness Modifier (Element Only): {10}\\nElemental Crit? {11}\\nDB Second Damage: {12} {13}" +
                    "\\nAffinity: -{14}/{15}%\\nHit Count: {16}\\nAverage Motion Value/Hit: {17}\\nKO/Hit: {18}" +
                    "\\nExhaust/Hit {19}\\n\\nEffective Weapon Raw: {20}\\nEffective Raw Damage: {21}" +
                    "\\nEffective Alternate Damage: {22}\\nEffective Secondary Damage: {23}", formatArray);
                return formatString;
            }
            else if (mode == 2)
            {
                string[] formatArray = grabParameters(mode);
                string formatString = String.Format("{40}\\n\\nCalculation type: {0}\\n\\nFixed Damage? {1}" +
                    "\\nCritical Boost? {2}\\nMind’s Eye? {3}\\nStatus Crit? {4}\\nRueful Crit? {5}" +
                    "\\n\\nWeapon Raw: {6}\\nSharpness Modifier (Raw): {7}\\nAlternate Damage: {8} {9}" +
                    "\\nSharpness Modifier (Element Only): {10}\\nElemental Crit? {11}" +
                    "\\nDB Second Damage: {12} {13}\\nAffinity: -{14}/{15}%\\nHit Count: {16}" +
                    "\\nAverage Motion Value/Hit: {17}\\nKO/Hit: {18}\\nExhaust/Hit {19}" +
                    "\\n\\nHitzone Value: {20}\\nElemental Hitzone Value: {21}" +
                    "\\nSecondary Element Hitzone Value: {22}\\nKO Hitzone Value: {23}" +
                    "\\nExhaust Hitzone Value: {24}\\nQuest Defense Modifier: {25}\\nMonster Health: {26}" +
                    "\\nMonster Status: {27}\\nG-Rank? {28}\\n\\nEffective Weapon Raw: {29}" +
                    "\\nEffective Raw Damage: {30}\\nEffective Alternate Damage: {31}" +
                    "\\nEffective Secondary Damage: {32}\\n\\nFinal Damage Dealt: {33}\\nBounce? {34}" +
                    "\\nRaw Damage/Hit: {35}\\nAlternate Damage/Hit: {36}\\nSecondary Damage/Hit: {37}" +
                    "\\nKO Damage/Hit: {38}\\nExhaust Damage/Hit: {39}", formatArray);

                return formatString;
            }
            else
            {
                throw new Exception();
            }
        }

        private string formatStatusHistory()
        {
            string[] formatArray = grabParameters(3);
            string formatString = String.Format("{12}\\n\\nStatus Crit? {0}\\nAffinity: {1} (Only positive Affinity applies to Status Crit)" +
                "\\n\\nHit Count: {2}\\nStatus Type: {3} {4}\\n\\nKO Hitzone: {5}" +
                "\\nExhaust Hitzone: {6}\\nInitial Threshold: {7}\\nIncrease Threshold: {8}" +
                "\\nMaximum Threshold: {9}\\nKO Quest Modifier: {10}" +
                "\\nExhaust Quest Modifier: {11} (Note: Hyper Monsters can’t be Exhausted)", formatArray);
            return formatString;
        }

        private string[] grabParameters(int mode)
        {
            List<string> parameters = new List<string>();
            if (mode == 1 || mode == 2)
            {
                
                if (calcAverage.Checked)
                {
                    parameters.Add("Average");
                }
                else if (calcPositive.Checked)
                {
                    parameters.Add("Positive");
                }
                else if (calcNegative.Checked)
                {
                    parameters.Add("Negative");
                }
                else if (calcNeutral.Checked)
                {
                    parameters.Add("Neutral");
                }

                if (paraFixed.Checked)
                {
                    parameters.Add("Yes");
                }
                else
                {
                    parameters.Add("No");
                }

                if (paraCritBoost.Checked)
                {
                    parameters.Add("Yes");
                }
                else
                {
                    parameters.Add("No");
                }

                if (paraMinds.Checked)
                {
                    parameters.Add("Yes");
                }
                else
                {
                    parameters.Add("No");
                }

                if (paraStatusCrit.Checked)
                {
                    parameters.Add("Yes");
                }
                else
                {
                    parameters.Add("No");
                }

                if (paraMadAff.Checked)
                {
                    parameters.Add("Yes");
                }
                else
                {
                    parameters.Add("No");
                }

                parameters.Add(paraRaw.Text);
                parameters.Add(paraRawSharp.Text);
                parameters.Add((string)paraAltType.SelectedItem);
                parameters.Add(paraElePower.Text);
                parameters.Add(paraEleSharp.Text);
                parameters.Add((string)paraEleCrit.SelectedItem);

                parameters.Add((string)paraSecEle.SelectedItem);
                parameters.Add(paraSecPower.Text);

                if (paraChaotic.Checked)
                {
                    parameters.Add(paraNegAff.Text);
                    parameters.Add(paraPosAff.Text);
                }
                else
                {
                    if (int.Parse(paraAffinity.Text) > 0)
                    {
                        parameters.Add("0");
                        parameters.Add(paraAffinity.Text);
                    }
                    else
                    {
                        parameters.Add(paraAffinity.Text);
                        parameters.Add("0");
                    }
                }

                parameters.Add(paraHitCount.Text);
                parameters.Add(paraAvgMV.Text);
                parameters.Add(paraKO.Text);
                parameters.Add(paraExh.Text);

                if (mode == 2)
                {
                    parameters.Add(paraHitzone.Text);
                    parameters.Add(paraEleHit.Text);
                    parameters.Add(paraSecZone.Text);
                    parameters.Add(paraKOZone.Text);
                    parameters.Add(paraExhZone.Text);
                    parameters.Add(paraQuestMod.Text);

                    parameters.Add(paraHealth.Text);
                    parameters.Add(paraMonStatus.Text);
                    if (paraGRank.Checked)
                    {
                        parameters.Add("Yes");
                    }
                    else
                    {
                        parameters.Add("No");
                    }
                }

                parameters.Add(calcRawWeap.Text);
                parameters.Add(calcRawOut.Text);
                parameters.Add(calcEleOut.Text);
                parameters.Add(calcSecOut.Text);

                if(mode == 2)
                {
                    parameters.Add(calcFinal.Text);
                    parameters.Add(calcBounce.Text);
                    parameters.Add(calcRawAll.Text);
                    parameters.Add(calcEleAll.Text);
                    parameters.Add(calcSecAll.Text);
                    parameters.Add(calcKOAll.Text);
                    parameters.Add(calcExhAll.Text);
                }

                parameters.Add(calcDetails.Text);

                return parameters.ToArray();
            }
            else if (mode == 3)
            {
                if (staCritCheck.Checked)
                {
                    parameters.Add("Yes");
                }
                else
                {
                    parameters.Add("No");
                }

                parameters.Add(staAffinity.Text);
                parameters.Add(staHitCount.Text);
                parameters.Add(staType.Text);
                parameters.Add(staPower.Text);
                parameters.Add(staKOZone.Text);
                parameters.Add(staExhZone.Text);
                parameters.Add(staInit.Text);
                parameters.Add(staInc.Text);
                parameters.Add(staMax.Text);
                parameters.Add(staKOMod.Text);
                parameters.Add(staExhMod.Text);
                parameters.Add(staPrint.Text);

                return parameters.ToArray();
            }

            throw new Exception();
        }

        private void damageHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(damageHistory.SelectedItems.Count != 0)
            {
                historyDetails.Text = ((string)damageHistory.SelectedItems[0].Tag).Replace("\\n", Environment.NewLine);
            }
        }

        private void statusHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (statusHistory.SelectedItems.Count != 0)
            {
                historyDetails.Text = ((string)statusHistory.SelectedItems[0].Tag).Replace("\\n", Environment.NewLine);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedIndex == 4)
            {
                ImportSetUp();
                ImportModifiers();
                Export();
            }
            else if(tabControl1.SelectedIndex == 5)
            {
                ImportSetUp();
                ImportModifiers();
                ExportStatus();
            }
        }
    }


    internal class weaponStorage
    {
        public List<string> families = new List<string>();
        public List<string> weapons = new List<string>();
        public Dictionary<string, List<string>> familyWeapons = new Dictionary<string, List<string>>();
        public Dictionary<string, weapon> weaponDetails = new Dictionary<string, weapon>();

        /// <summary>
        /// Adds one family to the weaponStorage.
        /// </summary>
        /// <param name="family"></param>
        public void addFamily(List<weapon> family)
        {
            List<string> newWeapons = new List<string>();
            string[] firstName = family[0].name.Split(new string[] { " Lv. " }, StringSplitOptions.None);
            string familyName = firstName[0];

            families.Add(familyName);

            foreach (weapon newWeap in family)
            {
                weapons.Add(newWeap.name);
                newWeapons.Add(newWeap.name);
                weaponDetails.Add(newWeap.name, newWeap);
            }
            familyWeapons.Add(familyName, newWeapons);
        }

        /// <summary>
        /// Gets all weapons in the order they were added, by ID.
        /// </summary>
        /// <returns></returns>
        public List<weapon> getWeapons() //gets all the details of the weapons in the order they were added, that is, by ID
        {
            List<weapon> allWeapons = new List<weapon>();
            foreach (string allWeaps in weapons)
            {
                allWeapons.Add(weaponDetails[allWeaps]);
            }
            return allWeapons;
        }

        /// <summary>
        /// Gets all weapon details specified in names.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public List<weapon> getWeapons(List<string> names)
        {
            List<weapon> chosenWeapons = new List<weapon>();
            foreach (string name in names)
            {
                chosenWeapons.Add(weaponDetails[name]);
            }

            return chosenWeapons;
        }

        /// <summary>
        /// Gets a family's details.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<weapon> getFamilyDetails(string name) //gets a single family's details.
        {
            List<string> family = familyWeapons[name];
            List<weapon> weapons = new List<weapon>();
            foreach (string familyWeapon in family)
            {
                weapons.Add(weaponDetails[familyWeapon]);
            }

            return weapons;
        }

        public List<string> getFamilies(List<string> familyNames)
        {
            List<string> weapons = new List<string>();
            foreach (string familyName in familyNames)
            {
                foreach (string weaponName in familyWeapons[familyName])
                {
                    weapons.Add(weaponName);
                }
            }

            return weapons;
        }

        /// <summary>
        /// Gets all of the families' final details
        /// </summary>
        /// <returns></returns>
        public List<string> getFinal()
        {
            return getFinal(families);
        }

        /// <summary>
        /// Gets all the details of the final upgrades in the family.
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        public List<string> getFinal(List<string> family) //gets the details of the final upgrades
        {
            List<string> finals = new List<string>();
            foreach (string str in family)
            {
                int last = familyWeapons[str].Count;
                string lastWeap = familyWeapons[str][last - 1];
                finals.Add(lastWeap);
            }

            return finals;
        }

        public List<string> filterWeapons(bool[] filter)
        {
            List<string> filtered = new List<string>();
            foreach (string familyName in families)
            {
                weapon familyWeapon = weaponDetails[familyWeapons[familyName][0]];
                string familyElement = familyWeapon.element;
                string familySecond = familyWeapon.elementTwo;

                if (filter[0])
                {
                    if (familyElement == "Fire" || familySecond == "DB - Fire")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[1])
                {
                    if (familyElement == "Water" || familySecond == "DB - Water")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[2])
                {
                    if (familyElement == "Thunder" || familySecond == "DB - Thunder")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[3])
                {
                    if (familyElement == "Ice" || familySecond == "DB - Ice")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[4])
                {
                    if (familyElement == "Dragon" || familySecond == "DB - Dragon")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[5])
                {
                    if (familyElement == "Poison" || familySecond == "DB - Poison")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[6])
                {
                    if (familyElement == "Para" || familySecond == "DB - Para")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[7])
                {
                    if (familyElement == "Sleep")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[8])
                {
                    if (familyElement == "Blast" || familySecond == "DB - Blast")
                    {
                        filtered.Add(familyName);
                    }
                }

                if (filter[9])
                {
                    if (familyElement == "(No Element)")
                    {
                        filtered.Add(familyName);
                    }
                }
            }

            return filtered;
        }


        /// <summary>
        /// Resets everything in weaponStorage.
        /// </summary>
        public void Clear() //resets everything, for use in switching weapon classes
        {
            families.Clear();
            weapons.Clear();
            familyWeapons.Clear();
            weaponDetails.Clear();
        }
    }

    internal class weapon
    {
        public int ID;
        public string name;
        public int raw;
        public string element = "(No Element)";
        public int eleValue = 0;
        public string elementTwo = "(No Element)";
        public int eleValueTwo = 0;
        public string displayAffinity;
        public int positiveAffinity;
        public int negativeAffinity;
        public string sharpness = "(No Sharpness)";
        public string sharpnessOne = "(No Sharpness)";
        public string sharpnessTwo = "(No Sharpness)";
    }

    /// <summary>
    /// Stores the relevant variables from the database portion of the application to
    /// import to the calculator portion.
    /// No ctor. Will be filled in when the UpdateButt is clicked.
    /// </summary>
    internal class ImportedStats
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
        public double KOPower;
        public double exhaustPower;

        public string secElement;
        public double secPower;

        public string damageType;
        public double hitzone;
        public double eleHitzone;
        public double secHitzone;
        public double questMod;
        public double KOHitzone;
        public double KOQuestMod;
        public double exhaustHitzone;
        public double exhaustMod;
        public double health; //Stores the monster's health.

        public double rawMod = 1; //Stores the multiplier of the raw damage.
        public double eleMod = 1; //Stores the elemental multiplier. Has a cap of 1.2x, surpassed when used Demon Riot on an Element Phial SA.
        public double secMod = 1; //Stores the elemental multiplier for the second element. Has same restrictions as above.
        public double expMod = 1; //Stores the explosive multiplier. Has a cap of 1.3x, 1.4x when considering Impact Phial CB.
        public double staMod = 1; //Stores the status multiplier. Has a cap of 1.25x, surpassed when using Demon Riot on a Status Phial SA.
        public double staSecMod = 1;
        public double KOMod = 1;
        public double ExhMod = 1;

        public bool chaotic; //Shows whether or not a Chaotic Gore weapon is being used.
        public bool CB = false; //Shows whether or not the explosive multiplier should be increased because Impact Phials are being used. 
        public bool GL = false; //Shows whether or not the explosive multiplier should be unlimited because GL is being used
        public bool DemonRiot = false; //Shows whether or not Demon Riot is being used.
        public bool ruefulCrit = false;

        public int addRaw = 0; //Stores the additive portion of raw
        public int addElement = 0; //Stores the additive portion of element after Atk +1 or +2
        public int addSecElement = 0; //Stores the additive portion of element after Atk +1 or +2
        public int addStatus = 0; //Stores the additive portion of status after Atk +1 or +2
        public int addSecStatus = 0; //Stores the addition portion of status after Atk+1 or +2
        public bool criticalBoost = false;
        public bool mindsEye;
        public int eleCrit = 0;
        public bool statusCrit = false;

        public string monsterStatus;
        public bool GRank;


        public Dictionary<string, Tuple<double, double>> sharpnessValues = new Dictionary<string, Tuple<double, double>>();

        public ImportedStats()
        {
            sharpnessValues.Add("(No Sharpness)", new Tuple<double, double>(1.00, 1.00));
            sharpnessValues.Add("Purple", new Tuple<double, double>(1.39, 1.20));
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
