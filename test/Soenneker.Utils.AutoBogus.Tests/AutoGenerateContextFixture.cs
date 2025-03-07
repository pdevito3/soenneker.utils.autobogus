using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus.Context;
using Soenneker.Utils.AutoBogus.Extensions;
using Xunit;

namespace Soenneker.Utils.AutoBogus.Tests;

public class AutoGenerateContextFixture
{
    private readonly Faker _faker;
    private readonly List<string> _ruleSets;
    private readonly AutoFakerConfig _fakerConfig;
    private AutoFakerContext _context;

    public AutoGenerateContextFixture()
    {
        _faker = new Faker();
        _ruleSets = new List<string>();
        _fakerConfig = new AutoFakerConfig();
    }

    public class GenerateMany_Internal
        : AutoGenerateContextFixture
    {
        private int _value;
        private readonly List<int> _items;

        public GenerateMany_Internal()
        {
            _value = _faker.Random.Int();
            _items = new List<int> { _value };
            _context = new AutoFakerContext(_fakerConfig)
            {
                RuleSets = _ruleSets
            };
        }

        [Fact]
        public void Should_Generate_Configured_RepeatCount()
        {
            int count = _faker.Random.Int(3, 5);
            List<int>? expected = Enumerable.Range(0, count).Select(i => _value).ToList();

            _fakerConfig.RepeatCount = context => count;

            AutoGenerateContextExtension.GenerateMany(_context, null, _items, false, 1, () => _value);

            _items.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_Generate_Duplicates_If_Not_Unique()
        {
            AutoGenerateContextExtension.GenerateMany(_context, 2, _items, false, 1, () => _value);

            _items.Should().BeEquivalentTo(new[] { _value, _value });
        }

        [Fact]
        public void Should_Not_Generate_Duplicates_If_Unique()
        {
            int first = _value;
            int second = _faker.Random.Int();

            AutoGenerateContextExtension.GenerateMany(_context, 2, _items, true, 1, () =>
            {
                int item = _value;
                _value = second;

                return item;
            });

            _items.Should().BeEquivalentTo(new[] { first, second });
        }

        [Fact]
        public void Should_Short_Circuit_If_Unique_Attempts_Overflow()
        {
            var attempts = 0;

            AutoGenerateContextExtension.GenerateMany(_context, 2, _items, true, 1, () =>
            {
                attempts++;
                return _value;
            });

            attempts.Should().Be(AutoFakerConfig.GenerateAttemptsThreshold);

            _items.Should().BeEquivalentTo(new[] { _value });
        }
    }
}