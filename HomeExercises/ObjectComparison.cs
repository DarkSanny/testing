using System;
using System.CodeDom;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace HomeExercises
{
    public class ObjectComparison
    {
        [Test]
        [Description("Проверка текущего царя")]
        [Category("ToRefactor")]
        public void CheckCurrentTsar()
        {

            // Перепишите код на использование Fluent Assertions.
            actualTsar.Name.Should().Be(expectedTsar.Name);
            actualTsar.Age.Should().Be(expectedTsar.Age);
            actualTsar.Height.Should().Be(expectedTsar.Height);
            actualTsar.Weight.Should().Be(expectedTsar.Weight);

            actualTsar.Parent.Name.Should().Be(expectedTsar.Parent.Name);
            actualTsar.Parent.Age.Should().Be(expectedTsar.Parent.Age);
            actualTsar.Parent.Height.Should().Be(expectedTsar.Parent.Height);
            actualTsar.Parent.Parent.Should().Be(expectedTsar.Parent.Parent);

        }

        [Test]
        [Description("Альтернативное решение. Какие у него недостатки?")]
        public void CheckCurrentTsar_WithCustomEquality()
        {

            // Какие недостатки у такого подхода? 
            // Если тест упадет, мы не узнаем из результата какие свойства не совпадают
            // Но в отличает от CheckCurrentTsar тут учитывается связная структура класса Person
            // Если у родителя царя добавить родителя (создать новые экземпляры класса Person с одинаковыми параметрами в ожидаемом и актуальном результатах), 
            // то тест CheckCurrentTsar упадет из за id
            // В данном случае метод AreEqual рекурсивно проверяет всех родителей
            Assert.True(AreEqual(actualTsar, expectedTsar));
        }

        private bool AreEqual(Person actual, Person expected)
        {
            if (actual == expected) return true;
            if (actual == null || expected == null) return false;
            return
            actual.Name == expected.Name
            && actual.Age == expected.Age
            && actual.Height == expected.Height
            && actual.Weight == expected.Weight
            && AreEqual(actual.Parent, expected.Parent);
        }

        /*
         * Вариант 1
         * Разделение каждого свойства на отдельные методы позволяет видеть в результате свойство, на котором упал тест
         * Например если изменить свойство Age, то тест с именем SameAges упадет и будет видно различие между ожидаемым и полученным результатом
         * Если у объекта Person изменится конструктор, то достаточно исправить создание переменной expectedTsar в SetUp методе
         * Если у объекта Person увеличится количество свойств, то достаточно создать 1 тестовый метод, покрывающий это свойство
         * Писать для каждого свойства тестирующий метод слишком жирно (и тривиально), поэтому этот подход не годится.
         */
        private Person expectedTsar;
        private Person actualTsar;

        [SetUp]
        public void SetUp()
        {
            actualTsar = TsarRegistry.GetCurrentTsar();
            expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));
        }

        [Test]
        public void SameNames()
        {
            actualTsar.Name.Should().Be(expectedTsar.Name);
        }

        [Test]
        public void SameAges()
        {
            actualTsar.Age.Should().Be(expectedTsar.Age);
        }

        [Test]
        public void SameHeights()
        {
            actualTsar.Height.Should().Be(expectedTsar.Height);
        }

        [Test]
        public void SameWeights()
        {
            actualTsar.Weight.Should().Be(expectedTsar.Weight);
        }

        [Test]
        public void SameParentsNames()
        {
            actualTsar.Parent.Name.Should().Be(expectedTsar.Parent.Name);
        }

        [Test]
        public void SameParentsAges()
        {
            actualTsar.Parent.Age.Should().Be(expectedTsar.Parent.Age);
        }

        [Test]
        public void SameParentsHeights()
        {
            actualTsar.Parent.Height.Should().Be(expectedTsar.Parent.Height);
        }

        [Test]
        public void SameParentsParents()
        {
            actualTsar.Parent.Parent.Should().Be(expectedTsar.Parent.Parent);
        }

        /*
         * Вариант 2
         * Способ включает в себя все преимущества первого варианта, но при этом имеет всего один метод
         * Чтобы указать какие свойства нужно сравнивать, необходимо добавить его в опции сравнения с помощью метода Including
         * Таким образом, при добавлении нового свойства, нужно добавить одну строчку в метод, чтобы новое свойство тоже сравнивалось
         * Если одно или несколько свойств будут различаться, то в результате будут показаны все различия
         */
        [Test]
        public void TestWithIncluding()
        {
            expectedTsar.ShouldBeEquivalentTo(actualTsar, options => options
                .Including(thar => thar.Name)
                .Including(thar => thar.Age)
                .Including(thar => thar.Height)
                .Including(thar => thar.Weight)
                .Including(thar => thar.Parent.Name)
                .Including(thar => thar.Parent.Age)
                .Including(thar => thar.Parent.Height)
                .Including(thar => thar.Parent.Parent));
        }

        /*
         * Вариант 3
         * Вариант аналогичен второму варианту, только теперь мы не включаем свойства в сравнение, а наоборот исключаем их
         * В рассматриваемом объекте не сравниваемых свойств оказалось меньше, чем сравниваемых
         * Поэтому можно сравнить объекты по всем свойствам, исключая лишние
         * В этом случае, при добавлении новых свойств, необходимо расширять тест только в том случае, если новое свойство не нужно рассматривать
         */
        [Test]
        public void WestWithExcluding()
        {
            expectedTsar.ShouldBeEquivalentTo(actualTsar, options => options
                .Excluding(thar => thar.Id)
                .Excluding(thar => thar.Parent.Id));
        }

        /* Вариант 4
         * Во всех предыдущих вариантах не было рассмотрено случая, когда у родителя тоже есть родитель (не null)
         * В такой ситуации все тесты, сравнивающие thar.Parent.Parent упадут (за исключением CheckCurrentTsar_WithCustomEquality)
         * В данном варианте мы параллельно идем по родителям ожидаемого и актуального царя и сравниваем их
         * С помощью сообщения можно узнать на каком родителе(в случае самого царя - 0) произошла ошибка
         * При необходимости можно написать алгоритм, выводящий полную последовательность сравниваемых людей 
         * Я считаю данный вариант наилучшим, так как он сравнивает всю последовательность царей, исключая лишние свойства, так же как в 3ем варианте
         * При падении теста он выведет свойство, из-за которого упал тест, а так же родителя, у которого это свойство рассматривалось
         */
        [Test]
        public void TestWithLoop()
        {
            var expect = expectedTsar;
            var actual = actualTsar;
            var count = 0;
            while (expect != null && actual != null)
            {
                expect.ShouldBeEquivalentTo(actual, options => options
                    .Excluding(thar => thar.Id)
                    .Excluding(thar => thar.Parent), "Порядок родителя {0}", count++);
                expect = expect.Parent;
                actual = actual.Parent;
            }
            expect.ShouldBeEquivalentTo(actual);
        }

    }

    public class TsarRegistry
    {
        public static Person GetCurrentTsar()
        {
            return new Person(
                "Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));
        }
    }

    public class Person
    {
        public static int IdCounter = 0;
        public int Age, Height, Weight;
        public string Name;
        public Person Parent;
        public int Id;

        public Person(string name, int age, int height, int weight, Person parent)
        {
            Id = IdCounter++;
            Name = name;
            Age = age;
            Height = height;
            Weight = weight;
            Parent = parent;
        }
    }
}
