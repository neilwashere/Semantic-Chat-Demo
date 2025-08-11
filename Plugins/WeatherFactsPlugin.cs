using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace semantic_chat_demo.Plugins;

/// <summary>
/// A plugin that provides bombastic and entertaining weather facts
/// </summary>
public class WeatherFactsPlugin
{
    private readonly Random _random = new();

    private readonly string[] _bombasticFacts =
    [
        "BEHOLD! Lightning strikes the Earth approximately 100 times per SECOND, unleashing the spectacular fury of nature's own electrical orchestra!",
        "ASTOUNDING! A single raindrop can fall at the mind-bending speed of up to 20 mph, making it nature's tiny meteorite of magnificent wetness!",
        "INCREDIBLE! The largest hailstone ever recorded was a colossal 8 inches in diameter - imagine being pelted by ice boulders from the heavens!",
        "PHENOMENAL! Tornadoes can spin at absolutely DEVASTATING speeds of over 300 mph, creating nature's most terrifying dance of destruction!",
        "SPECTACULAR! A single thunderstorm cloud can contain more energy than 10 atomic bombs - talk about Mother Nature flexing her muscles!",
        "MIND-BLOWING! Snow is actually 90% air, making it nature's fluffiest architectural marvel that somehow manages to bury entire cities!",
        "EXTRAORDINARY! The hottest temperature ever recorded was a SCORCHING 134°F in Death Valley - hot enough to literally cook an egg on the sidewalk!",
        "MAGNIFICENT! Hurricane winds can reach speeds of 200+ mph, turning nature into a blender of biblical proportions!",
        "BREATHTAKING! A single cloud can weigh as much as a million pounds, yet somehow floats majestically in the sky like nature's impossible magic trick!",
        "STUPENDOUS! The coldest temperature ever recorded was -128.6°F in Antarctica - cold enough to freeze your breath before it leaves your mouth!",
        "AMAZING! Rainbows require EXACTLY the right angle of 42 degrees between the sun, your eye, and the raindrops to create nature's most dazzling light show!",
        "OUTRAGEOUS! A bolt of lightning is 5 times hotter than the surface of the sun, making it nature's own miniature star falling to Earth!"
    ];

    [KernelFunction, Description("Get a random bombastic weather fact that will absolutely blow your mind")]
    public string GetBombasticWeatherFact()
    {
        var randomIndex = _random.Next(_bombasticFacts.Length);
        return _bombasticFacts[randomIndex];
    }

    [KernelFunction, Description("Get a weather fact about a specific weather phenomenon")]
    public string GetWeatherFactAbout(
        [Description("The weather phenomenon to get a fact about (lightning, rain, snow, tornado, hurricane, etc.)")]
        string phenomenon)
    {
        // Find facts that mention the phenomenon
        var relevantFacts = _bombasticFacts
            .Where(fact => fact.ToLower().Contains(phenomenon.ToLower()))
            .ToList();

        if (relevantFacts.Any())
        {
            var randomIndex = _random.Next(relevantFacts.Count);
            return relevantFacts[randomIndex];
        }

        // If no specific fact found, return a general bombastic response
        return $"ASTOUNDING! While I don't have a specific bombastic fact about {phenomenon}, " +
               $"let me tell you that ALL weather phenomena are absolutely MAGNIFICENT displays of nature's incredible power! " +
               GetBombasticWeatherFact();
    }
}
