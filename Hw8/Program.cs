using System;
using System.Collections;
using System.Collections.Generic;

namespace Hw8
{
    internal class Program
    {
        //proejct
        enum ProjectStatus
        {
            Draft,     
            InProgress,  
            Closed   
        }

        enum TaskStatus
        {
            Assigned,  
            InProgress, 
            OnReview,  
            Done     
        }

        class Person
        {
            public string Name { get; }
            public List<ProjectTask> MyTasks { get; } = new List<ProjectTask>();

            public Person(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Имя не может быть пустым");
                Name = name;
            }

            public void AddTask(ProjectTask t) { MyTasks.Add(t); }
            public void RemoveTask(ProjectTask t) { MyTasks.Remove(t); }

            public override string ToString() { return Name; }
        }

         class Report
        {
            public string Text { get; }
            public DateTime Date { get; }
            public Person Executor { get; }

            public Report(string text, Person executor)
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentException("Текст отчёта не может быть пустым");
                if (executor == null)
                    throw new ArgumentNullException(nameof(executor));

                Text = text;
                Date = DateTime.Now;
                Executor = executor;
            }

            public override string ToString()
            {
                return $"[{Date:dd.MM.yyyy HH:mm}] {Executor}: {Text}";
            }
        }

        class ProjectTask
        {
            public string Description { get; }
            public DateTime Deadline { get; }
            public Person Initiator { get; }   
            public Person Executor { get; private set; }
            public TaskStatus Status { get; private set; }
            public Report Report { get; private set; } 

            public ProjectTask(string description, DateTime deadline, Person initiator)
            {
                if (string.IsNullOrWhiteSpace(description))
                    throw new ArgumentException("Описание задачи не может быть пустым");
                if (initiator == null)
                    throw new ArgumentNullException(nameof(initiator));

                Description = description;
                Deadline = deadline;
                Initiator = initiator;
                Status = TaskStatus.Assigned;
                Executor = null;
                Report = null;
            }

            public void AssignTo(Person p)
            {
                if (p == null)
                    throw new ArgumentNullException(nameof(p));
                if (Status != TaskStatus.Assigned)
                    throw new InvalidOperationException("Назначить можно только в статусе Assigned");
                if (Executor != null)
                    throw new InvalidOperationException("У задачи уже есть исполнитель");

                Executor = p;
                p.AddTask(this);
            }

            public void TakeToWork(Person p)
            {
                if (p != Executor)
                    throw new InvalidOperationException("Только исполнитель может взять задачу");
                if (Status != TaskStatus.Assigned)
                    throw new InvalidOperationException("Взять в работу можно только из Assigned");

                Status = TaskStatus.InProgress;
            }

            public void DelegateTo(Person newExecutor)
            {
                if (newExecutor == null)
                    throw new ArgumentNullException(nameof(newExecutor));
                if (Status != TaskStatus.Assigned)
                    throw new InvalidOperationException("Делегировать можно только из Assigned");
                if (newExecutor == Executor)
                    throw new InvalidOperationException("Нельзя делегировать самому себе");

                if (Executor != null)
                    Executor.RemoveTask(this);

                Executor = newExecutor;
                newExecutor.AddTask(this);
            }

            public void Reject(Person p)
            {
                if (p != Executor)
                    throw new InvalidOperationException("Только исполнитель может отклонить задачу");
                if (Status != TaskStatus.Assigned)
                    throw new InvalidOperationException("Отклонить можно только из Assigned");

                Executor.RemoveTask(this);
                Executor = null;
            }

            public void SubmitReport(string text)
            {
                if (Status != TaskStatus.InProgress)
                    throw new InvalidOperationException("Отчёт — только из InProgress");
                if (Report != null)
                    throw new InvalidOperationException("Отчёт уже есть (один на задачу)");

                Report = new Report(text, Executor);
                Status = TaskStatus.OnReview;
            }

            public void ApproveReport(Person byWhom)
            {
                if (byWhom != Initiator)
                    throw new InvalidOperationException("Утверждает только инициатор задачи");
                if (Status != TaskStatus.OnReview)
                    throw new InvalidOperationException("Утвердить можно только из OnReview");
                if (Report == null)
                    throw new InvalidOperationException("Нет отчёта");

                Status = TaskStatus.Done;
            }

