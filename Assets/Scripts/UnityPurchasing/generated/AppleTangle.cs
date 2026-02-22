using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleTangle
    {
        private static byte[] data = System.Convert.FromBase64String("mqRNfRw0L4N5wY70NUZeAf7PJvrj1erj5rD49uTkGuHg1ebk5BrV+NbTv9WH1O7V7OPmsOHj9uewttT2Uv5YdqfB988i6vhTqHm7hi2uZfI805okYrA8Qnxc16cePTCUe5tEt+PmsPjr4fPh8c41jKJxk+wbEY5oZfHONYyicZPsGxGOaMulQxKiqJr6dD77orUO4Ai7nGHIDtNHsqmwCbeAiYyEi4aAxYqLxZGNjJbFhoCXwQcONFKVOuqgBMIvFIidCAJQ8vKf1Wfkk9Xr4+aw+Ork5Brh4ebn5M9jrWMS6OTk4ODl1YfU7tXs4+aw1fTj5rDh7/bvpJWViYDFrIuGy9SViYDFt4qKkcWmpNX78ujV09XR15zFhJaWkIiAlsWEhoaAlZGEi4aAgmrtUcUSLknJxYqVU9rk1WlSpirD1cHj5rDh7vb4pJWViYDFpoCXkWfk5ePsz2OtYxKGgeDk1WQX1c/ji4HFhoqLgYyRjIqLlsWKg8WQloCXhIaRjIaAxZaRhJGAiICLkZbL1dN8qcidUghpfjkWkn4XkzeS1aok4gmY3GZutsU23SFUWn+q744azhntzuPk4ODi5+Tz+42RkZWW38rKkvPV8ePmsOHm9uiklZWJgMW3ioqRjIOMhoSRjIqLxaSQkY2Kl4yRnNRQ30gR6uvld+5UxPPLkTDZ6D6H87xC4OyZ8qWz9PuRNlJuxt6iRjCK1WfhXtVn5kZF5ufk5+fk59Xo4+xORpR3orawJErKpFYdHgaVKANGqU05m8fQL8AwPOozjjFHwcb0EkRJycWGgJeRjIOMhoSRgMWViomMhpyRjIOMhoSRgMWHnMWEi5zFlYSXkeHj9uewttT21fTj5rDh7/bvpJWVVNW9Cb/h12mNVmr4O4CWGoK7gFlbEZZ+CzeB6i6cqtE9R9scnRqOLYmAxayLhsvUw9XB4+aw4e72+KSV2MOCxW/WjxLoZyo7DkbKHLaPvoFwe5/pQaJuvjHz0tYuIeqoK/GMNCz8lxC46zCaun4XwOZfsGqouOgU4OXmZ+Tq5dVn5O/nZ+Tk5QF0TOzFpqTVZ+TH1ejj7M9jrWMS6OTk5GqWZIUj/r7syndXHaGtFYXde/AQ7bvVZ+T04+aw+MXhZ+Tt1Wfk4dWViYDFpoCXkYyDjIaEkYyKi8WkkMWEi4HFhoCXkYyDjIaEkYyKi8WVxYqDxZGNgMWRjYCLxYSVlYmMhoSB0MbwrvC8+FZxEhN5eyq1XyS9tYeJgMWWkYSLgYSXgcWRgJeIlsWE+mBmYP582KLSF0x+pWvJMVR19z2RjYqXjJGc1PPV8ePmsOHm9uiklcrVZCbj7c7j5ODg4ufn1WRT/2RWy6VDEqKomu271frj5rD4xuH91fOsPZN61vGARJJxLMjn5uTl5EZn5NDX1NHV1tO/8ujW0NXX1dzX1NHVoJv6qY61c6RsIZGH7vVmpGLWb2QlhtaSEt/iybMOP+rE6z9flvyqUJKSy4SVlYmAy4aKiMqElZWJgIaEbvxsOxyuiRDiTsfV5w392x217DbqeNgWzqzN/y0bK1Bc6zy7+TMu2Ojj7M9jrWMS6OTk4ODl5mfk5OW5tU9vMD8BGTXs4tJVkJDE");
        private static int[] order = new int[] { 17,29,8,59,58,30,32,15,38,53,28,29,53,32,40,21,28,34,43,46,48,51,35,53,29,53,52,34,29,54,44,39,38,52,50,59,56,43,46,40,46,55,52,58,45,55,48,52,49,55,58,55,58,55,55,57,56,57,59,59,60 };
        private static int key = 229;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

