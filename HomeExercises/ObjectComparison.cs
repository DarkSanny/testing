using System;
using System.CodeDom;
using System.Linq.Expressions;
using System.Threading;
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
         * С помощью метода ShouldBeEquivalentTo можно сравнить два объекта по всем свойствам исключая лишние
		 * Так как класс Person имеет односвязную структуру, то необходимо рекурсивно проверять родителей
		 * Поэтому нужно разрешить бесконечную рекурсию с помощью AllowingInfiniteRecursion
		 * Если исключить из сравнения только Id, то любые два родителя будут не эквивалентны, так как их Id разные
		 * Чтобы исключить свойство Id у всех родителей, можно исключить все свойства заканчивающиеся на Parent.Id
		 * Таким образом у каждого Parent исключится из сравнения свойство Id
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
		public void CompareTsars()
		{
			expectedTsar.ShouldBeEquivalentTo(actualTsar, options => options
				.Excluding(tsar => tsar.SelectedMemberInfo.Name == nameof(Person.Id) && tsar.SelectedMemberInfo.DeclaringType == typeof(Person)));
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
