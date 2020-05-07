
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Threading;
using VDS.RDF;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataManager dm;

        public MainWindow()
        {
            InitializeComponent();

            dm = new DataManager();
            InitItemsSources();

            Task.Factory.StartNew(() => SystemWorkImitation());
        }

        public void InitItemsSources()
        {
            triplesGrid1.ItemsSource = null;
            triplesGrid1.ItemsSource = dm.Triples;
            triplesGrid1.Items.Refresh();


            ListView.ItemsSource = null;
            ListView.ItemsSource = dm.counts;
            ListView.Items.Refresh();

            ListViewNamespace.ItemsSource = null;
            ListViewNamespace.ItemsSource = dm.prefNamespaces;


            PropTriplePredicate.ItemsSource = null;
            PropTriplePredicate.ItemsSource = dm.properties;
            PropTriplePredicate.Items.Refresh();
 
            PropTripleSubject.ItemsSource = null;
            PropTripleSubject.ItemsSource = dm.objects;
            PropTripleSubject.Items.Refresh();

            PropTripleObject.ItemsSource = null;
            PropTripleObject.ItemsSource = dm.objects;
            PropTripleObject.Items.Refresh();

            ObjectClass.ItemsSource = null;
            ObjectClass.ItemsSource = dm.classes;
            ObjectClass.Items.Refresh();

            ObjectSubClass.ItemsSource = null;
            ObjectSubClass.ItemsSource = dm.classes;
            ObjectSubClass.Items.Refresh();
            
            
        }


        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {

            if (!InputChecker.isValidInput(SubjectMainClass.Text))
            {
                MessageBox.Show( "Invalid input", "Error");
                return;
            }
            if (dm.graph.GetTriplesWithSubjectPredicate(dm.graph.CreateUriNode("mc:" + SubjectMainClass.Text),
                    dm.graph.CreateUriNode("rdfs:subClassOf")).Any() || dm.graph.GetTriplesWithSubjectObject(dm.graph.CreateUriNode("mc:" + SubjectMainClass.Text),
                    dm.graph.CreateUriNode("rdfs:Class")).Any())
            {
                MessageBox.Show("Class " + SubjectMainClass.Text + " already exists", "Error");
                return;
            }
            dm.AddTriple("mc:" + SubjectMainClass.Text, "rdf:type", "rdfs:Class");
            MessageBox.Show("Success!", "Triple asserted!");
            InitItemsSources();
        }
        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            SubjectMainClass.Clear();
        }


        private void ResetSubclassButton_OnClick(object sender, RoutedEventArgs e)
        {
            ObjectSubClass.Text = "";
            SubjectSubClass.Clear();
        }

        private void AddSubclassButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!InputChecker.isValidInput(SubjectSubClass.Text, ObjectSubClass.Text))
            {
                MessageBox.Show("Invalid input", "Error");
                return;
            }

            //if (!dm.graph.GetTriplesWithSubjectObject(dm.graph.CreateUriNode("mc:"+ObjectSubClass.Text),
            //    dm.graph.CreateUriNode("rdfs:Class")).Any() || !dm.graph.GetTriplesWithSubjectPredicate(dm.graph.CreateUriNode("mc:" + ObjectSubClass.Text),
            //        dm.graph.CreateUriNode("rdfs:subClassOf")).Any())
            //{
            //    MessageBox.Show("Class "+ObjectSubClass.Text+" does not exist","Error" );
            //    return;
            //}
            if (dm.graph.GetTriplesWithSubjectPredicate(dm.graph.CreateUriNode("mc:" + SubjectSubClass.Text), 
                dm.graph.CreateUriNode("rdfs:subClassOf")).Any() || dm.graph.GetTriplesWithSubjectObject(dm.graph.CreateUriNode("mc:" + SubjectSubClass.Text),
                    dm.graph.CreateUriNode("rdfs:Class")).Any())
            {
                MessageBox.Show( "Class " + SubjectSubClass.Text + " already exists", "Error");
                return;
            }


            dm.AddTriple("mc:" + SubjectSubClass.Text, "rdfs:subClassOf", "mc:" + ObjectSubClass.Text);
            MessageBox.Show("Success!", "Triple asserted!");
            InitItemsSources();
        }

        private void AddPropertyButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!InputChecker.isValidInput(PropertySubject.Text))
            {
                MessageBox.Show("Invalid input","Error");
                return;
            }

            if (dm.properties.Contains(PropertySubject.Text))
            {
                MessageBox.Show( "This property already exists", "Error");
                return;
            }

            dm.properties.Add(PropertySubject.Text);
            dm.AddTriple("mp:" + PropertySubject.Text, "rdf:type", "rdf:Property");
            MessageBox.Show("Success!", "Triple asserted!");
            InitItemsSources();

        }

        private void ResetPropertyButton_OnClick(object sender, RoutedEventArgs e)
        {
            PropertySubject.Clear();
        }

        private void AddPropTripleButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!InputChecker.isValidInput(PropTripleSubject.Text,PropTripleObject.Text,PropTriplePredicate.Text))
            {
                MessageBox.Show("Invalid input", "Error");
                return;
            }

            if (PropTripleSubject.Text == PropTripleObject.Text)
            {
                MessageBox.Show("You can not add triple with equal Subject and Object", "Error");
                return;
            }

            if (dm.Triples.Contains(new Tuple<string, string, string>("Object:" + PropTripleSubject.Text,
                PropTriplePredicate.Text, "Object:" + PropTripleObject.Text)))
            {
                MessageBox.Show("This triple already exists", "Error");
                return;
            }
            if(PropTriplePredicate.Text == "whatFor")
                dm.AddTriple("mo:" + PropTripleSubject.Text, "wikiProperty:P1542","mo:"+PropTripleObject.Text );
            else if (PropTriplePredicate.Text == "causesWhy")
                dm.AddTriple("mo:" + PropTripleSubject.Text, "wikiProperty:P2868", "mo:"+PropTripleObject.Text );
            else 
                dm.AddTriple("mo:" + PropTripleSubject.Text,"mp:"+PropTriplePredicate.Text,"mo:"+PropTripleObject.Text );

            MessageBox.Show("Success!", "Triple asserted!");
            InitItemsSources();

        }

        private void ResetPropTripleButton_OnClick(object sender, RoutedEventArgs e)
        {
            PropTripleObject.Text = "";
            PropTriplePredicate.Text = "";
            PropTripleSubject.Text= "";
        }

        private void AddIndividButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!InputChecker.isValidInput(SubjectIndivid.Text, ObjectClass.Text))
            {
                MessageBox.Show("Invalid input", "Error");
                return;
            }

            if (dm.objects.Contains(SubjectIndivid.Text))
            {
                MessageBox.Show("This object already exists", "Error");
                return;
            }

            dm.AddTriple("mo:" + SubjectIndivid.Text, "rdf:type", "mc:"+ObjectClass.Text);
            MessageBox.Show("Success!", "Triple asserted!");
            InitItemsSources();
        }

        private void ResetIndividButton_OnClick(object sender, RoutedEventArgs e)
        {
            SubjectIndivid.Clear();
            ObjectClass.Text = "";
        }
        
        public void WriteToTextBlock(string message)
        {
            TextBlock1.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(() => { TextBlock1.AppendText(message + "\n"); }));
        }

        public void SystemWorkImitation()
        {
            Task experiments = new Task(() => Experiments());
            Task monitoringSystem = new Task(() => MonitoringSystem());
            Task formulasComponent = new Task(() => FormulasComponent());
            Task expAnalys = new Task(() => ExperienceAnalysis());
            Task neiroSet = new Task(() => NeiroSet());

            Wait();
            Wait();
            
            monitoringSystem.Start();
            monitoringSystem.Wait();
            
            expAnalys.Start();
            expAnalys.Wait();
            
            experiments.Start();
            experiments.Wait();
            
            neiroSet.Start();
            neiroSet.Wait();
            
            formulasComponent.Start();
            formulasComponent.Wait();

        }

        public void NeiroSet()
        {
            WriteToTextBlock("Переобучение нейросетей на основе полученных после проведения экспериментов данных...");
            Wait(); 
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("Отправка результата работы алгоритмов нейросетей в базу знаний...");
            Wait();
            
            
        }
        
        public void FormulasComponent()
        {
            WriteToTextBlock("Запрос расчета действия у компонента формул...");
            Wait();
            WriteToTextBlock("Расчеты действия...");
            Wait();
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("Отправка результатов работы компонента формул в базу знаний...");
            Wait();
            WriteToTextBlock("Отправка команды на САУ...");
            Wait();
            WriteToTextBlock("Обработка команды в физический сигнал...");
            Wait();
            WriteToTextBlock("Отправка сигнала на микроконтроллеры...");
            Wait();
            
            
        }
        public void Experiments()
        {
            WriteToTextBlock("Работа компонента экспериментов...");
            Wait();
            WriteToTextBlock("Запрос из базы знаний правил проведения экспериментов...");
            Wait();
            WriteToTextBlock("Получены правила проведения экспериментов.");
            WriteToTextBlock("Проведение экспериментов...");
            Wait();
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("...");
            Wait();
            WriteToTextBlock("Получены результаты экспериментов.");
            WriteToTextBlock("Отправка полученных данных в базу данных...");
            Wait();
            WriteToTextBlock("Отправка новых данных из базы данных в нейросетевой компонент...");
            Wait();
        }
        
        public void ExperienceAnalysis()
        {
            WriteToTextBlock("Запрос системы отслеживания к базе данных.");
            Wait();
            WriteToTextBlock("Получен ответ от базы данных.");
            Wait();
            WriteToTextBlock("Работа компонента отслеживания...");
            Wait();
            WriteToTextBlock("Анализ данных...");
            Wait();
            Wait();
            WriteToTextBlock("Выявлены неполадки в системе!");
            WriteToTextBlock("Работа компонента вычислителя...");
            Wait();
            Wait();
            
            WriteToTextBlock("Запуск компонента экспериментов...");
        }
        public void MonitoringSystem()
        {
            WriteToTextBlock("Запрос KPI из базы знаний...");
            Wait();
            WriteToTextBlock("Получен KPI.");
            
            WriteToTextBlock("Запрос новых данных из базы данных...");
            Wait();
            WriteToTextBlock("Получены данные из базы данных.");
            Wait();
            
            WriteToTextBlock("Работа алгоритма сравнения показателей...");
            Wait();
            Wait();
            
            WriteToTextBlock("Конец работа алгоритма сравнения показателей.");
            
            WriteToTextBlock("Отправка результата работы алгоритма в базу данных...");
            Wait();
            WriteToTextBlock("Получены данные из базы данных.");
            Wait();
            
        }

        public void Wait()
        {
            Random random = new Random();
            var temp =random.Next(1000, 5000);
            Thread.Sleep(temp);
        }
        
    }
}
