using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DomainLayer.Market.SearchEngine;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests
{
	public class SpellingTests
	{
		[Test]
		[TestCase("speling", "spelling")]
		[TestCase("acess", "access")]
		[TestCase("supposidly", "supposedly")]
		public void Should_correct_single_words(string mispelled, string expected)
		{
			// Arrange
			var spelling = new Spelling();

			// Act
			string actual = spelling.Correct(mispelled);

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[Test]
		[TestCase("I HAVEE Gowt thisse WrnG", "i have got this wrong")]
		public void Should_lower_case_sentence_correction(string mispelled, string expected)
		{
			var spelling = new Spelling();

			// Act
			string actual = spelling.CorrectSentence(mispelled);

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[Test]
		[TestCase("I havve wrtten somm woordz wwrong", "i have written some words wrong")]
		public void Should_correct_sentences(string mispelled, string expected)
		{
			// Arrange
			var spelling = new Spelling();

			// Act
			string actual = spelling.CorrectSentence(mispelled);

			// Assert
			Assert.AreEqual(expected, actual);
		}
	}
}