            public void ReturnForRevision(Person byWhom)
            {
                if (byWhom != Initiator)
                    throw new InvalidOperationException("Возвращает только инициатор задачи");
                if (Status != TaskStatus.OnReview)
                    throw new InvalidOperationException("Вернуть можно только из OnReview");

                Report = null;
                Status = TaskStatus.InProgress;
            }

            public override string ToString()
            {
                string exec = Executor == null ? "(нет)" : Executor.Name;
                return $"\"{Description}\" | {Status} | исполнитель: {exec}";
            }
        }


        class Project
        {
            public string Description { get; }
            public DateTime Deadline { get; }
            public Person Customer { get; }  
            public Person TeamLead { get; }   
            public ProjectStatus Status { get; private set; }
            public List<ProjectTask> Tasks { get; } = new List<ProjectTask>();

            public Project(string description, DateTime deadline, Person customer, Person teamLead)
            {
                if (string.IsNullOrWhiteSpace(description))
                    throw new ArgumentException("Описание проекта не может быть пустым");
                if (customer == null) throw new ArgumentNullException(nameof(customer));
                if (teamLead == null) throw new ArgumentNullException(nameof(teamLead));

                Description = description;
                Deadline = deadline;
                Customer = customer;
                TeamLead = teamLead;
                Status = ProjectStatus.Draft;
            }

            public void AddTask(ProjectTask t, Person byWhom)
            {
                if (t == null) throw new ArgumentNullException(nameof(t));
                if (Status != ProjectStatus.Draft)
                    throw new InvalidOperationException("Задачи — только в статусе Draft");
                if (byWhom != TeamLead)
                    throw new InvalidOperationException("Задачи назначает только тимлид");

                Tasks.Add(t);
            }

            public void Start()
            {
                if (Status != ProjectStatus.Draft)
                    throw new InvalidOperationException("Запустить можно только из Draft");
                if (Tasks.Count == 0)
                    throw new InvalidOperationException("Нельзя запустить проект без задач");

                Status = ProjectStatus.InProgress;
            }

            public bool IsAllTasksDone()
            {
                foreach (ProjectTask t in Tasks)
                    if (t.Status != TaskStatus.Done)
                        return false;
                return true;
            }

            public void Close()
            {
                if (Status != ProjectStatus.InProgress)
                    throw new InvalidOperationException("Закрыть можно только из InProgress");
                if (!IsAllTasksDone())
                    throw new InvalidOperationException("Не все задачи выполнены");

                Status = ProjectStatus.Closed;
            }

            public void Print()
            {
                Console.WriteLine($"=== Проект: {Description} ===");
                Console.WriteLine($"Заказчик: {Customer}, Тимлид: {TeamLead}");
                Console.WriteLine($"Срок: {Deadline:dd.MM.yyyy}, Статус: {Status}");
                Console.WriteLine("Задачи:");
                foreach (ProjectTask t in Tasks)
                    Console.WriteLine("  " + t);
            }
        }
        // 9.1
        public enum AccType
        {
            saving,
            current
        }

        public class BankAccount : IDisposable
        {
            private Queue Transaction;
            private static int s_numberseed = 0;

            private string Number { get; set; }
            private decimal Balance { get; set; }
            private AccType accType { get; set; }

            public string number
            {
                get { return Number; }
            }

            public decimal balance
            {
                get { return Balance; }
                set { Balance = value; }
            }

            public AccType Type
            {
                get { return accType; }
                set { accType = value; }
            }

            public BankAccount(string number, decimal balance, AccType accType)
            {
                Number = s_numberseed + "";
                s_numberseed++;
                Balance = balance;
                this.accType = accType;
                Transaction = new Queue();
            }

            public BankAccount()
            {
                Number = "" + s_numberseed;
                s_numberseed++;
                Balance = 0;
                accType = AccType.current;
                Transaction = new Queue();
            }

            public void Print()
            {
                string nameType = "";
                if (accType == AccType.current)
                    nameType = "текущий";
                else
                    nameType = "сберегательный";

                Console.WriteLine($"Номер счета: {number}\nБаланс: {balance}\nТип счета: {nameType}");
            }

            public void MakeDeposit(decimal amount)
            {
                if (amount <= 0)
                    throw new ArgumentException("Сумма должна быть больше нуля");

                Balance += amount;
                Console.WriteLine($"Счет после вклада: {Balance}");

                BankTransaction tr = new BankTransaction(amount);
                Transaction.Enqueue(tr);
            }

            public void MakeWithdraw(decimal amount)
            {
                if (amount <= 0)
                    throw new ArgumentException("Сумма должна быть больше нуля");
                if (Balance - amount < 0)
                    throw new ArgumentException("Сумма превышает баланс");

                Balance -= amount;
                Console.WriteLine($"Счет после снятия: {Balance}");

                BankTransaction tr = new BankTransaction(-amount);
                Transaction.Enqueue(tr);
            }

            public void Transfer(BankAccount fromAccount, decimal amount)
            {
                if (fromAccount == null)
                    throw new ArgumentNullException(nameof(fromAccount), "Счёт-источник не может быть пустым");

                fromAccount.MakeWithdraw(amount);
                this.MakeDeposit(amount);
            }
            public void PrintHistory()
            {
                Console.WriteLine($"История операций по счёту {number}:");
                if (Transaction.Count == 0)
                {
                    Console.WriteLine("  (операций не было)");
                    return;
                }
                foreach (object o in Transaction)
                {
                    BankTransaction t = (BankTransaction)o;
                    Console.WriteLine("  " + t.getDA());
                }
            }
            //9.3
            public void Dispose()
            {
                string file = $"trHis_of_Acc{number}.txt";
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file))
                {
                    foreach (object o in Transaction)
                    {
                        BankTransaction t = (BankTransaction)o;
                    }
                }
                GC.SuppressFinalize(this);
            }

        }// 9.2
        public class BankTransaction
        {
            private readonly DateTime date;
            private readonly decimal amount;

            public DateTime Date { get { return date; } }
            public decimal Amount { get { return amount; } }

            public BankTransaction(decimal amount)
            {
                this.date = DateTime.Now;
                this.amount = amount;
            }

            public string getDA()
            {
                return $"{date:dd.MM.yyyy HH:mm:ss} : {amount}";
            }
        }
        //9.1hw
        public class Song
        {
            private string name;
            private string author;
            private Song prev;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public string Author
            {
                get { return author; }
                set { author = value; }
            }

            public Song Prev
            {
                get { return prev; }
                set { prev = value; }
            }
            public string Signature
            {
                get { return name + " - " + author; }
            }

            public Song(string name, string author)
            {
                this.name = name;
                this.author = author;
                this.prev = null;
            }

            public Song(string name, string author, Song prev)
            {
                this.name = name;
                this.author = author;
                this.prev = prev;
            }

            public Song()
            {
                this.name = "";
                this.author = "";
                this.prev = null;
            }

            public void SetName(string n) { name = n; }
            public void SetAuthor(string a) { author = a; }
            public void SetPrev(Song p) { prev = p; }

            public void Print()
            {
                Console.WriteLine($"\"{name}\" — {author}");
            }

            public string Title()
            {
                return name + " - " + author;
            }

            public override bool Equals(object d)
            {
                Song other = d as Song;
                if (other == null)
                    return false;
                return this.name == other.name && this.author == other.author;
            }

            public override int GetHashCode()
            {
                return (name + author).GetHashCode();
            }
            static void Main(string[] args)
            {
                BankAccount a = new BankAccount("0", 1000m, AccType.current);
                BankAccount b = new BankAccount();

                a.MakeDeposit(500m);
                a.MakeWithdraw(200m);
                b.MakeDeposit(300m);
                b.Transfer(a, 100m);

                Console.WriteLine('a');
                a.Print();
                a.PrintHistory();

                Console.WriteLine('b');
                b.Print();
                b.PrintHistory();

                Console.WriteLine("\nПопытка снять -50 с A:");
                try { a.MakeWithdraw(-50m); }
                catch (ArgumentException ex) { Console.WriteLine("Ошибка: " + ex.Message); }

                Console.ReadLine();
                //9.3
                Console.WriteLine('c');
                using (BankAccount c = new BankAccount())
                {
                    c.MakeDeposit(1000);
                    c.MakeWithdraw(500);
                    c.Print();
                    c.PrintHistory();
                }
                if (System.IO.File.Exists("history_0.txt"))
                {
                    Console.WriteLine(System.IO.File.ReadAllText("history_0.txt"));
                }
                
                //9.1hw

                Song s1 = new Song("A1", "B1");
                s1.Print();
                Console.WriteLine("Signature: " + s1.Signature);
                Console.WriteLine("Prev is null: " + (s1.Prev == null));

                Song s2 = new Song("A2", "B1", s1);
                s2.Print();
                Console.WriteLine("Signature: " + s2.Signature);
                Console.WriteLine("Prev is s1: " + (s2.Prev == s1));

                Song mySong = new Song();
                mySong.Print();
                Console.WriteLine("Signature: \"" + mySong.Signature + "\"");

                Console.WriteLine("\n=== Проверка вычисляемого Signature ===");
                Song s3 = new Song("A3", "B2");
                Console.WriteLine("До изменения name: " + s3.Signature);
                s3.Name = "A4";
                Console.WriteLine("После изменения name: " + s3.Signature);

                Song s4 = new Song("A1", "B1");
                Console.WriteLine("\ns1.Equals(s4): " + s1.Equals(s4)); 
                Console.WriteLine("s1.Equals(s2): " + s1.Equals(s2));

                //project

                Person[] team = new Person[10];
                for (int i = 0; i < 10; i++)
                    team[i] = new Person("P" + (i + 1));

                Person customer = team[0];
                Person teamLead = team[1];

                Project project = new Project(
                    "Proj1",
                    DateTime.Now.AddMonths(1),
                    customer,
                    teamLead
                );

                int[] executorIndexes = { 2, 3, 4, 5, 6, 7, 8, 9, 0, 1 };

                for (int i = 0; i < 10; i++)
                {
                    ProjectTask t = new ProjectTask(
                        "T" + (i + 1),
                        DateTime.Now.AddDays(7 + i),
                        teamLead
                    );
                    project.AddTask(t, teamLead);

                    t.AssignTo(team[executorIndexes[i]]);
                }

                Console.WriteLine("=== После создания и назначения ===");
                project.Print();

                project.Start();
                Console.WriteLine("\n=== Проект переведён в Исполнение ===");
                project.Print();

                Console.WriteLine("\n=== Выполнение задач ===");
                foreach (ProjectTask t in project.Tasks)
                {
                    Person exec = t.Executor;
                    Console.WriteLine($"\nЗадача {t.Description}, исполнитель {exec}");

                    bool returnForRevision = (t.Description == "T3");

                    t.TakeToWork(exec);
                    Console.WriteLine("  → В работе");

                    t.SubmitReport("Report for " + t.Description);
                    Console.WriteLine("  → Отчёт сдан, на проверке");

                    if (returnForRevision)
                    {
                        t.ReturnForRevision(t.Initiator);
                        Console.WriteLine("  → Возврат на доработку");

                        t.SubmitReport("Revised report for " + t.Description);
                        Console.WriteLine("  → Отчёт пересдан");
                    }

                    t.ApproveReport(t.Initiator);
                    Console.WriteLine("  → Выполнена");
                }

                Console.WriteLine("\n=== Все задачи выполнены? ===");
                Console.WriteLine(project.IsAllTasksDone());

                project.Close();
                Console.WriteLine("\n=== Проект закрыт ===");
                project.Print();

                Console.WriteLine("\n=== Проверка исключений ===");

                try
                {
                    ProjectTask extra = new ProjectTask("T11", DateTime.Now, teamLead);
                    project.AddTask(extra, teamLead);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("Ошибка 1: " + ex.Message);
                }

                try
                {
                    Person notLead = team[5];
                    ProjectTask extra = new ProjectTask("T12", DateTime.Now, notLead);
                    project.AddTask(extra, notLead);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("Ошибка 2: " + ex.Message);
                }


                Console.WriteLine("\n=== Задачи по людям ===");
                foreach (Person p in team)
                {
                    Console.WriteLine($"{p.Name}: задач {p.MyTasks.Count}");
                }
            }
        }
    }
}